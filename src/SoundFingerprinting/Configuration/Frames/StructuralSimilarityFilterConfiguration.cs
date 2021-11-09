namespace SoundFingerprinting.Configuration.Frames
{
    public class StructuralSimilarityFilterConfiguration
    {
        public static StructuralSimilarityFilterConfiguration None => new StructuralSimilarityFilterConfiguration();
        
        public static StructuralSimilarityFilterConfiguration GaussianDiff => 
            new StructuralSimilarityFilterConfiguration(differenceThreshold: 235,
                areaThreshold: 30,
                kernelSize: 5,
                sigma: 1.5,
                borderWidth: 5,
                minDeltaX: 1,
                minDeltaY: 1,
                SsimImplementation.GaussianDiff);
        
        public static StructuralSimilarityFilterConfiguration NaiveDiff => 
            new StructuralSimilarityFilterConfiguration(differenceThreshold: 235, 
                areaThreshold: 30, 0, 0, 
                borderWidth: 5, 
                minDeltaX: 1, 
                minDeltaY: 1, 
                SsimImplementation.NaiveDiff);
        
        public static StructuralSimilarityFilterConfiguration SSIM => 
            new StructuralSimilarityFilterConfiguration(differenceThreshold: 235, 
                areaThreshold: 30,
                kernelSize: 5,
                sigma: 1.5,
                borderWidth: 5,
                minDeltaX: 1,
                minDeltaY: 1, SsimImplementation.SSIM);

        public StructuralSimilarityFilterConfiguration(int differenceThreshold, int areaThreshold, int kernelSize, double sigma, int borderWidth, int minDeltaX, int minDeltaY, SsimImplementation implementation)
        {
            DifferenceThreshold = differenceThreshold;
            AreaThreshold = areaThreshold;
            KernelSize = kernelSize;
            Sigma = sigma;
            BorderWidth = borderWidth;
            MinDeltaX = minDeltaX;
            MinDeltaY = minDeltaY;
            Implementation = implementation;
        }

        private StructuralSimilarityFilterConfiguration()
        {
            Implementation = SsimImplementation.None;
        }

        public int DifferenceThreshold { get; }

        public int AreaThreshold { get; }

        public int KernelSize { get; }

        public double Sigma { get; }

        public int BorderWidth { get; }

        public int MinDeltaX { get; }

        public int MinDeltaY { get; }

        public SsimImplementation Implementation { get; }
    }
}
