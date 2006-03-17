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
using System.Diagnostics;

using Sooda.ObjectMapper;
using System.IO;
using Sooda.UnitTests.Objects;

using NUnit.Framework;

namespace Sooda.UnitTests.TestCases.Soql
{
    [TestFixture]
    public class QueryTranslatorTest
    {
        private string Normalize(string s)
        {
            string s1;

            s = s.Replace('\n', ' ');
            s = s.Replace('\r', ' ');
            s = s.Replace('\t', ' ');

            s1 = s;
            do
            {
                s = s1;
                s1 = s.Replace("  ", " ");
            } while (s1 != s);
            s = s.Replace("( ", "(");
            s = s.Replace(" )", ")");
            s = s.Trim();
            return s;
        }

        private void AssertTranslation(string input, string output)
        {
            StringWriter sw = new StringWriter();
            Sooda.Sql.SoqlToSqlConverter converter = new Sooda.Sql.SoqlToSqlConverter(sw, _DatabaseSchema.GetSchema(), new Sooda.Sql.SqlServerBuilder());
            Sooda.QL.SoqlPrettyPrinter prettyPrinter = new Sooda.QL.SoqlPrettyPrinter(Console.Out);

            Sooda.QL.SoqlQueryExpression query = Sooda.QL.SoqlParser.ParseQuery(input);
            prettyPrinter.PrintQuery(query);
            Console.WriteLine();
            Console.WriteLine("--------");
            converter.ConvertQuery(query);
            string o = sw.ToString();

            Console.WriteLine(o);
            Console.WriteLine("--------");

            o = Normalize(o);

            if (output == null)
            {
                Console.WriteLine("AssertTranslation(\n\t\"{0}\",\n\t\"{1}\");", input, o);
                return;
            }
            output = Normalize(output);

            Assert.AreEqual(output, o);
        }

        [Test]
        public void SimpleTest1()
        {
            AssertTranslation(
                "select * from Contact",
                @"
select   t0.id as [ContactId],
         t0.primary_group as [PrimaryGroup],
         t0.type as [Type],
         t0.name as [Name],
         t0.active as [Active],
         t0.last_salary as [LastSalary]
from     Contact t0"
);
        }
        [Test]
        public void SimpleTest2()
        {
            AssertTranslation(
                "select ContactId from Contact",
                "select t0.id as ContactId from Contact t0");
        }
        [Test]
        public void SimpleTest3()
        {
            AssertTranslation(
                "select PrimaryGroup.Id from Contact",
                "select t1.id as PrimaryGroup_Id from Contact t0 left outer join _Group t1 on (t0.primary_group = t1.id)");
        }
        [Test]
        public void SimpleTest4()
        {
            AssertTranslation(
                "select Id from Group",
                "select t0.id as Id from _Group t0");
        }
        [Test]
        public void SimpleTest5()
        {
            AssertTranslation(
                "select id from Group",
                "select t0.id as id from _Group t0");
        }
        [Test]
        public void OneToManyCountTest3()
        {
            AssertTranslation(
                "select Name,Members.Count from Group",
                "select t0.name as Name, (select count(*) from Contact where primary_group=t0.id) as Members_Count from _Group t0");
        }
        [Test]
        public void OneToManyCountTest1()
        {
            AssertTranslation(
                "select Name,Members.Count from Group where Members.Count = 0",
                "select t0.name as Name, (select count(*) from Contact where primary_group=t0.id) as Members_Count from _Group t0 where ((select count(*) from Contact where primary_group=t0.id) = 0)");
        }
        [Test]
        public void OneToManyCountTest2()
        {
            AssertTranslation(
                "select Name,Members.Count from Group where Members.Count <> Members.Count",
                "select t0.name as Name, (select count(*) from Contact where primary_group=t0.id) as Members_Count from _Group t0 where ((select count(*) from Contact where primary_group=t0.id) <> (select count(*) from Contact where primary_group=t0.id))");
        }
        [Test]
        public void SelectFromRelationTest1()
        {
            AssertTranslation(
                "select c.Contact.Name,c.Role.Name from ContactToRole c where c.Contact.PrimaryGroup.Manager.Name = 'Mary Manager'",
                "select t0.name as c_Contact_Name, t1.name as c_Role_Name from ContactRole c left outer join Contact t0 on (c.contact_id = t0.id) left outer join _Role t1 on (c.role_id = t1.id) left outer join _Group t2 on (t0.primary_group = t2.id) left outer join Contact t3 on (t2.manager = t3.id) where (t3.name = {L:AnsiString:Mary Manager})");
        }
        [Test]
        public void SelectCountAsteriskTest1()
        {
            AssertTranslation(
                "select count(*) from Contact",
                "select count(*) from Contact t0");
        }
        [Test]
        public void ManyToManyCountTest1()
        {
            AssertTranslation(
                "select Name,Roles.Count from Contact",
                "select t0.name as Name, (select count(*) from ContactRole where contact_id=t0.id) as Roles_Count from Contact t0");
        }
        [Test]
        public void ManyToManyCountTest2()
        {
            AssertTranslation(
                "select Name,Roles.Count from Contact where Roles.Count > 3",
                "select t0.name as Name, (select count(*) from ContactRole where contact_id=t0.id) as Roles_Count from Contact t0 where ((select count(*) from ContactRole where contact_id=t0.id) > 3)");
        }

