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
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinDrawningTool : Form
    {
        private readonly IFingerprintService fingerprintService;

        private readonly ITagService tagService;

        public WinDrawningTool(IFingerprintService fingerprintService, ITagService tagService)
        {
            this.fingerprintService = fingerprintService;
            this.tagService = tagService;

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
                            IFingerprintingConfig config = new DefaultFingerprintingConfig();
                            config.Stride = new StaticStride((int)_nudStride.Value);
                            fingerprintService.FingerprintConfig = config;
                            List<bool[]> fingerprints = fingerprintService.CreateFingerprints(Path.GetFullPath(_tbPathToFile.Text));
                            int width = config.FingerprintLength;
                            int height = FingerprintService.LogBins;
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
                            fingerprintService.FingerprintConfig.Stride = new StaticStride((int)_nudStride.Value);
                            List<bool[]> result = fingerprintService.CreateFingerprints(Path.GetFullPath(_tbPathToFile.Text));
                            int i = -1;
                            int width = fingerprintService.FingerprintConfig.FingerprintLength;
                            int height = FingerprintService.LogBins;
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
                                fullpath, new DefaultFingerprintingConfig().SampleRate, 0, 0);
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
                            float[][] data = fingerprintService.CreateSpectrogram(Path.GetFullPath(_tbPathToFile.Text));
                            Bitmap image = Imaging.GetSpectrogramImage(
                                data, (int)_nudWidth.Value, (int)_nudHeight.Value);
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

        /// <summary>
        ///   Draw wavelets
        /// </summary>
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
                        fingerprintService.FingerprintConfig.Stride = new StaticStride((int)_nudStride.Value);
                        Image image = Imaging.GetWaveletSpectralImage(
                            Path.GetFullPath(_tbPathToFile.Text), fingerprintService);
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