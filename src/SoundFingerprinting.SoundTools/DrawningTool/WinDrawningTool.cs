namespace SoundFingerprinting.SoundTools.DrawningTool
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.FFT;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Strides;
    using SoundFingerprinting.Wavelets;

    public partial class WinDrawningTool : Form
    {
        private readonly IAudioService audioService;
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly FingerprintConfiguration fingerprintConfiguration;
        private readonly IImageService imageService;
        private readonly ISpectrumService spectrumService;

        public WinDrawningTool(
            IAudioService audioService,
            IFingerprintCommandBuilder fingerprintCommandBuilder,
            ISpectrumService spectrumService)
        {
            this.audioService = audioService;
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            fingerprintConfiguration = new DefaultFingerprintConfiguration();
            imageService = new ImageService(new StandardHaarWaveletDecomposition());
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

                    Task.Factory.StartNew(
                        () =>
                            {
                                var songToDraw = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                                          .From(songFileName)
                                                                          .WithFingerprintConfig(
                                                                            config =>
                                                                            {
                                                                                config.SpectrogramConfig.Stride = new IncrementalStaticStride(strideSize, config.SamplesPerFingerprint);
                                                                                config.NormalizeSignal = normalize;
                                                                            })
                                                                           .UsingServices(audioService);

                                List<bool[]> fingerprints = songToDraw.Fingerprint()
                                                                      .Result
                                                                      .Select(fingerprint => fingerprint)
                                                                      .ToList();
                                int width = songToDraw.FingerprintConfiguration.SpectrogramConfig.ImageLength;
                                int height = songToDraw.FingerprintConfiguration.SpectrogramConfig.LogBins;
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
                                var songToDraw = fingerprintCommandBuilder.BuildFingerprintCommand()
                                                                          .From(songFileName)
                                                                          .WithFingerprintConfig(
                                                                            config =>
                                                                            {
                                                                                config.SpectrogramConfig.Stride = new IncrementalStaticStride(strideSize, config.SamplesPerFingerprint);
                                                                            })
                                                                          .UsingServices(audioService);
                                List<bool[]> result = songToDraw.Fingerprint()
                                                                .Result
                                                                .Select(fingerprint => fingerprint)
                                                                .ToList();
                                int i = -1;
                                int width = songToDraw.FingerprintConfiguration.SpectrogramConfig.ImageLength;
                                int height = songToDraw.FingerprintConfiguration.SpectrogramConfig.LogBins;
                                foreach (bool[] item in result)
                                {
                                    using (var image = imageService.GetImageForFingerprint(item, width, height))
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
                        AudioSamples data = audioService.ReadMonoSamplesFromFile(fullpath, new DefaultFingerprintConfiguration().SampleRate);
                        using (Image image = imageService.GetSignalImage(data.Samples, (int)_nudWidth.Value, (int)_nudHeight.Value))
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
                        AudioSamples samples = audioService.ReadMonoSamplesFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintConfiguration.SampleRate);
                        float[][] data = spectrumService.CreateSpectrogram(samples, fingerprintConfiguration.SpectrogramConfig.Overlap, fingerprintConfiguration.SpectrogramConfig.WdftSize);
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
                        AudioSamples samples = audioService.ReadMonoSamplesFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintConfiguration.SampleRate);
                        Image image =
                            imageService.GetLogSpectralImages(
                                spectrumService.CreateLogSpectrogram(
                                    samples, fingerprintConfiguration.SpectrogramConfig),
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
                        AudioSamples samples = audioService.ReadMonoSamplesFromFile(Path.GetFullPath(_tbPathToFile.Text), fingerprintConfiguration.SampleRate);
                        Image image =
                            imageService.GetWaveletsImages(
                                spectrumService.CreateLogSpectrogram(
                                    samples, fingerprintConfiguration.SpectrogramConfig),
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