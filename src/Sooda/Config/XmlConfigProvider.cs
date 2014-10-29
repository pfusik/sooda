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

using System;
using System.IO;
using System.Xml;
using System.Security;
using System.Configuration;
using System.Collections.Specialized;

using Sooda.Logging;

namespace Sooda.Config
{
    public class XmlConfigProvider : ISoodaConfigProvider
    {
        private static readonly Logger logger = LogManager.GetLogger("Sooda.Config");
        private readonly NameValueCollection dataDictionary = new NameValueCollection();
        private string fileName;

        public XmlConfigProvider()
            : this("Sooda.config.xml")
        {
        }

        public XmlConfigProvider(string fileName)
        {
            this.fileName = fileName;
            LoadFromFile(fileName, null);
            FindAndLoadOverride();
        }

        public XmlConfigProvider(string fileName, string xpathExpression)
        {
            this.fileName = fileName;
            LoadFromFile(fileName, xpathExpression);
            FindAndLoadOverride();
        }

        public void Clear()
        {
            dataDictionary.Clear();
        }

        public void LoadFromFile(string fileName, string xpathExpression)
        {
            LoadFromFile(fileName, xpathExpression, "");
        }

        private void LoadFromFile(string fileName, string xpathExpression, string prefix)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode startingNode = doc.DocumentElement;

            if (xpathExpression != null)
                startingNode = doc.SelectSingleNode(xpathExpression);

            LoadFromNode(startingNode, prefix, Path.GetDirectoryName(fileName));
        }

        private void LoadFromNode(XmlNode startingNode, string prefix, string baseDir)
        {
            for (XmlNode node = startingNode.FirstChild;
                    node != null;
                    node = node.NextSibling)
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.FirstChild != null && node.FirstChild.NodeType == XmlNodeType.Element)
                    {
                        string newPrefix;

                        if (prefix.Length == 0)
                            newPrefix = node.Name + ".";
                        else
                            newPrefix = prefix + "." + node.Name + ".";
                        LoadFromNode(node, newPrefix, baseDir);
                    }
                    else
                    {
                        string keyName = prefix + node.Name;

                        string value = ProcessValue(node.InnerXml, baseDir);
                        logger.Debug("{0}={1}", keyName, value);
                        dataDictionary[keyName] = value;
                    }
                }
        }

        public void FindAndLoadOverride()
        {
            FileInfo fi = new FileInfo(fileName);
            string originalExtension = fi.Extension;

            string newExtension = ".site" + originalExtension;
            string newFileName = Path.ChangeExtension(fileName, newExtension);

            if (File.Exists(newFileName))
            {
                logger.Debug("Loading config override from file: " + newFileName);
                LoadFromFile(newFileName, null);
            }
            else
            {
                logger.Debug("Config override file " + newFileName + " not found");
            }

            newExtension = "." + GetMachineName().ToLower(System.Globalization.CultureInfo.CurrentCulture) + originalExtension;
            newFileName = Path.ChangeExtension(fileName, newExtension);

            if (File.Exists(newFileName))
            {
                logger.Debug("Loading config override from file: " + newFileName);
                LoadFromFile(newFileName, null);
            }
            else
            {
                logger.Debug("Config override file " + newFileName + " not found");
            }

            OverrideFromAppConfig();
        }

        public string GetMachineName()
        {
            string overrideMachineName = ConfigurationManager.AppSettings["sooda.hostname"];
            if (overrideMachineName != null)
                return overrideMachineName;

            return Environment.MachineName;
        }

        private string ProcessValue(string s, string baseDir)
        {
            s = s.Replace("${CONFIGDIR}", baseDir);
            // Console.WriteLine("After replacement: {0}", s);
            return s;
        }

        public string GetString(string key)
        {
            return dataDictionary[key];
        }

        public static XmlConfigProvider FindConfigFile(string fileName)
        {
            return FindConfigFile(fileName, 10);
        }

        private static XmlConfigProvider TryFindConfigFile(string fileName, int maxParentDirectories)
        {
            FileInfo fi;
            int depth;

            try
            {
                fi = new FileInfo(fileName);
                depth = maxParentDirectories;
                for (DirectoryInfo di = fi.Directory; (di != null) && (depth > 0); di = di.Parent, depth--)
                {
                    string targetFileName = Path.Combine(di.FullName, fi.Name);
                    //logger.Debug("Checking for " + targetFileName);
                    if (File.Exists(targetFileName))
                        return new XmlConfigProvider(targetFileName);
                }
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                // ignore
                return null;
            }
        }

        public static XmlConfigProvider FindConfigFile(string fileName, int maxParentDirectories)
        {
            //
            // Path.Combine will either take the file name (if it's rooted) or make a relative
            // to the specified base directory
            //

            try
            {
                XmlConfigProvider retVal = TryFindConfigFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), maxParentDirectories);
                if (retVal != null)
                    return retVal;
            }
            catch (SecurityException e)
            {
                // ignore
                logger.Debug("SecurityException when scanning AppDomain.BaseDirectory", e);
            }

            try
            {
                XmlConfigProvider retVal = TryFindConfigFile(Path.Combine(Directory.GetCurrentDirectory(), fileName), maxParentDirectories);
                if (retVal != null)
                    return retVal;
            }
            catch (SecurityException e)
            {
                // ignore
                logger.Debug("SecurityException when scanning Directory.GetCurrentDirectory()", e);
            }

            throw new SoodaConfigException("Config file not found in " + fileName + " and " + maxParentDirectories + " parent directories");
        }

        private void OverrideFromAppConfig()
        {
            foreach (string s in ConfigurationManager.AppSettings.Keys)
            {
                dataDictionary[s] = ConfigurationManager.AppSettings[s];
            }
        }
    }
}
