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
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.QL.TypedWrappers;
using Sooda.QL;

using Sooda.Logging;
using System.Collections;

namespace Sooda.Sql
{
    /// <summary>
    /// Summary description for SoqlToSqlConverter.
    /// </summary>
    public class SoqlToSqlConverter : SoqlPrettyPrinter
    {
        private ISqlBuilder _builder;
        private bool _generatingOrderBy = false;

        public bool DisableBooleanExpansion = false;
        public bool GenerateColumnAliases = true;
        public bool UpperLike = false;
        public StringCollection ActualFromAliases = new StringCollection();
        public ArrayList FromJoins = new ArrayList();
        public StringCollection WhereJoins = new StringCollection();

        public SoqlToSqlConverter(TextWriter output, SchemaInfo schema, ISqlBuilder builder)
            : base(output)
        {
            Schema = schema;
            _builder = builder;
        }

        private IFieldContainer GenerateTableJoins(SoqlPathExpression expr, out string p, out string firstTableAlias)
        {
            // logger.Debug("GenerateTableJoins({0})", expr);
            IFieldContainer currentContainer;
            SoqlPathExpression e;

            // make the list bi-directional

            for (e = expr; e.Left != null; e = e.Left)
            {
                e.Left.Next = e;
            }

            SoqlPathExpression firstToken = e;
            SoqlPathExpression startingToken;

            // check if the first name on the list is an alias in current context
            if (TableAliases.ContainsKey(firstToken.PropertyName))
            {
                string ta = TableAliases[firstToken.PropertyName];

                currentContainer = FindContainerByName(ta);
                p = firstToken.PropertyName;
                startingToken = firstToken.Next;
            }
            else
            {
                if (Parent != null)
                {
                    if (Parent.TableAliases.ContainsKey(firstToken.PropertyName))
                    {
                        return Parent.GenerateTableJoins(expr, out p, out firstTableAlias);
                    };
                };

                // artificial first token

                // TODO - find default container for current field

                currentContainer = FindStartingContainerByFieldName(firstToken.PropertyName, out p);
                startingToken = firstToken;
            }

            string lastTableAlias = GetTableAliasForExpressionPrefix(p);
            firstTableAlias = lastTableAlias;

            SoqlPathExpression currentToken;
            bool nullable = false;

            for (currentToken = startingToken; currentToken != null; currentToken = currentToken.Next)
            {
                lastTableAlias = GetTableAliasForExpressionPrefix(p);
                // logger.Trace("Container: {0} Prop: {1} {2} {3}", currentContainer.Name, currentToken.PropertyName, currentContainer.GetType().FullName, currentToken.GetType().FullName);

                FieldInfo fi = currentContainer.FindFieldByName(currentToken.PropertyName);
                if (fi == null)
                {
                    throw new Exception(String.Format("{0} not found in {1}", currentToken.PropertyName, currentContainer.Name));
                }

                if (p.Length > 0)
                    p += ".";

                p += currentToken.PropertyName;

                if (fi.ReferencedClass == null)
                {
                    currentContainer = null;
                    continue;
                };

                if (fi.IsNullable)
                    nullable = true;

                if (fi.Table.OrdinalInClass > 0)
                {
                    string extPrefix = AddPrimaryKeyJoin(firstTableAlias, (ClassInfo)currentContainer, lastTableAlias, fi);
                    AddRefJoin(firstTableAlias, p, extPrefix, fi, nullable);
                }
                else
                {
                    AddRefJoin(firstTableAlias, p, lastTableAlias, fi, nullable);
                }
                currentContainer = fi.ReferencedClass;
            }

            return currentContainer;
        }

        private void ReplaceEmbeddedSoql(string s0)
        {
            int p;
            string s = s0;
            string parameterStart = "SOQL{{";
            string parameterEnd = "}}";

            p = s.IndexOf(parameterStart);
            while (p != -1)
            {
                int r = s.IndexOf(parameterEnd, p);
                if (r == -1)
                    break;

                string after = s.Substring(r + parameterEnd.Length);
                string before = s.Substring(0, p);
                Output.Write(before);
                string soqlString = s.Substring(p + parameterStart.Length, r - p - parameterStart.Length);
                SoqlExpression expr = SoqlParser.ParseExpression(soqlString);
                expr.Accept(this);

                s = after;
                p = s.IndexOf(parameterStart);
            }
            Output.Write(s);
        }

        public override void Visit(SoqlRawExpression v)
        {
            ReplaceEmbeddedSoql(v.Text);
        }

