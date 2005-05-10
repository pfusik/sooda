// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
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

using Sooda.QL;

namespace Sooda.QL {
    public class SoqlParser {
        private SqlTokenizer tokenizer = new SqlTokenizer();

        private SoqlParser(string query) {
            tokenizer.InitTokenizer(query);
        }

        private SoqlFunctionCallExpression ParseFunctionCall(string functionName) {
            SoqlExpressionCollection par = new SoqlExpressionCollection();

            while (!tokenizer.IsEOF() && tokenizer.TokenType != SoqlTokenType.RightParen) {
                par.Add(ParseExpression());
                if (tokenizer.TokenType != SoqlTokenType.Comma)
                    break;
                tokenizer.GetNextToken();
            }
            tokenizer.Expect(SoqlTokenType.RightParen);

            return new SoqlFunctionCallExpression(functionName, par);
        }

        private SoqlQueryExpression ParseSimplifiedQuery(string fromClass, string alias) {
            SoqlQueryExpression query = new SoqlQueryExpression();

            query.From.Add(fromClass);
            query.FromAliases.Add(String.Empty);
            tokenizer.ExpectKeyword("where");
            query.WhereClause = ParseBooleanExpression();
            return query;
        }

        private SoqlExpression ParsePathLikeExpression(string firstKeyword) {
            if (0 == String.Compare(firstKeyword, "soodaclass", true, System.Globalization.CultureInfo.InvariantCulture))
                return new SoqlSoodaClassExpression();

            SoqlPathExpression prop = new SoqlPathExpression(firstKeyword);

            while (tokenizer.TokenType == SoqlTokenType.Dot) {
                tokenizer.GetNextToken();
                if (tokenizer.TokenType == SoqlTokenType.Asterisk) {
                    tokenizer.GetNextToken();
                    return new SoqlAsteriskExpression(prop);
                } else if (tokenizer.IsKeyword("contains")) // lowercase
                {
                    string collectionName = prop.PropertyName;

                    tokenizer.EatKeyword();
                    if (tokenizer.TokenType != SoqlTokenType.LeftParen)
                        throw new SoqlException("'(' expected on Contains()", tokenizer.TokenPosition);
                    SoqlExpression expr = ParseLiteralExpression();
                    return new SoqlContainsExpression(prop.Left, collectionName, expr);
                } else if (tokenizer.IsKeyword("count")) // lowercase
                {
                    string collectionName = prop.PropertyName;
                    tokenizer.EatKeyword();
                    return new SoqlCountExpression(prop.Left, collectionName);
                } else if (tokenizer.IsKeyword("soodaclass")) // lowercase
                {
                    tokenizer.EatKeyword();
                    return new SoqlSoodaClassExpression(prop);
                } else {
                    string keyword = tokenizer.EatKeyword();
                    prop = new SoqlPathExpression(prop, keyword);
                }
            }
            return prop;
        }

        private SoqlRawExpression ParseRawExpression() {
            StringBuilder sb = new StringBuilder();
            int parenBalance = 0;
            bool iws = tokenizer.IgnoreWhiteSpace;
            tokenizer.IgnoreWhiteSpace = false;
            while (!tokenizer.IsEOF() && (tokenizer.TokenType != SoqlTokenType.RightParen || parenBalance > 0)) {
                sb.Append(tokenizer.TokenValue);
                if (tokenizer.TokenType == SoqlTokenType.LeftParen)
                    parenBalance++;
                if (tokenizer.TokenType == SoqlTokenType.RightParen)
                    parenBalance--;
                tokenizer.GetNextToken();
            }
            tokenizer.IgnoreWhiteSpace = iws;
            tokenizer.Expect(SoqlTokenType.RightParen);
            return new SoqlRawExpression(sb.ToString());
        }

