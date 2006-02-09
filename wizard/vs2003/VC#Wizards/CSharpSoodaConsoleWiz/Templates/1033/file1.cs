using System;
using Sooda;

namespace [!output SAFE_NAMESPACE_NAME]
{
	/// <summary>
	/// Summary description for [!output SAFE_CLASS_NAME].
	/// </summary>
	class [!output SAFE_CLASS_NAME]
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            //
            // TODO
            // 1. adjust your connection parameters in App.config file
            //
            // 2. adjust your mapping schema in SoodaSchema.xml 
            //    then rebuild your project. NOTE: each time you add a
            //    new class, you need to rebuild twice because of VS.NET
            //    file caching issues
            //    
            using (SoodaTransaction transaction = new SoodaTransaction())
            {
                // You may use Sooda objects here.
                transaction.Commit();
            }
		}
	}
}
