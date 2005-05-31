using System;

using log4netlib = log4net;

namespace Sooda.Logging.log4net
{
    public sealed class LoggingImplementation : Sooda.Logging.ILoggingImplementation
    {
        public Sooda.Logging.Logger GetLogger(string name)
        {
            return new log4netwrapper.LoggerWrapper(log4netlib.LogManager.GetLogger(name));
        }
    }
}
