namespace Soundfingerprinting.SoundTools.BassResampler
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Soundfingerprinting.Audio;
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinBassResampler : Form
    {
        private readonly IExtendedAudioService extendedAudioService;

        public WinBassResampler(IExtendedAudioService extendedAudioService)
        {
            this.extendedAudioService = extendedAudioService;
            InitializeComponent();
            Icon = Resources.Sound;
        }

        private void TbPathToFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { FileName = "pathToAudioFile.mp3", Filter = Resources.MusicFilter };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToFile.Text = ofd.FileName;
            }
        }

        private void BtnResampleClick(object sender, EventArgs e)
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
                    Filter = Resources.FileFilterWav,
                    FileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text) + ".wav"
                };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Action action = () =>
                    {
                        string pathToRecoded = Path.GetFullPath(sfd.FileName);
                        extendedAudioService.RecodeFileToMonoWave(_tbPathToFile.Text, pathToRecoded, (int)_nudSampleRate.Value);
                    };

                FadeControls(false);
                action.BeginInvoke(
                    result =>
                        {
                            action.EndInvoke(result);
                            FadeControls(true);
                            MessageBox.Show(
                                Resources.FileConverted,
                                Resources.FileConverted,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        },
                    null);
            }
        }

        private void FadeControls(bool isVisible)
        {
            Invoke(
                new Action(
                    () =>
                        {
                            _tbPathToFile.Enabled = isVisible;
                            _nudSampleRate.Enabled = isVisible;
                            _btnResample.Enabled = isVisible;
                        }));
        }
    }
}