﻿using Grammars.Common.Convertors.Implementation.Factorization;

namespace Grammars.Common.Convertors.Convertors;

public class LeftFactorizationConvertor : IGrammarConvertor
{
    public CommonGrammar Convert( CommonGrammar grammar )
    {
        return new LeftFactorizationHandler().Factorize( grammar );
    }
}