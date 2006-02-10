using System;
using System.Collections;
using System.Collections.Specialized;

using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace SoodaCompileStubs
{
	class EntryPoint
	{
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SoodaCompileStubs assembly_base_name stubs_dir [skeleton_extra_files]");
                return 1;
            }
            string assemblyBaseName = args[0];
            string basePath = args[1];

            string miniSkeletonCSX = Path.Combine(basePath, "_MiniSkeleton.csx");
            string miniStubsCSX = Path.Combine(basePath, "_MiniStubs.csx");
            string stubsCSX = Path.Combine(basePath, "_Stubs.csx");
            string dbschemaBIN = Path.Combine(basePath, "_DBSchema.bin");
            string soodaDll = typeof(Sooda.SoodaObject).Assembly.Location;

            string objectsAssemblyDll = Path.Combine(basePath, assemblyBaseName + ".dll");
            string stubsDll = Path.Combine(basePath, assemblyBaseName + ".Stubs.dll");

            bool rebuildStubs = false;

            DateTime maxSourceTime = DateTime.MinValue;
            DateTime minTargetTime = DateTime.MaxValue;
            DateTime dt;
            bool success = false;
            ArrayList sourceFiles = new ArrayList();
            for (int i = 2; i < args.Length; ++i)
            {
                sourceFiles.Add(Path.GetFullPath(args[i]));
            }

            sourceFiles.Add(miniSkeletonCSX);
            sourceFiles.Add(miniStubsCSX);
            sourceFiles.Add(dbschemaBIN);
            sourceFiles.Add(stubsCSX);
            sourceFiles.Add(soodaDll);

            string[] targetFiles = {
                                       objectsAssemblyDll,
                                       stubsDll
                                   };

            try
            {

                foreach (string fileName in sourceFiles)
                {
                    if (File.Exists(fileName))
                    {
                        dt = File.GetLastWriteTime(fileName);
                        if (dt > maxSourceTime)
                        {
                            maxSourceTime = dt;
                        }
                    }
                }
                foreach (string fileName in targetFiles)
                {
                    if (File.Exists(fileName))
                    {
                        dt = File.GetLastWriteTime(fileName);
                        if (dt < minTargetTime)
                        {
                            minTargetTime = dt;
                        }
                    }
                    else
                    {
                        rebuildStubs = true;
                    }
                }

                // FAT filesystem...
                if (maxSourceTime - minTargetTime > TimeSpan.FromSeconds(2))
                {
                    rebuildStubs = true;
                }
                if (!rebuildStubs)
                {
                    success = true;
                    Console.WriteLine("Stubs assembly '{0}.Stubs.dll' doesn't need a rebuild!", assemblyBaseName);
                    return 0;
                }

                Console.WriteLine("Rebuilding '{0}.Stubs.dll'", assemblyBaseName);

                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
#if DOTNET2
                CSharpCodeProvider compiler = codeProvider;
#else
                ICodeCompiler compiler = codeProvider.CreateCompiler();
#endif

                // Step 1. Create mini-stubs

                Console.WriteLine("Creating mini stubs: '{0}'...", Path.GetFileName(stubsDll));

                CompilerParameters options = new CompilerParameters();
                options.ReferencedAssemblies.Add(soodaDll);
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Data.dll");
                for (int i = 2; i < args.Length; ++i)
                {
                    if (args[i].EndsWith(".dll"))
                        options.ReferencedAssemblies.Add(Path.GetFullPath(args[i]));
                }
                options.OutputAssembly = stubsDll;
                options.GenerateInMemory = false;

                CompilerResults results = compiler.CompileAssemblyFromFile(options, miniStubsCSX);
                if (results.NativeCompilerReturnValue != 0)
                {
                    Console.WriteLine("Compilation failed:");
                    foreach (string s in results.Output)
                    {
                        Console.WriteLine("{0}", s);
                    }
                    return 1;
                }

                // Step 2. Create mini-skeletons

                Console.WriteLine("Creating mini skeletons: '{0}'...", Path.GetFileName(objectsAssemblyDll));

                options = new CompilerParameters();
                options.ReferencedAssemblies.Add(soodaDll);
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Data.dll");
                options.ReferencedAssemblies.Add(stubsDll);
                for (int i = 2; i < args.Length; ++i)
                {
                    if (args[i].EndsWith(".dll"))
                        options.ReferencedAssemblies.Add(Path.GetFullPath(args[i]));
                }
                options.OutputAssembly = objectsAssemblyDll;
                options.GenerateInMemory = false;

                ArrayList skeletonSourceFiles = new ArrayList();
                skeletonSourceFiles.Add(miniSkeletonCSX);
                for (int i = 2; i < args.Length; ++i)
                {
                    if (args[i].EndsWith(".dll"))
                        continue;

                    skeletonSourceFiles.Add(Path.GetFullPath(args[i]));
                }

#if DOTNET2
                results = compiler.CompileAssemblyFromFile(options, (string[])skeletonSourceFiles.ToArray(typeof(string)));
#else
                results = compiler.CompileAssemblyFromFileBatch(options, (string[])skeletonSourceFiles.ToArray(typeof(string)));
#endif
                if (results.NativeCompilerReturnValue != 0)
                {
                    Console.WriteLine("Compilation failed:");
                    foreach (string s in results.Output)
                    {
                        Console.WriteLine("{0}", s);
                    }
                    return 1;
                }

                options = new CompilerParameters();
                options.ReferencedAssemblies.Add(soodaDll);
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Data.dll");
                options.ReferencedAssemblies.Add("System.Xml.dll");
                options.ReferencedAssemblies.Add(objectsAssemblyDll);
                for (int i = 2; i < args.Length; ++i)
                {
                    if (args[i].EndsWith(".dll"))
                        options.ReferencedAssemblies.Add(Path.GetFullPath(args[i]));
                }
                
                options.CompilerOptions = "/res:" + Path.Combine(basePath, "_DBSchema.bin");
                options.OutputAssembly = stubsDll;
                options.GenerateInMemory = false;

                Console.WriteLine("Creating full stubs: '{0}'...", Path.GetFileName(stubsDll));

                results = compiler.CompileAssemblyFromFile(options, stubsCSX);
                if (results.NativeCompilerReturnValue != 0)
                {
                    Console.WriteLine("Compilation failed:");
                    foreach (string s in results.Output)
                    {
                        Console.WriteLine("{0}", s);
                    }
                    return 1;
                }

                success = true;
                Console.WriteLine("Success.");
                return 0;
            }
            finally
            {
                if (!success)
                {
                    Console.WriteLine("Deleting partially written output files.");
                    foreach (string s in targetFiles)
                    {
                        if (File.Exists(s))
                        {
                            Console.WriteLine("'{0}'...", s);
                            File.Delete(s);
                        }
                    }
                }
            }
        }
	}
}
