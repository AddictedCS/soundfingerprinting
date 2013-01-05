// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com

namespace Soundfingerprinting.SoundTools.QueryDb
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
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
        private const int MIN_TRACK_LENGTH = 20;

        /// <summary>
        ///   Maximum track length (in seconds)
        /// </summary>
        private const int MAX_TRACK_LENGTH = 60*20;

        #region DataGrid Columns

        private const string COL_SONG_NAME = "SongNameTitle";
        private const string COL_RESULT_NAME = "ResultSongNameTitle";
        private const string COL_HIT = "CounterHit";
        private const string COL_RESULT = "Result";
        private const string COL_POSITION = "Position";
        private const string COL_HAMMING_AVG = "HammingAvg";
        private const string COL_HAMMING_AVG_BY_TRACK = "HammingAvgByTrack";
        private const string COL_MIN_HAMMING = "MinHamming";
        private const string COL_SORT_VALUE = "SortValue";
        private const string COL_START_QUERY_INDEX = "StartQueryIndex";
        private const string COL_TOTAL_TABLES = "TotalTables";
        private const string COL_TOTAL_TRACK_VOTES = "TotalTrackVotes";
        private const string COL_ELAPSED_TIME = "ElapsedTime";
        private const string COL_TOTAL_FINGERPRINTS = "TotalFingerprints";
        private const string COL_MAX_PATH = "MaxPath";
        private const string COL_SIMILARITY = "Similarity";
        private readonly Random _random = new Random((int) unchecked(DateTime.Now.Ticks << 4));

        #endregion

        /// <summary>
        ///   Connection string to the underlying data source
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        ///   Data access manager, allows one to access the underlying data source
        /// </summary>
        private readonly DaoGateway _dalManager;

        /// <summary>
        ///   Network ensemble
        /// </summary>
        private readonly NNEnsemble _ensemble;

        /// <summary>
        ///   List of files [.mp3] which are going to be recognized
        /// </summary>
        private readonly List<string> _fileList;

        /// <summary>
        ///   Number of hash keys per table used in MinHash + LSH schema [normally 5]
        /// </summary>
        private readonly int _hashKeys;

        /// <summary>
        ///   Number of hash tables used in the MinHash + LSH schema [normally 20]
        /// </summary>
        private readonly int _hashTables;

        /// <summary>
        ///   Fingerprint manager
        /// </summary>
        private readonly FingerprintManager _manager;

        /// <summary>
        ///   Permutation storage
        /// </summary>
        private readonly IPermutations _permStorage;

        /// <summary>
        ///   Audio audioService used for reading the data from the .mp3 files
        /// </summary>
        private readonly BassAudioService audioService = new BassAudioService();

        /// <summary>
        ///   The size of the query [E.g. 253 samples]
        /// </summary>
        private readonly IStride _queryStride;

        /// <summary>
        ///   Number of fingerprints to analyze
        /// </summary>
        private readonly int _secondsToAnalyze;

        /// <summary>
        ///   Start second
        /// </summary>
        private readonly int _startSecond;

        /// <summary>
        ///   Recognition threshold 17% ~5 tables from 25 hash functions
        /// </summary>
        private readonly int _threshold;

        /// <summary>
        ///   Number of top wavelets involved
        /// </summary>
        private readonly int _topWavelets;

        /// <summary>
        ///   Running thread allows one to Abort forcibly the recognition
        /// </summary>
        private Thread _runningThread;

        /// <summary>
        ///   Flag which signalizes stop of the recognition process
        /// </summary>
        private bool _stopQuerying;

        /// <summary>
        ///   Protected constructor of WinQueryResults class
        /// </summary>
        /// <param name = "connectionString">Connection string used for the underlying data source</param>
        /// <param name = "secondsToAnalyze">Number of consequent fingerprints to analyze</param>
        /// <param name = "startSecond">Starting seconds</param>
        /// <param name = "stride">Stride used in the query</param>
        /// <param name = "topWavelets">Number of top wavelets to analyze</param>
        /// <param name = "fileList">List of all files to be recognized</param>
        protected WinQueryResults(string connectionString, int secondsToAnalyze, int startSecond,
                                  IStride stride, int topWavelets, List<string> fileList)
        {
            InitializeComponent(); /*Initialize Designer Components*/
            Icon = Resources.Sound;
            _connectionString = connectionString;
            _topWavelets = topWavelets;
            _dalManager = new DaoGateway(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);
            _permStorage = new DbPermutations(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);

            _manager = new FingerprintManager
                { FingerprintConfig = new DefaultFingerpringConfig() { TopWavelets = topWavelets } };
            _dalManager.SetConnectionString(_connectionString); /*Set connection string for DAL manager*/
            _secondsToAnalyze = secondsToAnalyze; /*Number of fingerprints to analyze from each song*/
            _startSecond = startSecond;
            _fileList = fileList; /*List of files to analyze*/
            _dgvResults.Columns.Add(COL_SONG_NAME, "Initial Song");
            _dgvResults.Columns[COL_SONG_NAME].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_RESULT_NAME, "Result Song");
            _dgvResults.Columns[COL_RESULT_NAME].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_POSITION, "Position");
            _dgvResults.Columns[COL_POSITION].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_RESULT, "Result");
            _dgvResults.Columns[COL_RESULT].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_HAMMING_AVG, "Hamming Avg.");
            _dgvResults.Columns[COL_HAMMING_AVG].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _queryStride = stride;
        }

        /// <summary>
        ///   Public constructor for LSH + Min Hash algorithm
        /// </summary>
        /// <param name = "connectionString">Connection string</param>
        /// <param name = "secondsToAnalyze">Number of fingerprints to analyze</param>
        /// <param name = "stride">Stride</param>
        /// <param name = "fileList">File list</param>
        /// <param name = "hashTables">Min hash hash tables</param>
        /// <param name = "hashKeys">Min hash hash keys</param>
        /// <param name = "startSecond">Starting second of analysis</param>
        /// <param name = "thresholdTables">Number of threshold tables</param>
        /// <param name = "topWavelets">Number of top wavelets to consider</param>
        public WinQueryResults(string connectionString, int secondsToAnalyze, int startSecond, IStride stride, List<string> fileList, int hashTables, int hashKeys, int thresholdTables, int topWavelets)
            : this(connectionString, secondsToAnalyze, startSecond, stride, topWavelets, fileList)
        {
            _hashTables = hashTables;
            _hashKeys = hashKeys;
            _threshold = thresholdTables;
            _dgvResults.Columns.Add(COL_HAMMING_AVG_BY_TRACK, "Hamming Avg. By Track");
// ReSharper disable PossibleNullReferenceException
            _dgvResults.Columns[COL_HAMMING_AVG_BY_TRACK].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_MIN_HAMMING, "Min. Hamming");
            _dgvResults.Columns[COL_MIN_HAMMING].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_SORT_VALUE, "Sort Value");
            _dgvResults.Columns[COL_SORT_VALUE].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_TOTAL_TABLES, "Total Table Votes");
            _dgvResults.Columns[COL_TOTAL_TABLES].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_TOTAL_TRACK_VOTES, "Total Track Votes");
            _dgvResults.Columns[COL_TOTAL_TRACK_VOTES].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_START_QUERY_INDEX, "Query Index Start");
            _dgvResults.Columns[COL_START_QUERY_INDEX].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_TOTAL_FINGERPRINTS, "Total Fingerprints");
            _dgvResults.Columns[COL_TOTAL_FINGERPRINTS].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_SIMILARITY, "Min Similarity");
            _dgvResults.Columns[COL_SIMILARITY].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _dgvResults.Columns.Add(COL_ELAPSED_TIME, "Elapsed Time");
            _dgvResults.Columns[COL_ELAPSED_TIME].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
