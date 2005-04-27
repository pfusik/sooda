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
                    text += ch;
                }
                _tokenType = CDILToken.String;
                _tokenValue = text;
                return;
            }
            if (Char.IsNumber(ch))
            {
                string text = "";
                while ((p = PeekChar()) != -1 && Char.IsNumber((char)p))
                {
                    ch = (char)p;
                    ReadChar();
                    text += ch;
                }
                _tokenType = CDILToken.Integer;
                _tokenValue = Convert.ToInt32(text);
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
                switch (tokenName)
                {
                    case "let":
                        _tokenType = CDILToken.Let;
                        break;

                    case "return":
                        _tokenType = CDILToken.Return;
                        break;

                    case "new":
                        _tokenType = CDILToken.New;
                        break;

                    case "if":
                        _tokenType = CDILToken.If;
                        break;

                    case "throw":
                        _tokenType = CDILToken.Throw;
                        break;

                    case "base":
                        _tokenType = CDILToken.Base;
                        break;

                    case "this":
                        _tokenType = CDILToken.This;
                        break;

                    case "arg":
                        _tokenType = CDILToken.Arg;
                        break;

                    case "cast":
                        _tokenType = CDILToken.Cast;
                        break;

                    default:
                        _tokenType = CDILToken.Keyword;
                        break;
                }
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