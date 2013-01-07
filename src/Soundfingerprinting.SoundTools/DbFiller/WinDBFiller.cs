// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com

namespace Soundfingerprinting.SoundTools.DbFiller
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Fingerprinting.Configuration;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.NeuralHashing.Ensemble;
    using Soundfingerprinting.SoundTools.Properties;

    using Un4seen.Bass.AddOn.Tags;

    public partial class WinDBFiller : Form
    {
        
        #region Private Fields

        private const int MaxThreadToProcessFiles = 4; /*2 MaxThreadToProcessFiles used to process the files*/

        private const int MinTrackLength = 20; /*20 sec - minimal track length*/
        private const int MaxTrackLength = 60*15; /*15 min - maximal track length*/
        private readonly DaoGateway dalManager = new DaoGateway(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString); /*Dal Fingerprint service*/
        private readonly List<string> filters = new List<string>(new[] {"*.mp3", "*.wav", "*.ogg", "*.flac"}); /*File filters*/
        private readonly object lockObject = new object(); /*Cross Thread operation*/
        private readonly IPermutations permStorage = new DbPermutations(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString); /*Database permutations*/
        private volatile int badFiles; /*Number of Bad files*/
        private volatile int duplicates; /*Number of Duplicates*/
        private NNEnsemble ensemble;
        private List<String> fileList; /*List of file to process*/
        private HashAlgorithm hashAlgorithm = HashAlgorithm.LSH; /*Hashing algorithm*/
        private int hashKeys;
        private int hashTables;
        private volatile int left; /*Number of left items*/
        private List<Album> listOfAllAlbums = new List<Album>(); /*List of all albums*/
        private volatile int processed; /*Number of Processed files*/
        private bool stopFlag;
        private Album unknownAlbum;

        private readonly IFingerprintService fingerprintService;


        #endregion

        #region Constructors

        /// <summary>
        ///   Constructor
        /// </summary>
        public WinDBFiller(IFingerprintService fingerprintService)
        {
            this.fingerprintService = fingerprintService;
            InitializeComponent();
            Icon = Resources.Sound;
            foreach (object item in ConfigurationManager.ConnectionStrings) /*Detect all the connection strings*/
            {
                _cmbDBFillerConnectionString.Items.Add(item.ToString());
            }

            if (_cmbDBFillerConnectionString.Items.Count > 0)
                _cmbDBFillerConnectionString.SelectedIndex = 0;

            _btnStart.Enabled = false;
            _btnStop.Enabled = false;
            _nudThreads.Value = MaxThreadToProcessFiles;
            _pbTotalSongs.Visible = false;
            hashAlgorithm = 0; /**/
            _lbAlgorithm.SelectedIndex = 0; /*Set default algorithm LSH*/

            if (hashAlgorithm == HashAlgorithm.LSH)
            {
                _nudHashKeys.ReadOnly = false;
                _nudHashTables.ReadOnly = false;
            }

            string[] items = Enum.GetNames(typeof (StrideType)); /*Add enumeration types in the combo box*/
            _cmbStrideType.Items.AddRange(items);
            _cmbStrideType.SelectedIndex = 0;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///   Root folder selection (with songs to be inserted)
        /// </summary>
        private void TbRootFolderTextChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _tbRootFolder.Enabled = false;

            fileList = WinUtils.GetFiles(filters, _tbRootFolder.Text);

            Invoke(new Action(() =>
                              {
                                  Cursor = Cursors.Default;
                                  _tbRootFolder.Enabled = true;
                                  if (fileList != null)
                                  {
                                      _nudTotalSongs.Value = fileList.Count;
                                      _btnStart.Enabled = true;
                                  }
                                  _tbSingleFile.Text = null;
                              }));
        }


        /// <summary>
        ///   Root folder selection
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void TbRootFolderMouseDoubleClick(object sender, MouseEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _tbRootFolder.Text = fbd.SelectedPath;
                Cursor = Cursors.WaitCursor;
                _tbRootFolder.Enabled = false;

                fileList = WinUtils.GetFiles(filters, _tbRootFolder.Text);

                Invoke(new Action(() =>
                                  {
                                      Cursor = Cursors.Default;
                                      _tbRootFolder.Enabled = true;
                                      if (fileList != null)
                                      {
                                          _nudTotalSongs.Value = fileList.Count;
                                          _btnStart.Enabled = true;
                                      }
                                      _tbSingleFile.Text = null;
                                  }));
            }
        }

        /// <summary>
        ///   Get a single file to be inserted into database
        /// </summary>
        private void TbSingleFileTextChanged(object sender, EventArgs e)
        {
            if (File.Exists(_tbSingleFile.Text))
            {
                if (filters.Any(filter => filter.Contains(Path.GetExtension(_tbSingleFile.Text))))
                {
                    if (fileList == null)
                        fileList = new List<string>();
                    if (!fileList.Contains(_tbSingleFile.Text))
                    {
                        fileList.Add(_tbSingleFile.Text);
                        _btnStart.Enabled = true;
                    }
                    _nudTotalSongs.Value = fileList.Count;
                }
            }
        }

        /// <summary>
        ///   Get single or multiple files
        /// </summary>
        [FileDialogPermission(SecurityAction.Demand)]
        private void TbSingleFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            string filter = WinUtils.GetMultipleFilter("Audio files", filters);
            OpenFileDialog ofd = new OpenFileDialog {Filter = filter, Multiselect = true};

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSingleFile.Text = null;
                foreach (string file in ofd.FileNames)
                {
                    _tbSingleFile.Text += "\"" + Path.GetFileName(file) + "\" ";
                }

                if (fileList == null)
                    fileList = new List<string>();
                foreach (string file in ofd.FileNames)
                {
                    if (!fileList.Contains(file))
                    {
                        _btnStart.Enabled = true;
                        fileList.Add(file);
                    }
                }
                _nudTotalSongs.Value = fileList.Count;
            }
        }

        /// <summary>
        ///   Select algorithm to perform hashing
        /// </summary>
        private void LbAlgorithmSelectedIndexChanged(object sender, EventArgs e)
        {
            hashAlgorithm = (HashAlgorithm) _lbAlgorithm.SelectedIndex;
            switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    _gbMinHash.Enabled = true;
                    _gbHasher.Enabled = false;
                    break;
                case HashAlgorithm.NeuralHasher:
                    _gbMinHash.Enabled = false;
                    _gbHasher.Enabled = true;
                    break;
                case HashAlgorithm.None:
                    _gbMinHash.Enabled = false;
                    _gbHasher.Enabled = false;
                    break;
            }
        }

        /// <summary>
        ///   Start inserting into the database
        /// </summary>
        private void BtnStartClick(object sender, EventArgs e)
        {
            string connectionString = _cmbDBFillerConnectionString.SelectedItem.ToString(); //Set Connection String
            try
            {
                dalManager.SetConnectionString(connectionString); //Try Connection String
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                FadeAllControls(false);
                return;
            }
            if (!String.IsNullOrEmpty(_tbRootFolder.Text) || !String.IsNullOrEmpty(_tbSingleFile.Text) && fileList == null)
            {
                fileList = new List<string>();
                if (!String.IsNullOrEmpty(_tbRootFolder.Text))
                    TbRootFolderTextChanged(this, null);
                if (!String.IsNullOrEmpty(_tbSingleFile.Text))
                    TbSingleFileTextChanged(this, null);
            }
            if (fileList == null || fileList.Count == 0)
            {
                MessageBox.Show(Resources.FileListEmpty, Resources.FileListEmptyCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FadeAllControls(true); //Fade all controls

            int rest = fileList.Count%MaxThreadToProcessFiles;
            int filesPerThread = fileList.Count/MaxThreadToProcessFiles;

            listOfAllAlbums = dalManager.ReadAlbums(); //Get all albums
            unknownAlbum = dalManager.ReadUnknownAlbum(); //Read unknown album

            int topWavelets = (int) _nudTopWav.Value;
            fingerprintService.FingerprintConfig.TopWavelets = topWavelets;
             switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    hashTables = (int) _nudHashTables.Value; //If LSH is used # of Hash tables
                    hashKeys = (int) _nudHashKeys.Value; //If LSH is used # of keys per table
                    break;
                case HashAlgorithm.NeuralHasher:
                    if (String.IsNullOrEmpty(_tbPathToEnsemble.Text)) //Check if the path to ensemble is specified
                    {
                        MessageBox.Show(Resources.SpecifyPathToNetworkEnsemble, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FadeAllControls(false);
                        return;
                    }
                    try
                    {
                        ensemble = NNEnsemble.Load(_tbPathToEnsemble.Text); //Load the ensemble
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        FadeAllControls(false);

                        return;
                    }
                    break;
                case HashAlgorithm.None:
                    break;
            }
            BeginInvoke(new Action(() => { }), null);

            ResetControls();
            int runningThreads = MaxThreadToProcessFiles;
            for (int i = 0; i < MaxThreadToProcessFiles; i++) //Start asynchronous operation
            {
                int start = i*filesPerThread; //Define start and end indexes
                int end = (i == MaxThreadToProcessFiles - 1) ? i*filesPerThread + filesPerThread + rest : i*filesPerThread + filesPerThread;
                Action<int, int> action = InsertInDatabase;
                action.BeginInvoke(start, end,
                    (result) =>
                    {
                        //End Asynchronous operation
                        Action<int, int> item = (Action<int, int>) result.AsyncState;
                        item.EndInvoke(result);
                        Interlocked.Decrement(ref runningThreads);
                        if (runningThreads == 0)
                        {
                            /********* END OF INSERTION PROCESS HERE!********/

                            Invoke(new Action(() =>
                                              {
                                                  _pbTotalSongs.Visible = false;
                                                  FadeAllControls(false);
                                                  _tbRootFolder.Text = null;
                                                  _tbSingleFile.Text = null;
                                              }));
                            MessageBox.Show(Resources.InsertionEnded, Resources.End, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }, action);
            }
        }

        /// <summary>
        ///   Stop inserting into the database
        /// </summary>
        private void BtnStopClick(object sender, EventArgs e)
        {
            stopFlag = true;
        }

        /// <summary>
        ///   Export file into an .csv
        /// </summary>
        private void BtnExportClick(object sender, EventArgs e)
        {
            WinUtils.ExportInExcel(_dgvFillDatabase);
        }

        /// <summary>
        ///   Browse for Ensemble files
        /// </summary>
        private void BtnBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {Filter = Resources.FileFilterEnsemble};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToEnsemble.Text = ofd.FileName;
            }
        }

        #endregion

        /// <summary>
        ///   Reset all controls
        /// </summary>
        private void ResetControls()
        {
            duplicates = 0;
            badFiles = 0;
            processed = 0;
            left = 0;
            _pbTotalSongs.Visible = true; //Set the progress bar control
            _pbTotalSongs.Minimum = 0;
            _pbTotalSongs.Maximum = fileList.Count;
            _pbTotalSongs.Step = 1;
            left = fileList.Count;
            _nudProcessed.Value = processed;
            _nudLeft.Value = left;
        }

        /// <summary>
        ///   Actual synchronous insert in the database
        /// </summary>
        /// <param name = "start">Start index</param>
        /// <param name = "end">End index</param>
        private void InsertInDatabase(int start, int end)
        {
            BassAudioService audioService = new BassAudioService(); //Proxy used to read from file
            IFingerprintingConfiguration configuration = new DefaultFingerprintingConfiguration();
            IStride stride = null;
            Invoke(new Action(() =>
                              {
                                  stride = WinUtils.GetStride((StrideType) _cmbStrideType.SelectedIndex, //Get stride according to the underlying combo box selection
                                      (int)_nudStride.Value, 0, configuration.SamplesPerFingerprint);
                              }), null);

            Action actionInterface =
                () =>
                {
                    _pbTotalSongs.PerformStep();
                    _nudProcessed.Value = processed;
                    _nudLeft.Value = left;
                    _nudBadFiles.Value = badFiles;
                    _nudDetectedDuplicates.Value = duplicates;
                };

            Action<object[], Color> actionAddItems =
                (parameters, color) =>
                {
                    int index = _dgvFillDatabase.Rows.Add(parameters);
                    _dgvFillDatabase.FirstDisplayedScrollingRowIndex = index;
                    if (color != Color.Empty)
                        _dgvFillDatabase.Rows[index].DefaultCellStyle.BackColor = color;
                };


            for (int i = start; i < end; i++) //Process the corresponding files
            {
                if (stopFlag)
                    return;

                TAG_INFO tags = audioService.GetTagInfoFromFile(fileList[i]); //Get Tags from file
                if (tags == null)
                {
                    //TAGS are null
                    badFiles++;
                    Invoke(actionAddItems, new Object[] {"TAGS ARE NULL", fileList[i], 0, 0}, Color.Red);
                    continue;
                }

                string artist = tags.artist; //Artist
                string title = tags.title; //Title
                double duration = tags.duration; //Duration

                if (duration < MinTrackLength || duration > MaxTrackLength) //Check whether the duration is ok
                {
                    //Duration too small
                    badFiles++;
                    Invoke(actionAddItems, new Object[] {"BAD DURATION", fileList[i], 0, 0}, Color.Red);
                    continue;
                }

                if (String.IsNullOrEmpty(artist) || String.IsNullOrEmpty(title)) //Check whether the tags are properly defined
                {
                    //Title or Artist tag is null
                    badFiles++;
                    Invoke(actionAddItems, new Object[] {"TAGS MISSING", fileList[i], 0, 0}, Color.Red);
                    continue;
                }

                Album album = GetCoresspondingAlbum(tags); //Get Album (whether new or unknown or aborted)
                if (album == null) //Check whether the user aborted
                    return;
                Track track = null;
                lock (this)
                {
                    try
                    {
                        if (dalManager.ReadTrackByArtistAndTitleName(artist, title) != null) // Check if this file is already in the database
                        {
                            duplicates++; //There is such file in the database
                            continue;
                        }

                        track = new Track(-1, artist, title, album.Id, (int) duration); //Create New Track
                        dalManager.InsertTrack(track); //Insert new Track in the database
                    }
                    catch (Exception e)
                    {
                        //catch any exception and abort the insertion
                        MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                int count = 0;
                try
                {
                    List<bool[]> images = fingerprintService.CreateFingerprints(fileList[i]); //Create Fingerprints and insert them in database
                    List<Fingerprint> inserted = Fingerprint.AssociateFingerprintsToTrack(images, track.Id);
                    dalManager.InsertFingerprint(inserted);
                    count = inserted.Count;

                    switch (hashAlgorithm) //Hash if there is a need in doing so
                    {
                        case HashAlgorithm.LSH: //LSH + Min Hash has been chosen
                            HashFingerprintsUsingMinHash(inserted, track, hashTables, hashKeys);
                            break;
                        case HashAlgorithm.NeuralHasher:
                            HashFingerprintsUsingNeuralHasher(inserted, track);
                            break;
                        case HashAlgorithm.None:
                            break;
                    }
                }
                catch (Exception e)
                {
                    //catch any exception and abort the insertion
                    MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Invoke(actionAddItems, new Object[] {artist, title, album.Name, duration, count}, Color.Empty);
                left--;
                processed++;

                Invoke(actionInterface);
            }
        }

        /// <summary>
        ///   Hash Fingerprints using Min-Hash algorithm
        /// </summary>
        /// <param name = "listOfFingerprintsToHash">List of fingerprints already inserted in the database</param>
        /// <param name = "track">Track of the corresponding fingerprints</param>
        /// <param name = "hashTables">Number of hash tables</param>
        /// <param name = "hashKeys">Number of hash keys</param>
        private void HashFingerprintsUsingMinHash(IEnumerable<Fingerprint> listOfFingerprintsToHash, Track track, int hashTables, int hashKeys)
        {
            List<HashBinMinHash> listToInsert = new List<HashBinMinHash>();
            MinHash minHash = new MinHash(permStorage);
            foreach (Fingerprint fingerprint in listOfFingerprintsToHash)
            {
                int[] hashBins = minHash.ComputeMinHashSignature(fingerprint.Signature); //Compute Min Hashes
                Dictionary<int, long> hashTable = minHash.GroupMinHashToLSHBuckets(hashBins, hashTables, hashKeys);
                foreach (KeyValuePair<int, long> item in hashTable)
                {
                    HashBinMinHash hash = new HashBinMinHash(-1, item.Value, item.Key, track.Id, fingerprint.Id);
                    listToInsert.Add(hash);
                }
            }
            dalManager.InsertHashBin(listToInsert); //Insert 
        }

        /// <summary>
        ///   Hash Fingerprints using Neural hasher component for solving k-nearest neighbor problem
        /// </summary>
        /// <param name = "listOfFingerprintsToHash">List of fingerprints already inserted in the database</param>
        /// <param name = "track">Track of the corresponding fingerprints</param>
        private void HashFingerprintsUsingNeuralHasher(IEnumerable<Fingerprint> listOfFingerprintsToHash, Track track)
        {
            FingerprintDescriptor descriptor = new FingerprintDescriptor();
            List<HashBinNeuralHasher> listToInsert = new List<HashBinNeuralHasher>();
            foreach (Fingerprint fingerprint in listOfFingerprintsToHash)
            {
                ensemble.ComputeHash(descriptor.DecodeFingerprint(fingerprint.Signature));
                long[] hashbins = ensemble.ExtractHashBins();
                for (int i = 0; i < hashbins.Length; i++)
                {
                    HashBinNeuralHasher hash = new HashBinNeuralHasher(i, hashbins[i], i, track.Id);
                    listToInsert.Add(hash);
                }
            }
            dalManager.InsertHashBin(listToInsert);
        }

        /// <summary>
        ///   Get corresponding Album
        /// </summary>
        /// <param name = "tags">File Tags</param>
        /// <returns>Album to be used while inserting the fingerprints into the database</returns>
        private Album GetCoresspondingAlbum(TAG_INFO tags)
        {
            string album = tags.album;
            Album albumToInsert = null;
            if (String.IsNullOrEmpty(album)) //Check whether the album is not null
            {
                albumToInsert = unknownAlbum; //The album is unknown
            }
            else
            {
                lock (lockObject)
                {
                    foreach (Album a in listOfAllAlbums)
                        if (a.Name == album)
                        {
                            albumToInsert = a; //There is already such an album in the database
                            break;
                        }
                    if (albumToInsert == null) //No such album in the database INSERT!
                    {
                        int releaseYear = -1;
                        try
                        {
                            releaseYear = Convert.ToInt32(tags.year.Split('-')[0].Trim());
                        }
                        catch (Exception)
                        {
                            /*swallow*/
                        }

                        albumToInsert = (releaseYear < 1900 || releaseYear > 2200) ? new Album(-1, album) : new Album(-1, album, releaseYear);
                        try
                        {
                            dalManager.InsertAlbum(albumToInsert); //Insert new ALBUM
                        }
                        catch (DalGatewayException ex)
                        {
                            if (MessageBox.Show(ex.Message + "\n Continue?", Resources.ExceptioInDal, MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                                return null;
                            albumToInsert = unknownAlbum;
                        }
                        if (albumToInsert != unknownAlbum)
                            listOfAllAlbums.Add(albumToInsert); //Modify Local Variable
                    }
                }
            }
            return albumToInsert;
        }

        /// <summary>
        ///   Fade out all controls
        /// </summary>
        /// <param name = "visible">Read only controls</param>
        private void FadeAllControls(bool visible)
        {
            Invoke(new Action(
                () =>
                {
                    _cmbDBFillerConnectionString.Enabled = !visible;
                    _tbRootFolder.Enabled = !visible;
                    _tbSingleFile.Enabled = !visible;
                    _lbAlgorithm.Enabled = !visible;
                    _nudHashKeys.Enabled = !visible;
                    _nudHashTables.Enabled = !visible;
                    _nudStride.Enabled = !visible;
                    _btnStart.Enabled = !visible;
                    _btnStop.Enabled = visible;
                    _nudTopWav.Enabled = !visible;
                    _cmbStrideType.Enabled = !visible;
                }));
        }
    }
}