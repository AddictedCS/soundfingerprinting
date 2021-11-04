namespace SoundFingerprinting.Configuration.Frames
{
    public class OutliersFilterConfiguration
    {
        public static OutliersFilterConfiguration None => new OutliersFilterConfiguration();

        public static OutliersFilterConfiguration Default => new OutliersFilterConfiguration(1.0d);

        public static OutliersFilterConfiguration Custom(double outlierSigma) => new OutliersFilterConfiguration(outlierSigma);

        private OutliersFilterConfiguration(double outlierSigma)
        {
            Enabled = true;
            OutlierSigma = outlierSigma;
        }

        private OutliersFilterConfiguration()
        {
            // no op
        }
        
        public bool Enabled { get; }

        public double OutlierSigma { get; }
    }
}
