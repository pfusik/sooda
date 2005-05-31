using System;
using NLogLib = NLog;

namespace Sooda.Logging.NLog
{
    
public sealed class LoggingImplementation : Sooda.Logging.ILoggingImplementation
{
    public Sooda.Logging.Logger GetLogger(string name)
    {
        return new NLogLib.LoggerWrapper(NLogLib.LogManager.GetLogger(name));
    }
}
}
