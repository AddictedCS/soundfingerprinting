namespace SoundFingerprinting.Math
{
    internal class FScore
    {
        private readonly int truePositives;
        private readonly int trueNegatives;
        private readonly int falsePositives;
        private readonly int falseNegatives;

        public FScore(int truePositives, int trueNegatives, int falsePositives, int falseNegatives)
        {
            this.truePositives = truePositives;
            this.trueNegatives = trueNegatives;
            this.falsePositives = falsePositives;
            this.falseNegatives = falseNegatives;
        }

        public double Precision
        {
            get
            {
                return (double)truePositives / (truePositives + falsePositives);
            }
        }

        public double Recall
        {
            get
            {
                return (double)truePositives / (truePositives + falseNegatives);
            }
        }

        public double F1
        {
            get
            {
                return 2 * (Precision * Recall) / (Precision + Recall);
            }
        }

        public override string ToString()
        {
            return $"Precision: {Precision}, Recall: {Recall}, F1: {F1}";
        }
    }
}
