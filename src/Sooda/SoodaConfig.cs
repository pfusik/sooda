// 
// Copyright (c) 2002-2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using System.Reflection;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Data;

using Sooda.Config;

using Sooda.Logging;

namespace Sooda {
    public sealed class SoodaConfig {
        private static ISoodaConfigProvider configProvider = null;
        private static Logger logger = LogManager.GetLogger("Sooda.Config");

        static SoodaConfig()
        {
            try
            {
                Assembly a = Assembly.GetEntryAssembly();
                if (a != null)
                {
                    SetConfigProviderFromAttribute((SoodaConfigAttribute)Attribute.GetCustomAttribute(a, typeof(SoodaConfigAttribute), false));
                }

                if (configProvider == null)
                {
                    try {
#if DOTNET2
                        string typeName = System.Configuration.ConfigurationManager.AppSettings["sooda.config"];
#else
                        string typeName = System.Configuration.ConfigurationSettings.AppSettings["sooda.config"];
#endif
                        // Console.WriteLine("typeName: {0}", typeName);
                        if (typeName == "xmlconfig")
                        {
#if DOTNET2
                            string xmlconfigfile = System.Configuration.ConfigurationManager.AppSettings["sooda.xmlconfigfile"];
#else
                            string xmlconfigfile = System.Configuration.ConfigurationSettings.AppSettings["sooda.xmlconfigfile"];
#endif
                            if (xmlconfigfile == null)
                                xmlconfigfile = "sooda.config.xml";
                            SetConfigProvider(XmlConfigProvider.FindConfigFile(xmlconfigfile));
                        }
                        else if (typeName != null) {
                            Type t = Type.GetType(typeName);
                            SetConfigProvider(Activator.CreateInstance(t) as ISoodaConfigProvider);
                        }
                    }
                    catch (Exception e) {
                        throw new SoodaConfigException(String.Format("Error while loading configuration provider {0}", e));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SoodaConfigException(String.Format("Error while loading configuration provider {0}", ex));
            }
            finally
            {

                if (configProvider == null) {
                    SetConfigProvider(new AppSettingsConfigProvider());
                }
            }
        }

        public static void SetConfigProvider(ISoodaConfigProvider provider) {
            if (provider == null)
                throw new ArgumentException("provider");
            logger.Debug("Setting config provider to " + provider.GetType().FullName);
            configProvider = provider;
        }

        private static void SetConfigProviderFromAttribute(SoodaConfigAttribute at)
        {
            if (at == null)
                return;

            if (at.XmlConfigFileName != null) {
                SetConfigProvider(XmlConfigProvider.FindConfigFile(at.XmlConfigFileName));
                return ;
            }
            if (at.ProviderType != null) {
                SetConfigProvider(Activator.CreateInstance(at.ProviderType) as ISoodaConfigProvider);
                return ;
            }
        }
        
        public static string GetString(string itemName) {
            return GetString(itemName, null);
        }

        public static string GetString(string itemName, string defVal) {
            string s = configProvider.GetString(itemName);
            string retVal = defVal;

            if (s != null)
                retVal = s;

            return retVal;
        }
    }
}
