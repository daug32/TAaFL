﻿using System.Diagnostics;
using Grammars.Grammars.LeftRoRightOne.Models;
using Grammars.Grammars.LeftRoRightOne.Models.ValueObjects;
using Grammars.LL.Console.Parsers.Implementation;
using Grammars.LL.Console.Parsers.Implementation.Models;

namespace Grammars.LL.Console.Parsers;

public class GrammarParser
{
    private const char CommentIdentifier = '#';
    private const string RuleNameSeparator = "->";
    private const char RuleValueSeparator = ',';

    private readonly RuleNameParser _ruleNameParser = new();
    private readonly RuleValueParser _ruleValueParser = new();

    public Grammar ParseFromFile( string fullFilePath )
    {
        if ( !File.Exists( fullFilePath ) )
        {
            throw new ArgumentException( $"File was not found. FilePath: {Path.GetFileName( fullFilePath )}" );
        }
        
        using var reader = new StreamReader( fullFilePath );
        
        var rules = new List<GrammarRuleParseResult>();

        int lineNumber = 0;
        GrammarRuleParseResult? lastRule = null;
        for ( string? line = reader.ReadLine(); line != null; line = reader.ReadLine() )
        {
            lineNumber++;

            GrammarRuleParseResult? newRule;
            try
            {
                newRule = ParseLine( line, lineNumber, lastRule );
            }
            catch ( Exception ex )
            {
                throw new AggregateException( $"\nLine({lineNumber}): \"{line}\"\n", ex );
            }

            if ( newRule is not null )
            {
                if ( lastRule is not null )
                {
                    rules.Add( lastRule! );
                }

                lastRule = newRule;
            }
        }

        if ( lastRule is not null )
        {
            rules.Add( lastRule );
        }

        return new Grammar(
            startRule: rules.First().RuleName,
            rules: rules.Select( x => new GrammarRule( x.RuleName, x.Values ) ) );
    }

    private GrammarRuleParseResult? ParseLine( string line, int lineNumber, GrammarRuleParseResult? lastRule )
    {
        // Empty line
        if ( String.IsNullOrWhiteSpace( line ) )
        {
            return null;
        }
            
        // Comment
        if ( IsComment( line ) )
        {
            return null;
        }

        GrammarLineParseResult lineParseResult = ParseGrammarRule( line );
        if ( !lineParseResult.HasData )
        {
            return null;
        }

        // Rule name was probably declared previously, rule values are enumerating
        if ( lineParseResult.RuleName is null )
        {
            // Did not declared any rule before, throw an exception
            if ( lastRule is null )
            {
                throw new FormatException( $"Rule values are enumerated without declaring a ruleName. Line: {lineNumber}" );
            }
                
            lastRule.Values.AddRange( lineParseResult.Rules ?? throw new UnreachableException() );
            return null;
        }

        var rule = new GrammarRuleParseResult( lineParseResult.RuleName );
        if ( lineParseResult.Rules is not null )
        {
            rule.Values = lineParseResult.Rules;
        }

        return rule;
    }

    private static bool IsComment( string line )
    {
        foreach ( char symbol in line )
        {
            if ( Char.IsWhiteSpace( symbol ) )
            {
                continue;
            }

            return symbol == CommentIdentifier;
        }

        return false;
    }

    private GrammarLineParseResult ParseGrammarRule( string line )
    {
        var result = new GrammarLineParseResult();
        
        int ruleDeclarationIndex = line.IndexOf( RuleNameSeparator, StringComparison.Ordinal );

        int lineSymbolIndex = 0;
        if ( ruleDeclarationIndex >= 0 )
        {
            result.RuleName = _ruleNameParser.ParseFromLine( line, ruleDeclarationIndex, out int lastLineReadSymbolIndex );
            lineSymbolIndex = lastLineReadSymbolIndex + RuleNameSeparator.Length;
        }
        
        result.Rules = ParseRules( line, lineSymbolIndex );

        return result;
    }

    private List<RuleValue> ParseRules( string line, int lineSymbolIndex )
    {
        var possibleValues = new List<RuleValue>();

        bool isReadingValue = false;
        var lastRawValue = new List<char>();
        for ( ; lineSymbolIndex < line.Length; lineSymbolIndex++ )
        {
            char symbol = line[lineSymbolIndex];

            // Whitespaces do not count if don't read a rule value
            if ( !isReadingValue && Char.IsWhiteSpace( symbol ) )
            {
                continue;
            }

            // If a separator, commit current rule and create a new one
            if ( symbol == RuleValueSeparator )
            {
                isReadingValue = false;
                
                if ( lastRawValue.Any() )
                {
                    possibleValues.Add( _ruleValueParser.Parse( lastRawValue ) );
                    lastRawValue = new List<char>();
                }

                continue;
            }

            isReadingValue = true;

            // We are reading a rule value
            lastRawValue.Add( symbol );
        }

        if ( lastRawValue.Any() )
        {
            possibleValues.Add( _ruleValueParser.Parse( lastRawValue ) );
        }

        return possibleValues;
    }
}