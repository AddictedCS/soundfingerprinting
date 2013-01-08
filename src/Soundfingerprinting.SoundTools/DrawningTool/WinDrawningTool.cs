namespace Soundfingerprinting.SoundTools.DrawningTool
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Fingerprinting.Windows;
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinDrawningTool : Form
    {
        private readonly IFingerprintService fingerprintService;

        private readonly IAudioService audioService;

        private readonly ITagService tagService;

        private readonly IWorkUnitBuilder workUnitBuilder;

        private readonly IFingerprintingConfiguration fingerprintingConfiguration;

        public WinDrawningTool(IFingerprintService fingerprintService, IAudioService audioService, ITagService tagService, IWorkUnitBuilder workUnitBuilder, IFingerprintingConfiguration fingerprintingConfiguration)
        {
            this.fingerprintService = fingerprintService;
            this.audioService = audioService;
            this.tagService = tagService;
            this.workUnitBuilder = workUnitBuilder;
            this.fingerprintingConfiguration = fingerprintingConfiguration;

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
                MessageBox.Show(
                    Resources.SelectAPathToBeDrawn,
                    Resources.SelectFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_lbImageTypes.SelectedIndex == 0)
            {
                string fileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text);
                SaveFileDialog sfd = new SaveFileDialog
                    { FileName = fileName + "_fingerprints_" + ".jpg", Filter = Resources.FileFilterJPeg };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.GetFullPath(sfd.FileName);
                    FadeControls(false);
                    Action action = () =>
                        {
                            var unit =
                                workUnitBuilder.BuildWorkUnit().On(Path.GetFullPath(_tbPathToFile.Text)).
                                    WithCustomConfiguration(
                                        config => { config.Stride = new StaticStride((int)_nudStride.Value); });

                            List<bool[]> fingerprints = unit.GetFingerprintsUsingService(fingerprintService).Result;
                            IFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
                            int width = configuration.FingerprintLength;
                            int height = configuration.LogBins;
                            Bitmap image = Imaging.GetFingerprintsImage(fingerprints, width, height);
                            image.Save(path);
                            image.Dispose();
                        };

                    action.BeginInvoke(
                        (result) =>
                            {
                                FadeControls(true);
                                MessageBox.Show(
                                    Resources.ImageIsDrawn,
                                    Resources.Finished,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                action.EndInvoke(result);
                            },
                        action);
                }
            }
            else if (_lbImageTypes.SelectedIndex == 1)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    string fileName = Path.GetFileName(_tbPathToFile.Text);
                    FadeControls(false);
                    Action action = () =>
                        {
                            var unit =
                               workUnitBuilder.BuildWorkUnit().On(Path.GetFullPath(_tbPathToFile.Text)).
                                   WithCustomConfiguration(
                                       config => { config.Stride = new StaticStride((int)_nudStride.Value); });
                            List<bool[]> result = unit.GetFingerprintsUsingService(fingerprintService).Result;

                            int i = -1;
                            IFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
                            int width = configuration.FingerprintLength;
                            int height = configuration.LogBins;
                            foreach (bool[] item in result)
                            {
                                Image image = Imaging.GetFingerprintImage(item, width, height);
                                image.Save(path + "\\" + fileName + i++ + ".jpg", ImageFormat.Jpeg);
                            }

                        };
                    action.BeginInvoke(
                        (result) =>
                            {
                                FadeControls(true);
                                MessageBox.Show(
                                    Resources.ImageIsDrawn,
                                    Resources.Finished,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                action.EndInvoke(result);
                            },
                        action);
                }
            }
        }

        private void BtnDrawSignalClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(
                    Resources.SelectAPathToBeDrawn,
                    Resources.SelectFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        using (IAudioService proxy = new BassAudioService())
                        {
                            float[] data = proxy.ReadMonoFromFile(
                                fullpath, new DefaultFingerprintingConfiguration().SampleRate, 0, 0);
                            Bitmap image = Imaging.GetSignalImage(data, (int)_nudWidth.Value, (int)_nudHeight.Value);
                            image.Save(sfd.FileName, ImageFormat.Jpeg);
                            image.Dispose();
                        }
                    };

                action.BeginInvoke(
                    (result) =>
                        {
                            FadeControls(true);
                            action.EndInvoke(result);
                            MessageBox.Show(
                                Resources.ImageIsDrawn,
                                Resources.Finished,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        },
                    null);
            }
        }

        private void BtnDrawSpectrumClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(
                    Resources.SelectAPathToBeDrawn,
                    Resources.SelectFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        float[][] data = audioService.CreateSpectrogram(
                            Path.GetFullPath(_tbPathToFile.Text),
                            fingerprintingConfiguration.WindowFunction,
                            fingerprintingConfiguration.SampleRate,
                            fingerprintingConfiguration.Overlap,
                            fingerprintingConfiguration.WdftSize);

                        Bitmap image = Imaging.GetSpectrogramImage(data, (int)_nudWidth.Value, (int)_nudHeight.Value);
                        image.Save(sfd.FileName, ImageFormat.Jpeg);
                        image.Dispose();
                    };

                action.BeginInvoke(
                    (result) =>
                        {
                            FadeControls(true);
                            action.EndInvoke(result);
                            MessageBox.Show(
                                Resources.ImageIsDrawn,
                                Resources.Finished,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
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
                            _btnDrawWavelets.Enabled = isVisible;
                        }));
        }

        private void BtnDrawWaveletsClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(
                    Resources.SelectAPathToBeDrawn,
                    Resources.SelectFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        Image image =
                            Imaging.GetWaveletSpectralImage(
                                audioService.CreateLogSpectrogram(
                                    Path.GetFullPath(_tbPathToFile.Text),
                                    fingerprintingConfiguration.WindowFunction,
                                    new AudioServiceConfiguration()
                                        {
                                            LogBase = fingerprintingConfiguration.LogBase,
                                            LogBins = fingerprintingConfiguration.LogBins,
                                            MaxFrequency = fingerprintingConfiguration.MaxFrequency,
                                            MinFrequency = fingerprintingConfiguration.MinFrequency,
                                            Overlap = fingerprintingConfiguration.Overlap,
                                            SampleRate = fingerprintingConfiguration.SampleRate,
                                            WdftSize = fingerprintingConfiguration.WdftSize
                                        }),
                                fingerprintingConfiguration);
                        image.Save(path);
                        image.Dispose();

                    };
                action.BeginInvoke(
                    (result) =>
                        {
                            FadeControls(true);
                            MessageBox.Show(
                                Resources.ImageIsDrawn,
                                Resources.Finished,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            action.EndInvoke(result);
                        },
                    action);
            }
        }
    }
}