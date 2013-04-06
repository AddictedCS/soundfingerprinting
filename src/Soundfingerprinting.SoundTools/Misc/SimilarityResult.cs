namespace Soundfingerprinting.SoundTools.Misc
{
    public class SimilarityResult
    {
        public SimilarityResult()
        {
            Info = new FingerprintInformatio();
        }

        public FingerprintInformatio Info { get; set; }

        public bool ComparisonDone { get; set; }

        public double SumJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double AverageJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double MaxJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double MinJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double AtLeastOneTableWillVoteForTheCandidate { get; set; }

        public double AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate { get; set; }

        public double WillBecomeACandidateByPassingThreshold { get; set; }

        public int NumberOfAnalizedFingerprints { get; set; }

        public class FingerprintInformatio
        {
            public string Filename { get; set; }

            public int MinFrequency { get; set; }

            public int TopWavelets { get; set; }

            public int StrideSize { get; set; }

            public bool RandomStride { get; set; }
        }
    }
}