        public void OutputLiteral(object literalValue, SoqlLiteralValueModifiers modifier)
        {
            if (literalValue is String)
            {
                Output.Write("'");
                Output.Write(((string)literalValue).Replace("'", "''"));
                Output.Write("'");
                if (modifier != null)
                {
                    if (modifier.DataTypeOverride == FieldDataType.AnsiString)
                    {
                        Output.Write("A");
                    }
                }
            }
            else if (literalValue is DateTime)
            {
                Output.Write("'");
                Output.Write(((DateTime)literalValue).ToString("yyyyMMddHH:mm:ss"));
                Output.Write("'D");
            }
            else
            {
                Output.Write(literalValue);
            }
        }

        public override void Visit(SoqlLiteralExpression v)
        {
            OutputLiteral(v.LiteralValue, v.Modifiers);
        }

        public override void Visit(SoqlBooleanLiteralExpression v)
        {
            if (DisableBooleanExpansion)
            {
                if (v.Value)
                {
                    Output.Write("1");
                }
                else
                {
                    Output.Write("0");
                }
            }
            else
            {
                if (v.Value)
                {
                    Output.Write("(1=1)");
                }
                else
                {
                    Output.Write("(0=1)");
                }
            }
        }

        public override void Visit(Sooda.QL.TypedWrappers.SoqlBooleanWrapperExpression v)
        {
            if (DisableBooleanExpansion)
            {
                base.Visit(v);
                return;
            }

            if (v.InnerExpression is SoqlBooleanExpression)
            {
                base.Visit(v);
            }
            else
            {
                Output.Write("(");
                base.Visit(v);
                Output.Write(" <> 0)");
            }
        }


        public override void Visit(SoqlAsteriskExpression v)
        {
            string p;
            string firstTableAlias;
            IFieldContainer currentContainer;

            if (Parent != null && v.Left == null)
            {
                Output.Write("*");
                return;
            }

            if (v.Left != null)
            {
                currentContainer = GenerateTableJoins(v.Left, out p, out firstTableAlias);
            }
            else
            {
                p = ExpressionPrefixToTableAlias[Query.From[0]];
                currentContainer = FindContainerByName(Query.From[0]);
                firstTableAlias = p;
                firstTableAlias = GetTableAliasForExpressionPrefix(p);
            }

            bool first = true;
            foreach (FieldInfo fi in currentContainer.GetAllFields())
            {
                if (!first)
                {
                    if (IndentOutput)
                    {
                        Output.WriteLine(",");
                        WriteIndentString();
                        Output.Write("         ");
                    }
                    else
                    {
                        Output.Write(",");
                    }
                }
                if (fi.Table.OrdinalInClass > 0)
                {
                    string extPrefix = AddPrimaryKeyJoin(firstTableAlias, (ClassInfo)currentContainer, GetTableAliasForExpressionPrefix(p), fi);
                    Output.Write(extPrefix);
                }
                else
                {
                    Output.Write(GetTableAliasForExpressionPrefix(p));
                }
                Output.Write(".");
                Output.Write(fi.DBColumnName);
                if (GenerateColumnAliases)
                {
                    Output.Write(" as ");
                    Output.Write(_builder.QuoteFieldName(fi.Name));
                }
                first = false;
            }
        }

        public override void Visit(SoqlSoodaClassExpression v)
        {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null)
            {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            }
            else
            {
                if (Query.From.Count == 1)
                {
                    p = ExpressionPrefixToTableAlias[Query.From[0]];
                    currentClass = (ClassInfo)FindContainerByName(Query.From[0]);
                }
                else
                {
                    throw new Exception("Ambiguous SoodaClass!");
                }
            }
            if (currentClass.GetSubclassesForSchema(Schema).Count == 0)
            {
                Output.Write("'");
                Output.Write(currentClass.Name);
                Output.Write("'");
            }
            else
            {
                Output.Write("(case ");
                Output.Write(p);
                Output.Write(".");
                Output.Write(currentClass.SubclassSelectorField.DBColumnName);
                if (!currentClass.IsAbstractClass())
                {
                    Output.Write(" when ");
                    if (currentClass.SubclassSelectorField.DataType == FieldDataType.String 
                        || currentClass.SubclassSelectorField.DataType == FieldDataType.AnsiString)
                    {
                        Output.Write("'");
                        Output.Write(currentClass.SubclassSelectorValue);
                        Output.Write("'");
                    }
                    else
                        Output.Write(currentClass.SubclassSelectorValue);
                    Output.Write(" then ");
                    Output.Write("'");
                    Output.Write(currentClass.Name);
                    Output.Write("'");
                }

                foreach (ClassInfo subci in currentClass.GetSubclassesForSchema(Schema))
                {
                    if (!subci.IsAbstractClass())
                    {

                        Output.Write(" when ");
                        if (subci.SubclassSelectorField.DataType == FieldDataType.String
                            || subci.SubclassSelectorField.DataType == FieldDataType.AnsiString)
                        {
                            Output.Write("'");
                            Output.Write(subci.SubclassSelectorValue);
                            Output.Write("'");
                        }
                        else
                            Output.Write(subci.SubclassSelectorValue);
                        
                        Output.Write(" then ");
                        Output.Write("'");
                        Output.Write(subci.Name);
                        Output.Write("'");
                    }
                }
                Output.Write(" else null end)");
            }
        }

