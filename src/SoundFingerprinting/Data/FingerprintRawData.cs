namespace SoundFingerprinting.Data
{
    using System;

    [Serializable]
    public class FingerprintRawData
    {
        public FingerprintRawData(bool[] fingerprint, double begin, double end)
        {
            Fingerprint = fingerprint;
            Begin = begin;
            End = end;
        }

        public bool[] Fingerprint { get; set; }

        public double Begin { get; set; }

        public double End { get; set; }
    }
}