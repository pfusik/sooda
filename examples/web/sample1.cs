using System;
using Sooda;

class Sample1 {
    static void Main() {
        using (SoodaTransaction transaction = new SoodaTransaction()) {
            // load group with primary key 10
            Group g = Group.Load(10);

            // display its manager's name
            Console.WriteLine("name: {0}", g.Manager.Name);

            // double the salary
            g.Manager.Salary *= 2;

            // save changes to the database
            transaction.Commit();
        }
    }
}
