using System;
using System.Data.SqlTypes;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.ObjectMapper
{
    public class TestFieldValues : SoodaObjectReflectionBasedFieldValues
    {
        private static string[] _fieldNames = 
        {
            "A", "B", "C", "D", "E", 
            "A1", "B1", "C1", "D1", "E1", 
        };

        public int A;
        public string B;
        public DateTime C;
        public bool D;
        public decimal E;
        
        public SqlInt32 A1;
        public SqlString B1;
        public SqlDateTime C1;
        public SqlBoolean D1;
        public SqlDecimal E1;

        public TestFieldValues() : base(_fieldNames) { }
                                   
        public TestFieldValues(SoodaObjectReflectionBasedFieldValues values) : base(values)
        {
        }

        public override SoodaObjectFieldValues Clone()
        {
            return new TestFieldValues(this);
        }
    }
    
    [TestFixture]
    public class SoodaObjectReflectionBasedFieldValuesTest
    {
        [Test]
        public void GetBoxedFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.A = 3;
            val.B = "ala ma kota";
            val.C = d0;
            val.D = true;
            val.E = 100.0m;

            Assert.AreEqual(val.GetBoxedFieldValue(0), 3);
            Assert.AreEqual(val.GetBoxedFieldValue(1), "ala ma kota");
            Assert.AreEqual(val.GetBoxedFieldValue(2), d0);
            Assert.AreEqual(val.GetBoxedFieldValue(3), true);
            Assert.AreEqual(val.GetBoxedFieldValue(4), 100.0m);
        }
            
        [Test]
        public void GetBoxedSqlNotNullFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.A1 = 3;
            val.B1 = "ala ma kota";
            val.C1 = d0;
            val.D1 = true;
            val.E1 = 100.0m;

            Assert.AreEqual(val.GetBoxedFieldValue(5), 3);
            Assert.AreEqual(val.GetBoxedFieldValue(6), "ala ma kota");
            Assert.AreEqual(val.GetBoxedFieldValue(7), d0);
            Assert.AreEqual(val.GetBoxedFieldValue(8), true);
            Assert.AreEqual(val.GetBoxedFieldValue(9), 100.0m);
        }
            
        [Test]
        public void GetBoxedSqlNullFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.A1 = SqlInt32.Null;
            val.B1 = SqlString.Null;
            val.C1 = SqlDateTime.Null;
            val.D1 = SqlBoolean.Null;
            val.E1 = SqlDecimal.Null;

            Assert.AreEqual(val.GetBoxedFieldValue(5), null);
            Assert.AreEqual(val.GetBoxedFieldValue(6), null);
            Assert.AreEqual(val.GetBoxedFieldValue(7), null);
            Assert.AreEqual(val.GetBoxedFieldValue(8), null);
            Assert.AreEqual(val.GetBoxedFieldValue(9), null);
        }

        [Test]
        public void SetPlainFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.SetFieldValue(0, 3);
            val.SetFieldValue(1, "ala ma kota");
            val.SetFieldValue(2, d0);
            val.SetFieldValue(3, true);
            val.SetFieldValue(4, 100.0m);
            
            Assert.AreEqual(3, val.A);
            Assert.AreEqual("ala ma kota", val.B);
            Assert.AreEqual(d0, val.C);
            Assert.AreEqual(true, val.D);
            Assert.AreEqual(100.0m, val.E);
        }
            
        [Test]
        public void SetSqlNotNullFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.SetFieldValue(5, 3);
            val.SetFieldValue(6, "ala ma kota");
            val.SetFieldValue(7, d0);
            val.SetFieldValue(8, true);
            val.SetFieldValue(9, 100.0m);
            
            Assert.AreEqual((SqlInt32)3, val.A1);
            Assert.AreEqual((SqlString)"ala ma kota", val.B1);
            Assert.AreEqual((SqlDateTime)d0, val.C1);
            Assert.AreEqual((SqlBoolean)true, val.D1);
            Assert.AreEqual((SqlDecimal)100.0m, val.E1);
        }
            
        [Test]
        public void SetSqlNullFieldValueTest()
        {
            TestFieldValues val = new TestFieldValues();
            DateTime d0 = DateTime.Now.Date;

            val.SetFieldValue(5, null);
            val.SetFieldValue(6, null);
            val.SetFieldValue(7, null);
            val.SetFieldValue(8, null);
            val.SetFieldValue(9, null);
            
            Assert.AreEqual(SqlInt32.Null, val.A1);
            Assert.AreEqual(SqlString.Null, val.B1);
            Assert.AreEqual(SqlDateTime.Null, val.C1);
            Assert.AreEqual(SqlBoolean.Null, val.D1);
            Assert.AreEqual(SqlDecimal.Null, val.E1);
        }
    }
}
