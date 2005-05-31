using System;

namespace Sooda.StubGen.CDIL
{
    // CodeDOM intermediate language tokens

	public enum CDILToken
	{
        BOF,        // begin of token stream
        EOF,        // end of token stream

        LeftParen,  // (
        RightParen, // )
        Comma,      // ,
        Semicolon,  // ;
        Dot,        // .
        Dollar,     // $
        Assign,     // =

        Integer,    // 1
        String,     // 'aaaa'
        Boolean,    // true,false

        Keyword,    // keyword - generic
	}
}
