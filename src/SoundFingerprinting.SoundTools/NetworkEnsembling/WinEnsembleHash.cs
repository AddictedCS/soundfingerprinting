namespace SoundFingerprinting.SoundTools.NetworkEnsembling
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Windows.Forms;

    using Encog.Engine.Network.Activation;
    using Encog.ML.Data.Basic;
    using Encog.Util;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.MinHash;
    using SoundFingerprinting.Hashing.NeuralHashing;
    using SoundFingerprinting.Hashing.NeuralHashing.Ensemble;
    using SoundFingerprinting.Hashing.NeuralHashing.MMI;
    using SoundFingerprinting.Hashing.NeuralHashing.NeuralTrainer;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Utils;

    /// <summary>
    ///   Building Hashes, Assembling networks
    /// </summary>
    public partial class WinEnsembleHash : Form
    {
        private readonly Dictionary<string, bool> dictionaryPathToNetworks = new Dictionary<string, bool>();

        private readonly IModelService modelService;

        /// <summary>
        ///   Start folder to search networks
        /// </summary>
        private string folderToSearchNetworks = string.Empty;

        /// <summary>
        ///   Number of hash tables (using Min Hash + LSH schema)
        /// </summary>
        private int numberofgroupsminhash = 20;

        /// <summary>
        ///   Number of hash tables (using Neural hasher algorithm)
        /// </summary>
        private int numberofgroupsneuralhasher = 18;

        /// <summary>
        ///   Number of keys per table (using Min Hash + LSH schema)
        /// </summary>
        private int numberofhashesperkeyminhash = 5;

        /// <summary>
        ///   Number of keys per table (using Neural hasher algorithm)
        /// </summary>
        private int numberofhashesperkeyneuralhasher = 22;

        /// <summary>
        ///   Name of the final ensemble
        /// </summary>
        private string pathToEnsemble = "Ensemble.ens";

        public WinEnsembleHash(IFingerprintService fingerprintService, IModelService modelService)
        {
            this.modelService = modelService;
            InitializeComponent();
            Icon = Resources.Sound;
            new DatabasePermutations(modelService);
        }

        /// <summary>
        ///   Once the windows is loaded, define some default parameters
        /// </summary>
        private void WinMinMutualInfoLoad(object sender, EventArgs e)
        {
            _nudNumberOfGroupsNeuralHasher.Value = numberofgroupsneuralhasher;
            _nudNumberOfHashesPerKeyNeuralHasher.Value = numberofhashesperkeyneuralhasher;

            _nudNumberOfGroupsMinHash.Value = numberofgroupsminhash;
            _nudNumberOfHashesPerKeyMinHash.Value = numberofhashesperkeyminhash;

            _tbSaveToEnsembleFilename.Text = Path.GetFullPath(pathToEnsemble);
            _tbSaveToEnsembleFilename.TextChanged += TbEnsembleFilenameTextChanged;

            foreach (object item in ConfigurationManager.ConnectionStrings)
            {
                _cmbConnectionStringEnsemble.Items.Add(item.ToString());
                _cmbValidationConnectionStringNeuralHasher.Items.Add(item.ToString());
                _cmbConnectionStringMinHash.Items.Add(item.ToString());
            }

            _cmbConnectionStringEnsemble.SelectedIndex = 0;
            int count = ConfigurationManager.AppSettings.Count;
            for (int i = 0; i < count; i++)
            {
                dictionaryPathToNetworks.Add(Path.GetFullPath(ConfigurationManager.AppSettings[i]), true);
            }
            PopulateDataGridView();
            _tbSelectedNetworks.Text = dictionaryPathToNetworks.Count.ToString();
            _pbProgress.Visible = false;
            _pbMinHash.Visible = false;
            _tbStoredEnsembleFilename.ReadOnly = true;
        }

        /// <summary>
        ///   Name of the ensemble changed
        /// </summary>
        private void TbEnsembleFilenameTextChanged(object sender, EventArgs e)
        {
            pathToEnsemble = _tbSaveToEnsembleFilename.Text;
        }

        /// <summary>
        ///   Save the ensemble file
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            if (_sfdEnsembleSave.ShowDialog() == DialogResult.OK)
            {
                pathToEnsemble = _sfdEnsembleSave.FileName;
                _tbSaveToEnsembleFilename.Text = pathToEnsemble;
            }
        }

        /// <summary>
        ///   Insert new networks to the dictionary of networks
        /// </summary>
        private void BtnInsertClick(object sender, EventArgs e)
        {
            if (_fbdSelectNetworks.ShowDialog() == DialogResult.OK)
            {
                folderToSearchNetworks = _fbdSelectNetworks.SelectedPath;
                string[] networks = Directory.GetFiles(folderToSearchNetworks, "*.ntwrk", SearchOption.AllDirectories);
                dictionaryPathToNetworks.Clear();
                foreach (string network in networks)
                {
                    dictionaryPathToNetworks.Add(network, true);
                }
                PopulateDataGridView();
                _tbSelectedNetworks.Text = dictionaryPathToNetworks.Count.ToString();
            }
        }

        /// <summary>
        ///   Populate data grid view with the networks
        /// </summary>
        private void PopulateDataGridView()
        {
            _dgvNetworks.Invoke(
                new Action(
                    () =>
                        {
                            _dgvNetworks.Rows.Clear();
                            foreach (KeyValuePair<string, bool> pair in dictionaryPathToNetworks)
                            {
                                _dgvNetworks.Rows.Add(pair.Key, pair.Value);
                            }
                        }));
        }

        /// <summary>
        /// Check/Uncheck a network from learning
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void DgvNetworksCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex != -1)
            {
                dictionaryPathToNetworks[_dgvNetworks.Rows[e.RowIndex].Cells[0].Value.ToString()] =
                    !dictionaryPathToNetworks[_dgvNetworks.Rows[e.RowIndex].Cells[0].Value.ToString()];
                _dgvNetworks[e.ColumnIndex, e.RowIndex].Value =
                    !Convert.ToBoolean(_dgvNetworks[e.ColumnIndex, e.RowIndex].Value);
                int prevNumber = Convert.ToInt32(_tbSelectedNetworks.Text);
                int curNumber = Convert.ToBoolean(_dgvNetworks[e.ColumnIndex, e.RowIndex].Value)
                                    ? prevNumber + 1
                                    : prevNumber - 1;
                _tbSelectedNetworks.Text = curNumber.ToString();
            }
        }

        /// <summary>
        ///   Number of groups changed
        /// </summary>
        private void NudNumberOfGroupsValueChanged(object sender, EventArgs e)
        {
            numberofgroupsneuralhasher = (int) _nudNumberOfGroupsNeuralHasher.Value;
        }

        /// <summary>
        ///   Number of keys per table
        /// </summary>
        private void NudNumberOfHashesPerKeyValueChanged(object sender, EventArgs e)
        {
            numberofhashesperkeyneuralhasher = (int) _nudNumberOfHashesPerKeyNeuralHasher.Value;
        }

        /// <summary>
        ///   Start button clicked. Assembling started.
        /// </summary>
        private void BtnStartClick(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
                              {
                                  _btnStart.Enabled = false;
                                  _btnInsert.Enabled = false;
                                  _nudNumberOfGroupsNeuralHasher.Enabled = false;
                                  _nudNumberOfHashesPerKeyNeuralHasher.Enabled = false;
                                  _btnSaveEnsamble.Enabled = false;
                                  _tbSaveToEnsembleFilename.ReadOnly = true;
                              }));

            /*Ensemble*/
            Action action = () =>
                            {
                                List<Network> networks = new List<Network>();
                                /*Load all the networks*/
                                foreach (KeyValuePair<string, bool> item in dictionaryPathToNetworks)
                                {
                                    if (item.Value)
                                    {
                                        networks.Add((Network) SerializeObject.Load(item.Key));
                                        if (networks[networks.Count - 1].MedianResponces == null)
                                            networks.RemoveAt(networks.Count - 1);
                                    }
                                }
                                if ((networks.Count*10 /*Number of network outputs*/) < (numberofgroupsneuralhasher*numberofhashesperkeyneuralhasher))
                                {
                                    MessageBox.Show(string.Format("Not enough networks to create an ensemble of such size {0} {1}", numberofgroupsneuralhasher, numberofhashesperkeyneuralhasher));
                                    return;
                                }

                                /*Load a network trainer*/
                                IActivationFunction function = networks[0].GetActivation(0);
                                NetTrainer netTrainer = new NetTrainer(modelService);
                                double[][] inputs = null, outputs = null; /*Normalized input/output pairs*/
                                Dictionary<Int32, List<BasicMLData>> trackIdFingerprints = netTrainer.GetNormalizedTrackFingerprints(function, 10, 10);
                                double[][] binaryCodes = netTrainer.GetNormalizedBinaryCodes(function, 10);
                                Tuple<double[][], double[][]> tuple = netTrainer.FillStandardInputsOutputs(trackIdFingerprints, binaryCodes);
                                inputs = tuple.Item1;
                                outputs = tuple.Item2;

                                /*Construct outputs using median response*/
                                int samplesCount = inputs.Length; /*10240*/
                                NeuralNetworkEnsemble nNEnsembe = new NeuralNetworkEnsemble(networks.ToArray()); /*40 networks*/
                                for (int i = 0; i < samplesCount /*1024 * 10*/; i++)
                                {
                                    byte[] outputVec = nNEnsembe.ComputeHash(inputs[i]); /*Hash the inputs, returns 10*40 = 400*/
                                    outputs[i] = new double[outputVec.Length]; /*400*/
                                    for (int j = 0; j < outputVec.Length; j++)
                                    {
                                        outputs[i][j] = outputVec[j]; /*10240x400 matrix*/
                                    }
                                }

                                /*At this point we have a the 10240 hash vectors [0, 400] which represent the outputs for the actual input*/
                                /*Calculate minimal mutual information between those outputs*/
                                MinimalMutualInfoPattern mmiPattern = new MinimalMutualInfoPattern(numberofgroupsneuralhasher, numberofhashesperkeyneuralhasher);
                                mmiPattern.CreatePattern(outputs);
                                nNEnsembe.HashPattern = mmiPattern;
                                nNEnsembe.Save(pathToEnsemble);
                            };

            action.BeginInvoke((result) =>
                               {
                                   /*Ensemble ended*/
                                   action.EndInvoke(result);
                                   Invoke(new Action(() =>
                                                     {
                                                         _btnStart.Enabled = true;
                                                         _btnInsert.Enabled = true;
                                                         _nudNumberOfGroupsNeuralHasher.Enabled = true;
                                                         _nudNumberOfHashesPerKeyNeuralHasher.Enabled = true;
                                                         _btnSaveEnsamble.Enabled = true;
                                                         _tbSaveToEnsembleFilename.ReadOnly = false;
                                                     }));
                                   MessageBox.Show(Resources.EnsemblingEnded, Resources.Success, MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }, action);
        }

        /// <summary>
        ///   New connection string selected
        /// </summary>
        private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///   Compute hashes using NeuralNetworkEnsemble
        /// </summary>
        private void BntComputeHashesClick(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
                              {
                                  _btnSelectStoredEnsemble.Enabled = false;
                                  _bntComputeHashesNeuralHasher.Enabled = false;
                                  _pbProgress.Visible = true;
                              }));

            Action action = ComputeHashesUsingNeuralHasher;

            action.BeginInvoke((result) =>
                               {
                                   action.EndInvoke(result);
                                   Invoke(new Action(() =>
                                                     {
                                                         _btnSelectStoredEnsemble.Enabled = true;
                                                         _bntComputeHashesNeuralHasher.Enabled = true;
                                                         _pbProgress.Visible = false;
                                                     }));
                                   MessageBox.Show(Resources.End, Resources.HashBinsInserted, MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }, action);
        }


        /// <summary>
        ///   Compute hashes using neural network ensemble
        /// </summary>
        private void ComputeHashesUsingNeuralHasher()
        {
            string connectionString = null;
            _cmbValidationConnectionStringNeuralHasher.Invoke(new Action(() => { connectionString = _cmbValidationConnectionStringNeuralHasher.SelectedItem.ToString(); }));

            FingerprintDescriptor descriptor = new FingerprintDescriptor();

            if (String.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show(Resources.SelectValidationDB);
                return;
            }

            string path = _tbStoredEnsembleFilename.Text;
            if (String.IsNullOrEmpty(path))
            {
                MessageBox.Show(Resources.SelectPathToSerializedEnsemble, Resources.SelectFile, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            NeuralNetworkEnsemble ensemble = NeuralNetworkEnsemble.Load(path); /*Load the serialized ensemble used to create hashes*/
            IList<Track> tracks = modelService.ReadTracks(); /*Read all tracks from the database for which the ensemble will create hashes*/
            _pbProgress.Invoke(new Action(() =>
                                          {
                                              _pbProgress.Minimum = 1;
                                              _pbProgress.Maximum = tracks.Count;
                                              _pbProgress.Value = 1;
                                              _pbProgress.Step = 1;
                                          }));

            /*Create hashes for each fingerprint in the database*/
            for (int index = 0; index < tracks.Count; index++)
            {
                Track track = tracks[index];
                IList<Fingerprint> fingerprints;
                try
                {
                    fingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0);
                    if (fingerprints == null)
                        continue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                IList<HashBinNeuralHasher> listToInsert = new List<HashBinNeuralHasher>();
                foreach (Fingerprint fingerprint in fingerprints) /*For each track's fingerprint create hash*/
                {
                    ensemble.ComputeHash(descriptor.DecodeFingerprint(fingerprint.Signature));
                    long[] hashbins = ensemble.ExtractHashBins(); /*Extract hash bin / hash table*/
                    for (int i = 0; i < hashbins.Length; i++)
                    {
                        HashBinNeuralHasher hash = new HashBinNeuralHasher(i, hashbins[i], i, track.Id);
                        listToInsert.Add(hash);
                    }
                }
               // modelService.InsertHashBin(listToInsert);
                _pbProgress.Invoke(new Action(() => _pbProgress.PerformStep()));
            }
        }

        /// <summary>
        ///   Open file dialog to select NN Ensmble
        /// </summary>
        private void BtnSelectClick(object sender, EventArgs e)
        {
            if (_ofdSelectEnsemble.ShowDialog() == DialogResult.OK)
            {
                _tbStoredEnsembleFilename.Text = _ofdSelectEnsemble.FileName;
            }
        }

        /// <summary>
        ///   Start hashing using Min Hash + LSH
        /// </summary>
        private void BtnStartMinHashClick(object sender, EventArgs e)
        {
            string connectionString = _cmbConnectionStringMinHash.SelectedItem.ToString();
            if (String.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show(Resources.ConnectionStringEmpty, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _btnStartMinHash.Enabled = false;
            _pbMinHash.Visible = true;

            numberofgroupsminhash = (int) _nudNumberOfGroupsMinHash.Value; /*L Hash tables 20*/
            numberofhashesperkeyminhash = (int) _nudNumberOfHashesPerKeyMinHash.Value; /*B Keys per table 5*/

            Action action = ComputeHashBinsUsingMinHash; /*Compute Hash Bins using Min Hash*/

            action.BeginInvoke((result) =>
                               {
                                   action.EndInvoke(result);
                                   Invoke(new Action(() =>
                                                     {
                                                         _btnStartMinHash.Enabled = true;
                                                         _pbMinHash.Visible = false;
                                                     }));
                                   MessageBox.Show(Resources.Finished, Resources.InsertionEnded, MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }, action);
        }


        /// <summary>
        ///   Compute Hash Bins using Min Hash algorithm
        /// </summary>
        private void ComputeHashBinsUsingMinHash()
        {
            IList<Track> tracks = modelService.ReadTracks(); /*Read all tracks from the database*/
            _pbMinHash.Invoke(new Action(() => /*Progress bar Settings*/
                                         {
                                             _pbMinHash.Minimum = 1;
                                             _pbMinHash.Maximum = tracks.Count;
                                             _pbMinHash.Value = 1;
                                             _pbMinHash.Step = 1;
                                         }));

            for (int index = 0; index < tracks.Count; index++)
            {
                Track track = tracks[index];
                IList<Fingerprint> fingerprints;
                try
                {
                    fingerprints = modelService.ReadFingerprintsByTrackId(track.Id, 0); /*Read corresponding fingerprints of a specific track*/
                    if (fingerprints == null)
                        continue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                List<HashBinMinHash> listToInsert = new List<HashBinMinHash>(); /*Generate Min Hash signatures*/
                foreach (Fingerprint fingerprint in fingerprints)
                {
                    throw new NotImplementedException();
                    //int[] hashBins = minHashService.ComputeMinHashSignature(fingerprint.Signature); /*For each of the fingerprints*/
                    //Dictionary<int, long> hashTable = minHashService.GroupMinHashToLSHBuckets(hashBins, numberofgroupsminhash, numberofhashesperkeyminhash);
                    //foreach (KeyValuePair<int, long> item in hashTable)
                    //{
                    //    HashBinMinHash hash = new HashBinMinHash(-1, item.Value, item.Key, track.Id, fingerprint.Id);
                    //    listToInsert.Add(hash);
                    //}
                }
                modelService.InsertHashBin(listToInsert); /*Actual insert*/
                _pbMinHash.Invoke(new Action(() => _pbMinHash.PerformStep()));
            }
        }
    }
}