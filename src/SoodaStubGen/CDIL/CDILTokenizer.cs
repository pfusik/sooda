using System;
using System.IO;

namespace Sooda.StubGen.CDIL
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
                while ((p =ReadChar()) != -1)
                {
                    ch = (char)p;
                    if (ch == '\'')
                        break;
                    if (ch == '\\')
                    {
                        text += (char)ReadChar();
                    }
                    else
                    {
                        text += ch;
                    }
                }
                _tokenType = CDILToken.String;
                _tokenValue = text;
                return;
            }
            if (Char.IsNumber(ch) || ch == '-')
            {
                bool minus = ch == '-';
                if (minus) ReadChar();
                
                string text = "";
                while ((p = PeekChar()) != -1 && Char.IsNumber((char)p))
                {
                    ch = (char)p;
                    ReadChar();
                    text += ch;
                }
                if (text.Length == 0) throw BuildException("Number expected after -: " + ch);
                _tokenType = CDILToken.Integer;
                _tokenValue = minus ? -Convert.ToInt32(text) : Convert.ToInt32(text);
                return;
            }
            if (ch == '(')
            {
                ReadChar();
                _tokenType = CDILToken.LeftParen;
                return;
            }
            if (ch == ')')
            {
                ReadChar();
                _tokenType = CDILToken.RightParen;
                return;
            }
            if (ch == '=')
            {
                ReadChar();
                _tokenType = CDILToken.Assign;
                return;
            }

            if (ch == ',')
            {
                ReadChar();
                _tokenType = CDILToken.Comma;
                return;
            }

            if (ch == '.')
            {
                ReadChar();
                _tokenType = CDILToken.Dot;
                return;
            }

            if (ch == '$')
            {
                ReadChar();
                _tokenType = CDILToken.Dollar;
                return;
            }

            if (ch == ';')
            {
                ReadChar();
                _tokenType = CDILToken.Semicolon;
                return;
            }

            if (Char.IsLetter(ch) || ch == '_')
            {
                string tokenName = "";

                while (Char.IsLetterOrDigit(ch) || ch == '_')
                {
                    tokenName += (char)ReadChar();
                    ch = (char)PeekChar();
                }
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
            return new ArgumentException(msg + " Next token: " + _input.Substring(_pos, _input.Length - _pos));
        }
    }
}
