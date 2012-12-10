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
using System.IO;

namespace Sooda.CodeGen.CDIL
{
    public class CDILTokenizer
    {
        private CDILToken _tokenType;
        private object _tokenValue;
        private string _input;
        private int _pos;

        public CDILTokenizer()
        {
            _tokenType = CDILToken.BOF;
        }

        public CDILToken TokenType
        {
            get { return _tokenType; }
        }

        public object TokenValue
        {
            get { return _tokenValue; }
        }

        public CDILTokenizer(string text)
        {
            _input = text;
            _pos = 0;
            GetNextToken();
        }

        private int ReadChar()
        {
            if (_pos < _input.Length)
            {
                return (int)_input[_pos++];
            }
            else
            {
                return -1;
            }
        }

        private int PeekChar()
        {
            if (_pos < _input.Length)
            {
                return (int)_input[_pos];
            }
            else
            {
                return -1;
            }
        }

        private void SkipWhitespace()
        {
            while (Char.IsWhiteSpace((char)PeekChar()))
            {
                ReadChar();
            }
        }

        public void GetNextToken()
        {
            if (_tokenType == CDILToken.EOF)
            {
                throw BuildException("Cannot move past EOF in CDIL");
            }
            SkipWhitespace();
            int p = PeekChar();
            if (p == -1)
            {
                _tokenType = CDILToken.EOF;
                return;
            }
            char ch = (char)p;
            if (ch == '\'')
            {
                ReadChar();
                string text = "";
                while ((p = ReadChar()) != -1)
                {
                    ch = (char)p;
                    if (ch == '\'')
                        break;
                    if (ch == '\\')
                        text += (char)ReadChar();
                    else
                        text += ch;
                }
                _tokenType = CDILToken.String;
                _tokenValue = text;
                return;
            }
            if (Char.IsNumber(ch) || ch == '-')
            {
                string text = "";
                if (ch == '-')
                {
                    text = "-";
                    ReadChar();
                }
                while ((p = PeekChar()) != -1 && Char.IsNumber((char)p))
                {
                    ch = (char)p;
                    ReadChar();
                    text += ch;
                }
                if (text == "-") throw BuildException("Number expected after -");
                _tokenType = CDILToken.Integer;
                _tokenValue = Convert.ToInt32(text);
                return;
            }
            switch (ch)
            {
                case '(':
                    ReadChar();
                    _tokenType = CDILToken.LeftParen;
                    return;
                case ')':
                    ReadChar();
                    _tokenType = CDILToken.RightParen;
                    return;
                case '=':
                    ReadChar();
                    _tokenType = CDILToken.Assign;
                    return;
                case ',':
                    ReadChar();
                    _tokenType = CDILToken.Comma;
                    return;
                case '.':
                    ReadChar();
                    _tokenType = CDILToken.Dot;
                    return;
                case '$':
                    ReadChar();
                    _tokenType = CDILToken.Dollar;
                    return;
                case ';':
                    ReadChar();
                    _tokenType = CDILToken.Semicolon;
                    return;
                default:
                    break;
            }

            if (Char.IsLetter(ch) || ch == '_')
            {
                string tokenName = "";
                do
                {
                    tokenName += (char)ReadChar();
                    ch = (char)PeekChar();
                }
                while (Char.IsLetterOrDigit(ch) || ch == '_');
                _tokenValue = tokenName;
                _tokenType = CDILToken.Keyword;
                return;
            }

            throw BuildException("Unrecognized character: " + ch);

        }

        public void Expect(CDILToken token)
        {
            if (TokenType == token)
                GetNextToken();
            else
                throw BuildException("'" + token + "' expected");
        }

        public string EatKeyword()
        {
            if (TokenType < CDILToken.Keyword)
                throw BuildException("Keyword expected.");
            string retval = (string)_tokenValue;
            GetNextToken();
            return retval;
        }

        public void ExpectKeyword(string keyword)
        {
            if (TokenType < CDILToken.Keyword)
                throw BuildException("Keyword '" + keyword + "'expected.");
            if (keyword != (string)_tokenValue)
                throw BuildException("Keyword '" + keyword + "' expected.");
            GetNextToken();
        }

        public bool IsKeyword(string keyword)
        {
            if (TokenType < CDILToken.Keyword)
                return false;
            if (keyword != (string)_tokenValue)
                return false;
            return true;
        }

        public Exception BuildException(string msg)
        {
            return new ArgumentException(msg + " Next token: " + _input.Substring(_pos, _input.Length - _pos).Substring(0,50) + " Input: " + _input);
        }
    }
}
