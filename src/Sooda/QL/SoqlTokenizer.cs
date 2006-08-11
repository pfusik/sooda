// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Text;
using System.Collections;

namespace Sooda.QL
{
    public enum SoqlTokenType
    {
        EOF,
        BOF,
        Number,
        String,
        Keyword,
        //Punct,
        Whitespace,

        FirstPunct,

        Add,
        Sub,
        Mul,
        Div,
        Mod,
        LT,
        GT,
        LE,
        GE,
        EQ,
        NE,
        LeftParen,
        RightParen,
        LeftCurlyBrace,
        RightCurlyBrace,
        At,
        Dot,
        Comma,
        Not,
        QuestionMark,
        And,
        Or,
        Colon,

        LastPunct,
        Invalid,
        Asterisk = Mul,
    };

    public class SqlTokenizer
    {
        private string _inputString = null;
        private int _position = 0;
        private int _tokenPosition = 0;

        SoqlTokenType _tokenType;
        string _tokenValue;
        string _tokenValueLowercase;
        public bool IgnoreWhiteSpace = true;

        public int TokenPosition
        {
            get
            {
                return _tokenPosition;
            }
        }

        public SoqlTokenType TokenType
        {
            get
            {
                return _tokenType;
            }
            set
            {
                _tokenType = value;
            }
        }

        public string TokenValue
        {
            get
            {
                return _tokenValue;
            }
        }

        public string StringTokenValue
        {
            get
            {
                string s = _tokenValue;

                return s.Substring(1, s.Length - 2).Replace("''", "'");
            }
        }

        public SqlTokenizer() { }

        void SkipWhitespace()
        {
            int ch;

            while ((ch = PeekChar()) != -1)
            {
                if (!Char.IsWhiteSpace((char)ch))
                    break;
                ReadChar();
            };
        }

        public void InitTokenizer(string s)
        {
            _inputString = s;
            _position = 0;
            _tokenType = SoqlTokenType.BOF;

            GetNextToken();
        }

        int PeekChar()
        {
            if (_position < _inputString.Length)
            {
                return (int)_inputString[_position];
            }
            else
            {
                return -1;
            }
        }

        int ReadChar()
        {
            if (_position < _inputString.Length)
            {
                return (int)_inputString[_position++];
            }
            else
            {
                return -1;
            }
        }

        public void Expect(SoqlTokenType type)
        {
            if (_tokenType != type)
                throw new SoqlException("Expected token of type: " + type + ", got " + _tokenType + " (" + _tokenValue + ").", TokenPosition);

            GetNextToken();
        }


        public void ExpectKeyword(string s)
        {
            if (_tokenType != SoqlTokenType.Keyword)
                throw new SoqlException("Expected keyword: " + s + ", got " + _tokenType + ".", TokenPosition);

            if (_tokenValueLowercase != s)
                throw new SoqlException("Expected keyword: " + s + ", got " + _tokenValueLowercase + ".", TokenPosition);

            GetNextToken();
        }

        public string EatKeyword()
        {
            if (_tokenType != SoqlTokenType.Keyword)
                throw new SoqlException("Identifier expected", TokenPosition);

            string s = (string)_tokenValue;
            GetNextToken();
            return s;
        }

        public bool IsKeyword(string s)
        {
            if (_tokenType != SoqlTokenType.Keyword)
                return false;

            if (_tokenValueLowercase != s)
                return false;

            return true;
        }

        public bool IsKeyword()
        {
            if (_tokenType != SoqlTokenType.Keyword)
                return false;

            return true;
        }

        public bool IsEOF()
        {
            if (_tokenType != SoqlTokenType.EOF)
                return false;
            return true;
        }

        public bool IsNumber()
        {
            return _tokenType == SoqlTokenType.Number;
        }

        public bool IsToken(SoqlTokenType token)
        {
            return _tokenType == token;
        }

        public bool IsToken(object[] tokens)
        {
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (tokens[i] is string)
                {
                    if (IsKeyword((string)tokens[i]))
                        return true;
                }
                else
                {
                    if (_tokenType == (SoqlTokenType)tokens[i])
                        return true;
                }
            }
            return false;
        }

        public bool IsPunctuation()
        {
            return (_tokenType >= SoqlTokenType.FirstPunct && _tokenType < SoqlTokenType.LastPunct);
        }

        struct CharToTokenType
        {
            public char ch;
            public SoqlTokenType tokenType;

            public CharToTokenType(char ch, SoqlTokenType tokenType)
            {
                this.ch = ch;
                this.tokenType = tokenType;
            }
        }

        static CharToTokenType[] charToTokenType =
            {
                new CharToTokenType('+', SoqlTokenType.Add),
                new CharToTokenType('-', SoqlTokenType.Sub),
                new CharToTokenType('*', SoqlTokenType.Mul),
                new CharToTokenType('/', SoqlTokenType.Div),
                new CharToTokenType('%', SoqlTokenType.Mod),
                new CharToTokenType('<', SoqlTokenType.LT),
                new CharToTokenType('>', SoqlTokenType.GT),
                new CharToTokenType('=', SoqlTokenType.EQ),
                new CharToTokenType('(', SoqlTokenType.LeftParen),
                new CharToTokenType(')', SoqlTokenType.RightParen),
                new CharToTokenType('{', SoqlTokenType.LeftCurlyBrace),
                new CharToTokenType('}', SoqlTokenType.RightCurlyBrace),
                new CharToTokenType('@', SoqlTokenType.At),
                new CharToTokenType('.', SoqlTokenType.Dot),
                new CharToTokenType(',', SoqlTokenType.Comma),
                new CharToTokenType('!', SoqlTokenType.Not),
                new CharToTokenType('?', SoqlTokenType.QuestionMark),
                new CharToTokenType(':', SoqlTokenType.Colon),
            };

