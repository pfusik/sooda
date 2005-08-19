using System;

using log4net;

namespace log4netwrapper
{
    internal sealed class LoggerWrapper : Sooda.Logging.Logger
    {
        private ILog _logger;

        public LoggerWrapper(ILog logger)
        {
            _logger = logger;
        }

        // log4net has no trace so we map Trace to Debug

        #region Trace

        public override bool IsTraceEnabled 
        {
            get { return _logger.IsDebugEnabled; }
        }

        public override void Trace(IFormatProvider fp, string format, params object[] par)
        {
            _logger.DebugFormat(fp, format, par);
        }

        public override void Trace(string format, params object[] par)
        {
            _logger.DebugFormat(format, par);
        }

        public override void Trace(string message)
        {
            _logger.Debug(message);
        }

        #endregion

        #region Debug

        public override bool IsDebugEnabled 
        {
            get { return _logger.IsDebugEnabled; }
        }

        public override void Debug(IFormatProvider fp, string format, params object[] par)
        {
            _logger.DebugFormat(fp, format, par);
        }

        public override void Debug(string format, params object[] par)
        {
            _logger.DebugFormat(format, par);
        }

        public override void Debug(string message)
        {
            _logger.Debug(message);
        }

        #endregion

        #region Info

        public override bool IsInfoEnabled 
        {
            get { return _logger.IsInfoEnabled; }
        }

        public override void Info(IFormatProvider fp, string format, params object[] par)
        {
            _logger.InfoFormat(fp, format, par);
        }

        public override void Info(string format, params object[] par)
        {
            _logger.InfoFormat(format, par);
        }

        public override void Info(string message)
        {
            _logger.Info(message);
        }

        #endregion

        #region Warn

        public override bool IsWarnEnabled 
        {
            get { return _logger.IsWarnEnabled; }
        }

        public override void Warn(IFormatProvider fp, string format, params object[] par)
        {
            _logger.WarnFormat(fp, format, par);
        }

        public override void Warn(string format, params object[] par)
        {
            _logger.WarnFormat(format, par);
        }

        public override void Warn(string message)
        {
            _logger.Warn(message);
        }

        #endregion

        #region Error

        public override bool IsErrorEnabled 
        {
            get { return _logger.IsErrorEnabled; }
        }

        public override void Error(IFormatProvider fp, string format, params object[] par)
        {
            _logger.ErrorFormat(fp, format, par);
        }

        public override void Error(string format, params object[] par)
        {
            _logger.ErrorFormat(format, par);
        }

        public override void Error(string message)
        {
            _logger.Error(message);
        }

        #endregion

        #region Fatal

        public override bool IsFatalEnabled 
        {
            get { return _logger.IsFatalEnabled; }
        }

        public override void Fatal(IFormatProvider fp, string format, params object[] par)
        {
            _logger.FatalFormat(fp, format, par);
        }

        public override void Fatal(string format, params object[] par)
        {
            _logger.FatalFormat(format, par);
        }

        public override void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        #endregion
    }
}
