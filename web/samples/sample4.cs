using System;
using Sooda;

class Sample4 {
    static void Main() {
        using (SoodaTransaction transaction = new SoodaTransaction()) {
            SoodaWhereClause where = new SoodaWhereClause(
                              "Name = {0} and " +
                              "Owner.PrimaryGroup.Manager.Name like 'Mary Mana%'",
                              "My Bike");

            // get a list of matching objects
            VehicleList lst = Vehicle.GetList(where);

            // check the number of returned matches
            if (lst.Count == 1) {
                Console.WriteLine("Found exactly one matching vehicle");

                // try to cast the first vehicle to "Bike" class
                Bike b = lst[0] as Bike;

                if (b != null) {
                    // cast successful
                    Console.WriteLine("It's a bike! Wheel diameter is {0}",
                                      b.WheelDiameter);
                }
            }
        }
    }
}
