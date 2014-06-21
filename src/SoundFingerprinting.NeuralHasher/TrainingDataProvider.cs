namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.DAO;
    using SoundFingerprinting.Data;

    public class TrainingDataProvider : ITrainingDataProvider
    {
        private readonly IModelService modelService;

        public TrainingDataProvider(IModelService modelService)
        {
            this.modelService = modelService;
        }

        public Dictionary<IModelReference, double[][]> GetSpectralImagesToTrain(int[] spectralImageIndexsToConsider, int numberOfTracks)
        {
            var tracks = GetTrackFromDataSource(numberOfTracks);

            int maxIndex = spectralImageIndexsToConsider.Max();
            var tracksWithSpectralImages = new Dictionary<IModelReference, double[][]>();
            foreach (var track in tracks)
            {
                var spectralImages = GetSpectralImagesForTrack(track, maxIndex);
                var spectralImagesToConsider = spectralImages.Where(image => spectralImageIndexsToConsider.Contains(image.OrderNumber))
                                                             .OrderBy(image => image.OrderNumber);
                int index = 0;
                tracksWithSpectralImages.Add(track.TrackReference, new double[spectralImageIndexsToConsider.Length][]);
                foreach (var spectralImageToConsider in spectralImagesToConsider)
                {
                    tracksWithSpectralImages[track.TrackReference][index++] = ConvertFloatToDouble(spectralImageToConsider.Image);
                }
            }

            return tracksWithSpectralImages;
        }

        private IEnumerable<SpectralImageData> GetSpectralImagesForTrack(TrackData track, int maxIndex)
        {
            var spectralImages = modelService.GetSpectralImagesByTrackId(track.TrackReference);
            if (spectralImages.Count < maxIndex + 1)
            {
                throw new Exception(
                    "Track " + track.Artist + " " + track.Title
                    + " does not contain enough spectral images for training set generation");
            }

            return spectralImages;
        }

        private IEnumerable<TrackData> GetTrackFromDataSource(int numberOfTracks)
        {
            var tracks = modelService.ReadAllTracks();

            if (tracks.Count != numberOfTracks)
            {
                throw new Exception("Number of tracks in the datasource is not equal to required number of tracks. Please provide a valid database with training data only.");
            }

            return tracks;
        }

        private double[] ConvertFloatToDouble(float[] spectralImageToConsider)
        {
            double[] converted = new double[spectralImageToConsider.Length];
            for (int i = 0; i < converted.Length; i++)
            {
                converted[i] = spectralImageToConsider[i];
            }

            return converted;
        }
    }
}
