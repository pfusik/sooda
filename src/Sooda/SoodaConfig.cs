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

using Sooda.Config;
using System;
using System.Reflection;

namespace Sooda
{
    public sealed class SoodaConfig
    {
        private static ISoodaConfigProvider configProvider = null;

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
                    try
                    {
                        string typeName = System.Configuration.ConfigurationManager.AppSettings["sooda.config"];
                        // Console.WriteLine("typeName: {0}", typeName);
                        if (typeName == "xmlconfig")
                        {
                            string xmlconfigfile = System.Configuration.ConfigurationManager.AppSettings["sooda.xmlconfigfile"];
                            if (xmlconfigfile == null)
                                xmlconfigfile = "sooda.config.xml";
                            SetConfigProvider(XmlConfigProvider.FindConfigFile(xmlconfigfile));
                        }
                        else if (typeName != null)
                        {
                            Type t = Type.GetType(typeName);
                            SetConfigProvider(Activator.CreateInstance(t) as ISoodaConfigProvider);
                        }
                    }
                    catch (Exception e)
                    {
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

                if (configProvider == null)
                {
                    SetConfigProvider(new AppSettingsConfigProvider());
                }
            }
        }

        public static void SetConfigProvider(ISoodaConfigProvider provider)
        {
            if (provider == null)
                throw new ArgumentException("provider");
            configProvider = provider;
        }

        private static void SetConfigProviderFromAttribute(SoodaConfigAttribute at)
        {
            if (at == null)
                return;

            if (at.XmlConfigFileName != null)
            {
                SetConfigProvider(XmlConfigProvider.FindConfigFile(at.XmlConfigFileName));
                return;
            }
            if (at.ProviderType != null)
            {
                SetConfigProvider(Activator.CreateInstance(at.ProviderType) as ISoodaConfigProvider);
                return;
            }
        }

        public static string GetString(string itemName)
        {
            return GetString(itemName, null);
        }

        public static string GetString(string itemName, string defVal)
        {
            string s = configProvider.GetString(itemName);
            string retVal = defVal;

            if (s != null)
                retVal = s;

            return retVal;
        }
    }
}
