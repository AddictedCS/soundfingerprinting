namespace SoundFingerprinting.DAO
{
    using System;
    using System.Collections.Generic;
    using SoundFingerprinting.Data;

    public static class TracksUtility
    {
        public static TrackInfo? CombineTracks(TrackInfo? audioTrack, TrackInfo? videoTrack)
        {
            var track = (audioTrack, videoTrack) switch
            {
                (null, null) => null,
                (null, _) => videoTrack,
                (_, null) => audioTrack,
                (TrackInfo audio, TrackInfo video) => new TrackInfo(audio.Id, audio.Title, audio.Artist, MergeMetaFields(audio.MetaFields, video.MetaFields), MediaType.Audio | MediaType.Video)
            };

            return track;
        }

        private static Dictionary<string, string> MergeMetaFields(IDictionary<string, string> audioTrackMetaFields, IDictionary<string, string> videoTrackMetaFields)
        {
            var merged = new Dictionary<string, string>();
            foreach (var audioKeyPair in audioTrackMetaFields)
            {
                if (videoTrackMetaFields.ContainsKey(audioKeyPair.Key) && !videoTrackMetaFields[audioKeyPair.Key].Equals(audioKeyPair.Value, StringComparison.InvariantCulture))
                {
                    merged[$"audio_{audioKeyPair.Key}"] = audioTrackMetaFields[audioKeyPair.Key];
                    merged[$"video_{audioKeyPair.Key}"] = videoTrackMetaFields[audioKeyPair.Key];
                }
                else
                {
                    merged[audioKeyPair.Key] = audioKeyPair.Value;
                }
            }

            foreach (var videoKeyPair in videoTrackMetaFields)
            {
                if (!merged.ContainsKey($"video_{videoKeyPair.Key}"))
                {
                    merged[videoKeyPair.Key] = videoKeyPair.Value;
                }
            }

            return merged;
        }
    }
}