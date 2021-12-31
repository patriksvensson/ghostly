using System;

namespace Ghostly.Core.Diagnostics
{
    public sealed class LoggingErrorHandler : IErrorHandler
    {
        private readonly IGhostlyLog _log;
        private readonly string _message;

        public LoggingErrorHandler(IGhostlyLog log, string message = null)
        {
            _log = log;
            _message = message;
        }

        public void HandleError(Exception ex)
        {
            _log.Error(ex, _message);
        }
    }
}