// ReSharper restore PossibleNullReferenceException
            _btnExport.Enabled = false;
            _nudTotal.Value = _fileList.Count;
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
                        MessageBox.Show(Resources.RecognitionEndedWithAnError + e.Message + Resources.LineFeed + e.StackTrace, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show(Resources.RecognitionEnded, Resources.RecognitionEnded, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Invoke(new Action(
                        () =>
                        {
                            _btnStop.Enabled = true;
                            _btnExport.Enabled = true;
                        }));
                }, action);
        }

        /// <summary>
        ///   Public constructor for NN algorithm
        /// </summary>
        /// <param name = "connectionString">Connection string</param>
        /// <param name = "secondsToAnalyze">Number of fingerprints to analyze</param>
        /// <param name = "startSeconds">Starting seconds</param>
        /// <param name = "stride">Query stride</param>
        /// <param name = "topWavelets">Number of top wavelets</param>
        /// <param name = "fileList">File list to analyze</param>
        /// <param name = "pathToEnsemble">Path to ensemble</param>
        public WinQueryResults(string connectionString, int secondsToAnalyze, int startSeconds, IStride stride, int topWavelets, List<string> fileList, string pathToEnsemble)
            : this(connectionString, secondsToAnalyze, startSeconds, stride, topWavelets, fileList)
        {
            _ensemble = NNEnsemble.Load(pathToEnsemble);
            _dgvResults.Columns.Add(COL_HIT, "Number of hits");
            _dgvResults.Columns[COL_HIT].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Action action = ExtractCandidatesWithNeuralHasher;
            action.BeginInvoke((result) => action.EndInvoke(result), action);
        }

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
                        _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
                };

            Action<float> actionRecognition = (recognition) => _tbResults.Text = recognition.ToString();

            foreach (string pathToFile in _fileList)
            {
                //Samples to skip from each of the song
                IStride samplesToSkip = _queryStride;
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

                if (duration < MIN_TRACK_LENGTH || duration > MAX_TRACK_LENGTH) //Check whether the duration is ok
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
                Track track = _dalManager.ReadTrackByArtistAndTitleName(artist, title);
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
            _runningThread = Thread.CurrentThread;
            int recognized = 0, verified = 0;
            IStride samplesToSkip = _queryStride;
            Action<int> action = ((check) => _nudChecked.Value = check);
            Action<object[], Color> actionAddItems = (parameters, color) =>
                                                     {
                                                         int index = _dgvResults.Rows.Add(parameters);
                                                         _dgvResults.FirstDisplayedScrollingRowIndex = index;
                                                         if (color != Color.Empty)
                                                             _dgvResults.Rows[index].DefaultCellStyle.BackColor = color;
                                                     };
            Action<float> actionRecognition = (recognition) => _tbResults.Text = recognition.ToString();

            for (int i = 0; i < _fileList.Count; i++) /*For each song in the list, query the DATABASE*/
            {
                if (InvokeRequired)
                    Invoke(action, i);
                else
                    action(i);
                if (_stopQuerying)
                    break;

                string pathToFile = _fileList[i]; /*Path to song to recognize*/
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

                if (duration < MIN_TRACK_LENGTH || duration > MAX_TRACK_LENGTH) //Check whether the duration is ok
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
                Track actualTrack = _dalManager.ReadTrackByArtistAndTitleName(artist, title);
                if (actualTrack == null)
                {
                    Invoke(actionAddItems, new Object[] {title + "-" + artist, "No such song in the database!", -1, false, 0, -1, -1, -1, -1, -1, -1, -1, elapsedMiliseconds}, Color.Red);
                    continue;
                }
                List<bool[]> signatures = _manager.CreateFingerprints(
                    pathToFile, samplesToSkip, _secondsToAnalyze * 1000, _startSecond * 1000);
                Dictionary<Int32, QueryStats> allCandidates = QueryFingerprintManager.QueryOneSongMinHash(
                    signatures,
                    _dalManager,
                    _permStorage,
                    _secondsToAnalyze,
                    _hashTables,
                    _hashKeys,
                    _threshold,
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
                    recognizedTrack = _dalManager.ReadTrackById(item.Key);
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
                        recognizedTrack = _dalManager.ReadTrackById(anonymType.Pair.Key);
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
            _stopQuerying = true;
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
            if (_runningThread != null)
                _runningThread.Abort();
        }
    }
}