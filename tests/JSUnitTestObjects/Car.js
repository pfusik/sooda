//@cc_on
//@set @debug(off)

import System;
import System.Collections;
import System.Diagnostics;
import System.Data;
import Sooda;

package Sooda.UnitTests.Objects
{
    public class Car extends Sooda.UnitTests.Objects.Stubs.Car_Stub
    {
        public function Car(transaction : SoodaTransaction)
        {
            super(transaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function Car()
        {
            super(SoodaTransaction.ActiveTransaction);
            //Do not modify this constructor.
            this.InitObject();
            
        }
        public function Car(transaction : SoodaTransaction, factory : ISoodaObjectFactory)
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
