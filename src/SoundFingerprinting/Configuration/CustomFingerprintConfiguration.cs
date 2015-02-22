namespace SoundFingerprinting.Configuration
{
    using System;

    using SoundFingerprinting.Strides;
    
    public class CustomFingerprintConfiguration : DefaultFingerprintConfiguration
    {
        public int Overlap
        {
            get
            {
                return base.Overlap;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Overlap can't be negative", "value");
                }

                base.Overlap = value;
            }
        }

        public new int WdftSize
        {
            get
            {
                return base.WdftSize;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("WdftSize can't be negative", "value");
                }

                base.WdftSize = value;
            }
        }

        public new int MinFrequency
        {
            get
            {
                return base.MinFrequency;
            }

            set
            {
                if (value <= 0 || value > 44100)
                {
                    throw new ArgumentException("MinFrequency can't be negative or bigger than 44100", "value");
                }

                base.MinFrequency = value;
            }
        }

        public new int MaxFrequency
        {
            get
            {
                return base.MaxFrequency;
            }

            set
            {
                if (value <= 0 || value > 44100)
                {
                    throw new ArgumentException("MaxFrequency can't be negative or bigger than 44100", "value");
                }

                base.MaxFrequency = value;
            }
        }

        public new int TopWavelets
        {
            get
            {
                return base.TopWavelets;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("TopWavelets can't be negative or equal to 0", "value");
                }

                base.TopWavelets = value;
            }
        }

        public new int SampleRate
        {
            get
            {
                return base.SampleRate;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("SampleRate can't be negative or equal to 0", "value");
                }

                base.SampleRate = value;
            }
        }

        public new double LogBase
        {
            get
            {
                return base.LogBase;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("LogBase can't be negative or equal to 0", "value");
                }

                base.LogBase = value;
            }
        }

        public new int LogBins
        {
            get
            {
                return base.LogBins;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("LogBins can't be negative or equal to 0", "value");
                }

                base.LogBins = value;
            }
        }

        public new int FingerprintLength
        {
            get
            {
                return base.FingerprintLength;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("FingerprintLength can't be negative or equal to 0", "value");
                }

                base.FingerprintLength = value;
            }
        }

        public new IStride Stride
        {
            get
            {
                return base.Stride;
            }

            set
            {
                base.Stride = value;
            }
        }

        public new bool NormalizeSignal
        {
            get
            {
                return base.NormalizeSignal;
            }

            set
            {
                base.NormalizeSignal = value;
            }
        }

        public new bool UseDynamicLogBase
        {
            get
            {
                return base.UseDynamicLogBase;
            }

            set
            {
                base.UseDynamicLogBase = value;
            }
        }

        public new int NumberOfLSHTables
        {
            get
            {
                return base.NumberOfLSHTables;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("NumberOfLSHTables can't be negative or equal to 0", "value");
                }

                base.NumberOfLSHTables = value;
            }
        }

        public new int NumberOfMinHashesPerTable
        {
            get
            {
                return base.NumberOfMinHashesPerTable;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("NumberOfMinHashesPerTable can't be negative or equal to 0", "value");
                }

                base.NumberOfMinHashesPerTable = value;
            }
        }
    }
}