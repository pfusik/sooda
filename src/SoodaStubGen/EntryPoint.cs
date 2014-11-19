//
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// Copyright (c) 2006-2014 Piotr Fusik <piotr@fusik.info>
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

using Sooda.CodeGen;
using System;
using System.IO;

namespace SoodaStubGen
{
    public class EntryPoint
    {
        static int Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("SoodaStubGen FILE.soodaproject");
            Console.WriteLine("or:");
            Console.WriteLine("SoodaStubGen [OPTIONS] --schema FILE.xml --namespace NAME --output DIR");
            Console.WriteLine();
            Console.WriteLine("        --lang csharp    - (default) generate C# code");
#if !NO_VB
            Console.WriteLine("        --lang vb        - generate VB.NET code");
#endif
#if !NO_JSCRIPT
            Console.WriteLine("        --lang js        - generate JS.NET code (broken)");
#endif

            Console.WriteLine("        --lang TYPE      - generate code using the specified CodeDOM codeProvider");
            Console.WriteLine();
            Console.WriteLine("        --project vs2005 - (default) generate VS 2005 project file (.??proj)");
            Console.WriteLine("        --project null   - generate no project file");
            Console.WriteLine("        --project TYPE   - generate project file using custom type");
            Console.WriteLine();
            Console.WriteLine("        --schema FILE.xml     - generate code from the specified schema");
            Console.WriteLine("        --namespace NAME      - specify the namespace to use");
            Console.WriteLine("        --output DIR          - specify the output directory for files");
            Console.WriteLine("        --assembly NAME       - specify the name of resulting assembly");
            Console.WriteLine("        --base-class NAME     - specify the name of stub base class (SoodaObject)");
            Console.WriteLine("        --rebuild-if-changed  - rebuild only if source files newer than targets");
            Console.WriteLine("        --force-rebuild       - rebuild always");
            Console.WriteLine("        --rewrite-skeletons   - force overwrite of skeleton classes");
            Console.WriteLine("        --rewrite-project     - force overwrite of project file");
            Console.WriteLine("        --separate-stubs      - enable separate compilation of stubs");
            Console.WriteLine("        --merged-stubs        - disable separate compilation of stubs");
            Console.WriteLine("        --schema-embed-xml    - embed schema as an XML file");
            Console.WriteLine("        --schema-embed-bin    - embed schema as an BIN file");
            Console.WriteLine("        --help                - display this help");
            Console.WriteLine();
            Console.WriteLine("        --null-progagation    - enable null propagation");
            Console.WriteLine("        --no-null-progagation - disable null propagation (default)");
            Console.WriteLine("        --nullable-as [boxed | sqltype | raw | nullable ] (default = boxed)");
            Console.WriteLine("        --not-null-as [boxed | sqltype | raw | nullable ] (default = raw)");
            Console.WriteLine("                         - specify the way primitive values are handled");
            Console.WriteLine("        --no-typed-queries    - disable Typed Queries");
            Console.WriteLine("        --no-soql             - disable SOQL queries");
            Console.WriteLine();
            return 1;
        }

        public static int Main(string[] args)
        {
            try
            {
                Sooda.CodeGen.SoodaProject project = new SoodaProject();
                Sooda.CodeGen.CodeGenerator generator = new CodeGenerator(project, new ConsoleCodeGeneratorOutput());

                string writeProjectTo = null;

                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "/?":
                        case "-?":
                        case "--help":
                        case "-h":
                            return Usage();

                        // generator options (this run only)

                        case "--rebuild-if-changed":
                            generator.RebuildIfChanged = true;
                            break;

                        case "--force-rebuild":
                            generator.RebuildIfChanged = false;
                            break;

                        case "-l":
                        case "--lang":
                            project.Language = args[++i];
                            break;

                        case "-p":
                        case "--project":
                            string projectType = args[++i];
                            ExternalProjectInfo projectInfo = new ExternalProjectInfo(projectType);
                            project.ExternalProjects.Add(projectInfo);
                            break;

                        case "--separate-stubs":
                            project.SeparateStubs = true;
                            break;

                        case "--merged-stubs":
                            project.SeparateStubs = false;
                            break;

                        case "--schema-embed-xml":
                            project.EmbedSchema = EmbedSchema.Xml;
                            break;

                        case "--schema-embed-bin":
                            project.EmbedSchema = EmbedSchema.Binary;
                            break;

                        case "--nullable-as":
                            project.NullableRepresentation = (PrimitiveRepresentation)Enum.Parse(typeof(PrimitiveRepresentation), args[++i], true);
                            break;

                        case "--not-null-as":
                            project.NotNullRepresentation = (PrimitiveRepresentation)Enum.Parse(typeof(PrimitiveRepresentation), args[++i], true);
                            break;

                        case "-s":
                        case "--schema":
                            project.SchemaFile = args[++i];
                            break;

                        case "-a":
                        case "--assembly-name":
                            project.AssemblyName = args[++i];
                            break;

                        case "-bc":
                        case "--base-class":
                            project.BaseClassName = args[++i];
                            break;

                        case "--null-propagation":
                            project.NullPropagation = true;
                            break;

                        case "--no-null-propagation":
                            project.NullPropagation = false;
                            break;

                        case "--no-typed-queries":
                            project.WithTypedQueryWrappers = false;
                            break;

                        case "--no-soql":
                            project.WithSoql = false;
                            break;

                        case "--rewrite-skeletons":
                            generator.RewriteSkeletons = true;
                            break;

                        case "--rewrite-projects":
                        case "--rewrite-project":
                            generator.RewriteProjects = true;
                            break;

                        case "-n":
                        case "-ns":
                        case "--namespace":
                            project.OutputNamespace = args[++i];
                            break;

                        case "-o":
                        case "-out":
                        case "--output":
                            project.OutputPath = args[++i];
                            break;

                        case "--write-project":
                            writeProjectTo = args[++i];
                            break;

                        default:
                            if (args[i].EndsWith(".soodaproject"))
                            {
                                string fullPath = Path.GetFullPath(args[i]);
                                Console.WriteLine("Loading project from file '{0}'...", fullPath);
                                Environment.CurrentDirectory = Path.GetDirectoryName(fullPath);
                                generator.Project = SoodaProject.LoadFrom(fullPath);
                                //XmlSerializer ser = new XmlSerializer(typeof(SoodaProject));
                                //ser.Serialize(Console.Out, project);
                                //Console.WriteLine("OUT: {0}", project.OutputPath);
                            }
                            else
                            {
                                Console.WriteLine("Unknown option '{0}'", args[i]);
                                return Usage();
                            }
                            break;
                    }
                };

                if (writeProjectTo != null)
                {
                    project.WriteTo(writeProjectTo);
                }

                generator.Run();

                return 0;
            }
            catch (SoodaCodeGenException ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                return 1;
            }
        }
    }
}
