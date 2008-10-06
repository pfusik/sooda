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
using System.Xml;
using Sooda.Caching;

// TODO - this is very hackish - needs to be cleaned up
// original intention was preserving memory, but this has gone too far

namespace Sooda.ObjectMapper
{
    public class SoodaRelationTupleChangedArgs : EventArgs
    {
        public object Left;
        public object Right;
        public int Mode;

        public SoodaRelationTupleChangedArgs(object left, object right, int mode)
        {
            this.Left = left;
            this.Right = right;
            this.Mode = mode;
        }
    }

    public delegate void SoodaRelationTupleChanged(object sender, SoodaRelationTupleChangedArgs args);

    public abstract class SoodaRelationTable
    {
        public struct Tuple
        {
            public object ref1;
            public object ref2;
            public int tupleMode;       // -1 - remove, +1 - add
            public bool saved;
        }

        private Tuple[] tuples = null;
        private int count = 0;
        private string tableName;
        private string leftColumnName;
        private string rightColumnName;
        private Sooda.Schema.RelationInfo relationInfo;
        private bool _dirty = false;

        public event SoodaRelationTupleChanged OnTupleChanged;

        protected SoodaRelationTable(string tableName, string leftColumnName, string rightColumnName, Sooda.Schema.RelationInfo relationInfo)
        {
            this.relationInfo = relationInfo;
            this.tableName = tableName;
            this.leftColumnName = leftColumnName;
            this.rightColumnName = rightColumnName;
        }

        public int TupleCount
        {
            get
            {
                return count;
            }
        }

        public Tuple[] Tuples
        {
            get
            {
                return tuples;
            }
        }

        public void Add(object ref1, object ref2)
        {
            SetTupleMode(ref1, ref2, 1);
        }

        public void Remove(object ref1, object ref2)
        {
            SetTupleMode(ref1, ref2, -1);
        }

        void SetTupleMode(object ref1, object ref2, int tupleMode)
        {
            _dirty = true;
            for (int i = 0; i < count; ++i)
            {
                if (tuples[i].ref1.Equals(ref1) && tuples[i].ref2.Equals(ref2))
                {
                    tuples[i].tupleMode = tupleMode;
                    if (this.OnTupleChanged != null)
                        this.OnTupleChanged(this, new SoodaRelationTupleChangedArgs(ref1, ref2, tupleMode));
                    return;
                }
            }
            MakeRoom();
            tuples[count].ref1 = ref1;
            tuples[count].ref2 = ref2;
            tuples[count].tupleMode = tupleMode;
            count++;
            if (this.OnTupleChanged != null)
                this.OnTupleChanged(this, new SoodaRelationTupleChangedArgs(ref1, ref2, tupleMode));
        }

        void MakeRoom()
        {
            if (tuples == null || count >= tuples.Length)
            {
                int newCapacity = 64;
                if (tuples != null)
                    newCapacity = tuples.Length * 2;
                if (newCapacity < 64)
                    newCapacity = 64;

                Tuple[] newTuples = new Tuple[newCapacity];
                if (tuples != null)
                    tuples.CopyTo(newTuples, 0);
                tuples = newTuples;
            }
        }

        public void SaveTuples(SoodaTransaction tran, bool isPrecommit)
        {
            if (count == 0)
                return;

            SoodaDataSource ds = tran.OpenDataSource(relationInfo.GetDataSource());
            
            ISoodaObjectFactory leftFactory = tran.GetFactory(relationInfo.GetRef1ClassInfo());
            ISoodaObjectFactory rightFactory = tran.GetFactory(relationInfo.GetRef2ClassInfo());

            bool first = true;
            for (int i = 0; i < count; ++i)
            {
                if (!tuples[i].saved)
                {
                    if (isPrecommit)
                    {
                        SoodaObject leftObject = leftFactory.GetRef(tran, tuples[i].ref1);
                        SoodaObject rightObject = rightFactory.GetRef(tran, tuples[i].ref2);
                        tran.PrecommitObject(leftObject);
                        tran.PrecommitObject(rightObject);
                        if (first)
                        {
                            first = false;
                            tran.PrecommitRelation(this.relationInfo);
                        }
                    }
                                            
                    ds.MakeTuple(tableName, leftColumnName, rightColumnName, tuples[i].ref1, tuples[i].ref2, tuples[i].tupleMode);
                    tuples[i].saved = true;
                }
            }
        }

        public void Commit()
        {
            count = 0;
            tuples = null;
        }

        internal void Serialize(XmlWriter writer, SoodaSerializeOptions options)
        {
            if (count == 0)
                return;

            writer.WriteStartElement("relation");
            writer.WriteAttributeString("type", GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("tupleCount", count.ToString());
            for (int i = 0; i < count; ++i)
            {
                string modeString = null;

                if (tuples[i].tupleMode == 1)
                    modeString = "add";
                else if (tuples[i].tupleMode == -1)
                    modeString = "remove";
                else
                    continue;
                writer.WriteStartElement("tuple");
                writer.WriteAttributeString("mode", modeString);
                writer.WriteAttributeString("r1", tuples[i].ref1.ToString());
                writer.WriteAttributeString("r2", tuples[i].ref2.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        internal void BeginDeserialization(int tupleCount)
        {
            tuples = new Tuple[tupleCount < 16 ? 16 : tupleCount];
            count = 0;
            _dirty = false;
        }

        internal void InvalidateCacheAfterCommit(ISoodaCache cache)
        {
            if (_dirty)
            {
                cache.Invalidate(this.relationInfo.Name, "", SoodaCacheInvalidateReason.ManyToManyModified);
                cache.Invalidate(this.relationInfo.GetRef1ClassInfo().Name, "", SoodaCacheInvalidateReason.ManyToManyModified);
                cache.Invalidate(this.relationInfo.GetRef2ClassInfo().Name, "", SoodaCacheInvalidateReason.ManyToManyModified);
            }
            _dirty = false;
        }

        protected abstract object DeserializeTupleLeft(XmlReader reader);
        protected abstract object DeserializeTupleRight(XmlReader reader);

        internal void DeserializeTuple(XmlReader reader)
        {
            string mode = reader.GetAttribute("mode");
            int modeValue = 0;

            if (mode == "add")
            {
                modeValue = 1;
            }
            else if (mode == "remove")
            {
                modeValue = -1;
            }
            else
                throw new ArgumentException("Invalid tuple mode on deserialize: " + mode);

            tuples[count].tupleMode = modeValue;
            tuples[count].ref1 = DeserializeTupleLeft(reader);
            tuples[count].ref2 = DeserializeTupleRight(reader);
            count++;
        }
    }
}
