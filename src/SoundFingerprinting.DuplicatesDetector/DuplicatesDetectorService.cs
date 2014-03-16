namespace SoundFingerprinting.DuplicatesDetector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Strides;

    public class DuplicatesDetectorService
    {
        private const int MinimumHammingSimilarity = 0;

        private const int ThresholdVotes = 5;

        private readonly IStride createStride = new IncrementalRandomStride(512, 1024, 128 * 64, 0);

        private readonly IModelService modelService;

        private readonly IAudioService audioService;

        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;

        private readonly IQueryFingerprintService queryFingerprintService;

        public DuplicatesDetectorService(IModelService modelService, IAudioService audioService, IFingerprintCommandBuilder fingerprintCommandBuilder, IQueryFingerprintService queryFingerprintService)
        {
            this.modelService = modelService;
            this.audioService = audioService;
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.queryFingerprintService = queryFingerprintService;
        }

        /// <summary>
        ///   Create fingerprints out of down sampled samples
        /// </summary>
        /// <param name = "samples">Down sampled to 5512 samples</param>
        /// <param name = "track">Track</param>
        public void CreateInsertFingerprints(float[] samples, TrackData track)
        {
            if (track == null)
            {
                return; /*track is not eligible*/
            }

            var trackReference = modelService.InsertTrack(track);
           
            /*Create fingerprints that will be used as initial fingerprints to be queried*/
            var hashes = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                       .From(samples)
                                                       .WithFingerprintConfig(config => config.Stride = createStride)
                                                       .UsingServices(audioService)
                                                       .Hash()
                                                       .Result;
           
            modelService.InsertHashDataForTrack(hashes, trackReference);
        }

        /// <summary>
        ///   Find duplicates between existing tracks in the database
        /// </summary>
        /// <param name = "callback">Callback invoked at each processed track</param>
        /// <returns>Sets of duplicates</returns>
        public HashSet<TrackData>[] FindDuplicates(Action<TrackData, int, int> callback)
        {
            var tracks = modelService.ReadAllTracks();
            var duplicates = new List<HashSet<TrackData>>();
            int total = tracks.Count, current = 0;
            var queryConfiguration = new CustomQueryConfiguration { ThresholdVotes = ThresholdVotes, MaximumNumberOfTracksToReturnAsResult = int.MaxValue };
            foreach (var track in tracks)
            {
                var trackDuplicates = new HashSet<TrackData>();

                var hashes = modelService.ReadHashDataByTrack(track.TrackReference);
                var result = queryFingerprintService.Query(modelService, hashes, queryConfiguration);

                if (result.IsSuccessful)
                {
                    foreach (var resultEntry in result.ResultEntries)
                    {
                        if (track.Equals(resultEntry.Track))
                        {
                            continue;
                        }

                        if (MinimumHammingSimilarity > resultEntry.Similarity)
                        {
                            continue;
                        }

                        trackDuplicates.Add(resultEntry.Track);
                    }

                    if (trackDuplicates.Any())
                    {
                        HashSet<TrackData> duplicatePair = new HashSet<TrackData>(trackDuplicates) { track };
                        duplicates.Add(duplicatePair);
                    }
                }

                if (callback != null)
                {
                    callback.Invoke(track, total, ++current);
                }
            }

            for (int i = 0; i < duplicates.Count - 1; i++)
            {
                HashSet<TrackData> set = duplicates[i];
                for (int j = i + 1; j < duplicates.Count; j++)
                {
                    IEnumerable<TrackData> result = set.Intersect(duplicates[j]);
                    if (result.Any())
                    {
                        foreach (var track in duplicates[j])
                        {
                            // collapse all duplicates in one set
                            set.Add(track);
                        }

                        duplicates.RemoveAt(j); /*Remove the duplicate set*/
                        j--;
                    }
                }
            }

            return duplicates.ToArray();
        }

        public void ClearStorage()
        {
            var tracks = modelService.ReadAllTracks();
            foreach (var track in tracks)
            {
                modelService.DeleteTrack(track.TrackReference);
            }
        }
    }
}