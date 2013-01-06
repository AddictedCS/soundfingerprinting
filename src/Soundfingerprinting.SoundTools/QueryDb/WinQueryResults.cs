namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    using Soundfingerprinting.AudioProxies;
    using Soundfingerprinting.AudioProxies.Strides;
    using Soundfingerprinting.DbStorage;
    using Soundfingerprinting.DbStorage.Entities;
    using Soundfingerprinting.Fingerprinting;
    using Soundfingerprinting.Hashing;
    using Soundfingerprinting.NeuralHashing.Ensemble;
    using Soundfingerprinting.SoundTools.Properties;

    using Un4seen.Bass.AddOn.Tags;

    /// <summary>
    ///   <c>WinQueryResult</c> form, which will show all the results related to the recognition process
    /// </summary>
    public partial class WinQueryResults : Form
    {
        /// <summary>
        ///   Minimum track length (in seconds)
        /// </summary>
        private const int MinTrackLength = 20;

        /// <summary>
        ///   Maximum track length (in seconds)
        /// </summary>
        private const int MaxTrackLength = 60 * 20;

        #region DataGrid Columns

        private const string ColSongName = "SongNameTitle";
        private const string ColResultName = "ResultSongNameTitle";
        private const string ColHit = "CounterHit";
        private const string ColResult = "Result";
        private const string ColPosition = "Position";
        private const string ColHammingAvg = "HammingAvg";
        private const string ColHammingAvgByTrack = "HammingAvgByTrack";
        private const string ColMinHamming = "MinHamming";
        private const string ColSortValue = "SortValue";
        private const string ColStartQueryIndex = "StartQueryIndex";
        private const string ColTotalTables = "TotalTables";
        private const string ColTotalTrackVotes = "TotalTrackVotes";
        private const string ColElapsedTime = "ElapsedTime";
        private const string ColTotalFingerprints = "TotalFingerprints";
        private const string ColSimilarity = "Similarity";

        #endregion

        /// <summary>
        ///   Connection string to the underlying data source
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        ///   Data access service, allows one to access the underlying data source
        /// </summary>
        private readonly DaoGateway dalManager;

        /// <summary>
        ///   Network ensemble
        /// </summary>
        private readonly NNEnsemble ensemble;

        /// <summary>
        ///   List of files [.mp3] which are going to be recognized
        /// </summary>
        private readonly List<string> fileList;

        /// <summary>
        ///   Number of hash keys per table used in MinHash + LSH schema [normally 5]
        /// </summary>
        private readonly int hashKeys;

        /// <summary>
        ///   Number of hash tables used in the MinHash + LSH schema [normally 20]
        /// </summary>
        private readonly int hashTables;

        /// <summary>
        ///   Permutation storage
        /// </summary>
        private readonly IPermutations permStorage;

        /// <summary>
        ///   Audio audioService used for reading the data from the .mp3 files
        /// </summary>
        private readonly BassAudioService audioService = new BassAudioService();

        /// <summary>
        ///   The size of the query [E.g. 253 samples]
        /// </summary>
        private readonly IStride queryStride;

        /// <summary>
        ///   Number of fingerprints to analyze
        /// </summary>
        private readonly int secondsToAnalyze;

        /// <summary>
        ///   Start second
        /// </summary>
        private readonly int startSecond;

        /// <summary>
        ///   Recognition threshold 17% ~5 tables from 25 hash functions
        /// </summary>
        private readonly int threshold;

        /// <summary>
        ///   Running thread allows one to Abort forcibly the recognition
        /// </summary>
        private Thread runningThread;

        /// <summary>
        ///   Flag which signalizes stop of the recognition process
        /// </summary>
        private bool stopQuerying;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinQueryResults"/> class. 
        ///   Protected constructor of WinQueryResults class
        /// </summary>
        /// <param name="connectionString">
        /// Connection string used for the underlying data source
        /// </param>
        /// <param name="secondsToAnalyze">
        /// Number of consequent fingerprints to analyze
        /// </param>
        /// <param name="startSecond">
        /// Starting seconds
        /// </param>
        /// <param name="stride">
        /// Stride used in the query
        /// </param>
        /// <param name="topWavelets">
        /// Number of top wavelets to analyze
        /// </param>
        /// <param name="fileList">
        /// List of all files to be recognized
        /// </param>
        protected WinQueryResults(
            string connectionString,
            int secondsToAnalyze,
            int startSecond,
            IStride stride,
            int topWavelets,
            List<string> fileList)
        {
            InitializeComponent(); /*Initialize Designer Components*/
            Icon = Resources.Sound;
            this.connectionString = connectionString;
            dalManager = new DaoGateway(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);
            permStorage = new DbPermutations(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);

            dalManager.SetConnectionString(this.connectionString); /*Set connection string for DAL service*/
            this.secondsToAnalyze = secondsToAnalyze; /*Number of fingerprints to analyze from each song*/
            this.startSecond = startSecond;
            this.fileList = fileList; /*List of files to analyze*/
            _dgvResults.Columns.Add(ColSongName, "Initial Song");
            _dgvResults.Columns[ColSongName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColResultName, "Result Song");
            _dgvResults.Columns[ColResultName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColPosition, "Position");
            _dgvResults.Columns[ColPosition].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColResult, "Result");
            _dgvResults.Columns[ColResult].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColHammingAvg, "Hamming Avg.");
            _dgvResults.Columns[ColHammingAvg].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            queryStride = stride;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinQueryResults"/> class. 
        /// Public constructor for LSH + Min Hash algorithm
        /// </summary>
        /// <param name="connectionString">
        /// Connection string
        /// </param>
        /// <param name="secondsToAnalyze">
        /// Number of fingerprints to analyze
        /// </param>
        /// <param name="startSecond">
        /// Starting second of analysis
        /// </param>
        /// <param name="stride">
        /// Stride
        /// </param>
        /// <param name="fileList">
        /// File list
        /// </param>
        /// <param name="hashTables">
        /// Min hash hash tables
        /// </param>
        /// <param name="hashKeys">
        /// Min hash hash keys
        /// </param>
        /// <param name="thresholdTables">
        /// Number of threshold tables
        /// </param>
        /// <param name="topWavelets">
        /// Number of top wavelets to consider
        /// </param>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
        public WinQueryResults(string connectionString, int secondsToAnalyze, int startSecond, IStride stride, List<string> fileList, int hashTables, int hashKeys, int thresholdTables, int topWavelets)
            : this(connectionString, secondsToAnalyze, startSecond, stride, topWavelets, fileList)
        {
            this.hashTables = hashTables;
            this.hashKeys = hashKeys;
            threshold = thresholdTables;
            _dgvResults.Columns.Add(ColHammingAvgByTrack, "Hamming Avg. By Track");
            // ReSharper disable PossibleNullReferenceException
            _dgvResults.Columns[ColHammingAvgByTrack].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColMinHamming, "Min. Hamming");
            _dgvResults.Columns[ColMinHamming].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColSortValue, "Sort Value");
            _dgvResults.Columns[ColSortValue].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalTables, "Total Table Votes");
            _dgvResults.Columns[ColTotalTables].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalTrackVotes, "Total Track Votes");
            _dgvResults.Columns[ColTotalTrackVotes].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColStartQueryIndex, "Query Index Start");
            _dgvResults.Columns[ColStartQueryIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColTotalFingerprints, "Total Fingerprints");
            _dgvResults.Columns[ColTotalFingerprints].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColSimilarity, "Min Similarity");
            _dgvResults.Columns[ColSimilarity].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(ColElapsedTime, "Elapsed Time");
            _dgvResults.Columns[ColElapsedTime].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // ReSharper restore PossibleNullReferenceException
            _btnExport.Enabled = false;
            _nudTotal.Value = this.fileList.Count;
            Action action = ExtractCandidatesWithMinHashAlgorithm; /*Extract candidates using MinHash + LSH algorithm*/
            action.BeginInvoke(
                (result) =>
                    {
                        try
                        {
                            action.EndInvoke(result);
                        }
                        catch (ThreadAbortException)
                        {
                            /*Recognition aborted*/
                            return;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(
                                Resources.RecognitionEndedWithAnError + e.Message + Resources.LineFeed + e.StackTrace,
                                Resources.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                        MessageBox.Show(
                            Resources.RecognitionEnded,
                            Resources.RecognitionEnded,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        Invoke(
                            new Action(
                                () =>
                                    {
                                        _btnStop.Enabled = true;
                                        _btnExport.Enabled = true;
                                    }));
                    },
                action);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinQueryResults"/> class. 
        ///   Public constructor for NN algorithm
        /// </summary>
        /// <param name="connectionString">
        /// Connection string
        /// </param>
        /// <param name="secondsToAnalyze">
        /// Number of fingerprints to analyze
        /// </param>
        /// <param name="startSeconds">
        /// Starting seconds
        /// </param>
        /// <param name="stride">
        /// Query stride
        /// </param>
        /// <param name="topWavelets">
        /// Number of top wavelets
        /// </param>
        /// <param name="fileList">
        /// File list to analyze
        /// </param>
        /// <param name="pathToEnsemble">
        /// Path to ensemble
        /// </param>
        public WinQueryResults(string connectionString, int secondsToAnalyze, int startSeconds, IStride stride, int topWavelets, List<string> fileList, string pathToEnsemble)
            : this(connectionString, secondsToAnalyze, startSeconds, stride, topWavelets, fileList)
        {
            ensemble = NNEnsemble.Load(pathToEnsemble);
            _dgvResults.Columns.Add(ColHit, "Number of hits");
            _dgvResults.Columns[ColHit].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Action action = ExtractCandidatesWithNeuralHasher;
            action.BeginInvoke(action.EndInvoke, action);
        }

        public IFingerprintService FingerprintService { get; set; }

        /// <summary>
        ///   Extract possible candidates from the data source using Neural Hasher
        /// </summary>
        private void ExtractCandidatesWithNeuralHasher()
        {
            int recognized = 0, verified = 0;

            Action<object[], Color> actionAddItems =
                (parameters, color) =>
                {
                    int index = _dgvResults.Rows.Add(parameters);
                    _dgvResults.FirstDisplayedScrollingRowIndex = index;
                    if (color != Color.Empty)
                    {
                        _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
                    }
                };

            Action<float> actionRecognition = (recognition) => _tbResults.Text = recognition.ToString();

            foreach (string pathToFile in fileList)
            {
                //Samples to skip from each of the song
                IStride samplesToSkip = queryStride;
                long elapsedMiliseconds = 0;

                TAG_INFO tags = audioService.GetTagInfoFromFile(pathToFile); //Get Tags from file
                if (tags == null)
                {
                    //TAGS are null
                    Invoke(actionAddItems, new Object[] {"TAGS ARE NULL!", pathToFile}, Color.Red); //Show that the file has no tags
                    continue;
                }

                string artist = tags.artist; //Artist
                string title = tags.title; //Title
                double duration = tags.duration; //Duration

                if (duration < MinTrackLength || duration > MaxTrackLength) //Check whether the duration is ok
                {
                    //Duration too small
                    Invoke(actionAddItems, new Object[] {"BAD DURATION!", pathToFile}, Color.Red);
                    continue;
                }

                if (String.IsNullOrEmpty(artist) || String.IsNullOrEmpty(title)) //Check whether the tags are properly defined
                {
                    Invoke(actionAddItems, new Object[] {"NO TAGS!", pathToFile}, Color.Red);
                    continue;
                }

                /*
                * Returned dictionary is sorted by Values
                * allCandidates.ElementAt(0) will return the pair with best query results
                */
                Dictionary<Int32, QueryStats> allCandidatesNotSorted = new Dictionary<int, QueryStats>(); //QueryFingerprintManager.QueryOneSongNeuralHasher(_ensemble, pathToFile, samplesToSkip, audioService, _dalManager, _secondsToAnalyze, ref elapsedMiliseconds);

                if (allCandidatesNotSorted == null)
                {
                    Invoke(actionAddItems, new Object[] {"BAD CANDIDATES!", pathToFile}, Color.Red);
                    continue;
                }

                //Sort candidates
                IOrderedEnumerable<KeyValuePair<Int32, QueryStats>> result =
                    allCandidatesNotSorted.OrderByDescending((pair) => pair.Value.NumberOfTrackIdOccurences);
                int candidatesCount = result.Count();
                if (candidatesCount == 0)
                {
                    Invoke(actionAddItems, new Object[] {artist + "-" + title, "NO CANDIDATES!"}, Color.Yellow);
                    verified++;
                    continue;
                }

                bool found = false;
                KeyValuePair<Int32, QueryStats> item = result.ElementAt(0);
                Track track = dalManager.ReadTrackByArtistAndTitleName(artist, title);
                if (track == null)
                {
                    Invoke(actionAddItems, new Object[] {artist + "-" + title, "No such track in the database!"}, Color.Yellow);
                    continue;
                }

                if (track.Id == item.Key) /*Compare returned track by actual*/
                {
                    recognized++;
                    found = true;
                    Invoke(actionAddItems, new Object[] {title + "-" + artist, track.Title + "-" + track.Artist, 1, found, 0, candidatesCount}, Color.Empty);
                }


                if (!found)
                {
                    var itemFound = result.Select((pair, indexAt) => new {Pair = pair, IndexAt = indexAt}).Where((a) => a.Pair.Key == track.Id);
                    if (itemFound != null && itemFound.Count() > 0)
                    {
                        int indexOfCount = itemFound.ElementAt(0).IndexAt + 1;
                        Invoke(actionAddItems, new Object[] {title + "-" + artist, title + "-" + artist, indexOfCount, true, 0, candidatesCount}, Color.Yellow);
                    }
                    else
                    {
                        Invoke(actionAddItems, new Object[] {title + "-" + artist, "Not found!", -1, false, 0, candidatesCount}, Color.Yellow);
                    }
                }
                verified++;
                //results
                Invoke(actionRecognition, (float) recognized/verified);
            }
        }

        /// <summary>
        ///   Extract Candidates from the underlying data source using Min Hash + LSH Schema
        /// </summary>
        private void ExtractCandidatesWithMinHashAlgorithm()
        {
            runningThread = Thread.CurrentThread;
            int recognized = 0, verified = 0;
            IStride samplesToSkip = queryStride;
            Action<int> action = ((check) => _nudChecked.Value = check);
            Action<object[], Color> actionAddItems = (parameters, color) =>
                                                     {
                                                         int index = _dgvResults.Rows.Add(parameters);
                                                         _dgvResults.FirstDisplayedScrollingRowIndex = index;
                                                         if (color != Color.Empty)
                                                             _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
                                                     };
            Action<float> actionRecognition = (recognition) => _tbResults.Text = recognition.ToString();

            for (int i = 0; i < fileList.Count; i++) /*For each song in the list, query the DATABASE*/
            {
                if (InvokeRequired)
                    Invoke(action, i);
                else
                    action(i);
                if (stopQuerying)
                    break;

                string pathToFile = fileList[i]; /*Path to song to recognize*/
                TAG_INFO tags = audioService.GetTagInfoFromFile(pathToFile); //Get Tags from file

                if (tags == null)
                {
                    //TAGS are null
                    Invoke(actionAddItems, new Object[] {"TAGS ARE NULL", pathToFile}, Color.Red);
                    continue;
                }

                string artist = tags.artist; //Artist
                string title = tags.title; //Title
                double duration = tags.duration; //Duration

                if (duration < MinTrackLength || duration > MaxTrackLength) //Check whether the duration is ok
                {
                    //Duration too small
                    Invoke(actionAddItems, new Object[] {"BAD DURATION", pathToFile}, Color.Red);
                    continue;
                }

                if (String.IsNullOrEmpty(artist) || String.IsNullOrEmpty(title)) //Check whether the tags are properly defined
                {
                    //Title or Artist tag is null
                    Invoke(actionAddItems, new Object[] {"NO TAGS", pathToFile}, Color.Red);
                    continue;
                }

                long elapsedMiliseconds = 0;

                /*Get correct track trackId*/
                Track actualTrack = dalManager.ReadTrackByArtistAndTitleName(artist, title);
                if (actualTrack == null)
                {
                    Invoke(actionAddItems, new Object[] {title + "-" + artist, "No such song in the database!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds}, Color.Red);
                    continue;
                }

                FingerprintService.FingerprintConfig.Stride = samplesToSkip;
                List<bool[]> signatures = FingerprintService.CreateFingerprints(
                    pathToFile, secondsToAnalyze * 1000, startSecond * 1000);
                Dictionary<Int32, QueryStats> allCandidates = QueryFingerprintManager.QueryOneSongMinHash(
                    signatures,
                    dalManager,
                    permStorage,
                    secondsToAnalyze,
                    hashTables,
                    hashKeys,
                    threshold,
                    ref elapsedMiliseconds); /*Query the database using Min Hash*/

                if (allCandidates == null) /*No candidates*/
                {
                    Invoke(actionAddItems, new Object[] {title + "-" + artist, "No candidates!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds}, Color.Red);
                    continue;
                }

                /*Order by Hamming Similarity*/
                OrderedParallelQuery<KeyValuePair<Int32, QueryStats>> order = allCandidates.AsParallel() /*Using PLINQ*/
                    .OrderBy((pair) =>
                             {
                                 return pair.Value.OrderingValue = pair.Value.HammingDistance/pair.Value.NumberOfTotalTableVotes
                                                                   + 0.4*pair.Value.MinHammingDistance;
                             });

                Track recognizedTrack = null;
                bool found = false;

                if (order.Count() > 0)
                {
                    KeyValuePair<Int32, QueryStats> item = order.ElementAt(0);
                    recognizedTrack = dalManager.ReadTrackById(item.Key);
                    if (actualTrack.Id == recognizedTrack.Id)
                    {
                        recognized++;
                        found = true;
                    }
                    Invoke(actionAddItems, new Object[]
                                           {
                                               title + "-" + artist, /*Actual title and artist*/
                                               recognizedTrack.Title + "-" + recognizedTrack.Artist, /*Recognized Title Track Name*/
                                               1, /*Position in the ordered list*/
                                               actualTrack.Id == recognizedTrack.Id, /*Found?*/
                                               item.Value.HammingDistance/item.Value.NumberOfTotalTableVotes, /*Average hamming distance*/
                                               (double) item.Value.HammingDistanceByTrack/item.Value.NumberOfTrackIdOccurences,
                                               (double) item.Value.MinHammingDistance,
                                               item.Value.OrderingValue,
                                               item.Value.NumberOfTotalTableVotes,
                                               item.Value.NumberOfTrackIdOccurences,
                                               item.Value.StartQueryIndex,
                                               allCandidates.Count,
                                               item.Value.Similarity,
                                               elapsedMiliseconds
                                           }, Color.Empty);
                }

                verified++;
                if (!found)
                {
                    /*If the element is not found, search it in all candidates*/
                    var query = order.Select((pair, indexAt) => new {Pair = pair, IndexAt = indexAt}).Where((a) => a.Pair.Key == actualTrack.Id);
                    if (query != null && query.Count() > 0)
                    {
                        var anonymType = query.ElementAt(0);
                        recognizedTrack = dalManager.ReadTrackById(anonymType.Pair.Key);
                        Invoke(actionAddItems, new Object[]
                                               {
                                                   title + "-" + artist,
                                                   recognizedTrack.Title + "-" + recognizedTrack.Artist, /*Recognized Title Track Name*/
                                                   anonymType.IndexAt + 1, /*Position in the ordered list*/
                                                   actualTrack.Id == recognizedTrack.Id, /*Found?*/
                                                   anonymType.Pair.Value.HammingDistance/anonymType.Pair.Value.NumberOfTotalTableVotes, /*Main Criterion*/
                                                   (double) anonymType.Pair.Value.HammingDistanceByTrack/anonymType.Pair.Value.NumberOfTrackIdOccurences,
                                                   (double) anonymType.Pair.Value.MinHammingDistance,
                                                   anonymType.Pair.Value.OrderingValue,
                                                   anonymType.Pair.Value.NumberOfTotalTableVotes,
                                                   anonymType.Pair.Value.NumberOfTrackIdOccurences,
                                                   anonymType.Pair.Value.StartQueryIndex,
                                                   allCandidates.Count,
                                                   anonymType.Pair.Value.Similarity,
                                                   elapsedMiliseconds
                                               }, Color.Yellow);
                    }
                    else
                        Invoke(actionAddItems, new Object[]
                                               {
                                                   title + "-" + artist, "Not Found", -1, /*Position in the ordered list*/
                                                   false, -1, -1, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds
                                               }, Color.Yellow);
                }
                Invoke(actionRecognition, (float) recognized/verified);
            }
        }

        /// <summary>
        ///   Callback invoked when <c>Stop</c> button is pressed, meaning that the recognition is over
        /// </summary>
        private void BtnStopClick(object sender, EventArgs e)
        {
            stopQuerying = true;
            Invoke(new Action(() => { _btnStop.Enabled = false; }));
        }

        /// <summary>
        ///   Callback invoked when the <c>Export</c> button is pressed
        /// </summary>
        private void BtnExportClick(object sender, EventArgs e)
        {
            /*Export into CSV file*/
            WinUtils.ExportInExcel(_dgvResults, "Recognition Rate", _tbResults.Text, "Date", DateTime.Now);
        }


        /// <summary>
        ///   Callback invoked when the Win Query results is closing
        /// </summary>
        private void WinQueryResultsFormClosing(object sender, FormClosingEventArgs e)
        {
            if (runningThread != null)
                runningThread.Abort();
        }
    }
}