        private SoqlExpression ParseLiteralExpression() {
            if (tokenizer.IsToken(SoqlTokenType.LeftParen)) {
                tokenizer.GetNextToken();
                SoqlExpression e;
                if (tokenizer.IsKeyword("select"))
                    e = ParseQuery();
                else
                    e = ParseExpression();
                tokenizer.Expect(SoqlTokenType.RightParen);
                return e;
            };

            if (tokenizer.IsNumber()) {
                SoqlExpression e = new SoqlLiteralExpression(Int32.Parse((string)tokenizer.TokenValue));
                tokenizer.GetNextToken();
                return e;
            }

            if (tokenizer.TokenType == SoqlTokenType.String) {
                SoqlExpression e = new SoqlLiteralExpression(tokenizer.StringTokenValue);
                tokenizer.GetNextToken();
                return e;
            }

            if (tokenizer.TokenType == SoqlTokenType.LeftCurlyBrace) {
                tokenizer.GetNextToken();
                int val = Int32.Parse((string)tokenizer.TokenValue);
                tokenizer.GetNextToken();
                tokenizer.Expect(SoqlTokenType.RightCurlyBrace);
                return new SoqlParameterLiteralExpression(val);
            }

            if (tokenizer.TokenType == SoqlTokenType.Asterisk) {
                tokenizer.GetNextToken();
                return new SoqlAsteriskExpression(null);
            }

            if (tokenizer.TokenType == SoqlTokenType.Keyword) {
                string keyword = tokenizer.EatKeyword();

                if (0 == String.Compare(keyword, "null", true, System.Globalization.CultureInfo.InvariantCulture))
                    return new SoqlNullLiteral();

                if (0 == String.Compare(keyword, "true", true, System.Globalization.CultureInfo.InvariantCulture))
                    return new SoqlBooleanLiteralExpression(true);

                if (0 == String.Compare(keyword, "false", true, System.Globalization.CultureInfo.InvariantCulture))
                    return new SoqlBooleanLiteralExpression(false);

                if (tokenizer.TokenType == SoqlTokenType.LeftParen) {
                    tokenizer.GetNextToken();

                    if (0 == String.Compare(keyword, "rawquery", true, System.Globalization.CultureInfo.InvariantCulture))
                        return ParseRawExpression();

                    SoqlFunctionCallExpression callExpr = ParseFunctionCall(keyword);
                    return callExpr;
                }

                /*if (tokenizer.IsKeyword("as"))
                {
                    tokenizer.GetNextToken();
                    string alias = tokenizer.EatKeyword();
                    string className = keyword;

                    return ParseSimplifiedQuery(className, alias);
                }
                */

                if (tokenizer.IsKeyword("where")) {
                    // className WHERE expr

                    return ParseSimplifiedQuery(keyword, String.Empty);
                }

                return ParsePathLikeExpression(keyword);
            }

            if (tokenizer.TokenType == SoqlTokenType.Sub) {
                tokenizer.GetNextToken();
                return new SoqlUnaryNegationExpression(ParseLiteralExpression());
            }

            throw new SoqlException("Unexpected token: " + tokenizer.TokenValue, tokenizer.TokenPosition);
        }

        private SoqlExpression ParseMultiplicativeException() {
            SoqlExpression e = ParseLiteralExpression();

            if (tokenizer.IsToken(SoqlTokenType.Mul)) {
                tokenizer.GetNextToken();
                return new SoqlBinaryExpression(e, ParseMultiplicativeException(), SoqlBinaryOperator.Mul);
            }
            if (tokenizer.IsToken(SoqlTokenType.Div)) {
                tokenizer.GetNextToken();
                return new SoqlBinaryExpression(e, ParseMultiplicativeException(), SoqlBinaryOperator.Div);
            }
            if (tokenizer.IsToken(SoqlTokenType.Mod)) {
                tokenizer.GetNextToken();
                return new SoqlBinaryExpression(e, ParseMultiplicativeException(), SoqlBinaryOperator.Mod);
            }
            return e;
        }

        private SoqlExpression ParseAdditiveExpression() {
            SoqlExpression e = ParseMultiplicativeException();

            if (tokenizer.IsToken(SoqlTokenType.Add)) {
                tokenizer.GetNextToken();
                return new SoqlBinaryExpression(e, ParseAdditiveExpression(), SoqlBinaryOperator.Add);
            }
            if (tokenizer.IsToken(SoqlTokenType.Sub)) {
                tokenizer.GetNextToken();
                return new SoqlBinaryExpression(e, ParseAdditiveExpression(), SoqlBinaryOperator.Sub);
            }
            return e;
        }

        private SoqlExpression ParseInExpression(SoqlExpression lhs) {
            SoqlExpressionCollection rhs = new SoqlExpressionCollection();

            tokenizer.ExpectKeyword("in");
            tokenizer.Expect(SoqlTokenType.LeftParen);
            rhs.Add(ParseAdditiveExpression());
            while (tokenizer.TokenType == SoqlTokenType.Comma) {
                tokenizer.Expect(SoqlTokenType.Comma);
                rhs.Add(ParseAdditiveExpression());
            }
            tokenizer.Expect(SoqlTokenType.RightParen);
            return new SoqlBooleanInExpression(lhs, rhs);
        }

        private SoqlExpression ParseBooleanRelation() {
            SoqlExpression e = ParseAdditiveExpression();

            if (tokenizer.IsToken(SoqlTokenType.EQ)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.Equal);
            };

            if (tokenizer.IsToken(SoqlTokenType.NE)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.NotEqual);
            };

