using System;
using Sooda;

class Sample2 {
    static void Main() {
        using (SoodaTransaction transaction = new SoodaTransaction()) {
            // load group with primary key 10
            Group g = Group.Load(10);

            // check if the manager is in role 'Customer'
            if (g.Manager.Roles.Contains(Role.Customer)) {
                // we have a manager that is a customer
                Console.WriteLine("{0} is both a manager and a customer!", 
                        g.Manager.Name);
            }

            // create a new contact that will become a member of Group[10]
            Contact newEmployee = new Contact();

            // add necessary roles
            newEmployee.Roles.Add(Role.Customer);
            newEmployee.Roles.Add(Role.Employee);

            // set some required attributes
            newEmployee.Name = "Nancy Newcomer";
            newEmployee.Type = ContactType.Employee;

            // add new employee as a member of the group
            g.Members.Add(newEmployee);

            // print some debugging information
            Console.WriteLine("Added {0} as a new member of {1}", 
                    newEmployee.Name, g.Name);

            // display group members - this includes Nancy Newcomer which 
            // has just been added
            foreach (Contact c in g.Members) {
                Console.WriteLine("member name: {0}", c.Name);
            }

            // check if the administrator is a member of the group
            if (g.Members.Contains(Contact.Administrator)) {
                Console.WriteLine("Administrator is a member of {0}", g.Name);
            }

            // commit changes to the database
            transaction.Commit();
        }
    }
}
