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

namespace Sooda {
    public sealed class SoodaConfig {
        public delegate string ExpandVariable(string variable);

        private static ISoodaConfigProvider configProvider = null;
        private static NLog.Logger logger = NLog.LogManager.GetLogger("Sooda.Config");

        public static ExpandVariable ExpanderDelegate = null;

        static SoodaConfig() {
            try {
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (a.IsDefined(typeof(SoodaConfigAttribute), false)) {
                        SoodaConfigAttribute[] attrs = (SoodaConfigAttribute[])a.GetCustomAttributes(typeof(SoodaConfigAttribute), false);
                        logger.Debug("Found SoodaConfigAttribute in " + a.FullName);
                        foreach (SoodaConfigAttribute at in attrs) {
                            if (at.XmlConfigFileName != null) {
                                // Console.WriteLine("fname: {0}", at.XmlConfigFileName);
                                SetConfigProvider(XmlConfigProvider.FindConfigFile(at.XmlConfigFileName));
                                return ;
                            }
                            if (at.ProviderType != null) {
                                CreateConfigProvider(at.ProviderType);
                                return ;
                            }
                        }
                    }
                }
            } catch (Exception e) {
                logger.Debug("Error while scanning for SoodaConfigAttribute: {0}", e);
            }

            try {
                string s = System.Configuration.ConfigurationSettings.AppSettings["SoodaConfigClass"];
                if (s != null) {}
            }
            catch (Exception e) {
                logger.Error("Error while loading configuration provider {0}", e);
            }

            if (configProvider == null) {
                logger.Debug("Using default AppSettingsConfigProvider");
                configProvider = new AppSettingsConfigProvider();
            }
        }

        public static void CreateConfigProvider(string typeName) {
            logger.Debug("Loading configuration class: " + typeName);
            Type t = Type.GetType(typeName);
            CreateConfigProvider(t);
        }

        public static void CreateConfigProvider(Type t) {
            logger.Debug("Creating ConfigProvider");
            SetConfigProvider(Activator.CreateInstance(t) as ISoodaConfigProvider);
        }

        public static void SetConfigProvider(ISoodaConfigProvider provider) {
            if (provider == null)
                throw new ArgumentException("provider");
            logger.Debug("Setting config provider to " + provider.GetType().FullName);
            configProvider = provider;
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
