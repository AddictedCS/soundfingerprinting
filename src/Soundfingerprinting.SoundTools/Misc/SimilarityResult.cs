namespace Soundfingerprinting.SoundTools.Misc
{
    public class SimilarityResult
    {
        public SimilarityResult()
        {
            Info = new FingerprintInformation();
            MaxJaqSimilarityBetweenDatabaseAndQuerySong = double.MinValue;
            MinJaqSimilarityBetweenDatabaseAndQuerySong = double.MaxValue;
        }

        public FingerprintInformation Info { get; set; }

        public bool ComparisonDone { get; set; }

        public double SumJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double AverageJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double MaxJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double MinJaqSimilarityBetweenDatabaseAndQuerySong { get; set; }

        public double AtLeastOneTableWillVoteForTheCandidate { get; set; }

        public double AtLeastOneHashbucketFromHashtableWillBeConsideredACandidate { get; set; }

        public double WillBecomeACandidateByPassingThreshold { get; set; }

        public int NumberOfAnalizedFingerprints { get; set; }

        public class FingerprintInformation
        {
            public string Filename { get; set; }

            public int MinFrequency { get; set; }

            public int TopWavelets { get; set; }

            public int QueryStrideSize { get; set; }

            public bool IsQueryStrideRandom { get; set; }

            public bool IsDatabaseStrideRandom { get; set; }

            public int DatabaseStrideSize { get; set; }

            public int QueryFirstStrideSize { get; set; }

            public int Iterations { get; set; }

            public int HashTables { get; set; }

            public int HashKeys { get; set; }

            public string ComparedWithFile { get; set; }

        }
    }
}