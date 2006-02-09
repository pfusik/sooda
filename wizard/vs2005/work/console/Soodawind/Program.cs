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
            using (SoodaTransaction transaction = new SoodaTransaction())
            {

                transaction.Commit();
            }
        }
    }
}
