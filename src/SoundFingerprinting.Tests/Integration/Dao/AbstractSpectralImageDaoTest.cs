namespace SoundFingerprinting.Tests.Integration.DAO
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Audio.NAudio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Utils;

    [TestClass]
    public abstract class AbstractSpectralImageDaoTest : AbstractIntegrationTest
    {
        private readonly IAudioService audioService;
        private readonly ISpectrumService spectrumService;

        protected AbstractSpectralImageDaoTest()
        {
            audioService = new NAudioService();
            spectrumService = new SpectrumService();
        }

        public abstract ISpectralImageDao SpectralImageDao { get; set; }

        public abstract ITrackDao TrackDao { get; set; }

        [TestMethod]
        public void TestSpectralImagesAreInsertedInDataSource()
        {
            TrackData track = new TrackData("isrc", "artist", "title", "album", 1986, 200);
            var trackReference = TrackDao.InsertTrack(track);
            var audioSamples = audioService.ReadMonoSamplesFromFile(
                PathToMp3, new DefaultFingerprintConfiguration().SampleRate);
            var spectralImages = spectrumService.CreateLogSpectrogram(audioSamples, SpectrogramConfig.Default);
            var concatenatedSpectralImages = new List<float[]>();
            foreach (var spectralImage in spectralImages)
            {
                var concatenatedSpectralImage = ArrayUtils.ConcatenateDoubleDimensionalArray(spectralImage.Image);
                concatenatedSpectralImages.Add(concatenatedSpectralImage);
            }
            
            SpectralImageDao.InsertSpectralImages(concatenatedSpectralImages, trackReference);

            var readSpectralImages = SpectralImageDao.GetSpectralImagesByTrackId(trackReference);
            Assert.AreEqual(concatenatedSpectralImages.Count, readSpectralImages.Count);
            foreach (var readSpectralImage in readSpectralImages)
            {
                var expectedSpectralImage = concatenatedSpectralImages[readSpectralImage.OrderNumber];
                for (int i = 0; i < expectedSpectralImage.Length; i++)
                {
                    Assert.AreEqual(
                        concatenatedSpectralImages[readSpectralImage.OrderNumber][i], expectedSpectralImage[i]);
                }
            }
        }
    }
}