            if (tokenizer.IsToken(SoqlTokenType.LT)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.Less);
            };

            if (tokenizer.IsToken(SoqlTokenType.GT)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.Greater);
            };

            if (tokenizer.IsToken(SoqlTokenType.LE)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.LessOrEqual);
            };

            if (tokenizer.IsToken(SoqlTokenType.GE)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.GreaterOrEqual);
            };

            if (tokenizer.IsKeyword("like")) {
                tokenizer.GetNextToken();
                return new SoqlBooleanRelationalExpression(e, ParseAdditiveExpression(), SoqlRelationalOperator.Like);
            }

            if (tokenizer.IsKeyword("is")) {
                bool notnull = false;

                tokenizer.GetNextToken();
                if (tokenizer.IsKeyword("not")) {
                    notnull = true;
                    tokenizer.GetNextToken();
                }
                tokenizer.ExpectKeyword("null");

                return new SoqlBooleanIsNullExpression(e, notnull);
            }

            if (tokenizer.IsKeyword("in")) {
                return ParseInExpression(e);
            }

            return e;
        }

        private SoqlExpression ParseBooleanPredicate() {
            if (tokenizer.IsKeyword("not") || tokenizer.IsToken(SoqlTokenType.Not)) {
                tokenizer.GetNextToken();
                return new SoqlBooleanNegationExpression((SoqlBooleanExpression)ParseBooleanPredicate());
            }

            if (tokenizer.IsKeyword("exists")) {
                SoqlQueryExpression query;

                tokenizer.GetNextToken();
                tokenizer.Expect(SoqlTokenType.LeftParen);
                if (tokenizer.IsKeyword("select")) {
                    query = ParseQuery();
                } else {
                    query = new SoqlQueryExpression();
                    query.SelectExpressions.Add(new SoqlAsteriskExpression(null));
                    query.SelectAliases.Add(String.Empty);
                    ParseFrom(query);
                    tokenizer.ExpectKeyword("where");
                    query.WhereClause = ParseBooleanExpression();
                }

                tokenizer.Expect(SoqlTokenType.RightParen);
                return new SoqlExistsExpression(query);
            }

            return ParseBooleanRelation();
        }

        private SoqlExpression ParseBooleanAnd() {
            SoqlExpression e = ParseBooleanPredicate();

            while (tokenizer.IsKeyword("and") || tokenizer.IsToken(SoqlTokenType.And)) {
                tokenizer.GetNextToken();
                e = new SoqlBooleanAndExpression((SoqlBooleanExpression)e, (SoqlBooleanExpression)ParseBooleanPredicate());
            }
            return e;
        }

        private SoqlExpression ParseBooleanOr() {
            SoqlExpression e = ParseBooleanAnd();

            while (tokenizer.IsKeyword("or") || tokenizer.IsToken(SoqlTokenType.Or)) {
                tokenizer.GetNextToken();
                e = new SoqlBooleanOrExpression((SoqlBooleanExpression)e, (SoqlBooleanExpression)ParseBooleanAnd());
            }
            return e;
        }

        private SoqlBooleanExpression ParseBooleanExpression() {
            SoqlExpression expr = ParseBooleanOr();
            if (!(expr is SoqlBooleanExpression)) {
                throw new SoqlException("Boolean expected");
            }
            return (SoqlBooleanExpression)expr;
        }

        private SoqlExpression ParseExpression() {
            return ParseBooleanOr();
        }

        private void ParseSelectExpressions(SoqlQueryExpression query) {
            bool again;

            do {
                again = false;
                SoqlExpression expr = ParseExpression();
                string alias;

                if (tokenizer.IsKeyword("as")) {
                    tokenizer.EatKeyword();
                    alias = tokenizer.EatKeyword();
                } else {
                    alias = String.Empty;
                }
                query.SelectExpressions.Add(expr);
                query.SelectAliases.Add(alias);

                if (tokenizer.IsToken(SoqlTokenType.Comma)) {
                    tokenizer.GetNextToken();
                    again = true;
                }
            } while (again);
        }

        private void ParseGroupByExpressions(SoqlQueryExpression query) {
            tokenizer.ExpectKeyword("group");
            tokenizer.ExpectKeyword("by");
            query.GroupByExpressions = new SoqlExpressionCollection();

            bool again;

            do {
                again = false;
                SoqlExpression expr = ParseExpression();
                query.GroupByExpressions.Add(expr);

                if (tokenizer.IsToken(SoqlTokenType.Comma)) {
                    tokenizer.GetNextToken();
                    again = true;
                }
            } while (again);
        }

        private void ParseOrderByExpressions(SoqlQueryExpression query) {
            tokenizer.ExpectKeyword("order");
            tokenizer.ExpectKeyword("by");

            query.OrderByExpressions = new SoqlExpressionCollection();
            query.OrderByOrder = new System.Collections.Specialized.StringCollection();

            bool again;

            do {
                again = false;
                SoqlExpression expr = ParseExpression();
                string order = "asc";
                if (tokenizer.IsKeyword("asc")) {
                    order = "asc";
                    tokenizer.GetNextToken();
                } else if (tokenizer.IsKeyword("desc")) {
                    order = "desc";
                    tokenizer.GetNextToken();
                }
                query.OrderByExpressions.Add(expr);
                query.OrderByOrder.Add(order);

                if (tokenizer.IsToken(SoqlTokenType.Comma)) {
                    tokenizer.GetNextToken();
                    again = true;
                }
            } while (again);
        }

        private void ParseFrom(SoqlQueryExpression query) {
            bool again;
            do {
                again = false;
                string tableName = tokenizer.EatKeyword();
                string alias;

                if (tokenizer.IsKeyword("as")) {
                    tokenizer.EatKeyword();
                    alias = tokenizer.EatKeyword();
                } else if (tokenizer.IsEOF() || tokenizer.IsKeyword("where") || tokenizer.IsKeyword("group") || tokenizer.IsKeyword("order") || tokenizer.IsKeyword("having") || tokenizer.TokenType == SoqlTokenType.Comma) {
                    alias = String.Empty;
                } else {
                    alias = tokenizer.EatKeyword();
                }

                query.From.Add(tableName);
                query.FromAliases.Add(alias);

                if (tokenizer.IsToken(SoqlTokenType.Comma)) {
                    tokenizer.GetNextToken();
                    again = true;
                }
            } while (again);
        }

        private SoqlQueryExpression ParseQuery() {
            SoqlQueryExpression query = new SoqlQueryExpression();
            tokenizer.ExpectKeyword("select");

            if (tokenizer.IsKeyword("top")) {
                tokenizer.EatKeyword();
                query.TopCount = Convert.ToInt32(tokenizer.TokenValue);
                tokenizer.GetNextToken();
            }

            if (tokenizer.IsKeyword("distinct")) {
                tokenizer.EatKeyword();
                query.Distinct = true;
            }

            ParseSelectExpressions(query);
            tokenizer.ExpectKeyword("from");
            ParseFrom(query);

            if (tokenizer.IsKeyword("where")) {
                tokenizer.EatKeyword();
                query.WhereClause = ParseBooleanExpression();
            }

            if (tokenizer.IsKeyword("group")) {
                ParseGroupByExpressions(query);
            }

            if (tokenizer.IsKeyword("having")) {
                tokenizer.EatKeyword();
                query.Having = ParseBooleanExpression();
            }

            if (tokenizer.IsKeyword("order")) {
                ParseOrderByExpressions(query);
            }

            return query;
        }

        public static SoqlBooleanExpression ParseWhereClause(string whereClause) {
            SoqlBooleanExpression e = ParseBooleanExpression(whereClause);
            if (e == null)
                throw new SoqlException("Expression is not of boolean type");
            return e;
        }

        public static SoqlExpression ParseExpression(string expr) {
            SoqlParser parser = new SoqlParser(expr);
            SoqlExpression e = parser.ParseExpression();
            if (!parser.tokenizer.IsEOF())
                throw new SoqlException("Unexpected token: " + parser.tokenizer.TokenValue, parser.tokenizer.TokenPosition);
            return e;
        }

        public static void ParseOrderBy(SoqlQueryExpression query, string expr) {
            SoqlParser parser = new SoqlParser(expr);
            parser.ParseOrderByExpressions(query);
            if (!parser.tokenizer.IsEOF())
                throw new SoqlException("Unexpected token: " + parser.tokenizer.TokenValue, parser.tokenizer.TokenPosition);
        }

        public static SoqlBooleanExpression ParseBooleanExpression(string expr) {
            SoqlParser parser = new SoqlParser(expr);
            SoqlBooleanExpression e = parser.ParseBooleanExpression();
            if (!parser.tokenizer.IsEOF())
                throw new SoqlException("Unexpected token: " + parser.tokenizer.TokenValue, parser.tokenizer.TokenPosition);
            return e;
        }

        public static SoqlQueryExpression ParseQuery(string expr) {
            SoqlParser parser = new SoqlParser(expr);
            SoqlQueryExpression q = parser.ParseQuery();
            if (!parser.tokenizer.IsEOF())
                throw new SoqlException("Unexpected token: " + parser.tokenizer.TokenValue, parser.tokenizer.TokenPosition);
            return q;
        }
    }
}
