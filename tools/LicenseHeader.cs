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
using System.IO;
using System.Collections;
using System.Collections.Specialized;

public class LicenseHeader
{
    static string mode = "check";

    static StringCollection licenseText = new StringCollection();

    public static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: LicenseHeader <license_file> add|replace|check|missing filename1 .. filenameN");
            return 1;

        }
        string licenseFile = args[0];
        mode = args[1].ToLower();

        using (StreamReader lic = File.OpenText(licenseFile))
        {
            string line;
            
            while ((line = lic.ReadLine()) != null)
            {
                licenseText.Add(line);
            }
        }

        for (int i = 2; i < args.Length; ++i)
        {
            switch (mode)
            {
                case "check":
                    CheckLicense(args[i]);
                    break;
                    
                case "remove":
                    RemoveLicense(args[i]);
                    break;

                case "replace":
                    ReplaceLicense(args[i]);
                    break;

                case "add":
                    AddLicense(args[i], false);
                    break;
            }
        }

        return 0;
    }

    private static void RemoveLicense(TextWriter output, string fileName)
    {
        using (StreamReader input = File.OpenText(fileName))
        {
            string line = input.ReadLine();

            if (line.Trim() == "/*")
            {
                while ((line = input.ReadLine()) != null)
                {
                    line = line.TrimStart();
                    if (line.StartsWith("*/"))
                        break;
                }
                
                while ((line = input.ReadLine()) != null)
                {
                    if (line.TrimStart().Length != 0)
                        break;
                }
            }
            else if (line.TrimStart().StartsWith("//"))
            {
                while ((line = input.ReadLine()) != null)
                {
                    if (!line.TrimStart().StartsWith("//"))
                        break;
                }
                
                if (line.TrimStart().Length == 0)
                {
                    while ((line = input.ReadLine()) != null)
                    {
                        if (line.TrimStart().Length != 0)
                            break;
                    }
                }
            }

            output.WriteLine(line);
            output.Write(input.ReadToEnd());
            output.Flush();
        }
    }

    private static void RemoveLicense(string fileName)
    {
        string tmpFile = fileName + ".tmp";
        string bakFile = Path.ChangeExtension(fileName, ".bak");

        using (StreamWriter sw = File.CreateText(tmpFile))
        {
            RemoveLicense(sw, fileName);
        }

        if (File.Exists(bakFile))
            File.Delete(bakFile);
        
        if (FilesDiffer(fileName, tmpFile))
        {
            File.Move(fileName, bakFile);
            File.Move(tmpFile, fileName);
            Console.WriteLine("{1,20} {0}", fileName, "REMOVED");
        }
        else
        {
            File.Delete(tmpFile);
        }
    }
    
    private static void ReplaceLicense(string fileName)
    {
        string tmpFile = fileName + ".tmp";
        string bakFile = Path.ChangeExtension(fileName, ".bak");

        using (StreamWriter sw = File.CreateText(tmpFile))
        {
            RemoveLicense(sw, fileName);
        }
        AddLicense(tmpFile, true);

        if (File.Exists(bakFile))
            File.Delete(bakFile);
        
        if (FilesDiffer(fileName, tmpFile))
        {
            File.Move(fileName, bakFile);
            File.Move(tmpFile, fileName);
            Console.WriteLine("{1,20} {0}", fileName, "REPLACED");
        }
        else
        {
            File.Delete(tmpFile);
        }
    }
    
    private static void CheckLicense(string fileName)
    {
        StringCollection foundLicenseText = new StringCollection();

        using (StreamReader input = File.OpenText(fileName))
        {
            string line = input.ReadLine();

            if (line.Trim() == "/*")
            {
                while ((line = input.ReadLine()) != null)
                {
                    line = line.TrimStart();
                    if (line.StartsWith("*/"))
                        break;
                    foundLicenseText.Add(line);
                }
            }
            else if (line.TrimStart().StartsWith("//"))
            {
                while ((line = input.ReadLine()) != null)
                {
                    if (!line.TrimStart().StartsWith("//"))
                        break;

                    foundLicenseText.Add(line.TrimStart(' ', '/'));
                }
            }
        }

        Hashtable found = new Hashtable();
        int lineCount = 0;

        foreach (string s in licenseText)
        {
            string s2 = s.Trim();
            if (s2 != "")
            {
                found.Add(s2, s2);
                lineCount++;
            }
        }

        foreach (string s in foundLicenseText)
        {
            string s2 = s.Trim();
            if (s2 != "")
            {    
                if (found.Contains(s2))
                    found.Remove(s2);
            }
        }

        string status;

        if (found.Count == 0)
            status = "FOUND";
        else if (lineCount > 8)
        {
            if (found.Count < lineCount / 2)
            {
                status = "FOUND_MODIFIED(" + found.Count + ")";
            }
            else
            {
                status = "NOT_FOUND";
            }
        }
        else
        {
            status = "UNKNOWN";
        }

        Console.WriteLine("{1,20} {0}", fileName, status);
    }
    
    private static void AddLicense(TextWriter output, string fileName)
    {
        foreach (string s in licenseText)
        {
            output.WriteLine("// {0}", s);
        }
        output.WriteLine();
        using (StreamReader input = File.OpenText(fileName))
        {
            output.Write(input.ReadToEnd());
        }
        output.Flush();
    }
    
    private static void AddLicense(string fileName, bool quiet)
    {
        string tmpFile = fileName + ".tmp";
        string bakFile = Path.ChangeExtension(fileName, ".bak");

        using (StreamWriter sw = File.CreateText(tmpFile))
        {
            AddLicense(sw, fileName);
        }

        if (File.Exists(bakFile))
            File.Delete(bakFile);

        if (FilesDiffer(fileName, tmpFile))
        {
            if (!quiet)
                File.Move(fileName, bakFile);
            else
                File.Delete(fileName);
            File.Move(tmpFile, fileName);
            if (!quiet)
                Console.WriteLine("{1,20} {0}", fileName, "ADDED");
        }
        else
        {
            File.Delete(tmpFile);
        }
    }

    private static bool FilesDiffer(string fn1, string fn2)
    {
        string contents1;
        string contents2;
        using (StreamReader sr = File.OpenText(fn1))
        {
            contents1 = sr.ReadToEnd();
        }
        using (StreamReader sr = File.OpenText(fn2))
        {
            contents2 = sr.ReadToEnd();
        }
        return contents1 != contents2;
    }
}

