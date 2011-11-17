// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Soundfingerprinting.Hashing;
using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools.FilePermutations
{
    public partial class WinFilePermutation : Form
    {
        /// <summary>
        ///   Result list
        /// </summary>
        private readonly List<string> _fileEndList = new List<string>();

        /// <summary>
        ///   Start list with paths to initial files
        /// </summary>
        private List<string> _fileStartList = new List<string>();

        /// <summary>
        ///   Constructor
        /// </summary>
        public WinFilePermutation()
        {
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
                _fileStartList.Clear();
                foreach (string f in Directory.GetFiles(rootPath, "*.mp3", SearchOption.AllDirectories))
                    _fileStartList.Add(f);

                if (_fileEndList.Count > 0)
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
                _fileEndList.Clear();
                foreach (string f in Directory.GetFiles(rootPath, "*.mp3", SearchOption.AllDirectories))
                {
                    _fileEndList.Add(f);
                }
                if (_fileStartList.Count > 0)
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
                foreach (string file in _fileEndList)
                    File.Delete(file);
            const int numberOfShuffles = 5;
            string[] array = _fileStartList.ToArray();
            for (int i = 0; i < numberOfShuffles; i++)
                PermGenerator.RandomShuffleInPlace(array);
            _fileStartList = new List<string>(array);
            string rootPath = _tbEndFolder.Text;
            for (int i = 0; i < _nudItems.Value; i++)
            {
                try
                {
                    File.Copy(_fileStartList[i], rootPath + "\\" + Path.GetFileName(_fileStartList[i]));
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