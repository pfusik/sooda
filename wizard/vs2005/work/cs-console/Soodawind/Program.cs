using System;
using Sooda;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Soodawind;

[assembly: SoodaStubAssembly(typeof(_DatabaseSchema))]

namespace Soodawind
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // TODO - change database connection information in App.config
            //

            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                //
                // TODO - Add code that uses Sooda objects here
                // 

                transaction.Commit();
            }
        }
    }
}
