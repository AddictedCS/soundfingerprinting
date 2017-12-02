namespace SoundFingerprinting.Audio.NAudio
{
    using System.Collections.Concurrent;

    public class NAudioSoundCaptureService : ISoundCaptureService
    {
        private const int Mono = 1;
       
        private readonly INAudioFactory naudioFactory;
        private readonly ISamplesAggregator samplesAggregator;

        public NAudioSoundCaptureService() : 
            this(new SamplesAggregator(), new NAudioFactory())
        {
            // no op
        }

        internal NAudioSoundCaptureService(ISamplesAggregator samplesAggregator, INAudioFactory naudioFactory)
        {
            this.naudioFactory = naudioFactory;
            this.samplesAggregator = samplesAggregator;
        }

        public float[] ReadMonoSamples(int sampleRate, int secondsToRecord)
        {
            var producer = new BlockingCollection<float[]>();
            float[] samples;
            using (var waveIn = naudioFactory.GetWaveInEvent(sampleRate, Mono))
            {
                waveIn.DataAvailable += (sender, e) =>
                    {
                        var chunk = SamplesConverter.GetFloatSamplesFromByte(e.BytesRecorded, e.Buffer);
                        producer.Add(chunk);
                    };

                waveIn.RecordingStopped += (sender, args) => producer.CompleteAdding();

                waveIn.StartRecording();

                samples = samplesAggregator.ReadSamplesFromSource(
                    new BlockingQueueSamplesProvider(producer), secondsToRecord, sampleRate);

                waveIn.StopRecording();
            }

            return samples; 
        }
    }
}