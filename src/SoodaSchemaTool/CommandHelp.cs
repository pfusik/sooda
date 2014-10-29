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
using System.Reflection;

namespace SoodaSchemaTool
{
[Command("help", "display command usage information")]
public class CommandHelp : Command
{
    public override int Run(string[] args)
    {
        Console.WriteLine("Copyright (c) 2005-2006 by Jaroslaw Kowalski. All rights reserved.");
        Console.WriteLine();
        if (args.Length == 1)
        {
            Command cmd = CommandFactory.CreateCommand(args[0]);

            CommandAttribute ca = (CommandAttribute)Attribute.GetCustomAttribute(cmd.GetType(), typeof(CommandAttribute));
            Console.WriteLine("SoodaSchemaTool {0} - {1}", ca.Name, ca.Description);

            PropertyInfo[] properties = cmd.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (PropertyInfo pi in properties)
            {
                Console.WriteLine(" {0}", pi.Name);
            }
        }
        else
        {
            Console.WriteLine("Usage: SoodaSchemaTool command [arguments]");
            Console.WriteLine("Where command can be one of:");
            Console.WriteLine();

            foreach (Type t in CommandFactory.RegisteredTypes)
            {
                CommandAttribute ca = (CommandAttribute)Attribute.GetCustomAttribute(t, typeof(CommandAttribute));
                Console.WriteLine(" {0} - {1}", ca.Name, ca.Description);
            }

            Console.WriteLine();
            Console.WriteLine("Arguments are command-dependent and usage is available after:");
            Console.WriteLine("    SoodaSchemaTool help commandName");
        }
        return 0;
    }
}
}