        [Test]
        public void ManyToManyCountTest3()
        {
            AssertTranslation(
                "select Name,Roles.Count from Contact where Roles.Count < Roles.Count + 1",
                "select t0.name as Name, (select count(*) from ContactRole where contact_id=t0.id) as Roles_Count from Contact t0 where ((select count(*) from ContactRole where contact_id=t0.id) < ((select count(*) from ContactRole where contact_id=t0.id) + 1))");
        }

        [Test]
        public void OneToManyContainsTest()
        {
            AssertTranslation(
                "select Name from Group where Members.Contains(1)",
                "select t0.name as Name from _Group t0 where exists (select * from Contact where primary_group=t0.id and id in (1))");
        }

        [Test]
        public void OneToManyContainsTest2()
        {
            AssertTranslation(
                "select Name from Group where Members.Contains(select ContactId from Contact where Name='Mary Manager')",
                "select t0.name as Name from _Group t0 where exists (select * from Contact where primary_group=t0.id and id in (select t1.id as ContactId from Contact t1 where (t1.name = {L:AnsiString:Mary Manager})))");
        }

        [Test]
        public void OneToManyContainsTest3()
        {
            AssertTranslation(
                "select Name from Group where Members.Contains(Contact where Name='Mary Manager')",
                @"
select   t0.name as Name
from     _Group t0
where    exists (select * from Contact where primary_group=t0.id and id in (
    select   t1.id as [ContactId]
    from     Contact t1
    where    (t1.name = {L:AnsiString:Mary Manager})))
");
        }

        [Test]
        public void ManyToManyContainsTest()
        {
            AssertTranslation(
                "select Name from Contact where Roles.Contains(3)",
                "select t0.name as Name from Contact t0 where exists (select * from ContactRole where contact_id=t0.id and role_id in (3))");
        }

        [Test]
        public void ManyToManyContainsTest2()
        {
            AssertTranslation(
                "select Name from Contact where Roles.Contains(select Id from Role where Name like 'Man%')",
                "select t0.name as Name from Contact t0 where exists (select * from ContactRole where contact_id=t0.id and role_id in (select t1.id as Id from _Role t1 where (t1.name like {L:String:Man%})))");
        }

        [Test]
        public void ManyToManyContainsTest3()
        {
            AssertTranslation(
                "select Name from Contact where Roles.Contains(select Id from Role where Name like 'Man%' and Members.Contains(1))",
                "select t0.name as Name from Contact t0 where exists (select * from ContactRole where contact_id=t0.id and role_id in (select t1.id as Id from _Role t1 where ((t1.name like {L:String:Man%}) and exists (select * from ContactRole where role_id=t1.id and contact_id in (1)))))");
        }

        [Test]
        public void ManyToManyContainsTest4()
        {
            AssertTranslation(
                "select Name from Contact where Roles.Contains(Role where Name like 'Man%' and Members.Contains(1))",
                @"
select   t0.name as Name
from     Contact t0
where    exists (select * from ContactRole where contact_id=t0.id and role_id in (
    select   t1.id as [Id]
    from     _Role t1
    where    ((t1.name like {L:String:Man%}) and exists (select * from ContactRole where role_id=t1.id and contact_id in (
1)))))
");
        }

        [Test]
        public void ComplexTest1()
        {
            AssertTranslation(
                "select * from Contact where Roles.Contains(Role where Name like 'Manag%')",
                null);
        }

        [Test]
        public void ComplexTest2()
        {
            AssertTranslation(
                @"
                select c2.Name
                from Contact c2
                where exists (ContactToRole cr where cr.Role.Name = 'Unit Manager' and cr.Contact = c2.PrimaryGroup.Manager)",
                null);
        }

        [Test]
        public void ComplexTest3()
        {
            AssertTranslation(
                @"
                select * from ContactToRole cr where cr.Role.Name = 'Manager' and cr.Contact = 1",
                null);
        }

        [Test]
        public void ComplexTest4()
        {
            AssertTranslation(
                @"
                select *
                from ContactToRole cr
                where exists (Contact as c2 where cr.Contact.Name = c2.Name and cr.Contact != c2.ContactId and cr.Contact.PrimaryGroup.Manager = c2.ContactId)",
                null);
        }

        [Test]
        public void ComplexTest5()
        {
            AssertTranslation(
                @"
                select *
                from ContactToRole cr
                where Contact.PrimaryGroup.Manager.Name = 'zzz'
                and exists (Contact where Name = cr.Contact.Name)
                and exists (Contact where Name = cr.Contact.Name + 'a')
                and exists (Group where Manager.Name = 'Mary Manager'
                and Members.Count > 3
                and Members.Contains(Contact where Name='ZZZ' and PrimaryGroup.Members.Contains(3))
                and Manager.Roles.Contains(Role where Name='Customer'))",

                @"select   cr.contact_id as [Contact],
         cr.role_id as [Role]
from     ContactRole cr
         left outer join Contact t0 on (cr.contact_id = t0.id)
         left outer join _Group t1 on (t0.primary_group = t1.id)
         left outer join Contact t2 on (t1.manager = t2.id)
where    ((((t2.name = {L:AnsiString:zzz}) and exists (
    select   *
    from     Contact t3
    where    (t3.name = t0.name)
)) and exists (
    select   *
    from     Contact t4
    where    (t4.name = (t0.name + 'a'))
)) and exists (
    select   *
    from     _Group t5
             left outer join Contact t6 on (t5.manager = t6.id)
    where    ((((t6.name = {L:AnsiString:Mary Manager}) and ((select count(*) from Contact where primary_group=t5.id) > 3)) and exists (select * from Contact where primary_group=t5.id and id in (
        select   t7.id as [ContactId]
        from     Contact t7
                 left outer join _Group t8 on (t7.primary_group = t8.id)
        where    ((t7.name = {L:AnsiString:ZZZ}) and exists (select * from Contact where primary_group=t8.id and id in (
3)))))) and exists (select * from ContactRole where contact_id=t6.id and role_id in (
        select   t9.id as [Id]
        from     _Role t9
        where    (t9.name = {L:String:Customer}))))
))
");
        }

        [Test]
        public void HavingTest1()
        {
            AssertTranslation(
                @"
                select   c0.Name,count(*)
                from     Contact c0
                where    exists (select * from Contact c1 where c1.ContactId = c0.PrimaryGroup.Manager)
                group by c0.Name
                having   count(*) < 3",
                null);
        }

        [Test]
        public void BooleanExprTest()
        {
            AssertTranslation(@"select c.Name from Contact c where not true and false", null);
        }
#if A
        Test("select c.Contact.* from ContactToRole c");
        Test("select coalesce(Name,null),t0.PrimaryGroup,PrimaryGroup,PrimaryGroup,t0.Name,PrimaryGroup.Manager.Name from Contact where t0.ContactId=1 order by Name");
        Test("select Name from Contact");
        Test("select * from Contact");
        Test("select c1.PrimaryGroup.Manager,c1.Name,c2.Name,c2.Manager.Name from Contact c1, Group c2");
        Test("select Manager.Name from Contact c1, Group c2");
        Test("select ExtendedBike.*,Contact.*,Group.*,Bike.* from ExtendedBike,Contact,Group,Bike");
        Test("select PrimaryGroup.Manager.Name from Contact c1, Group c2 where c1.PrimaryGroup = c2.Id and c2.Name = 'Group1'");
        Test(@"select ExtendedBike.*,Contact.*,Group.*,Bike.*
             from ExtendedBike,Contact c,Group g,Bike
             where ExtendedBikeInfo = 'a'
             ");
#endif

    }
}
