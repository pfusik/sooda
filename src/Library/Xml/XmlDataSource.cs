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
using System.Data;
using System.Collections.Specialized;

using Sooda;
using Sooda.Schema;

namespace Sooda.Xml {
    public class XmlDataSource : Sooda.SoodaDataSource {
        DataSet ds = new DataSet();

        public XmlDataSource(DataSourceInfo dataSourceInfo) : base(dataSourceInfo) {
            // ds.Relations.Add(new DataRelation(
        }

        public override void Open() {}

        public override void Rollback() {}

        public override void Commit() {}

        public override void Close() {}

        public override IDataReader LoadObject(Sooda.SoodaObject obj, object keyValue, out TableInfo[] tables) {
            tables = null;
            // TODO:  Add XmlDataSource.LoadObject implementation
            return null;
        }

        public override IDataReader LoadObjectList(Sooda.Schema.ClassInfo classInfo, Sooda.SoodaWhereClause whereClause, Sooda.SoodaOrderBy orderBy, out TableInfo[] tables) {
            tables = null;
            // TODO:  Add XmlDataSource.LoadObjectList implementation
            return null;
        }

        public override IDataReader LoadRefObjectList(Sooda.Schema.RelationInfo relationInfo, int masterColumn, object masterValue, out TableInfo[] tables) {
            tables = null;
            // TODO:  Add XmlDataSource.LoadRefObjectList implementation
            return null;
        }

        public override void MakeTuple(string tableName, string leftColumn, string rightColumn, object leftVal, object rightVal, int mode) {
            // TODO:  Add XmlDataSource.MakeTuple implementation
        }

        public override void BeginSaveChanges()
        {
        }

        public override void FinishSaveChanges()
        {
        }

        public override void SaveObjectChanges(Sooda.SoodaObject obj) 
        {
            // TODO:  Add XmlDataSource.SaveObjectChanges implementation
        }

        public override IDataReader LoadObjectTable(SoodaObject obj, object keyValue, int tableNumber, out TableInfo[] tables) {
            tables = null;
            return null;
        }

        public override IDataReader ExecuteQuery(Sooda.QL.SoqlQueryExpression expr, SchemaInfo schema, object[] parameters) {
            return null;
        }

        public override IDataReader ExecuteRawQuery(string expr, object[] parameters)
        {
            return null;
        }
        
        public override int ExecuteNonQuery(string expr, object[] parameters)
        {
            return 0;
        }

        public override bool IsOpen
        {
            get { return false; }
        }
    }
}
