namespace SoundFingerprinting.SoundTools.BassResampler
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.SoundTools.Properties;

    public partial class WinBassResampler : Form
    {
        private readonly IWaveFileUtility waveFileUtility;
        private readonly IAudioService audioService;

        public WinBassResampler(IWaveFileUtility waveFileUtility, IAudioService audioService)
        {
            this.waveFileUtility = waveFileUtility;
            this.audioService = audioService;
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
                        string destination = Path.GetFullPath(sfd.FileName);
                        float[] samples = audioService.ReadMonoSamplesFromFile(
                            _tbPathToFile.Text, (int)_nudSampleRate.Value);
                        waveFileUtility.WriteSamplesToFile(samples, (int)_nudSampleRate.Value, destination);
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