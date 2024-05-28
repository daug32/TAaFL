﻿using Grammars.Common;
using Grammars.Common.ValueObjects;

namespace Grammars.LL.Models;

public class LlOneGrammar : CommonGrammar
{
    public LlOneGrammar( RuleName startRule, IEnumerable<GrammarRule> rules )
        : base( startRule, rules )
    {
    }

    public RunResult Run( string sentence )
    {
        try
        {
            return new LlOneGrammarRunner( this ).Run( sentence );
        }
        catch ( Exception )
        {
            return RunResult.Fail( sentence, RunError.NotLl() );
        }
    }
}