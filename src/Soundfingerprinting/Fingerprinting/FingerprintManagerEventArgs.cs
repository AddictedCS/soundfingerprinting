namespace Soundfingerprinting.Fingerprinting
{
    using System;

    public class FingerprintManagerEventArgs : EventArgs
    {
        private readonly Exception _unhandledException;

        public FingerprintManagerEventArgs(Exception exception)
        {
            _unhandledException = exception;
        }

        public Exception UnhandledException
        {
            get { return _unhandledException; }
        }
    }
}