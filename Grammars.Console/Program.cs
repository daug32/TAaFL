﻿using Grammar.Parsers;
using Grammar.Parsers.Implementation;
using Grammars.Common.Convertors;
using Grammars.Common.Extensions.Grammar;
using Grammars.Common.ValueObjects;
using Grammars.Common.ValueObjects.Symbols;
using Grammars.LL.Convertors;
using Grammars.LL.Models;
using Grammars.Visualization;

namespace Grammars.Console;

public class Program
{
    private static readonly string _exitCommand = "exit";
    
    public static void Main()
    {
        LlOneGrammar grammar = BuildGrammar();
        AskForSentences( grammar );
    }

    private static LlOneGrammar BuildGrammar()
    {
        LlOneGrammar grammar = new GrammarFileParser( @"../../../Grammars/common.txt", new ParsingSettings() )
            .Parse()
            // .Convert( new RemoveWhitespacesConvertor() )
            .ToConsole( "Original grammar" )
            .Convert( new ToLlOneGrammarConvertor() )
            .ToConsole( "LL one grammar" );

        bool hasEndSymbol = grammar.Rules.Values.Any( rule => 
            rule.Definitions.Any( definition => 
                definition.Symbols.Any( symbol => 
                    symbol.Type == RuleSymbolType.TerminalSymbol && symbol.Symbol.Type == TerminalSymbolType.End ) ) );

        if ( !hasEndSymbol )
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine( "WARNING: No end symbol was found"  );
            System.Console.ResetColor();
        }

        return grammar;
    }

    private static void AskForSentences( LlOneGrammar grammar )
    {
        while ( true )
        {
            System.Console.Write( $"Enter sentence or type \"{_exitCommand}\" to exit: " );

            string command = System.Console.ReadLine() ?? String.Empty;
            if ( _exitCommand.Equals( command, StringComparison.InvariantCultureIgnoreCase ) )
            {
                System.Console.WriteLine( "Exiting" );
                return;
            }

            RunResult result = grammar.Run( command );
            if ( result.RunResultType == RunResultType.Error )
            {
                System.Console.WriteLine( "Sentence is invalid." );
                System.Console.WriteLine( $"Sentence: {result.Sentence}" );

                if ( result.Error.InvalidSymbolIndex != null )
                {
                    System.Console.WriteLine( $"Location: {result.Error!.InvalidSymbolIndex}" );
                }

                if ( result.Error.IsNotLlGrammar )
                {
                    System.Console.WriteLine( "Not LL grammar" );
                }

                continue;
            }

            System.Console.WriteLine( "Sentence is correct" );
        }
    }
}