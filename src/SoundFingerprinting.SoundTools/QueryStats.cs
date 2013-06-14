
namespace SoundFingerprinting.SoundTools
{
    using System;

    /// <summary>
    ///   Query statistical information
    /// </summary>
    public class QueryStats
    {
        #region Private Fields

        private readonly int _endQueryIndex;
        private readonly int _startQueryIndex;

        #endregion

        /// <summary>
        ///   Public parameter less constructor
        /// </summary>
        public QueryStats()
        {
            HammingDistance = NumberOfTrackIdOccurences = NumberOfTotalTableVotes =
                                                          _startQueryIndex = _endQueryIndex = TotalFingerprints = HammingDistanceByTrack = 0;
            MaxPathThroughTables = MaxHammingDistance = MaxTableVote = Int32.MinValue;
            MinHammingDistance = Int32.MaxValue;
            OrderingValue = 0;
            Similarity = 0;
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "hammingDistance">Hamming distance</param>
        /// <param name = "numberOfTrackOccurences">Number of track id occurrences</param>
        /// <param name = "numberOfTotalVotes">Number of total table votes</param>
        /// <param name = "startQueryIndex">Start query index</param>
        /// <param name = "endQueryIndex">End query index</param>
        /// <param name = "totalFingerprints">Number of total fingerprints</param>
        public QueryStats(double hammingDistance, int numberOfTrackOccurences, int numberOfTotalVotes, int startQueryIndex, int endQueryIndex, int totalFingerprints)
        {
            HammingDistance = hammingDistance;
            NumberOfTrackIdOccurences = numberOfTrackOccurences;
            NumberOfTotalTableVotes = numberOfTotalVotes;
            _startQueryIndex = startQueryIndex;
            _endQueryIndex = endQueryIndex;
            TotalFingerprints = totalFingerprints;
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "hammingDistance">Hamming distance</param>
        /// <param name = "numberOfTrackOccurences">Number of track id occurences</param>
        /// <param name = "numberOfTotalVotes">Number of total table votes</param>
        /// <param name = "startQueryIndex">Start query index</param>
        /// <param name = "endQueryIndex">End query index</param>
        /// <param name = "totalFingerprints">Number of total fingerprints</param>
        /// <param name = "maxTableVote">Maximal table vote</param>
        /// <param name = "hammingDistanceByTrack">Hamming distance summed over track id</param>
        /// <param name = "minHammingDistance">Minimal hamming distance</param>
        /// <param name = "maxHammingDistance">Maximal hamming distance</param>
        /// <param name = "maxPathThroughTables">Maximal path through tables</param>
        /// <param name = "similarity">Similarity set</param>
        public QueryStats(double hammingDistance, int numberOfTrackOccurences, int numberOfTotalVotes, int startQueryIndex, int endQueryIndex, int totalFingerprints, int maxTableVote, int hammingDistanceByTrack, int minHammingDistance, int maxHammingDistance, int maxPathThroughTables, double similarity)
        {
            HammingDistance = hammingDistance;
            NumberOfTrackIdOccurences = numberOfTrackOccurences;
            NumberOfTotalTableVotes = numberOfTotalVotes;
            _startQueryIndex = startQueryIndex;
            _endQueryIndex = endQueryIndex;
            TotalFingerprints = totalFingerprints;
            MaxTableVote = maxTableVote;
            HammingDistanceByTrack = hammingDistanceByTrack;
            MinHammingDistance = minHammingDistance;
            MaxHammingDistance = maxHammingDistance;
            MaxPathThroughTables = maxPathThroughTables;
            Similarity = similarity;
        }

        /// <summary>
        ///   Start of the query index
        /// </summary>
        public int StartQueryIndex
        {
            get { return _startQueryIndex; }
        }

        /// <summary>
        ///   End of the query index
        /// </summary>
        public int EndQueryIndex
        {
            get { return _endQueryIndex; }
        }

        /// <summary>
        ///   Total fingeprints generated
        /// </summary>
        public int TotalFingerprints { get; set; }

        /// <summary>
        ///   Hamming distance (summed by fingerprint count)
        /// </summary>
        public double HammingDistance { get; set; }

        /// <summary>
        ///   Number of Track Id occurences
        /// </summary>
        public int NumberOfTrackIdOccurences { get; set; }

        /// <summary>
        ///   Number of total table votes
        /// </summary>
        public int NumberOfTotalTableVotes { get; set; }

        /// <summary>
        ///   Maximum table votes gathered by this specific query fingerprint
        /// </summary>
        public int MaxTableVote { get; set; }

        /// <summary>
        ///   Hamming distance by track count
        /// </summary>
        public int HammingDistanceByTrack { get; set; }

        /// <summary>
        ///   Minimal hamming distance gathered by a specific query fingerprint
        /// </summary>
        public int MinHammingDistance { get; set; }

        /// <summary>
        ///   Maximum hamming distance gathered by a specific query fingerprint
        /// </summary>
        public int MaxHammingDistance { get; set; }

        /// <summary>
        ///   Ordering value
        /// </summary>
        public double OrderingValue { get; set; }

        /// <summary>
        ///   Max path through tables (dynamic programming)
        /// </summary>
        public int MaxPathThroughTables { get; set; }

        /// <summary>
        ///   Similarity value
        /// </summary>
        public double Similarity { get; set; }
    }
}