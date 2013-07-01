namespace SoundFingerprinting
{
    using SoundFingerprinting.Builder;

    public interface IFingerprintUnitBuilder
    {
        ISourceFrom BuildFingerprints();
    }
}