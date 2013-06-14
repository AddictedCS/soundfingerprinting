namespace SoundFingerprinting.Hashing.NeuralHashing.NeuralTrainer
{
    using System;
    using System.Threading;

    public class TrainingAsyncResult : IAsyncResult
    {
        private readonly object _asyncState;

        public TrainingAsyncResult(object asyncState, bool isCompleted)
        {
            _asyncState = asyncState;
            IsCompleted = isCompleted;
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public bool CompletedSynchronously
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsCompleted { get; set; }

        #endregion
    }
}