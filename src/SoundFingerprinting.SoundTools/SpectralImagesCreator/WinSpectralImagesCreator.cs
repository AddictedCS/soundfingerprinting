namespace SoundFingerprinting.SoundTools.SpectralImagesCreator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.Utils;

    public partial class WinSpectralImagesCreator : Form
    {
        private readonly IModelService modelService;
        private readonly ISpectrumService spectrumService;
        private readonly IAudioService audioService;
        private readonly ITagService tagService;

        private List<string> filesToConsume; 

        public WinSpectralImagesCreator(IModelService modelService, ISpectrumService spectrumService, IAudioService audioService, ITagService tagService)
        {
            this.modelService = modelService;
            this.spectrumService = spectrumService;
            this.audioService = audioService;
            this.tagService = tagService;
            InitializeComponent();
        }

        private void TbSelectRootFolder(object sender, MouseEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                tbPathToRoot.Text = fbd.SelectedPath;
            }

            filesToConsume = WinUtils.GetFiles(new[] { "*.mp3" }, tbPathToRoot.Text);
            label2.Text += filesToConsume.Count;
        }

        private void BtnInsertSpectralImagesClick(object sender, EventArgs e)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            btnInsertSpectralImages.Enabled = false;
            Task.Factory.StartNew(
                () =>
                    {
                        int count = 0;
                        foreach (var file in filesToConsume)
                        {
                            var tagInfo = tagService.GetTagInfo(file);

                            var track = new TrackData(
                                tagInfo.ISRC,
                                tagInfo.Artist,
                                tagInfo.Title,
                                tagInfo.Album,
                                tagInfo.Year,
                                (int)tagInfo.Duration);
                            var trackReference = modelService.InsertTrack(track);
                            var audioSamples = audioService.ReadMonoSamplesFromFile(
                                file, FingerprintConfiguration.Default.SampleRate);
                            var images = spectrumService.CreateLogSpectrogram(audioSamples, SpectrogramConfig.Default);
                            var concatenatedImages = new List<float[]>();
                            foreach (var image in images)
                            {
                                var concatenatedImage = ConcatenateImage(image.Image);
                                concatenatedImages.Add(concatenatedImage);
                            }

                            modelService.InsertSpectralImages(concatenatedImages, trackReference);

                            Invoke(new Action(() => { label4.Text = "Inserted: " + ++count; }));
                        }
                    }).ContinueWith((task => { btnInsertSpectralImages.Enabled = true; }), context);
        }

        private float[] ConcatenateImage(float[][] image)
        {
            return ArrayUtils.ConcatenateDoubleDimensionalArray(image);
        }

        private void WinSpectralImagesCreatorLoad(object sender, EventArgs e)
        {
            var allSongs = modelService.ReadAllTracks().Count;
            label3.Text += allSongs;
        }
    }
}
