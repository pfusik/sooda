//
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its
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
using System.IO;
using System.Collections.Specialized;

using Sooda.Schema;
using Sooda.QL;


namespace Sooda.Sql {
    /// <summary>
    /// Summary description for SoqlToSqlConverter.
    /// </summary>
    public class SoqlToSqlConverter : SoqlPrettyPrinter {
        private ISqlBuilder _builder;

        public SoqlToSqlConverter(TextWriter output, SchemaInfo schema, ISqlBuilder builder) : base(output) {
            Schema = schema;
            _builder = builder;
        }

        private IFieldContainer GenerateTableJoins(SoqlPathExpression expr, out string p, out string firstTableAlias) {
            IFieldContainer currentContainer;
            SoqlPathExpression e;

            // make the list bi-directional

            for (e = expr; e.Left != null; e = e.Left) {
                e.Left.Next = e;
            }

            SoqlPathExpression firstToken = e;
            SoqlPathExpression startingToken;

            // check if the first name on the list is an alias in current context
            if (TableAliases.ContainsKey(firstToken.PropertyName)) {
                string ta = TableAliases[firstToken.PropertyName];

                currentContainer = FindContainerByName(ta);
                p = firstToken.PropertyName;
                startingToken = firstToken.Next;
            } else {
                if (Parent != null) {
                    if (Parent.TableAliases.ContainsKey(firstToken.PropertyName)) {
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

            for (currentToken = startingToken; currentToken != null; currentToken = currentToken.Next) {
                lastTableAlias = GetTableAliasForExpressionPrefix(p);
                // Console.WriteLine("Container: {0} Prop: {1}", currentContainer.Name, currentToken.PropertyName);

                FieldInfo fi = currentContainer.FindFieldByName(currentToken.PropertyName);
                if (fi == null) {
                    throw new Exception(String.Format("{0} not found in {1}", currentToken.PropertyName, currentContainer.Name));
                }

                if (p.Length > 0)
                    p += ".";

                p += currentToken.PropertyName;

                if (fi.ReferencedClass == null) {
                    currentContainer = null;
                    continue;
                };

                AddRefJoin(firstTableAlias, p, lastTableAlias, fi);
                currentContainer = fi.ReferencedClass;
            }

            return currentContainer;
        }

        public override void Visit(SoqlRawExpression v) {
            Output.Write(v.Text);
        }

        public override void Visit(SoqlBooleanLiteralExpression v) {
            if (v.Value) {
                Output.Write("(1=1)");
            } else {
                Output.Write("(1=0)");
            }
        }

        public override void Visit(SoqlAsteriskExpression v) {
            string p;
            string firstTableAlias;
            IFieldContainer currentContainer;

            if (Parent != null && v.Left == null) {
                Output.Write("*");
                return ;
            }

            if (v.Left != null) {
                currentContainer = GenerateTableJoins(v.Left, out p, out firstTableAlias);
            } else {
                p = ExpressionPrefixToTableAlias[Query.From[0]];
                currentContainer = FindContainerByName(Query.From[0]);
                firstTableAlias = p;
                firstTableAlias = GetTableAliasForExpressionPrefix(p);
            }

            bool first = true;
            foreach (FieldInfo fi in currentContainer.GetAllFields()) {
                if (!first) {
                    Output.WriteLine(",");
                    WriteIndentString();
                    Output.Write("         ");
                }
                if (fi.Table.OrdinalInClass > 0) {
                    string extPrefix = AddPrimaryKeyJoin(firstTableAlias, (ClassInfo)currentContainer, GetTableAliasForExpressionPrefix(p), fi);
                    Output.Write(extPrefix);
                } else {
                    Output.Write(GetTableAliasForExpressionPrefix(p));
                }
                Output.Write(".");
                Output.Write(fi.DBColumnName);
                Output.Write(" as ");
                Output.Write(_builder.QuoteFieldName(fi.Name));
                first = false;
            }
        }

        public override void Visit(SoqlSoodaClassExpression v) {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null) {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            } else {
                if (Query.From.Count == 1) {
                    p = ExpressionPrefixToTableAlias[Query.From[0]];
                    currentClass = (ClassInfo)FindContainerByName(Query.From[0]);
                }
                else {
                    throw new Exception("Ambiguous SoodaClass!");
                }
            }
            if (currentClass.Subclasses.Count == 0)
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
                foreach (ClassInfo subci in currentClass.Subclasses)
                {
                    Output.Write(" when ");
                    Output.Write(subci.SubclassSelectorValue);
                    Output.Write(" then ");
                    Output.Write("'");
                    Output.Write(subci.Name);
                    Output.Write("'");
                }
                Output.Write(" else null end)");
            }
        }
        
        public override void Visit(SoqlCountExpression v) {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null) {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            } else {
                currentClass = FindClassByCollectionName(v.CollectionName, out p);
            }

            CollectionOnetoManyInfo col1n = currentClass.FindCollectionOneToMany(v.CollectionName);
            if (col1n != null) {
                Output.Write("(select count(*) from ");
                Output.Write(col1n.Class.LocalTables[0].DBTableName);
                Output.Write(" where ");
                Output.Write(col1n.ForeignColumn);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetPrimaryKeyField().DBColumnName);
                Output.Write(")");
                return ;
            }

