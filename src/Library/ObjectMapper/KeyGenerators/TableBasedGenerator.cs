// 
// Copyright (c) 2002-2004 Jaroslaw Kowalski <jaak@polbox.com>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Data;

namespace Sooda.ObjectMapper.KeyGenerators
{
    public class TableBasedGenerator : IPrimaryKeyGenerator
    {
        private string keyName;
        private int poolSize = 10;
        private int currentValue = 0;
        private int maxValue = 0;
        private static Random random = new Random();
        private Sooda.Schema.DataSourceInfo dataSourceInfo;

        public TableBasedGenerator(string keyName, Sooda.Schema.DataSourceInfo dataSourceInfo)
        {
            this.keyName = keyName;
            this.dataSourceInfo = dataSourceInfo;
        }

        public object GetNextKeyValue()
        {
            lock (this)
            {
                if (currentValue >= maxValue)
                {
                    AcquireNextRange();
                }
                return currentValue++;
            }
        }

        public void AcquireNextRange()
        {
            // TODO - fix me, this is hack

            Sooda.Sql.SqlDataSource sds  = (Sooda.Sql.SqlDataSource)dataSourceInfo.CreateDataSource();
            try
            {
                sds.Open();

                IDbConnection conn = sds.Connection;

                bool gotKey = false;

                bool justInserted = false;
                int maxRandomTimeout = 2;
                for (int i = 0; (i < 10) && !gotKey; ++i)
                {
                    string query = "select key_value from KeyGen where key_name = '" + keyName + "'";
                    IDbCommand cmd = conn.CreateCommand();

                    if (!sds.DisableTransactions)
                        cmd.Transaction = sds.Transaction;

                    cmd.CommandText = query;
                    IDataReader reader = cmd.ExecuteReader();
                    int keyValue = -1;

                    if (reader.Read())
                        keyValue = reader.GetInt32(0);
                    reader.Close();

                    if (keyValue == -1)
                    {
                        if (justInserted)
                            throw new Exception("FATAL DATABASE ERROR - cannot get new key value");
                        cmd.CommandText = "insert into KeyGen(key_name, key_value) values('" + keyName + "', 1)";
                        cmd.ExecuteNonQuery();
                        justInserted = true;
                        continue;
                    }

                    //Console.WriteLine("Got key: {0}", keyValue);
                    //Console.WriteLine("Press any key to update database (simulating possible race condition here).");
                    //Console.ReadLine();

                    int nextKeyValue = keyValue + poolSize;

                    cmd.CommandText = "update KeyGen set key_value = " + nextKeyValue + " where key_name = '" + keyName + "' and key_value = " + keyValue;
                    int rows = cmd.ExecuteNonQuery();
                    // Console.WriteLine("{0} row(s) affected", rows);

                    if (rows != 1)
                    {
                        // Console.WriteLine("Conflict on write, sleeping for random number of milliseconds ({0} max)", maxRandomTimeout);
                        System.Threading.Thread.Sleep(1 + random.Next(maxRandomTimeout));
                        maxRandomTimeout = maxRandomTimeout * 2;
                        // conflict on write
                        continue;
                    }
                    else
                    {
                        this.currentValue = keyValue;
                        this.maxValue = nextKeyValue;
                        
                        sds.Commit();

                        //Console.WriteLine("New key range for {0} [{1}:{2}]", keyName, currentValue, maxValue);
                        return;
                    }
                }
                throw new Exception("FATAL DATABASE ERROR - cannot get new key value");
            }
            finally
            {
                sds.Close();
            }
        }
    }
}
