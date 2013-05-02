namespace Soundfingerprinting.SoundTools.FilePermutations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.Hashing.MinHash;
    using Soundfingerprinting.SoundTools.Properties;

    public partial class WinFilePermutation : Form
    {
        private readonly IPermutationGeneratorService permutationGeneratorService;

        private readonly List<string> fileEndList = new List<string>();

        private List<string> fileStartList = new List<string>();

        public WinFilePermutation(IPermutationGeneratorService permutationGeneratorService)
        {
            this.permutationGeneratorService = permutationGeneratorService;
            InitializeComponent();
            Icon = Resources.Sound;
            _btnPermute.Enabled = false;
            _nudItems.Value = 1024; /*Number of items to copy*/
        }

        /// <summary>
        ///   Start folder text changed. Verify if it is OK final folder
        /// </summary>
        private void TextBox1TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_tbStartFolder.Text)) return;

            try
            {
                string rootPath = _tbStartFolder.Text;
                fileStartList.Clear();
                foreach (string f in Directory.GetFiles(rootPath, "*.mp3", SearchOption.AllDirectories))
                    fileStartList.Add(f);

                if (fileEndList.Count > 0)
                    _btnPermute.Enabled = true;
            }
            catch
            {
                /*swallow*/
            }
        }

        /// <summary>
        ///   End folder text changed
        /// </summary>
        private void TbEndFolderTextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_tbEndFolder.Text)) return;

            try
            {
                string rootPath = _tbEndFolder.Text;
                fileEndList.Clear();
                foreach (string f in Directory.GetFiles(rootPath, "*.mp3", SearchOption.AllDirectories))
                {
                    fileEndList.Add(f);
                }
                if (fileStartList.Count > 0)
                    _btnPermute.Enabled = true;
            }
            catch
            {
                /*swallow*/
            }
        }

        /// <summary>
        ///   Copy files from start folder to end folder
        /// </summary>
        private void BtnPermuteClick(object sender, EventArgs e)
        {
            _btnPermute.Enabled = false;
            if (_chbDeletePrevious.Checked)
                foreach (string file in fileEndList)
                    File.Delete(file);
            const int numberOfShuffles = 5;
            string[] array = fileStartList.ToArray();
            for (int i = 0; i < numberOfShuffles; i++)
            {
                permutationGeneratorService.RandomShuffleInPlace(array);
            }
            fileStartList = new List<string>(array);
            string rootPath = _tbEndFolder.Text;
            for (int i = 0; i < _nudItems.Value; i++)
            {
                try
                {
                    File.Copy(fileStartList[i], rootPath + "\\" + Path.GetFileName(fileStartList[i]));
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.Message, "Exception!", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        _btnPermute.Enabled = true;
                        return;
                    }
                }
            }
            _btnPermute.Enabled = true;
        }

        /// <summary>
        ///   Start folder selected
        /// </summary>
        private void BtnBrowseStartClick(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _tbStartFolder.Text = fbd.SelectedPath;
            }
        }

        /// <summary>
        ///   End folder selected
        /// </summary>
        private void BtnBrowseEndClick(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _tbEndFolder.Text = fbd.SelectedPath;
            }
        }
    }
}