            CollectionManyToManyInfo colnn = currentClass.FindCollectionManyToMany(v.CollectionName);
            if (colnn != null) {
                RelationInfo ri = colnn.GetRelationInfo();

                Output.Write("(select count(*) from ");
                Output.Write(ri.Table.DBTableName);
                Output.Write(" where ");
                Output.Write(ri.Table.Fields[1 - colnn.MasterField].DBColumnName);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetPrimaryKeyField().DBColumnName);
                Output.Write(")");
                return ;
            }

            throw new Exception("Unknown collection " + v.CollectionName + " in " + currentClass.Name);
        }

        public override void Visit(SoqlContainsExpression v) {
            ClassInfo currentClass;
            string p = "";
            string firstTableAlias = null;

            if (v.Path != null) {
                IFieldContainer container = GenerateTableJoins(v.Path, out p, out firstTableAlias);
                currentClass = container as ClassInfo;
            } else {
                currentClass = FindClassByCollectionName(v.CollectionName, out p);
            }

            CollectionOnetoManyInfo col1n = currentClass.FindCollectionOneToMany(v.CollectionName);
            if (col1n != null) {
                Output.Write("exists (select * from ");
                Output.Write(col1n.Class.LocalTables[0].DBTableName);
                Output.Write(" where ");
                Output.Write(col1n.ForeignColumn);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetPrimaryKeyField().DBColumnName);
                Output.Write(" and ");
                Output.Write(col1n.Class.GetPrimaryKeyField().DBColumnName);
                Output.WriteLine(" in (");

                v.Expr.Accept(this);

                Output.Write("))");
                return ;
            }

            CollectionManyToManyInfo colnn = currentClass.FindCollectionManyToMany(v.CollectionName);
            if (colnn != null) {
                RelationInfo ri = colnn.GetRelationInfo();

                Output.Write("exists (select * from ");
                Output.Write(ri.Table.DBTableName);
                Output.Write(" where ");
                Output.Write(ri.Table.Fields[1 - colnn.MasterField].DBColumnName);
                Output.Write("=");

                Output.Write(GetTableAliasForExpressionPrefix(p));
                Output.Write(".");
                Output.Write(currentClass.GetPrimaryKeyField().DBColumnName);
                Output.Write(" and ");
                Output.Write(ri.Table.Fields[colnn.MasterField].DBColumnName);
                Output.WriteLine(" in (");

                v.Expr.Accept(this);

                Output.Write("))");
                return ;
            }

            throw new Exception("Unknown collection " + v.CollectionName + " in " + currentClass.Name);
        }

        public override void Visit(SoqlPathExpression v) {
            IFieldContainer currentContainer;
            string firstTableAlias;
            string p;

            if (v.Left != null) {
                currentContainer = GenerateTableJoins(v.Left, out p, out firstTableAlias);
            } else {
                currentContainer = FindStartingContainerByFieldName(v.PropertyName, out p);
                firstTableAlias = p;
                firstTableAlias = GetTableAliasForExpressionPrefix(p);
            }

            FieldInfo fi = currentContainer.FindFieldByName(v.PropertyName);
            if (fi == null) {
                throw new Exception(String.Format("{0} not found in {1}", v.PropertyName, currentContainer.Name));
            }
            if (fi.Table.OrdinalInClass > 0) {
                string extPrefix = AddPrimaryKeyJoin(firstTableAlias, (ClassInfo)currentContainer, GetTableAliasForExpressionPrefix(p), fi);
                Output.Write(extPrefix);
            } else {
                Output.Write(GetTableAliasForExpressionPrefix(p));
            }
            Output.Write(".");
                Output.Write(fi.DBColumnName);
        }

        public override void Visit(SoqlQueryExpression v) {
            if (this.Query != null) {
                SoqlToSqlConverter subconverter = new SoqlToSqlConverter(Output, Schema, _builder);
                subconverter.Parent = this;
                subconverter.IndentLevel = this.IndentLevel;
                subconverter.IndentStep = this.IndentStep;
                subconverter.Visit(v);
            } else {
                Init(v);

                IndentLevel++;
                try {
                    WriteIndentString();
                    Output.Write("select   ");

                    if (v.TopCount != -1) {
                        if (_builder.TopSupport == SqlTopSupportMode.SelectTop) {
                            Output.Write("top ");
                            Output.Write(v.TopCount);
                            Output.Write(" ");
                        }
                    }

					if (v.SelectExpressions.Count == 0) {
						// simplified query - emit the primary key here

						Sooda.Schema.ClassInfo ci = Schema.FindClassByName(v.From[0]);
						Output.Write(v.FromAliases[0]);
						Output.Write(".");
						Output.Write(ci.GetPrimaryKeyField().DBColumnName);
						Output.Write(" as ");
						Output.Write(_builder.QuoteFieldName(ci.GetPrimaryKeyField().Name));
					} else {
                        for (int i = 0; i < v.SelectExpressions.Count; ++i) {
                            if (i > 0) {
                                Output.WriteLine(",");
                                WriteIndentString();
                                Output.Write("         ");
                            }
                            v.SelectExpressions[i].Accept(this);
							if (v.SelectAliases[i].Length > 0) {
								Output.Write(" as ");
								Output.Write(_builder.QuoteFieldName(v.SelectAliases[i]));
							} else {
                                if (v.SelectExpressions[i] is ISoqlSelectAliasProvider) {
                                    Output.Write(" as ");

                                    ((ISoqlSelectAliasProvider)v.SelectExpressions[i]).WriteDefaultSelectAlias(Output);
                                }
                            }
                        }
                    }

                    StringWriter sw = new StringWriter();
                    TextWriter oldOutput = Output;
                    Output = sw;

                    SoqlBooleanExpression limitedWhere = (SoqlBooleanExpression)v.WhereClause;

                    for (int i = 0; i < v.From.Count; ++i) {
                        Sooda.Schema.ClassInfo ci = Schema.FindClassByName(v.From[i]);

                        if (ci == null)
                            continue;

                        SoqlBooleanExpression restriction = BuildClassRestriction(v.FromAliases[i], ci);

                        if (restriction != null) {
                            if (limitedWhere == null) {
                                limitedWhere = restriction;
                            } else {
                                limitedWhere = new SoqlBooleanAndExpression(limitedWhere, restriction);
                            }
                        }
                    }

                    if (limitedWhere != null) {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("where    ");
                        limitedWhere.Accept(this);
                    }
                    if (v.GroupByExpressions != null) {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("group by ");
                        for (int i = 0; i < v.GroupByExpressions.Count; ++i) {
                            if (i > 0)
                                Output.Write(", ");
                            v.GroupByExpressions[i].Accept(this);
                        }
                    }
                    if (v.Having != null) {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("having   ");
                        v.Having.Accept(this);
                    }
                    if (v.OrderByExpressions != null) {
                        Output.WriteLine();
                        WriteIndentString();
                        Output.Write("order by ");
                        for (int i = 0; i < v.OrderByExpressions.Count; ++i) {
                            if (i > 0)
                                Output.Write(", ");
                            v.OrderByExpressions[i].Accept(this);
                            Output.Write(" ");
                            Output.Write(v.OrderByOrder[i]);
                        }
                    }

                    if (v.TopCount != -1) {
                        if (_builder.TopSupport == SqlTopSupportMode.Limit) {
                            Output.Write(" limit ");
                            Output.Write(v.TopCount);
                        }
                    }
                    Output = oldOutput;

                    // output FROM here

                    Output.WriteLine();
                    WriteIndentString();
                    Output.Write("from     ");
                    for (int i = 0; i < v.From.Count; ++i) {
                        if (i > 0) {
                            Output.WriteLine(",");
                            WriteIndentString();
                            Output.Write("         ");
                        }

                        Output.Write(FindContainerByName(v.From[i]).GetAllFields()[0].Table.DBTableName);
                        if (v.FromAliases[i].Length > 0) {
                            Output.Write(" ");
                            Output.Write(v.FromAliases[i]);
                        }
                        foreach (string s in (StringCollection)v.FromJoins[i]) {
                            Output.WriteLine();
                            WriteIndentString();
                            Output.Write("         ");
                            Output.Write(s);
                        }
                    }

                    Output.Write(sw.ToString());
                } finally {
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

        public void Init(SoqlQueryExpression query) {
            this.Query = query;

            StringCollection killPrefixes = new StringCollection();

            for (int i = 0; i < query.From.Count; ++i) {
                query.FromJoins.Add(new StringCollection());

                string table = query.From[i];
                string alias = query.FromAliases[i];

                if (alias.Length == 0) {
                    alias = GetNextTablePrefix();
                    query.FromAliases[i] = alias;
                }

                if (!ExpressionPrefixToTableAlias.ContainsKey(alias)) {
                    ExpressionPrefixToTableAlias.Add(alias, alias);
                } else {
                    killPrefixes.Add(alias);
                };

                if (!ExpressionPrefixToTableAlias.ContainsKey(table)) {
                    ExpressionPrefixToTableAlias.Add(table, alias);
                } else {
                    killPrefixes.Add(table);
                };

                if (!TableAliases.ContainsKey(alias)) {
                    TableAliases.Add(alias, table);
                }
                if (!TableAliases.ContainsKey(table)) {
                    TableAliases.Add(table, table);
                }
            }

            foreach (string s in killPrefixes) {
                TableAliases.Remove(s);
                ExpressionPrefixToTableAlias.Remove(s);
            }
        }

        public IFieldContainer FindStartingContainerByFieldName(string fieldName, out string alias) {
            if (Query.From.Count == 1) {
                alias = ExpressionPrefixToTableAlias[Query.From[0]];
                return FindContainerByName(Query.From[0]);
            }

            IFieldContainer foundContainer = null;
            alias = null;

            foreach (string containerName in Query.From) {
                IFieldContainer container = FindContainerByName(containerName);

                if (container.ContainsField(fieldName)) {
                    if (foundContainer != null) {
                        throw new Exception(String.Format("Cannot determine table from field name '{0}'. Can be either {1}.{0} or {2}.{0}. Use prefixed names.",
                                                          fieldName,
                                                          foundContainer.Name, containerName));
                    }
                    alias = ExpressionPrefixToTableAlias[containerName];
                    foundContainer = container;
                }
            }
            if (foundContainer != null) {
                return foundContainer;
            }

            throw new Exception("Cannot determine table from field name '" + fieldName + "'. Use prefixed names.");
        }

        public ClassInfo FindClassByCollectionName(string collectionName, out string alias) {
            if (Query.From.Count == 1) {
                alias = ExpressionPrefixToTableAlias[Query.From[0]];
                return (ClassInfo)FindContainerByName(Query.From[0]);
            }

            IFieldContainer foundContainer = null;
            alias = null;

            foreach (string containerName in Query.From) {
                IFieldContainer container = FindContainerByName(containerName);

                if (container.ContainsCollection(collectionName) != 0) {
                    if (foundContainer != null) {
                        throw new Exception(String.Format("Cannot determine table from collection name '{0}'. Can be either {1}.{0} or {2}.{0}. Use prefixed names.",
                                                          collectionName,
                                                          foundContainer.Name, containerName));
                    }
                    alias = ExpressionPrefixToTableAlias[containerName];
                    foundContainer = container;
                }
            }
            if (foundContainer != null) {
                return foundContainer as ClassInfo;
            }

            throw new Exception("Cannot determine table from field name '" + collectionName + "'. Use prefixed names.");
        }

        public IFieldContainer FindContainerByName(string name) {
            ClassInfo ci = Schema.FindClassByName(name);
            if (ci != null)
                return ci;
            RelationInfo ri = Schema.FindRelationByName(name);

            if (ri != null)
                return ri;

            throw new Exception(String.Format("'{0}' is neither a class nor a relation", name));
        }

        public string AddPrimaryKeyJoin(string fromTableAlias, ClassInfo classInfo, string rootPrefix, FieldInfo fieldToReach) {
            if (fieldToReach.Table.DBTableName == classInfo.UnifiedTables[0].DBTableName)
                return rootPrefix;

            string newPrefix = rootPrefix + "_pkjoin_" + fieldToReach.Table.DBTableName;
            if (TableAliases.ContainsKey(newPrefix))
                return newPrefix;

            TableAliases[newPrefix] = "EXT";

            string s = String.Format("left outer join {0} {2} on ({1}.{3} = {2}.{4})",
                                     fieldToReach.Table.DBTableName,
                                     rootPrefix,
                                     newPrefix,
                                     classInfo.UnifiedTables[0].Fields[0].DBColumnName,
                                     classInfo.UnifiedTables[fieldToReach.Table.OrdinalInClass].Fields[0].DBColumnName);

            int foundPos = -1;
            for (int i = 0; i < Query.FromAliases.Count; ++i) {
                if (Query.FromAliases[i] == fromTableAlias) {
                    foundPos = i;
                    break;
                }
            }

            if (foundPos == -1)
                throw new NotSupportedException();

            StringCollection coll = (StringCollection)Query.FromJoins[foundPos];
            coll.Add(s);

            return newPrefix;
        }

        public void AddRefJoin(string fromTableAlias, string newPrefix, string lastTableAlias, FieldInfo field) {
            if (ExpressionPrefixToTableAlias.ContainsKey(newPrefix))
                return ;

            string tbl = GetNextTablePrefix();
            ExpressionPrefixToTableAlias.Add(newPrefix, tbl);

            int foundPos = -1;
            for (int i = 0; i < Query.FromAliases.Count; ++i) {
                if (Query.FromAliases[i] == fromTableAlias) {
                    foundPos = i;
                    break;
                }
            }

            if (foundPos == -1)
                throw new NotSupportedException();

            string s = String.Format("left outer join {0} {1} on ({2}.{3} = {1}.{4})",
                                     field.ReferencedClass.UnifiedTables[0].DBTableName,
                                     tbl,
                                     lastTableAlias,
                                     field.DBColumnName,
                                     field.ReferencedClass.GetPrimaryKeyField().DBColumnName);

            StringCollection coll = (StringCollection)Query.FromJoins[foundPos];
            coll.Add(s);
        }

        public string GetTableAliasForExpressionPrefix(string prefix) {
            string s = ExpressionPrefixToTableAlias[prefix];
            if (s == null) {
                if (Parent != null)
                    return Parent.GetTableAliasForExpressionPrefix(prefix);
                throw new Exception("Table alias unknown for exception prefix: " + prefix);
            }
            return s;
        }

        private string GetNextTablePrefix() {
            if (Parent != null)
                return Parent.GetNextTablePrefix();
            else
                return String.Format("t{0}", CurrentTablePrefix++);
        }

        public SoqlBooleanExpression BuildClassRestriction(string startingAlias, Sooda.Schema.ClassInfo classInfo) {
            if (classInfo.Subclasses.Count == 0 && classInfo.InheritsFromClass == null)
                return null;

            SoqlExpressionCollection literals = new SoqlExpressionCollection();

            foreach (ClassInfo subclass in classInfo.Subclasses) {
                if (subclass.SubclassSelectorValue != null) {
                    literals.Add(new SoqlLiteralExpression(subclass.SubclassSelectorValue));
                }
            }
            if (classInfo.SubclassSelectorValue != null) {
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

        public void ConvertQuery(SoqlQueryExpression expr) {
            expr.Accept(this);
        }

    }
}
