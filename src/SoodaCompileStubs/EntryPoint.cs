// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;

using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace SoodaCompileStubs
{
    class EntryPoint
    {
#if DOTNET2
        static CSharpCodeProvider compiler;
#else
        static ICodeCompiler compiler;
#endif

        static void Compile(string description, string outputAssembly, string sourceFile, string compilerOptions, string[] args, bool additionalSources, params string[] additionalReferences)
        {
            Console.WriteLine("Creating {0}: '{1}'...", description, Path.GetFileName(outputAssembly));

            CompilerParameters options = new CompilerParameters();
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Data.dll");
            options.ReferencedAssemblies.Add("System.Drawing.dll");
            foreach (string dll in additionalReferences)
                options.ReferencedAssemblies.Add(dll);

            ArrayList sourceFiles = new ArrayList();
            sourceFiles.Add(sourceFile);

            options.CompilerOptions = compilerOptions;

            for (int i = 2; i < args.Length; ++i)
            {
                string arg = args[i];
                if (arg.EndsWith(".dll"))
                    options.ReferencedAssemblies.Add(Path.GetFullPath(arg));
                else if (arg.StartsWith("/"))
                    options.CompilerOptions += " " + arg;
                else if (additionalSources)
                    sourceFiles.Add(Path.GetFullPath(arg));
            }

            options.OutputAssembly = outputAssembly;
            options.GenerateInMemory = false;

            CompilerResults results;
            if (sourceFiles.Count == 1)
                results = compiler.CompileAssemblyFromFile(options, sourceFile);
            else
            {
#if DOTNET2
                results = compiler.CompileAssemblyFromFile(options, (string[]) sourceFiles.ToArray(typeof(string)));
#else
                results = compiler.CompileAssemblyFromFileBatch(options, (string[]) sourceFiles.ToArray(typeof(string)));
#endif
            }
            if (results.NativeCompilerReturnValue != 0)
            {
                foreach (string s in results.Output)
                    Console.WriteLine(s);
                throw new Exception("Compilation failed");
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SoodaCompileStubs assembly_base_name stubs_dir [skeleton_extra_files] [options]");
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
            string stubsDoc = Path.Combine(basePath, assemblyBaseName + ".Stubs.xml");

            bool rebuildStubs = false;

            DateTime maxSourceTime = DateTime.MinValue;
            DateTime minTargetTime = DateTime.MaxValue;
            DateTime dt;
            ArrayList sourceFiles = new ArrayList();
            for (int i = 2; i < args.Length; ++i)
            {
                if (!args[i].StartsWith("/"))
                    sourceFiles.Add(Path.GetFullPath(args[i]));
            }

            sourceFiles.Add(miniSkeletonCSX);
            sourceFiles.Add(miniStubsCSX);
            sourceFiles.Add(dbschemaBIN);
            sourceFiles.Add(stubsCSX);
            sourceFiles.Add(soodaDll);

            string[] targetFiles = { objectsAssemblyDll, stubsDll };

            foreach (string fileName in sourceFiles)
            {
                if (File.Exists(fileName))
                {
                    dt = File.GetLastWriteTime(fileName);
                    if (dt > maxSourceTime)
                        maxSourceTime = dt;
                }
            }
            foreach (string fileName in targetFiles)
            {
                if (File.Exists(fileName))
                {
                    dt = File.GetLastWriteTime(fileName);
                    if (dt < minTargetTime)
                        minTargetTime = dt;
                }
                else
                    rebuildStubs = true;
            }

            // FAT filesystem...
            if (maxSourceTime - minTargetTime > TimeSpan.FromSeconds(2))
            {
                rebuildStubs = true;
            }

            if (!rebuildStubs)
            {
                Console.WriteLine("Stubs assembly '{0}' doesn't need a rebuild!", stubsDll);
                return 0;
            }

            Console.WriteLine("Rebuilding '{0}'", stubsDll);

#if DOTNET2
            compiler = new CSharpCodeProvider();
#else
            compiler = new CSharpCodeProvider().CreateCompiler();
#endif

            try
            {
                // Step 1. Create mini-stubs
                Compile("mini stubs", stubsDll, miniStubsCSX, string.Empty, args, false, soodaDll);

                // Step 2. Create mini-skeletons
                Compile("mini skeletons", objectsAssemblyDll, miniSkeletonCSX, string.Empty, args, true, soodaDll, stubsDll);

                // Step 3. Create full stubs
                string compilerOptions = "/doc:\"" + stubsDoc + "\" /res:\"" + dbschemaBIN + "\"";
                Compile("full stubs", stubsDll, stubsCSX, compilerOptions, args, false, soodaDll, "System.Xml.dll", objectsAssemblyDll
#if DOTNET35
                    , typeof(System.Linq.IQueryable<>).Assembly.Location
#endif
                );

                Console.WriteLine("Success.");
                return 0;
            }
            catch
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
                throw;
            }
        }
    }
}
