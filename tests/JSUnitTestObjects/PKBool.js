//@cc_on
//@set @debug(off)

import System;
import System.Collections;
import System.Diagnostics;
import System.Data;
import Sooda;

package Sooda.UnitTests.Objects
{
    public class PKBool extends Sooda.UnitTests.Objects.Stubs.PKBool_Stub
    {
        public function PKBool(transaction : SoodaTransaction)
        {
            super(transaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKBool()
        {
            super(SoodaTransaction.ActiveTransaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function PKBool(transaction : SoodaTransaction, factory : ISoodaObjectFactory)
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
