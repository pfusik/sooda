using System;
using Sooda;

class Sample3 {
    static void Main() {
        using (SoodaTransaction transaction = new SoodaTransaction()) {
            Vehicle v;

            // load vehicle with #10
            v = Vehicle.Load(10);

            if (v is Bike) {
                Console.WriteLine("vehicle #10 is a bike.");

                // do something with the bike
                Bike b = (Bike)v;
            }
        }
    }
}
