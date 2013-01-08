namespace Soundfingerprinting.Audio.Models
{
    public struct AudioServiceConfiguration
    {
        public int MinFrequency { get; set; }

        public int MaxFrequency { get; set; }

        public double LogBase { get; set; }

        public int LogBins { get; set; }

        public int SampleRate { get; set; }

        public int WdftSize { get; set; }

        public int Overlap { get; set; }
    }
}
