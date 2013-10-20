namespace SoundFingerprinting.NeuralHasher.NeuralTrainer
{
    using System;
    using System.Threading;

    public class TrainingAsyncResult : IAsyncResult
    {
        private readonly object asyncState;

        public TrainingAsyncResult(object asyncState, bool isCompleted)
        {
            this.asyncState = asyncState;
            IsCompleted = isCompleted;
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return asyncState; }
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