﻿using Grammars.Grammars.LeftRoRightOne.Models.ValueObjects;

namespace Grammars.Grammars.LeftRoRightOne.Models.Validations;

internal class GrammarRulesAchievabilityValidator
{
    // If a non terminal rule is achievable, then all its values are achievable
    public IEnumerable<RuleName> CheckAndGetFailed(
        RuleName startRule,
        IDictionary<RuleName, GrammarRule> rules )
    {
        HashSet<RuleName> uncheckedRules = rules.Keys.ToHashSet();
        
        var queue = new Queue<RuleName>();
        queue.Enqueue( startRule );

        while ( queue.Any() )
        {
            RuleName ruleName = queue.Dequeue();
            if ( !uncheckedRules.Contains( ruleName ) )
            {
                continue;
            }

            uncheckedRules.Remove( ruleName );

            GrammarRule rule = rules[ruleName];
            foreach ( RuleValue ruleValue in rule.Values )
            {
                foreach ( RuleSymbol symbol in ruleValue.Symbols )
                {
                    if ( symbol.Type != RuleSymbolType.NonTerminalSymbol )
                    {
                        continue;
                    }
                    
                    queue.Enqueue( symbol.RuleName! );
                }
            }
        }

        return uncheckedRules;
    }
}