        public override void Visit(SoqlCountExpression v)
        {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null)
            {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            }
            else
            {
                currentClass = FindClassByCollectionName(v.CollectionName, out p);
            }

            CollectionOnetoManyInfo col1n = currentClass.FindCollectionOneToMany(v.CollectionName);
            if (col1n != null)
            {
                Output.Write("(select count(*) from ");
                Output.Write(col1n.Class.LocalTables[0].DBTableName);
                this.SetTableUsageHint(Output, col1n.Class.LocalTables[0]);
                Output.Write(" where ");
                Output.Write(col1n.ForeignColumn);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetFirstPrimaryKeyField().DBColumnName);
                Output.Write(")");
                return;
            }

            CollectionManyToManyInfo colnn = currentClass.FindCollectionManyToMany(v.CollectionName);
            if (colnn != null)
            {
                RelationInfo ri = colnn.GetRelationInfo();

                Output.Write("(select count(*) from ");
                Output.Write(ri.Table.DBTableName);
                this.SetTableUsageHint(Output, ri.Table);
                Output.Write(" where ");
                Output.Write(ri.Table.Fields[1 - colnn.MasterField].DBColumnName);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetFirstPrimaryKeyField().DBColumnName);
                Output.Write(")");
                return;
            }

            throw new Exception("Unknown collection " + v.CollectionName + " in " + currentClass.Name);
        }

        private void SetTableUsageHint(TextWriter Output, TableInfo tableInfo)
        {
            if (_builder.OuterJoinSyntax != SqlOuterJoinSyntax.Oracle && tableInfo.TableUsageType == TableUsageType.Dictionary)
            {
                Output.Write(" WITH (NOLOCK) ");
            }
        }

        private string GetTableUsageHint(TableInfo tableInfo)
        {
            if (_builder.OuterJoinSyntax != SqlOuterJoinSyntax.Oracle && tableInfo.TableUsageType == TableUsageType.Dictionary)
            {
                return " WITH (NOLOCK) ";
            }
            return "";
        }

        public override void Visit(SoqlContainsExpression v)
        {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null)
            {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            }
            else
            {
                currentClass = FindClassByCollectionName(v.CollectionName, out p);
            }

            CollectionOnetoManyInfo col1n = currentClass.FindCollectionOneToMany(v.CollectionName);
            if (col1n != null)
            {
                Output.Write("exists (select * from ");
                Output.Write(col1n.Class.LocalTables[0].DBTableName);
                this.SetTableUsageHint(Output, col1n.Class.LocalTables[0]);
                Output.Write(" where ");
                Output.Write(col1n.ForeignColumn);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetFirstPrimaryKeyField().DBColumnName);
                Output.Write(" and ");
                Output.Write(col1n.Class.GetFirstPrimaryKeyField().DBColumnName);
                Output.Write(" in (");
                if (IndentOutput)
                    Output.WriteLine();

                v.Expr.Accept(this);

                Output.Write("))");
                return;
            }

            CollectionManyToManyInfo colnn = currentClass.FindCollectionManyToMany(v.CollectionName);
            if (colnn != null)
            {
                RelationInfo ri = colnn.GetRelationInfo();

                Output.Write("exists (select * from ");
                Output.Write(ri.Table.DBTableName);
                this.SetTableUsageHint(Output, ri.Table);
                Output.Write(" where ");
                Output.Write(ri.Table.Fields[1 - colnn.MasterField].DBColumnName);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetFirstPrimaryKeyField().DBColumnName);
                Output.Write(" and ");
                Output.Write(ri.Table.Fields[colnn.MasterField].DBColumnName);
                Output.Write(" in (");
                if (IndentOutput)
                    Output.WriteLine();

                v.Expr.Accept(this);

                Output.Write("))");
                return;
            }

            throw new Exception("Unknown collection " + v.CollectionName + " in " + currentClass.Name);
        }

        private FieldInfo VisitAndGetFieldInfo(SoqlPathExpression v, bool doOutput)
        {
            if (v.Left != null && v.Left.Left == null)
            {
                string firstToken = v.Left.PropertyName;
                string secondToken = v.PropertyName;

                ClassInfo ci = Schema.FindClassByName(firstToken);
                if (ci != null)
                {
                    if (ci.Constants != null)
                    {
                        foreach (ConstantInfo constInfo in ci.Constants)
                        {
                            if (constInfo.Name == secondToken)
                            {
                                if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.Integer)
                                {
                                    OutputLiteral(Convert.ToInt32(constInfo.Key), null);
                                }
                                else if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.String)
                                {
                                    OutputLiteral(constInfo.Key, null);
                                }
                                else if (ci.GetFirstPrimaryKeyField().DataType == FieldDataType.AnsiString)
                                {
                                    OutputLiteral(constInfo.Key, SoqlLiteralValueModifiers.AnsiString);
                                }
                                else
                                    throw new NotSupportedException("Constant of type: " + ci.GetFirstPrimaryKeyField().DataType + " not supported in SOQL");
                                return null;
                            }
                        }
                    }
                }
            }

            IFieldContainer currentContainer;
            string firstTableAlias;
            string p;

            if (v.Left != null)
            {
                currentContainer = GenerateTableJoins(v.Left, out p, out firstTableAlias);
            }
            else
            {
                currentContainer = FindStartingContainerByFieldName(v.PropertyName, out p);
                firstTableAlias = p;
                firstTableAlias = GetTableAliasForExpressionPrefix(p);
            }

            FieldInfo fi = currentContainer.FindFieldByName(v.PropertyName);
            if (fi == null)
            {
                throw new Exception(String.Format("{0} not found in {1}", v.PropertyName, currentContainer.Name));
            }

            if (doOutput)
            {
                if (_generatingOrderBy)
                    Output.Write(_builder.GetSQLOrderBy(fi, true));
                if (fi.Table.OrdinalInClass > 0)
                {
                    string extPrefix = AddPrimaryKeyJoin(firstTableAlias, (ClassInfo)currentContainer, GetTableAliasForExpressionPrefix(p), fi);
                    Output.Write(extPrefix);
                }
                else
                {
                    Output.Write(GetTableAliasForExpressionPrefix(p));
                }
                Output.Write(".");
                Output.Write(fi.DBColumnName);
                if (_generatingOrderBy)
                   Output.Write( _builder.GetSQLOrderBy(fi, false));
            }
            return fi;
        }

        public override void Visit(SoqlPathExpression v)
        {
            VisitAndGetFieldInfo(v, true);
        }

        public override void Visit(SoqlQueryExpression v)
        {
            if (this.Query != null)
            {
                SoqlToSqlConverter subconverter = new SoqlToSqlConverter(Output, Schema, _builder);
                subconverter.Parent = this;
                subconverter.IndentLevel = this.IndentLevel;
                subconverter.IndentStep = this.IndentStep;
                subconverter.Visit(v);
            }
            else
            {
                Init(v);

                IndentLevel++;
                try
                {
                    WriteIndentString();
                    if (IndentOutput)
                    {
                        Output.Write("select   ");
                    }
                    else
                    {
                        Output.Write("select ");
                    }

                    if (v.TopCount != -1)
                    {
                        if (_builder.TopSupport == SqlTopSupportMode.SelectTop)
                        {
                            Output.Write("top ");
                            Output.Write(v.TopCount);
                            Output.Write(" ");
                        }
                    }

                    if (v.SelectExpressions.Count == 0)
                    {
                        // simplified query - emit the primary key here

                        Sooda.Schema.ClassInfo ci = Schema.FindClassByName(v.From[0]);
                        bool first = true;

                        foreach (FieldInfo pkfi in ci.GetPrimaryKeyFields())
                        {
                            if (!first)
                                Output.Write(", ");
                            Output.Write(ActualFromAliases[0]);
                            Output.Write(".");
                            Output.Write(pkfi.DBColumnName);
                            if (GenerateColumnAliases)
                            {
                                Output.Write(" as ");
                                Output.Write(_builder.QuoteFieldName(pkfi.Name));
                            }
                            first = false;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < v.SelectExpressions.Count; ++i)
                        {
                            if (i > 0)
                            {
                                if (IndentOutput)
                                {
                                    Output.WriteLine(",");
                                    WriteIndentString();
                                    Output.Write("         ");
                                }
                                else
                                {
                                    Output.Write(",");
                                }
                            }
                            if (v.SelectExpressions[i] is SoqlQueryExpression)
                                Output.Write("(");
                            v.SelectExpressions[i].Accept(this);
                            if (v.SelectExpressions[i] is SoqlQueryExpression)
                                Output.Write(")");
                            if (v.SelectAliases[i].Length > 0)
                            {
                                Output.Write(" as ");
                                Output.Write(_builder.QuoteFieldName(v.SelectAliases[i]));
                            }
                            else if (GenerateColumnAliases)
                            {
                                if (v.SelectExpressions[i] is ISoqlSelectAliasProvider)
                                {
                                    Output.Write(" as ");

                                    ((ISoqlSelectAliasProvider)v.SelectExpressions[i]).WriteDefaultSelectAlias(Output);
                                }
                            }
                        }
                    }

                    StringWriter sw = new StringWriter();
                    TextWriter oldOutput = Output;
                    Output = sw;

                    if (v.GroupByExpressions != null && v.GroupByExpressions.Count > 0)
                    {
                        if (IndentOutput)
                        {
                            Output.WriteLine();
                            WriteIndentString();
                        }
                        else
                        {
                            Output.Write(" ");
                        }
                        Output.Write("group by ");
                        for (int i = 0; i < v.GroupByExpressions.Count; ++i)
                        {
                            if (i > 0)
                                Output.Write(", ");
                            v.GroupByExpressions[i].Accept(this);
                        }
                    }
                    if (v.Having != null)
                    {
                        if (IndentOutput)
                        {
                            Output.WriteLine();
                            WriteIndentString();
                        }
                        else
                        {
                            Output.Write(" ");
                        }
                        Output.Write("having   ");
                        v.Having.Accept(this);
                    }
                    if (v.OrderByExpressions != null && v.OrderByExpressions.Count > 0)
                    {
                        if (IndentOutput)
                        {
                            Output.WriteLine();
                            WriteIndentString();
                        }
                        else
                        {
                            Output.Write(" ");
                        }
                        Output.Write("order by ");
                        _generatingOrderBy = true;
                        for (int i = 0; i < v.OrderByExpressions.Count; ++i)
                        {
                            if (i > 0)
                                Output.Write(", ");
                            v.OrderByExpressions[i].Accept(this);
                            Output.Write(" ");
                            Output.Write(v.OrderByOrder[i]);
                        }
                        _generatingOrderBy = false;
                    }

                    if (v.TopCount != -1)
                    {
                        if (_builder.TopSupport == SqlTopSupportMode.Limit)
                        {
                            Output.Write(" limit ");
                            Output.Write(v.TopCount);
                        }
                    }

                    StringWriter whereSW = new StringWriter();
                    Output = whereSW;

                    SoqlBooleanExpression limitedWhere = (SoqlBooleanExpression)v.WhereClause;
                    for (int i = 0; i < v.From.Count; ++i)
                    {
                        Sooda.Schema.ClassInfo ci = Schema.FindClassByName(v.From[i]);

                        if (ci == null)
                            continue;

                        SoqlBooleanExpression restriction = BuildClassRestriction(ActualFromAliases[i], ci);

                        if (restriction != null)
                        {
                            if (limitedWhere == null)
                            {
                                limitedWhere = restriction;
                            }
                            else
                            {
                                limitedWhere = new SoqlBooleanAndExpression(limitedWhere, restriction);
                            }
                        }
                    }
                    
                    if (limitedWhere != null || WhereJoins.Count > 0)
                    {
                        if (IndentOutput)
                        {
                            Output.WriteLine();
                            WriteIndentString();
                            Output.Write("where    ");
                        }
                        else
                        {
                            Output.Write(" where ");
                        }
                        limitedWhere.Accept(this);
                        bool first = true;
                        if (limitedWhere != null)
                            first = false;

                        foreach (string s in WhereJoins)
                        {
                            if (!first)
                                Output.Write(" and ");
                            Output.Write(s);
                        }
                    }

                    Output = oldOutput;

                    if (IndentOutput)
                    {
                        // output FROM here

                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("from     ");
                    }
                    else
                    {
                        Output.Write(" from ");
                    }
                    for (int i = 0; i < v.From.Count; ++i)
                    {
                        if (i > 0)
                        {
                            if (IndentOutput)
                            {
                                Output.WriteLine(",");
                                WriteIndentString();
                                Output.Write("         ");
                            }
                            else
                            {
                                Output.Write(",");
                            }
                        }

                        TableInfo tbl = FindContainerByName(v.From[i]).GetAllFields()[0].Table;
                        Output.Write(tbl.DBTableName);
                        if (ActualFromAliases[i].Length > 0)
                        {
                            Output.Write(" ");
                            Output.Write(ActualFromAliases[i]);
                        }
                        this.SetTableUsageHint(Output, tbl);
                        foreach (string s in (StringCollection)FromJoins[i])
                        {
                            if (IndentOutput)
                            {
                                Output.WriteLine();
                                WriteIndentString();
                                Output.Write("         ");
                            }
                            else
                            {
                                Output.Write(" ");
                            }
                            Output.Write(s);
                        }
                    }

                    Output.Write(whereSW.ToString());
                    Output.Write(sw.ToString());
                }
                finally
                {
                    IndentLevel--;
                }
            }
        }

        public SoqlToSqlConverter Parent;
        public SchemaInfo Schema;

        public StringDictionary ExpressionPrefixToTableAlias = new StringDictionary();
        public StringDictionary TableAliases = new StringDictionary();
        public int CurrentTablePrefix = 0;

        private SoqlQueryExpression Query;

        public void Init(SoqlQueryExpression query)
        {
            this.Query = query;

            StringCollection killPrefixes = new StringCollection();

            for (int i = 0; i < query.From.Count; ++i)
            {
                FromJoins.Add(new StringCollection());

                string table = query.From[i];
                string alias = query.FromAliases[i];

                if (alias.Length == 0)
                {
                    alias = GetNextTablePrefix();
                }
                ActualFromAliases.Add(alias);

                if (!ExpressionPrefixToTableAlias.ContainsKey(alias))
                {
                    ExpressionPrefixToTableAlias.Add(alias, alias);
                }
                else
                {
                    killPrefixes.Add(alias);
                };

                if (!ExpressionPrefixToTableAlias.ContainsKey(table))
                {
                    ExpressionPrefixToTableAlias.Add(table, alias);
                }
                else
                {
                    killPrefixes.Add(table);
                };

                if (!TableAliases.ContainsKey(alias))
                {
                    TableAliases.Add(alias, table);
                }
                if (!TableAliases.ContainsKey(table))
                {
                    TableAliases.Add(table, table);
                }
            }

            foreach (string s in killPrefixes)
            {
                TableAliases.Remove(s);
                ExpressionPrefixToTableAlias.Remove(s);
            }
        }

        public IFieldContainer FindStartingContainerByFieldName(string fieldName, out string alias)
        {
            if (Query.From.Count == 1)
            {
                alias = ExpressionPrefixToTableAlias[Query.From[0]];
                return FindContainerByName(Query.From[0]);
            }

            IFieldContainer foundContainer = null;
            alias = null;

            foreach (string containerName in Query.From)
            {
                IFieldContainer container = FindContainerByName(containerName);

                if (container.ContainsField(fieldName))
                {
                    if (foundContainer != null)
                    {
                        throw new Exception(String.Format("Cannot determine table from field name '{0}'. Can be either {1}.{0} or {2}.{0}. Use prefixed names.",
                            fieldName,
                            foundContainer.Name, containerName));
                    }
                    alias = ExpressionPrefixToTableAlias[containerName];
                    foundContainer = container;
                }
            }
            if (foundContainer != null)
            {
                return foundContainer;
            }

            throw new Exception("Cannot determine table from field name '" + fieldName + "'. Use prefixed names.");
        }

        public ClassInfo FindClassByCollectionName(string collectionName, out string alias)
        {
            if (Query.From.Count == 1)
            {
                alias = ExpressionPrefixToTableAlias[Query.From[0]];
                return (ClassInfo)FindContainerByName(Query.From[0]);
            }

            IFieldContainer foundContainer = null;
            alias = null;

            foreach (string containerName in Query.From)
            {
                IFieldContainer container = FindContainerByName(containerName);

                if (container.ContainsCollection(collectionName) != 0)
                {
                    if (foundContainer != null)
                    {
                        throw new Exception(String.Format("Cannot determine table from collection name '{0}'. Can be either {1}.{0} or {2}.{0}. Use prefixed names.",
                            collectionName,
                            foundContainer.Name, containerName));
                    }
                    alias = ExpressionPrefixToTableAlias[containerName];
                    foundContainer = container;
                }
            }
            if (foundContainer != null)
            {
                return foundContainer as ClassInfo;
            }

            throw new Exception("Cannot determine table from field name '" + collectionName + "'. Use prefixed names.");
        }

        public IFieldContainer FindContainerByName(string name)
        {
            ClassInfo ci = Schema.FindClassByName(name);
            if (ci != null)
                return ci;
            RelationInfo ri = Schema.FindRelationByName(name);

            if (ri != null)
                return ri;

            throw new Exception(String.Format("'{0}' is neither a class nor a relation", name));
        }

        string AddJoin(TableInfo rightTable, string leftPrefix, string rightPrefix, FieldInfo leftField, FieldInfo rightField, bool innerJoin)
        {
            if (SoodaConfig.GetString("sooda.innerjoins", "false") != "true")
                innerJoin = false;
            if (_builder.OuterJoinSyntax == SqlOuterJoinSyntax.Oracle)
            {
                WhereJoins.Add(String.Format("({0}.{2} = {1}.{3}{4})",
                    leftPrefix, rightPrefix, leftField.DBColumnName, rightField.DBColumnName,
                    innerJoin ? "" : " (+)"));
                return String.Format(", {0} {1}", rightTable.DBTableName, rightPrefix);
            }
            else
            {
                return String.Format("{6} join {0} {2} {5} on ({1}.{3} = {2}.{4})",
                    rightTable.DBTableName, leftPrefix, rightPrefix, leftField.DBColumnName, rightField.DBColumnName,
                    this.GetTableUsageHint(rightTable), innerJoin ? "inner" : "left outer");
            }
        }

        public string AddPrimaryKeyJoin(string fromTableAlias, ClassInfo classInfo, string rootPrefix, FieldInfo fieldToReach)
        {
            // logger.Debug("AddPrimaryKeyJoin({0},{1},{2},{3})", fromTableAlias, classInfo.Name, rootPrefix, fieldToReach);
            if (fieldToReach.Table.DBTableName == classInfo.UnifiedTables[0].DBTableName)
                return rootPrefix;

            string newPrefix = rootPrefix + "_pkjoin_" + fieldToReach.Table.DBTableName;
            newPrefix = _builder.GetTruncatedIdentifier(newPrefix);
            if (TableAliases.ContainsKey(newPrefix))
                return newPrefix;

            TableAliases[newPrefix] = "EXT";

            FieldInfo fi = classInfo.UnifiedTables[0].Fields[0];
            string s = AddJoin(fieldToReach.Table, rootPrefix, newPrefix,
                fi, classInfo.UnifiedTables[fieldToReach.Table.OrdinalInClass].Fields[0], !fi.IsNullable);

            int foundPos = ActualFromAliases.IndexOf(fromTableAlias);

            if (foundPos == -1)
                throw new NotSupportedException();

            StringCollection coll = (StringCollection)FromJoins[foundPos];
            coll.Add(s);

            return newPrefix;
        }

        public void AddRefJoin(string fromTableAlias, string newPrefix, string lastTableAlias, FieldInfo field, bool nullable)
        {
            // logger.Debug("AddRefJoin({0},{1},{2},{3})", fromTableAlias, newPrefix, lastTableAlias, field);
            if (ExpressionPrefixToTableAlias.ContainsKey(newPrefix))
                return;

            string tbl = GetNextTablePrefix();
            ExpressionPrefixToTableAlias.Add(newPrefix, tbl);

            int foundPos = ActualFromAliases.IndexOf(fromTableAlias);

            if (foundPos == -1)
                throw new NotSupportedException();

            string s = AddJoin(field.ReferencedClass.UnifiedTables[0], lastTableAlias, tbl,
                field, field.ReferencedClass.GetFirstPrimaryKeyField(), !nullable);

            StringCollection coll = (StringCollection)FromJoins[foundPos];
            coll.Add(s);
        }

        public string GetTableAliasForExpressionPrefix(string prefix)
        {
            string s = ExpressionPrefixToTableAlias[prefix];
            if (s == null)
            {
                if (Parent != null)
                    return Parent.GetTableAliasForExpressionPrefix(prefix);
                throw new Exception("Table alias unknown for exception prefix: " + prefix);
            }
            return s;
        }

        private string GetNextTablePrefix()
        {
            if (Parent != null)
                return Parent.GetNextTablePrefix();
            else
                return String.Format("t{0}", CurrentTablePrefix++);
        }

        public SoqlBooleanExpression BuildClassRestriction(string startingAlias, Sooda.Schema.ClassInfo classInfo)
        {
            if (classInfo.GetSubclassesForSchema(Schema).Count == 0 && classInfo.InheritsFromClass == null)
                return null;

            SoqlExpressionCollection literals = new SoqlExpressionCollection();

            foreach (ClassInfo subclass in classInfo.GetSubclassesForSchema(Schema))
            {
                if (subclass.SubclassSelectorValue != null)
                {
                    literals.Add(new SoqlLiteralExpression(subclass.SubclassSelectorValue));
                }
            }
            if (classInfo.SubclassSelectorValue != null)
            {
                literals.Add(new SoqlLiteralExpression(classInfo.SubclassSelectorValue));
            }

            SoqlBooleanExpression restriction =
                new SoqlBooleanInExpression(
                new SoqlPathExpression(
                new SoqlPathExpression(startingAlias),
                classInfo.SubclassSelectorField.Name),
                literals
                );

            return restriction;
        }

        public void ConvertQuery(SoqlQueryExpression expr)
        {
            expr.Accept(this);
        }

        private void OutputModifiedLiteral(SoqlExpression expr, FieldInfo fieldInfo)
        {
            if (expr is SoqlLiteralExpression)
            {
                SoqlLiteralExpression e = (SoqlLiteralExpression)expr;

                Output.Write("{L:");
                Output.Write(fieldInfo.DataType.ToString());
                Output.Write(":");

                string serializedValue = fieldInfo.GetFieldHandler().RawSerialize(e.LiteralValue).Replace("\\", "\\\\").Replace("}", "\\}");

                Output.Write(serializedValue);
                Output.Write("}");
            }
            else if (expr is SoqlParameterLiteralExpression)
            {
                SoqlParameterLiteralExpression e = (SoqlParameterLiteralExpression)expr;

                Output.Write("{");
                Output.Write(e.ParameterPosition);
                Output.Write(":");
                Output.Write(fieldInfo.DataType.ToString());
                Output.Write("}");
            }
            else if (expr is SoqlBooleanLiteralExpression)
            {
                SoqlBooleanLiteralExpression e = (SoqlBooleanLiteralExpression)expr;

                Output.Write("{L:");
                Output.Write(fieldInfo.DataType.ToString());
                Output.Write(":");

                string serializedValue = fieldInfo.GetFieldHandler().RawSerialize(e.Value).Replace("\\", "\\\\").Replace("}", "\\}");

                Output.Write(serializedValue);
                Output.Write("}");
            }
            else
            {
                throw new ArgumentException("Not supported literal expression type: " + expr.GetType().FullName);
            }
        }

        public override void Visit(SoqlBooleanRelationalExpression v)
        {
            bool upper = UpperLike && v.op == SoqlRelationalOperator.Like;

            //
            // this is to support type coercions. Whenever we have
            //
            // path.expression OPERATOR literal 
            // or
            // path.expression OPERATOR parametrized literal
            // 
            // we may want to change the Modifiers of the literal to reflect the actual type of
            // the property being compared with.
            //
            // Unfortunately MSSQL crawls without this when comparing varchar() columns 
            // against nvarchar() parameter values.
            //

            bool oldBooleanExpansion;
            SoqlExpression unwrappedPar1 = SoqlExpression.Unwrap(v.par1);
            SoqlExpression unwrappedPar2 = SoqlExpression.Unwrap(v.par2);

            bool anyLiteral = unwrappedPar1 is ILiteralModifiers || unwrappedPar2 is ILiteralModifiers;
            if (anyLiteral)
            {
                bool anyPath = unwrappedPar1 is SoqlPathExpression || unwrappedPar2 is SoqlPathExpression;

                if (anyPath)
                {
                    FieldInfo pathFieldInfo;

                    if (unwrappedPar1 is SoqlPathExpression)
                    {
                        pathFieldInfo = VisitAndGetFieldInfo((SoqlPathExpression)unwrappedPar1, false);
                    }
                    else
                    {
                        pathFieldInfo = VisitAndGetFieldInfo((SoqlPathExpression)unwrappedPar2, false);
                    }

                    if (pathFieldInfo != null)
                    {
                        oldBooleanExpansion = DisableBooleanExpansion;
                        DisableBooleanExpansion = true;
                        Output.Write("(");
                        if (upper)
                            Output.Write("upper(");
                        if (unwrappedPar1 is ILiteralModifiers)
                        {
                            OutputModifiedLiteral(unwrappedPar1, pathFieldInfo);
                        }
                        else
                        {
                            v.par1.Accept(this);
                        }
                        if (upper)
                            Output.Write(")");
                        OutputRelationalOperator(v.op);
                        if (upper)
                            Output.Write("upper(");
                        if (unwrappedPar2 is ILiteralModifiers)
                        {
                            OutputModifiedLiteral(unwrappedPar2, pathFieldInfo);
                        }
                        else
                        {
                            v.par2.Accept(this);
                        }
                        if (upper)
                            Output.Write(")");
                        Output.Write(")");
                        DisableBooleanExpansion = oldBooleanExpansion;
                        return;
                    }
                }
            }

            // by default booleans expand to "b <> 0"
            // in relational expressions they expand to "b"
            // this is a dirty hack - will be fixed when we support
            // proper boolean-to-any-type mapping

            oldBooleanExpansion = DisableBooleanExpansion;
            DisableBooleanExpansion = true;
            if (upper)
            {
                Output.Write("(upper(");
                v.par1.Accept(this);
                Output.Write(")");
                OutputRelationalOperator(v.op);
                Output.Write("upper(");
                v.par2.Accept(this);
                Output.Write("))");
            }
            else
                base.Visit(v);
            DisableBooleanExpansion = oldBooleanExpansion;
        }
    }
}
