namespace SoundFingerprinting.NeuralHasher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SoundFingerprinting.Data;
    using SoundFingerprinting.NeuralHasher.Utils;

    public class TrainingDataProvider : ITrainingDataProvider
    {
        private readonly IModelService modelService;

        private readonly IBinaryOutputHelper binaryOutputHelper;

        public TrainingDataProvider(IModelService modelService, IBinaryOutputHelper binaryOutputHelper)
        {
            this.modelService = modelService;
            this.binaryOutputHelper = binaryOutputHelper;
        }

        public TrainingSet GetTrainingSet(int[] spectralImageIndexsToConsider, int numberOfTracks)
        {
            var spectralImages = GetSpectralImagesToTrain(spectralImageIndexsToConsider, numberOfTracks);
            return MapSpectralImagesToBinaryOutputs(spectralImages, numberOfTracks);
        }

        public List<double[][]> GetSpectralImagesToTrain(int[] spectralImageIndexsToConsider, int numberOfTracks)
        {
            var tracks = GetTrackFromDataSource(numberOfTracks);
            int maxIndex = spectralImageIndexsToConsider.Max();
            var spectralImagesToTrain = new List<double[][]>();
            var trackDatas = tracks as List<TrackData> ?? tracks.ToList();
            for (int trackIndex = 0; trackIndex < trackDatas.Count; trackIndex++)
            {
                var spectralImages = GetSpectralImagesForTrack(trackDatas[trackIndex], maxIndex);
                var spectralImagesToConsider = spectralImages.Where(image => spectralImageIndexsToConsider.Contains(image.OrderNumber))
                                                             .OrderBy(image => image.OrderNumber);
                int spectralImageIndex = 0;
                spectralImagesToTrain.Add(new double[spectralImageIndexsToConsider.Length][]);

                foreach (var spectralImageToConsider in spectralImagesToConsider)
                {
                    spectralImagesToTrain[trackIndex][spectralImageIndex++] = Array.ConvertAll(spectralImageToConsider.Image, f => (double)f); 
                }
            }

            return spectralImagesToTrain;
        }
        
        public TrainingSet MapSpectralImagesToBinaryOutputs(List<double[][]> spectralImagesToTrain, int binaryOutputsCount)
        {
            int trainingSongSnippets = spectralImagesToTrain[0].Length;
            double[][] inputs = new double[spectralImagesToTrain.Count * trainingSongSnippets][];
            double[][] outputs = new double[spectralImagesToTrain.Count * trainingSongSnippets][];
            
            int trackIndex = 0;
            int trainingDataIndex = 0;
            var binaryCodes = GetBinaryOutputs(binaryOutputsCount);
            foreach (var spectralImages in spectralImagesToTrain)
            {
                foreach (var spectralImage in spectralImages)
                {
                    inputs[trainingDataIndex] = spectralImage;
                    outputs[trainingDataIndex] = binaryCodes[trackIndex]; 
                    trainingDataIndex++;
                }

                trackIndex++;
            }

            return new TrainingSet { Inputs = inputs, Outputs = outputs };
        }

        private double[][] GetBinaryOutputs(int binaryLength)
        {
            byte[][] codes = binaryOutputHelper.GetBinaryCodes(binaryLength);
            int length = codes.GetLength(0);
            double[][] binaryOutputs = new double[length][];
            for (int i = 0; i < length; i++)
            {
                binaryOutputs[i] = Array.ConvertAll(codes[i], s => (double)s);
            }

            return binaryOutputs;
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
    }
}
