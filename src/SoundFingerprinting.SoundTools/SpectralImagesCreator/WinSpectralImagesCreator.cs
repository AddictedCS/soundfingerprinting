namespace SoundFingerprinting.SoundTools.SpectralImagesCreator
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Data;

    public partial class WinSpectralImagesCreator : Form
    {
        private readonly IModelService modelService;

        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;

        private readonly IAudioService audioService;

        private readonly ITagService tagService;

        private List<string> filesToConsume; 

        public WinSpectralImagesCreator(IModelService modelService, IFingerprintCommandBuilder fingerprintCommandBuilder, IAudioService audioService, ITagService tagService)
        {
            this.modelService = modelService;
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.audioService = audioService;
            this.tagService = tagService;
            InitializeComponent();
        }

        private void TbSelectRootFolder(object sender, MouseEventArgs e)
        {
            filesToConsume = WinUtils.GetFiles(new[] { ".mp3" }, tbPathToRoot.Text);
            label2.Text = "Count: " + filesToConsume.Count;
        }

        private void BtnInsertSpectralImagesClick(object sender, System.EventArgs e)
        {
            foreach (var file in filesToConsume)
            {
                var tagInfo = tagService.GetTagInfo(file);

                var track = new TrackData(
                    tagInfo.ISRC, tagInfo.Artist, tagInfo.Title, tagInfo.Album, tagInfo.Year, (int)tagInfo.Duration);
                var trackReference = modelService.InsertTrack(track);
                var images = fingerprintCommandBuilder.BuildFingerprintCommand().From(file)
                    .WithDefaultFingerprintConfig()
                    .UsingServices(audioService)
                    .CreateSpectralImages()
                    .Result;

                var concatenatedImages = new List<float[]>();
                foreach (var image in images)
                {
                    var concatenatedImage = ConcatenateImage(image);
                    concatenatedImages.Add(concatenatedImage);
                }

                modelService.InsertSpectralImages(concatenatedImages, trackReference);
            }
        }

        private float[] ConcatenateImage(float[][] image)
        {
            int rows = image.GetLength(0); /*128*/
            int cols = image[0].Length; /*32*/
            float[] concatenated = new float[rows * cols]; /* 128 * 32 */
            for (int row = 0; row < rows; row++)
            {
                Buffer.BlockCopy(image[row], 0, concatenated, row * image[row].Length * 4, image[row].Length * 4);
            }

            return concatenated;
        }
    }
}
