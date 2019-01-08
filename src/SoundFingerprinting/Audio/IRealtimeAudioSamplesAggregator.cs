namespace SoundFingerprinting.Audio
{
    using SoundFingerprinting.Strides;

    public interface IRealtimeAudioSamplesAggregator
    {
        AudioSamples Aggregate(AudioSamples chunk);
        
        IStride Stride { get; }
        
        int MinSize { get; }
    }
}