// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.IO;
using System.Windows.Forms;
using Soundfingerprinting.AudioProxies;
using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools.BassResampler
{
    /// <summary>
    ///   Re sampler
    /// </summary>
    public partial class WinBassResampler : Form
    {
        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public WinBassResampler()
        {
            InitializeComponent();
            Icon = Resources.Sound;
        }

        /// <summary>
        ///   Select a file to resample
        /// </summary>
        private void TbPathToFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {FileName = "filename.mp3", Filter = Resources.MusicFilter};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToFile.Text = ofd.FileName;
            }
        }

        /// <summary>
        ///   Resample the song
        /// </summary>
        private void BtnResampleClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_tbPathToFile.Text))
            {
                MessageBox.Show(Resources.SelectAPathToBeDrawn, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbPathToFile.Text)))
            {
                MessageBox.Show(Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd =
                new SaveFileDialog
                {
                    Filter = Resources.FileFilterWav,
                    FileName = Path.GetFileNameWithoutExtension(_tbPathToFile.Text) + ".wav"
                };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Action action =
                    () =>
                    {
                        using (BassAudioService bass = new BassAudioService())
                        {
                            string pathToRecoded = Path.GetFullPath(sfd.FileName);
                            bass.RecodeTheFile(_tbPathToFile.Text, pathToRecoded, (int) _nudSampleRate.Value);
                        }
                    };
                FadeControls(false);
                action.BeginInvoke(
                    (result) =>
                    {
                        action.EndInvoke(result);
                        FadeControls(true);
                        MessageBox.Show(Resources.FileConverted, Resources.FileConverted, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }, null);
            }
        }

        /// <summary>
        ///   Fade controls in the application
        /// </summary>
        /// <param name = "isVisible">Is visible parameter</param>
        private void FadeControls(bool isVisible)
        {
            Invoke(new Action(
                () =>
                {
                    _tbPathToFile.Enabled = isVisible;
                    _nudSampleRate.Enabled = isVisible;
                    _btnResample.Enabled = isVisible;
                }));
        }
    }
}