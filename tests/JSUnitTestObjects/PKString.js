//@cc_on
//@set @debug(off)

import System;
import System.Collections;
import System.Diagnostics;
import System.Data;
import Sooda;

package Sooda.UnitTests.Objects
{
    public class PKString extends Sooda.UnitTests.Objects.Stubs.PKString_Stub
    {
        public function PKString(transaction : SoodaTransaction)
        {
            super(transaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKString()
        {
            super(SoodaTransaction.ActiveTransaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKString(transaction : SoodaTransaction, factory : ISoodaObjectFactory)
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
