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

public class NewBuild {
    public static int Main(string[] args) {
        try {
            if (args.Length < 1) {
                Console.WriteLine("Usage: NewBuild.exe <version_filename> [minor|minor|revision|build]");
                return 1;
            }
            string fileName = args[0];
            string buildMode = "build";
            if (args.Length > 1) {
                buildMode = args[1];
            };

            string oldVersion;

            using (StreamReader sr = new StreamReader(fileName)) {
                oldVersion = sr.ReadLine();
            };

            string[] parts = oldVersion.Split('.');
            if (parts.Length != 4) {
                Console.WriteLine("Invalid build number: {0}", oldVersion);
                return 1;
            }
            switch (buildMode) {
            default:
            case "build":
                parts[3] = (Convert.ToInt32(parts[3]) + 1).ToString();
                break;

            case "major":
                parts[0] = (Convert.ToInt32(parts[0]) + 1).ToString();
                parts[1] = "0";
                parts[2] = "0";
                parts[3] = (Convert.ToInt32(parts[3]) + 1).ToString();
                break;

            case "minor":
                parts[1] = (Convert.ToInt32(parts[1]) + 1).ToString();
                parts[2] = "0";
                parts[3] = (Convert.ToInt32(parts[3]) + 1).ToString();
                break;

            case "revision":
                parts[2] = (Convert.ToInt32(parts[2]) + 1).ToString();
                parts[3] = (Convert.ToInt32(parts[3]) + 1).ToString();
                break;
            };

            string newVersion = String.Join(".", parts);
            Console.WriteLine("{0}: {1} => {2}", Path.GetFileName(fileName), oldVersion, newVersion);

            using (StreamWriter sw = new StreamWriter(fileName)) {
                sw.WriteLine(newVersion);
            }
            return 0;
        } catch (Exception e) {
            Console.WriteLine("Error: " + e.Message);
            return 1;

        }

    }
}