        static SoqlTokenType[] charIndexToTokenType = new SoqlTokenType[128];

        static SqlTokenizer()
        {
            for (int i = 0; i < 128; ++i)
            {
                charIndexToTokenType[i] = SoqlTokenType.Invalid;
            };

            foreach (CharToTokenType cht in charToTokenType)
            {
                // Console.WriteLine("Setting up {0} to {1}", cht.ch, cht.tokenType);
                charIndexToTokenType[(int)cht.ch] = cht.tokenType;
            }
        }

        public void GetNextToken()
        {
            if (_tokenType == SoqlTokenType.EOF)
                throw new Exception("Cannot read past end of stream.");

            if (IgnoreWhiteSpace)
            {
                SkipWhitespace();
            };

            _tokenPosition = _position;

            int i = PeekChar();
            if (i == -1)
            {
                TokenType = SoqlTokenType.EOF;
                return;
            }

            char ch = (char)i;

            if (!IgnoreWhiteSpace && Char.IsWhiteSpace(ch))
            {
                StringBuilder sb = new StringBuilder();
                int ch2;

                while ((ch2 = PeekChar()) != -1)
                {
                    if (!Char.IsWhiteSpace((char)ch2))
                        break;

                    sb.Append((char)ch2);
                    ReadChar();
                };

                TokenType = SoqlTokenType.Whitespace;
                _tokenValue = sb.ToString();
                return;
            }

            if (Char.IsDigit(ch))
            {
                TokenType = SoqlTokenType.Number;
                string s = "";

                s += ch;
                ReadChar();

                while ((i = PeekChar()) != -1)
                {
                    ch = (char)i;

                    if (Char.IsDigit(ch) || (ch == '.'))
                    {
                        s += (char)ReadChar();
                    }
                    else
                    {
                        break;
                    };
                };

                _tokenValue = s;
                return;
            }

            if (ch == '\'')
            {
                TokenType = SoqlTokenType.String;

                string s = "";

                s += ch;
                ReadChar();

                while ((i = PeekChar()) != -1)
                {
                    ch = (char)i;

                    s += (char)ReadChar();

                    if (ch == '\'')
                    {
                        if (PeekChar() == (int)'\'')
                        {
                            s += '\'';
                            ReadChar();
                        }
                        else
                            break;
                    }
                };

                _tokenValue = s;
                return;
            }

            if (ch == '_' || Char.IsLetter(ch))
            {
                TokenType = SoqlTokenType.Keyword;

                StringBuilder sb = new StringBuilder();

                sb.Append((char)ch);

                ReadChar();

                while ((i = PeekChar()) != -1)
                {
                    if ((char)i == '_' || Char.IsLetterOrDigit((char)i))
                    {
                        sb.Append((char)ReadChar());
                    }
                    else
                    {
                        break;
                    };
                };

                _tokenValue = sb.ToString();
                _tokenValueLowercase = _tokenValue.ToLower();
                return;
            }

            ReadChar();
            _tokenValue = ch.ToString();

            if (ch == '<' && PeekChar() == (int)'>')
            {
                TokenType = SoqlTokenType.NE;
                _tokenValue = "<>";
                ReadChar();
                return;
            }

            if (ch == '!' && PeekChar() == (int)'=')
            {
                TokenType = SoqlTokenType.NE;
                _tokenValue = "!=";
                ReadChar();
                return;
            }

            if (ch == '&' && PeekChar() == (int)'&')
            {
                TokenType = SoqlTokenType.And;
                _tokenValue = "&&";
                ReadChar();
                return;
            }

            if (ch == '|' && PeekChar() == (int)'|')
            {
                TokenType = SoqlTokenType.Or;
                _tokenValue = "||";
                ReadChar();
                return;
            }

            if (ch == '<' && PeekChar() == (int)'=')
            {
                TokenType = SoqlTokenType.LE;
                _tokenValue = "<=";
                ReadChar();
                return;
            }

            if (ch == '>' && PeekChar() == (int)'=')
            {
                TokenType = SoqlTokenType.GE;
                _tokenValue = ">=";
                ReadChar();
                return;
            }

            if (ch == '=' && PeekChar() == (int)'=')
            {
                TokenType = SoqlTokenType.EQ;
                _tokenValue = "==";
                ReadChar();
                return;
            }

            if (ch >= 32 && ch < 128)
            {
                SoqlTokenType tt = charIndexToTokenType[ch];

                if (tt != SoqlTokenType.Invalid)
                {
                    TokenType = tt;
                    _tokenValue = new String(ch, 1);
                    return;
                }
                else
                {
                    throw new Exception("Invalid punctuation: " + ch);
                }
            }
            throw new Exception("Invalid token: " + ch);
        }
    }
}
