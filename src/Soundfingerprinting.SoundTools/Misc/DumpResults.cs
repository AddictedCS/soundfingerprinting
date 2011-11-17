// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Xml.Serialization;

namespace Soundfingerprinting.SoundTools.Misc
{
    /// <summary>
    ///   Dump results
    /// </summary>
    [XmlRoot("Results")]
    public class DumpResults
    {
        public DumpResults()
        {
            Results = new Results();
            Info = new Info();
        }

        /// <summary>
        ///   Info about dumped object
        /// </summary>
        [XmlElement("Information")]
        public Info Info { get; set; }

        /// <summary>
        ///   Results
        /// </summary>
        [XmlElement("Results")]
        public Results Results { get; set; }

        /// <summary>
        ///   Comparison done
        /// </summary>
        public bool ComparisonDone { get; set; }

        /// <summary>
        ///   Sum of the fingerprint similarity between 2 different analyzed songs
        /// </summary>
        public double SumJaqFingerprintSimilarityBetweenDiffertSongs { get; set; }

        /// <summary>
        ///   Average fingerprint similarity between 2 different analyzed songs
        /// </summary>
        public double AverageJaqFingerprintsSimilarityBetweenDifferentSongs { get; set; }

        public double MaxJaqFingerprintsSimilarityBetweenDifferentSongs { get; set; }
    }

    /// <summary>
    ///   Information class
    /// </summary>
    public class Info
    {
        /// <summary>
        ///   Name of the analyzed file
        /// </summary>
        [XmlAttribute("File")]
        public string Filename { get; set; }

        /// <summary>
        ///   Fingerprint Managers minimal frequency value
        /// </summary>
        public int MinFrequency { get; set; }

        /// <summary>
        ///   Number of extracted Top Wavelets
        /// </summary>
        public int TopWavelets { get; set; }

        /// <summary>
        ///   Size of the stride
        /// </summary>
        public int StrideSize { get; set; }

        /// <summary>
        ///   Determines whether the stride was random or not
        /// </summary>
        public bool RandomStride { get; set; }
    }

    /// <summary>
    ///   Results class
    /// </summary>
    public class Results
    {
        /// <summary>
        ///   Summation over fingerprint [0 : 8191] Jacquard similarity
        /// </summary>
        public double SumJaqFingerprintsSimilarity { get; set; }

        /// <summary>
        ///   Average fingerprint [0 : 8191] similarity
        /// </summary>
        public double AverageJaqFingerprintSimilarity { get; set; }

        public double MaxJaqFingerprintSimilarity { get; set; }

        /// <summary>
        ///   Sum over min hash signature [0 : 100] similarity
        /// </summary>
        public double SumIdenticalMinHash { get; set; }

        /// <summary>
        ///   Average min hash signature [0 : 100] similarity
        /// </summary>
        public double AverageIdenticalMinHash { get; set; }

        /// <summary>
        ///   Sum over LSH bucket similarity
        /// </summary>
        public double SumJaqLSHBucketSimilarity { get; set; }

        /// <summary>
        ///   Average over LSH bucket similarity
        /// </summary>
        public double AverageJaqLSHBucketSimilarity { get; set; }

        /// <summary>
        ///   Total identical LSH buckets
        /// </summary>
        public int TotalIdenticalLSHBuckets { get; set; }
    }
}