//@cc_on
//@set @debug(off)

import System;
import System.Collections;
import System.Diagnostics;
import System.Data;
import Sooda;

package Sooda.UnitTests.Objects
{
    public class PKInt64 extends Sooda.UnitTests.Objects.Stubs.PKInt64_Stub
    {
        public function PKInt64(transaction : SoodaTransaction)
        {
            super(transaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKInt64()
        {
            super(SoodaTransaction.ActiveTransaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKInt64(transaction : SoodaTransaction, factory : ISoodaObjectFactory)
        {
            super(transaction, factory);
            //Do not modify this constructor.
            
        }
        private function InitObject()
        {
            //TODO: Add construction logic here.
        }
    }
}
