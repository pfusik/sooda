import Sooda
import Sooda.Config
import Sooda.UnitTests.BaseObjects.Boo
import System

SoodaConfig.SetConfigProvider(XmlConfigProvider.FindConfigFile("sooda.config.xml"))

using t = SoodaTransaction(typeof(_DatabaseSchema).Assembly):
    for i as Contact in Contact.GetList(true):
        Console.WriteLine(i.Name)
