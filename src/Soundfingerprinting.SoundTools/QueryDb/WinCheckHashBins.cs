// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools.QueryDb
{
    using Soundfingerprinting.Audio.Strides;
    using Soundfingerprinting.Fingerprinting.Configuration;

    public partial class WinCheckHashBins : Form
    {
        private readonly IFingerprintService fingerprintService;

        private readonly List<string> _filters = new List<string>(new[] {"*.mp3", "*.wav", "*.ogg", "*.flac"}); /*File filters*/

        private List<String> _fileList;
        private HashAlgorithm _hashAlgorithm = HashAlgorithm.LSH; /*Locality sensitive hashing component*/

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        [ConfigurationPermission(SecurityAction.Demand)]
        public WinCheckHashBins(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
            InitializeComponent();

            Icon = Resources.Sound;
            foreach (object item in ConfigurationManager.ConnectionStrings) /*Detect all the connection strings*/
                _cmbConnectionString.Items.Add(item.ToString());

            if (_cmbConnectionString.Items.Count > 0) /*Set connection string*/
                _cmbConnectionString.SelectedIndex = 0;
            /*Setting default values*/
            _cmbAlgorithm.SelectedIndex = (int) _hashAlgorithm;

            string[] items = Enum.GetNames(typeof (StrideType)); /*Add enumeration types in the combo box*/
            _cmbStrideType.Items.AddRange(items);
            _cmbStrideType.SelectedIndex = 0;

            switch (_cmbAlgorithm.SelectedIndex)
            {
                case (int) HashAlgorithm.LSH:
                    _gbMinHash.Enabled = true;
                    _gbNeuralHasher.Enabled = false;
                    break;
                case (int) HashAlgorithm.NeuralHasher:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = true;
                    break;
                case (int) HashAlgorithm.None:
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = false;
                    break;
            }
        }

        /// <summary>
        ///   Select an algorithm for KNN components
        /// </summary>
        private void CmbAlgorithmSelectedIndexChanged(object sender, EventArgs e)
        {
            _hashAlgorithm = (HashAlgorithm) _cmbAlgorithm.SelectedIndex;

            switch (_cmbAlgorithm.SelectedIndex)
            {
                case (int) HashAlgorithm.LSH: /*Locality sensitive hashing + min hash*/
                    _gbMinHash.Enabled = true;
                    _gbNeuralHasher.Enabled = false;
                    _nudQueryStrideMax.Value = 253;
                    break;
                case (int) HashAlgorithm.NeuralHasher: /*Neural hasher*/
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = true;
                    _nudQueryStrideMax.Value = 640;
                    break;
                case (int) HashAlgorithm.None: /*None*/
                    _gbMinHash.Enabled = false;
                    _gbNeuralHasher.Enabled = false;
                    break;
            }
        }

        /// <summary>
        ///   Browse root folder with the songs to be queried
        /// </summary>
        private void BtnBrowseFolderClick(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                _tbRootFolder.Enabled = false;
                _tbRootFolder.Text = fbd.SelectedPath;
                _fileList = WinUtils.GetFiles(_filters, _tbRootFolder.Text);

                Invoke(new Action(() =>
                                  {
                                      Cursor = Cursors.Default;
                                      _tbRootFolder.Enabled = true;
                                      if (_fileList != null)
                                      {
                                          _nudTotalSongs.Value = _fileList.Count;
                                          _btnStart.Enabled = true;
                                      }
                                      _tbSingleFile.Text = null;
                                  }));
            }
        }

        /// <summary>
        ///   Browse single files to be recognized
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnBrowseSongClick(object sender, EventArgs e)
        {
            string filter = WinUtils.GetMultipleFilter("Audio files", _filters);
            OpenFileDialog ofd = new OpenFileDialog {Filter = filter, Multiselect = true};

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSingleFile.Text = null;
                foreach (string file in ofd.FileNames)
                    _tbSingleFile.Text += "\"" + Path.GetFileName(file) + "\" ";

                if (_fileList == null)
                    _fileList = new List<string>();
                foreach (string file in ofd.FileNames.Where(file => !_fileList.Contains(file)))
                {
                    _btnStart.Enabled = true;
                    _fileList.Add(file);
                }
                _nudTotalSongs.Value = _fileList.Count;
            }
        }

        /// <summary>
        ///   Select a path to network ensemble
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void BtnSelectClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {FileName = "Ensemble", Filter = Resources.FileFilterEnsemble};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToEnsemble.Text = ofd.FileName;
            }
        }

        /// <summary>
        ///   Start recognition process
        /// </summary>
        private void BtnStartClick(object sender, EventArgs e)
        {
            DefaultFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
            WinQueryResults winform;
            switch (_hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    if (_fileList == null || _fileList.Count == 0)
                    {
                        MessageBox.Show(
                            Resources.SelectFolderWithSongs,
                            Resources.Songs,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        break;
                    }

                    winform = new WinQueryResults(
                        _cmbConnectionString.SelectedItem.ToString(),
                        (int)_nudNumberOfFingerprints.Value,
                        (int)_numStaratSeconds.Value,
                        WinUtils.GetStride(
                            (StrideType)_cmbStrideType.SelectedIndex,
                            (int)_nudQueryStrideMax.Value,
                            (int)_nudQueryStrideMin.Value,
                            configuration.SamplesPerFingerprint),
                        _fileList,
                        (int)_nudHashtables.Value,
                        (int)_nudKeys.Value,
                        Convert.ToInt32(_nudThreshold.Value),
                        (int)_nudTopWavelets.Value) { FingerprintService = fingerprintService };
                    winform.Show();
                    break;
                case HashAlgorithm.NeuralHasher:
                    if (_fileList == null || _fileList.Count == 0)
                    {
                        MessageBox.Show(
                            Resources.SelectFolderWithSongs,
                            Resources.Songs,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        break;
                    }

                    winform = new WinQueryResults(
                        _cmbConnectionString.SelectedItem.ToString(),
                        (int)_nudNumberOfFingerprints.Value,
                        (int)_numStaratSeconds.Value,
                        WinUtils.GetStride(
                            (StrideType)(_cmbStrideType.SelectedIndex),
                            (int)_nudQueryStrideMax.Value,
                            (int)_nudQueryStrideMin.Value,
                            configuration.SamplesPerFingerprint),
                        (int)_nudTopWavelets.Value,
                        _fileList,
                        _tbPathToEnsemble.Text) { FingerprintService = fingerprintService };
                    winform.Show();
                    break;
                case HashAlgorithm.None:
                    break;
            }
        }
    }
}