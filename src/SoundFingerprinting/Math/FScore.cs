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

        public double Precision => (double)truePositives / (truePositives + falsePositives);

        public double Recall => (double)truePositives / (truePositives + falseNegatives);

        public double F1 => 2 * (Precision * Recall) / (Precision + Recall);

        public override string ToString()
        {
            return $"Precision: {Precision:0.000}, Recall: {Recall:0.000}, F1: {F1:0.000}";
        }
    }
}
