namespace SoundFingerprinting.Utils
{
    using System;

    using SoundFingerprinting.Math;

    internal class TestRunnerEventArgs : EventArgs
    {
        public FScore FScore { get; set; }

        public int Verified { get; set; }

        public object[] RowWithDetails { get; set; }
    }
}