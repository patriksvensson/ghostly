using System;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;

namespace Ghostly.Uwp.Infrastructure
{
    public interface IExtendedExecutionService
    {
        Task<bool> RequestSession();
        Deferral GetDeferral();
    }

    public sealed class ExtendedExecutionService : IExtendedExecutionService
    {
        private readonly IGhostlyLog _log;
        private ExtendedExecutionSession _session;
        private int _count;

        public ExtendedExecutionService(IGhostlyLog log)
        {
            _log = log;
        }

        public async Task<bool> RequestSession()
        {
            if (_count > 0)
            {
                _log.Debug("There already is an active extended execution session.");
                return true;
            }

            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            newSession.Description = "Background tasks used by Ghostly.";
            newSession.Revoked += SessionRevoked;

            var result = await newSession.RequestExtensionAsync();
            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    _log.Debug("Extended execution session was allowed.");
                    _session = newSession;
                    return true;
                default:
                case ExtendedExecutionResult.Denied:
                    _log.Error("Extended execution session was denied.");
                    newSession.Dispose();
                    return false;
            }
        }

        public Deferral GetDeferral()
        {
            if (_session == null)
            {
                _log.Error("Can not get deferral since there is no active session.");
                throw new InvalidOperationException("There is no active session.");
            }

            _count++;
            _log.Debug("Getting deferral. Count={DeferralCount}", _count);
            return new Deferral(OnDeferralCompleted);
        }

        private void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            _log.Warning("Session was revoked: {ExtendedExecutionRevokedReason}", args.Reason);
            CloseSession();
        }

        private void OnDeferralCompleted()
        {
            _count--;
            _log.Debug("Deferral completed. Count={DeferralCount}", _count);
            if (_count <= 0)
            {
                CloseSession();
            }
        }

        private void CloseSession()
        {
            _count = 0;
            if (_session != null)
            {
                _log.Debug("Closing session.");
                _session.Dispose();
                _session = null;
            }
        }
    }
}
