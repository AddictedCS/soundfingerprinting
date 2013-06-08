namespace Soundfingerprinting.SoundTools.DrawningTool
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.Configuration;
    using Soundfingerprinting.FFT;
    using Soundfingerprinting.Image;
    using Soundfingerprinting.SoundTools.Properties;
    using Soundfingerprinting.Strides;
    using Soundfingerprinting.Windows;

    public partial class WinDrawningTool : Form
    {
        private readonly IAudioService audioService;

        private readonly ITagService tagService;

        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;

        private readonly IFingerprintingConfiguration fingerprintingConfiguration;

        private readonly IImageService imageService;

        private readonly ISpectrumService spectrumService;

        public WinDrawningTool(
            IAudioService audioService,
            ITagService tagService,
            IFingerprintUnitBuilder fingerprintUnitBuilder,
            IFingerprintingConfiguration fingerprintingConfiguration,
            IImageService imageService,
            ISpectrumService spectrumService)
        {
            this.audioService = audioService;
            this.tagService = tagService;
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.fingerprintingConfiguration = fingerprintingConfiguration;
            this.imageService = imageService;
            this.spectrumService = spectrumService;

            InitializeComponent();
            Icon = Resources.Sound;

            _lbImageTypes.Items.Add("Single file");
            _lbImageTypes.Items.Add("Separated images");
        }

        private void TbPathToFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = Resources.MusicFilter, FileName = "music.mp3" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToFile.Text = ofd.FileName;
            }
        }

        private void BtnDrawFingerprintsClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string songName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text);
            string songFileName = Path.GetFullPath(_tbPathToFile.Text);
            int strideSize = (int)_nudStride.Value;
            if (DrawSongAsASingleImage())
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog { FileName = songName + "_fingerprints_" + ".jpg", Filter = Resources.FileFilterJPeg };

                if (IsFileSelected(saveFileDialog))
                {
                    FadeControls(false);

                    string imageFilename = Path.GetFullPath(saveFileDialog.FileName);
                    bool normalize = _cbNormalize.Checked;
                    bool noWindow = _cbWindow.Checked;

                    Task.Factory.StartNew(
                        () =>
                            {
                                var songToDraw = fingerprintUnitBuilder.BuildFingerprints().On(songFileName).WithCustomConfiguration(
                                    config =>
                                        {
                                            config.Stride = new IncrementalStaticStride(strideSize, config.SamplesPerFingerprint);
                                            config.NormalizeSignal = normalize;
                                            config.WindowFunction = noWindow ? new CachingHanningWindow(new NoWindow()) : new CachingHanningWindow(new HanningWindow());
                                        });

                                List<bool[]> fingerprints = songToDraw.RunAlgorithm().Result;
                                int width = songToDraw.Configuration.FingerprintLength;
                                int height = songToDraw.Configuration.LogBins;
                                using (Image image = imageService.GetImageForFingerprints(fingerprints, width, height, 5))
                                {
                                    image.Save(imageFilename);
                                }
                            }).ContinueWith(
                                result =>
                                    {
                                        FadeControls(true);
                                        ShowImageWasDrawnDialog();
                                    });
                }
            }
            else if (DrawSongAsSeparateImages())
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = folderBrowserDialog.SelectedPath;
                    FadeControls(false);
                    Task.Factory.StartNew(
                        () =>
                            {
                                var songToDraw = this.fingerprintUnitBuilder.BuildFingerprints().On(songFileName).WithCustomConfiguration(
                                    config => { config.Stride = new IncrementalStaticStride(strideSize, config.SamplesPerFingerprint); });
                                List<bool[]> result = songToDraw.RunAlgorithm().Result;
                                int i = -1;
                                int width = songToDraw.Configuration.FingerprintLength;
                                int height = songToDraw.Configuration.LogBins;
                                foreach (bool[] item in result)
                                {
                                    using (Image image = imageService.GetImageForFingerprint(item, width, height))
                                    {
                                        image.Save(path + "\\" + songName + i++ + ".jpg", ImageFormat.Jpeg);
                                    }
                                }
                            }).ContinueWith(
                                result =>
                                    {
                                        FadeControls(true);
                                        ShowImageWasDrawnDialog();
                                    });
                }
            }
        }

        private static bool IsFileSelected(SaveFileDialog saveFileDialog)
        {
            return saveFileDialog.ShowDialog() == DialogResult.OK;
        }

        private bool DrawSongAsSeparateImages()
        {
            return _lbImageTypes.SelectedIndex == 1;
        }

        private static DialogResult ShowImageWasDrawnDialog()
        {
            return MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool DrawSongAsASingleImage()
        {
            return _lbImageTypes.SelectedIndex == 0;
        }

        private void BtnDrawSignalClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
                                     {
                                         Filter = Resources.FileFilterJPeg,
                                         FileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text) + "_signal_" + ".jpg"
                                     };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string fullpath = Path.GetFullPath(_tbPathToFile.Text);
                FadeControls(false);
                Action action = () =>
                    {
                        float[] data = audioService.ReadMonoFromFile(fullpath, new DefaultFingerprintingConfiguration().SampleRate);
                        using (Image image = imageService.GetSignalImage(data, (int)_nudWidth.Value, (int)_nudHeight.Value))
                        {
                            image.Save(sfd.FileName, ImageFormat.Jpeg);
                        }
                    };

                action.BeginInvoke(
                    result =>
                        {
                            FadeControls(true);
                            action.EndInvoke(result);
                            MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        },
                    null);
            }
        }

        private void BtnDrawSpectrumClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
                                     {
                                         Filter = Resources.FileFilterJPeg,
                                         FileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text) + "_spectrum_" + ".jpg"
                                     };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FadeControls(false);
                Action action = () =>
                    {
                        float[] samples = audioService.ReadMonoFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintingConfiguration.SampleRate);
                        float[][] data = spectrumService.CreateSpectrogram(samples, fingerprintingConfiguration.Overlap, fingerprintingConfiguration.WdftSize);
                        Image image = imageService.GetSpectrogramImage(data, (int)_nudWidth.Value, (int)_nudHeight.Value);
                        image.Save(sfd.FileName, ImageFormat.Jpeg);
                        image.Dispose();
                    };

                action.BeginInvoke(
                    result =>
                        {
                            FadeControls(true);
                            action.EndInvoke(result);
                            MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        },
                    null);
            }
        }

        /// <summary>
        ///   Fade all controls
        /// </summary>
        /// <param name = "isVisible">Set control as visible</param>
        private void FadeControls(bool isVisible)
        {
            Invoke(
                new Action(
                    () =>
                        {
                            _tbPathToFile.Enabled = isVisible;
                            _nudHeight.Enabled = isVisible;
                            _nudStride.Enabled = isVisible;
                            _nudWidth.Enabled = isVisible;
                            _lbImageTypes.Enabled = isVisible;
                            _btnDraw.Enabled = isVisible;
                            _btnDrawSignal.Enabled = isVisible;
                            _btnDrawSpectrum.Enabled = isVisible;
                            _btnDrawLogSpectrum.Enabled = isVisible;
                        }));
        }

        private void BtnDrawWaveletsClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text);
            SaveFileDialog sfd = new SaveFileDialog { FileName = fileName + ".jpg", Filter = Resources.FileFilterJPeg };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = Path.GetFullPath(sfd.FileName);
                FadeControls(false);
                Action action = () =>
                    {
                        float[] samples = audioService.ReadMonoFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintingConfiguration.SampleRate);
                        Image image = imageService.GetLogSpectralImages(
                            spectrumService.CreateLogSpectrogram(samples, fingerprintingConfiguration),
                            new IncrementalStaticStride((int)_nudStride.Value, fingerprintingConfiguration.SamplesPerFingerprint),
                            fingerprintingConfiguration.FingerprintLength,
                            fingerprintingConfiguration.Overlap,
                            5);

                        image.Save(path);
                        image.Dispose();
                    };
                action.BeginInvoke(
                    result =>
                        {
                            FadeControls(true);
                            MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            action.EndInvoke(result);
                        },
                    action);
            }
        }

        private void BtnDrawWavelets_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text);
            SaveFileDialog sfd = new SaveFileDialog { FileName = fileName + ".jpg", Filter = Resources.FileFilterJPeg };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = Path.GetFullPath(sfd.FileName);
                FadeControls(false);
                Action action = () =>
                    {
                        float[] samples = audioService.ReadMonoFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintingConfiguration.SampleRate);
                        Image image = imageService.GetWaveletsImages(
                            spectrumService.CreateLogSpectrogram(samples, fingerprintingConfiguration),
                            new IncrementalStaticStride((int)_nudStride.Value, fingerprintingConfiguration.SamplesPerFingerprint),
                            fingerprintingConfiguration.FingerprintLength,
                            fingerprintingConfiguration.Overlap,
                            5);

                        image.Save(path);
                        image.Dispose();
                    };
                action.BeginInvoke(
                    result =>
                        {
                            FadeControls(true);
                            MessageBox.Show(Resources.ImageIsDrawn, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            action.EndInvoke(result);
                        },
                    action);
            }
        }
    }
}