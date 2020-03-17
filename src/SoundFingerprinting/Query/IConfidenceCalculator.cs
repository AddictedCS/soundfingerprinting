using SoundFingerprinting.LCS;

namespace SoundFingerprinting.Query
{
    public interface IConfidenceCalculator
    {
        double CalculateConfidence(Coverage coverage);
    }
}