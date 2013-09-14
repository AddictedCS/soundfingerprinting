namespace SoundFingerprinting.SoundTools.DbFiller
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Dao.Entities;
    using SoundFingerprinting.Hashing.LSH;
    using SoundFingerprinting.Hashing.NeuralHashing.Ensemble;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Strides;

    public partial class WinDbFiller : Form
    {
        private const int MaxThreadToProcessFiles = 4; /*2 MaxThreadToProcessFiles used to process the files*/
        private const int MinTrackLength = 20; /*20 sec - minimal track length*/
        private const int MaxTrackLength = 60 * 15; /*15 min - maximal track length*/

        private readonly List<string> filters = new List<string>(new[] { "*.mp3", "*.wav", "*.ogg", "*.flac" }); /*File filters*/
        private readonly object lockObject = new object(); /*Cross Thread operation*/
        private readonly IModelService modelService; /*Dal Signature service*/
        private readonly ILSHService lshService;
        private readonly IFingerprintUnitBuilder fingerprintUnitBuilder;
        private readonly ITagService tagService;
        private volatile int badFiles; /*Number of Bad files*/
        private volatile int duplicates; /*Number of Duplicates*/
        private List<string> fileList; /*List of file to process*/
        private HashAlgorithm hashAlgorithm = HashAlgorithm.LSH; /*Hashing algorithm*/
        private int hashKeys;
        private int hashTables;
        private volatile int left; /*Number of left items*/
        private IList<Album> listOfAllAlbums = new List<Album>(); /*List of all albums*/
        private volatile int processed; /*Number of Processed files*/
        private bool stopFlag;
        private Album unknownAlbum;
        
        public WinDbFiller(
            IFingerprintUnitBuilder fingerprintUnitBuilder,
            ITagService tagService,
            IModelService modelService,
            ILSHService lshService)
        {
            this.modelService = modelService;
            this.lshService = lshService;
            this.fingerprintUnitBuilder = fingerprintUnitBuilder;
            this.tagService = tagService;
            InitializeComponent();
            Icon = Resources.Sound;
            foreach (object item in ConfigurationManager.ConnectionStrings)
            {
                _cmbDBFillerConnectionString.Items.Add(item.ToString());
            }

            if (_cmbDBFillerConnectionString.Items.Count > 0)
            {
                _cmbDBFillerConnectionString.SelectedIndex = 0;
            }

            _btnStart.Enabled = false;
            _btnStop.Enabled = false;
            _nudThreads.Value = MaxThreadToProcessFiles;
            _pbTotalSongs.Visible = false;
            hashAlgorithm = 0;
            _lbAlgorithm.SelectedIndex = 0; /*Set default algorithm LSH*/

            if (hashAlgorithm == HashAlgorithm.LSH)
            {
                _nudHashKeys.ReadOnly = false;
                _nudHashTables.ReadOnly = false;
            }

            string[] items = Enum.GetNames(typeof(StrideType)); /*Add enumeration types in the combo box*/
            _cmbStrideType.Items.AddRange(items);
            _cmbStrideType.SelectedIndex = 0;
            fileList = new List<string>();
        }

        private void RootFolderIsSelected(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _tbRootFolder.Enabled = false;
            fileList = WinUtils.GetFiles(filters, _tbRootFolder.Text);
            Invoke(new Action(RestoreCursorShowTotalFilesCount));
        }

        private void RestoreCursorShowTotalFilesCount()
        {
            Cursor = Cursors.Default;
            _tbRootFolder.Enabled = true;
            if (fileList != null)
            {
                _nudTotalSongs.Value = fileList.Count;
                _btnStart.Enabled = true;
            }

            _tbSingleFile.Text = null;
        }

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
                Invoke(new Action(RestoreCursorShowTotalFilesCount));
            }
        }

        private void TbSingleFileTextChanged(object sender, EventArgs e)
        {
            if (File.Exists(_tbSingleFile.Text))
            {
                if (filters.Any(filter => filter.Contains(Path.GetExtension(_tbSingleFile.Text))))
                {
                    if (!fileList.Contains(_tbSingleFile.Text))
                    {
                        fileList.Add(_tbSingleFile.Text);
                        _btnStart.Enabled = true;
                    }

                    _nudTotalSongs.Value = fileList.Count;
                }
            }
        }

        [FileDialogPermission(SecurityAction.Demand)]
        private void TbSingleFileMouseDoubleClick(object sender, MouseEventArgs e)
        {
            string filter = WinUtils.GetMultipleFilter("Audio files", filters);
            OpenFileDialog ofd = new OpenFileDialog { Filter = filter, Multiselect = true };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbSingleFile.Text = null;
                foreach (string file in ofd.FileNames)
                {
                    _tbSingleFile.Text += "\"" + Path.GetFileName(file) + "\" ";
                }

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

        private void LbAlgorithmSelectedIndexChanged(object sender, EventArgs e)
        {
            hashAlgorithm = (HashAlgorithm)_lbAlgorithm.SelectedIndex;
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

        private void BtnStartClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_tbRootFolder.Text) || !string.IsNullOrEmpty(_tbSingleFile.Text) && fileList == null)
            {
                fileList = new List<string>();
                if (!string.IsNullOrEmpty(_tbRootFolder.Text))
                {
                    RootFolderIsSelected(this, null);
                }

                if (!string.IsNullOrEmpty(_tbSingleFile.Text))
                {
                    TbSingleFileTextChanged(this, null);
                }
            }

            if (!fileList.Any())
            {
                MessageBox.Show(Resources.FileListEmpty, Resources.FileListEmptyCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FadeAllControls(true); // Fade all controls

            int rest = fileList.Count % MaxThreadToProcessFiles;
            int filesPerThread = fileList.Count / MaxThreadToProcessFiles;

            listOfAllAlbums = modelService.ReadAlbums(); // Get all albums
            unknownAlbum = modelService.ReadUnknownAlbum(); // Read unknown albums

            switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    hashTables = (int)_nudHashTables.Value; // If LSH is used # of Hash tables
                    hashKeys = (int)_nudHashKeys.Value; // If LSH is used # of keys per table
                    break;
                case HashAlgorithm.NeuralHasher:
                    if (string.IsNullOrEmpty(_tbPathToEnsemble.Text))
                    {
                        // Check if the path to ensemble is specified
                        MessageBox.Show(Resources.SpecifyPathToNetworkEnsemble, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        FadeAllControls(false);
                        return;
                    }

                    try
                    {
                        NeuralNetworkEnsemble.Load(_tbPathToEnsemble.Text); // Load the ensemble
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
            for (int i = 0; i < MaxThreadToProcessFiles; i++)
            {
                // Start asynchronous operation
                int start = i * filesPerThread; // Define start and end indexes
                int end = (i == MaxThreadToProcessFiles - 1) ? i * filesPerThread + filesPerThread + rest : i * filesPerThread + filesPerThread;
                Action<int, int> action = InsertInDatabase;
                action.BeginInvoke(
                    start,
                    end,
                    result =>
                        {
                            // End Asynchronous operation
                            Action<int, int> item = (Action<int, int>)result.AsyncState;
                            item.EndInvoke(result);
                            Interlocked.Decrement(ref runningThreads);
                            if (runningThreads == 0)
                            {
                                /********* END OF INSERTION PROCESS HERE!********/

                                Invoke(
                                    new Action(
                                        () =>
                                            {
                                                _pbTotalSongs.Visible = false;
                                                FadeAllControls(false);
                                                _tbRootFolder.Text = null;
                                                _tbSingleFile.Text = null;
                                            }));
                                MessageBox.Show(Resources.InsertionEnded, Resources.End, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        },
                    action);
            }
        }

        private void BtnStopClick(object sender, EventArgs e)
        {
            stopFlag = true;
        }

        private void BtnExportClick(object sender, EventArgs e)
        {
            WinUtils.ExportInExcel(_dgvFillDatabase);
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = Resources.FileFilterEnsemble };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbPathToEnsemble.Text = ofd.FileName;
            }
        }

        private void ResetControls()
        {
            duplicates = 0;
            badFiles = 0;
            processed = 0;
            left = 0;
            _pbTotalSongs.Visible = true; // Set the progress bar control
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
            int topWavelets = (int)_nudTopWav.Value;
            IStride stride = null;
            Invoke(
                new Action(
                    () =>
                        {
                            stride = WinUtils.GetStride(
                                (StrideType)_cmbStrideType.SelectedIndex,
                                // Get stride according to the underlying combo box selection
                                (int)_nudStride.Value,
                                0,
                                new DefaultFingerprintingConfiguration().SamplesPerFingerprint);
                        }),
                null);

            Action actionInterface = () =>
                {
                    _pbTotalSongs.PerformStep();
                    _nudProcessed.Value = processed;
                    _nudLeft.Value = left;
                    _nudBadFiles.Value = badFiles;
                    _nudDetectedDuplicates.Value = duplicates;
                };

            Action<object[], Color> actionAddItems = (parameters, color) =>
                {
                    int index = _dgvFillDatabase.Rows.Add(parameters);
                    _dgvFillDatabase.FirstDisplayedScrollingRowIndex = index;
                    if (color != Color.Empty)
                    {
                        _dgvFillDatabase.Rows[index].DefaultCellStyle.BackColor = color;
                    }
                };

            for (int i = start; i < end; i++)
            {
                // Process the corresponding files
                if (stopFlag)
                {
                    return;
                }

                TagInfo tags = tagService.GetTagInfo(fileList[i]); // Get Tags from file
                if (tags == null)
                {
                    // TAGS are null
                    badFiles++;
                    Invoke(actionAddItems, new object[] { "TAGS ARE NULL", fileList[i], 0, 0 }, Color.Red);
                    continue;
                }

                string artist = tags.Artist; // Artist
                string title = tags.Title; // Title
                double duration = tags.Duration; // Duration

                // Check whether the duration is OK
                if (duration < MinTrackLength || duration > MaxTrackLength)
                {
                    // Duration too small
                    badFiles++;
                    Invoke(actionAddItems, new object[] { "BAD DURATION", fileList[i], 0, 0 }, Color.Red);
                    continue;
                }

                // Check whether the tags are properly defined
                if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
                {
                    // Title or Artist tag is null
                    badFiles++;
                    Invoke(actionAddItems, new object[] { "TAGS MISSING", fileList[i], 0, 0 }, Color.Red);
                    continue;
                }

                Album album = GetCoresspondingAlbum(tags); // Get Album (whether new or unknown or aborted)

                if (album == null)
                {
                    return;
                }

                Track track;
                try
                {
                    lock (this)
                    {
                        // Check if this file is already in the database
                        if (modelService.ReadTrackByArtistAndTitleName(artist, title) != null)
                        {
                            duplicates++; // There is such file in the database
                            continue;
                        }

                        track = new Track(artist, title, album.Id, (int)duration);
                        modelService.InsertTrack(track); // Insert new Track in the database
                    }
                }
                catch (Exception e)
                {
                    // catch any exception and abort the insertion
                    MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int count;
                try
                {
                    List<SubFingerprint> subFingerprintsToTrack = fingerprintUnitBuilder
                           .BuildAudioFingerprintingUnit()
                           .From(fileList[i])
                           .WithCustomAlgorithmConfiguration(
                                config =>
                                    {
                                        config.TopWavelets = topWavelets;
                                        config.Stride = stride;
                                    })
                            .FingerprintIt()
                            .HashIt()
                            .ForTrack(track.Id)
                            .Result; // Create SubFingerprints

                    modelService.InsertSubFingerprint(subFingerprintsToTrack);
                    count = subFingerprintsToTrack.Count;

                    switch (hashAlgorithm)
                    {
                            // Hash if there is a need in doing so
                        case HashAlgorithm.LSH: // LSH + Min Hash has been chosen
                            HashSubFingerprintsUsingMinHash(subFingerprintsToTrack);
                            break;
                        case HashAlgorithm.NeuralHasher:
                            throw new NotImplementedException();
                        case HashAlgorithm.None:
                            break;
                    }
                }
                catch (Exception e)
                {
                    // catch any exception and abort the insertion
                    MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Invoke(actionAddItems, new object[] { artist, title, album.Name, duration, count }, Color.Empty);
                left--;
                processed++;

                Invoke(actionInterface);
            }
        }

        private void HashSubFingerprintsUsingMinHash(IEnumerable<SubFingerprint> listOfSubFingerprintsToHash)
        {
            List<HashBinMinHash> listToInsert = new List<HashBinMinHash>();
            foreach (SubFingerprint subFingerprint in listOfSubFingerprintsToHash)
            {
                long[] buckets = lshService.Hash(subFingerprint.Signature, hashTables, hashKeys);
                int tableCount = 1;
                foreach (long bucket in buckets)
                {
                    HashBinMinHash hash = new HashBinMinHash(bucket, tableCount++, subFingerprint.Id);
                    listToInsert.Add(hash);
                }
            }

            modelService.InsertHashBin(listToInsert);
        }

        private Album GetCoresspondingAlbum(TagInfo tags)
        {
            string album = tags.Album;
            Album albumToInsert = null;
            if (string.IsNullOrEmpty(album))
            {
                albumToInsert = unknownAlbum; // The album is unknown
            }
            else
            {
                lock (lockObject)
                {
                    albumToInsert = listOfAllAlbums.FirstOrDefault(a => a.Name == album);
                    if (albumToInsert == null)
                    {
                        // No such album in the database INSERT!
                        int releaseYear = -1;
                        try
                        {
                            releaseYear = Convert.ToInt32(tags.Year.Split('-')[0].Trim());
                        }
                        catch (Exception)
                        {
                            /*swallow*/
                            Debug.WriteLine("Release Year is in a bad format. Continuw processing...");
                        }

                        albumToInsert = new Album(album, releaseYear);
                        try
                        {
                            modelService.InsertAlbum(albumToInsert); // Insert new ALBUM
                        }
                        catch (Exception ex)
                        {
                            if (MessageBox.Show(ex.Message + "\n Continue?", Resources.ExceptioInDal, MessageBoxButtons.OKCancel, MessageBoxIcon.Error)
                                == DialogResult.Cancel)
                            {
                                return null;
                            }

                            albumToInsert = unknownAlbum;
                        }

                        if (albumToInsert != unknownAlbum)
                        {
                            listOfAllAlbums.Add(albumToInsert); // Modify Local Variable
                        }
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
            Invoke(
                new Action(
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