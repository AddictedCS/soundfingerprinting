namespace SoundFingerprinting.SoundTools.DbFiller
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

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Configuration;
    using SoundFingerprinting.DAO;
    using SoundFingerprinting.DAO.Data;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.Strides;

    public partial class WinDbFiller : Form
    {
        private const int MaxThreadToProcessFiles = 4; /*2 MaxThreadToProcessFiles used to process the files*/
        private const int MinTrackLength = 20; /*20 sec - minimal track length*/
        private const int MaxTrackLength = 60 * 10; /*15 min - maximal track length*/

        private readonly List<string> filters = new List<string>(new[] { "*.mp3", "*.wav", "*.ogg", "*.flac" }); /*File filters*/
        private readonly IModelService modelService; /*Dal Signature service*/
        private readonly IFingerprintCommandBuilder fingerprintCommandBuilder;
        private readonly IAudioService audioService;
        private readonly ITagService tagService;
        private volatile int badFiles; /*Number of Bad files*/
        private volatile int duplicates; /*Number of Duplicates*/
        private List<string> fileList; /*List of file to process*/
        private HashAlgorithm hashAlgorithm = HashAlgorithm.LSH; /*Hashing algorithm*/
        private volatile int left; /*Number of left items*/
        private volatile int processed; /*Number of Processed files*/
        private bool stopFlag;
        
        public WinDbFiller(
            IFingerprintCommandBuilder fingerprintCommandBuilder,
            IAudioService audioService,
            ITagService tagService,
            IModelService modelService)
        {
            this.modelService = modelService;
            this.fingerprintCommandBuilder = fingerprintCommandBuilder;
            this.audioService = audioService;
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

            switch (hashAlgorithm)
            {
                case HashAlgorithm.LSH:
                    break;
                case HashAlgorithm.NeuralHasher:
                    throw new NotImplementedException();
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
            _pbTotalSongs.Visible = true;
            _pbTotalSongs.Minimum = 0;
            _pbTotalSongs.Maximum = fileList.Count;
            _pbTotalSongs.Step = 1;
            left = fileList.Count;
            _nudProcessed.Value = processed;
            _nudLeft.Value = left;
        }

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
                                (int)_nudStride.Value,
                                0,
                                new DefaultFingerprintConfiguration().SamplesPerFingerprint);
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
                if (tags == null || tags.IsEmpty)
                {
                    badFiles++;
                    processed++;
                    left--;
                    Invoke(actionAddItems, new object[] { "TAGS ARE NULL", fileList[i], 0, 0 }, Color.Red);
                    Invoke(actionInterface);
                    continue;
                }

                string isrc = tags.ISRC;
                string artist = tags.Artist; // Artist
                string title = tags.Title; // Title
                int releaseYear = tags.Year;
                string album = tags.Album;
                double duration = tags.Duration; // Duration

                // Check whether the duration is OK
                if (duration < MinTrackLength || duration > MaxTrackLength)
                {
                    // Duration too small
                    badFiles++;
                    processed++;
                    left--;
                    Invoke(actionAddItems, new object[] { "Bad duration", fileList[i], 0, 0 }, Color.Red);
                    Invoke(actionInterface);
                    continue;
                }

                // Check whether the tags are properly defined
                if (string.IsNullOrEmpty(isrc) && (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title)))
                {
                    badFiles++;
                    processed++;
                    left--;
                    Invoke(
                        actionAddItems,
                        new object[] { "ISRC Tag is missing. Skipping file...", fileList[i], 0, 0 },
                        Color.Red);
                    Invoke(actionInterface);
                    continue;
                }

                IModelReference trackReference;
                try
                {
                    lock (this)
                    {
                        // Check if this file is already in the database
                        if (IsDuplicateFile(isrc, artist, title))
                        {
                            duplicates++; // There is such file in the database
                            processed++;
                            left--;
                            Invoke(actionInterface);
                            continue;
                        }

                        var track = new TrackData(isrc, artist, title, album, releaseYear, (int)duration);
                        trackReference = modelService.InsertTrack(track); // Insert new Track in the database
                    }
                }
                catch (Exception e)
                {
                    // catch any exception and abort the insertion
                    processed++;
                    left--;
                    badFiles++;
                    MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Invoke(actionInterface);
                    return;
                }

                int count;
                try
                {
                    var hashDatas = fingerprintCommandBuilder
                                        .BuildFingerprintCommand()
                                        .From(fileList[i])
                                        .WithFingerprintConfig(
                                            config =>
                                                {
                                                    config.TopWavelets = topWavelets;
                                                    config.SpectrogramConfig.Stride = stride;
                                                })
                                        .UsingServices(audioService)
                                        .Hash()
                                        .Result; // Create SubFingerprints

                    modelService.InsertHashDataForTrack(hashDatas, trackReference);
                    count = hashDatas.Count;
                }
                catch (Exception e)
                {
                    // catch any exception and abort the insertion
                    MessageBox.Show(e.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    badFiles++;
                    processed++;
                    left--;
                    Invoke(actionInterface);
                    return;
                }

                Invoke(actionAddItems, new object[] { artist, title, isrc, duration, count }, Color.Empty);
                left--;
                processed++;
                Invoke(actionInterface);
            }
        }

        private bool IsDuplicateFile(string isrc, string artist, string title)
        {
            if (!string.IsNullOrEmpty(isrc))
            {
                return modelService.ReadTrackByISRC(isrc) != null;
            }

            return modelService.ReadTrackByArtistAndTitleName(artist, title).Any();
        }

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
