namespace SoundFingerprinting.DuplicatesDetector.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Input;

    using Ninject;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.DuplicatesDetector.Infrastructure;
    using SoundFingerprinting.DuplicatesDetector.Model;
    using SoundFingerprinting.DuplicatesDetector.Services;

    /// <summary>
    ///   Report view-model, provides the report result to the view
    /// </summary>
    public class ReportViewModel : ViewModelBase
    {
        private const string StatusPlay = "Play";
        private const string StatusStop = "Stop";

        /// <summary>
        ///   Proxy used in playing files
        /// </summary>
        private readonly IExtendedAudioService audioService = ServiceContainer.Kernel.Get<IExtendedAudioService>();

        /// <summary>
        ///   Flag - if the player is playing something
        /// </summary>
        private bool isPlaying;

        /// <summary>
        ///  Flag - if the application is moving items from one folder to another
        /// </summary>
        private bool isMoving;

        /// <summary>
        ///   Opens the location folder where the result item is located
        /// </summary>
        private RelayCommand openFolderCommand;

        /// <summary>
        ///   Play the selected file
        /// </summary>
        private RelayCommand playCommand;

        /// <summary>
        ///   Save the results into a file
        /// </summary>
        private RelayCommand saveCommand;

        /// <summary>
        ///  Move items into a separate folder
        /// </summary>
        private RelayCommand moveItemsCommand;

        /// <summary>
        ///   Playing status (Stop/Play)
        /// </summary>
        private string playingStatus = StatusPlay;

        /// <summary>
        ///   Raw results
        /// </summary>
        private HashSet<TrackData>[] rawResult;

        /// <summary>
        ///   Results displayed
        /// </summary>
        private ObservableCollection<ResultItem> results;

        /// <summary>
        ///   Status of the player (Show which file is currently playing)
        /// </summary>
        private string currentlyPlayingFilename;

        /// <summary>
        /// Pointer to currently playing stream
        /// </summary>
        private int currentlyPlayingStream;

        public ReportViewModel()
        {
            isPlaying = false;
            isMoving = false;
        }

        public ObservableCollection<ResultItem> Results
        {
            get
            {
                return results;
            }

            set
            {
                if (results != value)
                {
                    results = value;
                    OnPropertyChanged("Results");
                }
            }
        }

        public ResultItem SelectedItem { get; set; }

        public HashSet<TrackData>[] Sets
        {
            get
            {
                return rawResult;
            }

            set
            {
                rawResult = value;
                results = new ObservableCollection<ResultItem>(ProcessRawData(rawResult));
            }
        }

        public string Status
        {
            get
            {
                return currentlyPlayingFilename;
            }

            set
            {
                if (currentlyPlayingFilename != value)
                {
                    currentlyPlayingFilename = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        public string PlayingStatus
        {
            get
            {
                return playingStatus;
            }

            set
            {
                if (playingStatus != value)
                {
                    playingStatus = value;
                    OnPropertyChanged("PlayingStatus");
                }
            }
        }

        public RelayCommand OpenFolderCommand
        {
            get
            {
                return openFolderCommand ?? (openFolderCommand = new RelayCommand(OpenFolder, CanOpenFolder));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(SaveResults, CanSaveResults));
            }
        }

        public RelayCommand MoveItemsCommand
        {
            get
            {
                return moveItemsCommand ?? (moveItemsCommand = new RelayCommand(MoveItems, CanMoveItems));
            }
        }

        public RelayCommand PlayCommand
        {
            get
            {
                return playCommand ?? (playCommand = new RelayCommand(PlayStopAudioFile, CanPlayStopAudioFile));
            }
        }

        public void StopPlaying()
        {
            if (isPlaying)
            {
                audioService.StopPlayingFile(currentlyPlayingStream); // stop previous play
                isPlaying = !isPlaying;
            }
        }

        public void MoveItems(object param)
        {
            // select folder
            IFolderBrowserDialogService fbd = GetService<IFolderBrowserDialogService>();
            if (fbd != null)
            {
                var result = fbd.Show();
                if (result == DialogResult.OK || result == DialogResult.Yes)
                {
                    string selectedPath = fbd.SelectedPath;
                    if (Directory.Exists(selectedPath))
                    {
                        isMoving = true;
                        Task.Factory.StartNew(
                            () =>
                            {
                                MoveDuplicatesToFolder(selectedPath);
                                isMoving = false;
                                Process.Start("explorer.exe", selectedPath);
                            }).ContinueWith(task => CommandManager.InvalidateRequerySuggested(), TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
        }

        public bool CanMoveItems(object param)
        {
            return Sets.Length > 0 && !isPlaying && !isMoving;
        }

        private void OpenFolder(object param)
        {
            if (SelectedItem != null)
            {
                string path = SelectedItem.Path;
                Process.Start("explorer.exe", Path.GetDirectoryName(path));
            }
        }

        // Is there any item selected, if yes, allow opening the folder and listening to the music
        private bool CanOpenFolder(object param)
        {
            return SelectedItem != null;
        }

        /// <summary>
        ///   Save results into a file
        /// </summary>
        /// <param name = "param">Parameter</param>
        private void SaveResults(object param)
        {
            if (Sets.Length > 0)
            {
                int totalItems = Sets.Sum(set => set.Count) + 1;
                object[][] array = new object[totalItems][];
                int i = 0;
                int setId = 0;
                array[i] = new object[4];
                array[i][0] = "Set ID";
                array[i][1] = "Path";
                array[i][2] = "Artist";
                array[i][3] = "Title";
                i++;
                foreach (HashSet<TrackData> set in Sets)
                {
                    foreach (TrackData track in set)
                    {
                        array[i] = new object[4];
                        array[i][0] = setId;
                        array[i][1] = track.Album;
                        array[i][2] = track.Artist;
                        array[i][3] = track.Title;
                        i++;
                    }

                    setId++;
                }

                ISaveFileDialogService sfd = GetService<ISaveFileDialogService>();
                if (sfd != null)
                {
                    if (sfd.SaveFile("Save result", "results.csv", "(*.csv)|*.csv") == DialogResult.OK)
                    {
                        string path = sfd.Filename;
                        if (!string.IsNullOrEmpty(path))
                        {
                            CSVWriter writer = new CSVWriter(path);
                            writer.Write(array);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   Check if the results can be exported
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        private bool CanSaveResults(object param)
        {
            return Sets.Length > 0;
        }

        /// <summary>
        ///   Play/Stop audio file
        /// </summary>
        /// <param name = "param">Parameter</param>
        private void PlayStopAudioFile(object param)
        {
            if (!isPlaying)
            {
                if (SelectedItem != null)
                {
                    string filename = SelectedItem.Path;
                    if (File.Exists(SelectedItem.Path))
                    {
                        string path = Path.GetFileName(filename);
                        Status = "Playing: " + path;
                        audioService.StopPlayingFile(currentlyPlayingStream); // stop previous play, if such
                        currentlyPlayingStream = audioService.PlayFile(filename); // play file
                        PlayingStatus = StatusStop; // change playing status 
                        isPlaying = true;
                    }
                }
                else
                {
                    audioService.StopPlayingFile(currentlyPlayingStream); // stop previous play, if such
                }
            }
            else
            {
                Status = string.Empty;
                audioService.StopPlayingFile(currentlyPlayingStream); // stop previous play
                isPlaying = !isPlaying;
                PlayingStatus = StatusPlay; // change status
            }
        }

        /// <summary>
        ///   Can play/stop audio file
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        private bool CanPlayStopAudioFile(object param)
        {
            return SelectedItem != null || isPlaying;
        }

        /// <summary>
        ///   Process raw results for suitable appearance
        /// </summary>
        /// <param name = "rawData">Raw data</param>
        /// <returns>List of items for view</returns>
        private List<ResultItem> ProcessRawData(IEnumerable<HashSet<TrackData>> rawData)
        {
            List<ResultItem> showResults = new List<ResultItem>();
            int counter = 0;
            foreach (HashSet<TrackData> item in rawData)
            {
                showResults.AddRange(item.Select(track => new ResultItem(counter, track)));
                counter++;
            }

            return showResults;
        }

        private void MoveDuplicatesToFolder(string selectedPath)
        {
            foreach (var set in Sets)
            {
                bool hasUniqueFiles = CheckIfUniqueNames(set);
                int count = 1;
                foreach (var duplicate in set)
                {
                    string fileName = Path.GetFileName(duplicate.Album); // Path is stored in album
                    if (!hasUniqueFiles)
                    {
                        fileName = string.Format("{0}_{1}", count, fileName);
                    }

                    string srcPath = duplicate.Album;
                    if (fileName != null)
                    {
                        string dstPath = Path.Combine(selectedPath, fileName);
                        if (!File.Exists(dstPath))
                        {
                            try
                            {
                                File.Move(srcPath, dstPath);
                                duplicate.Album = dstPath;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }

                    count++;
                }
            }
        }

        private bool CheckIfUniqueNames(IEnumerable<TrackData> source)
        {
            Dictionary<string, bool> distinct = new Dictionary<string, bool>();
            foreach (var track in source)
            {
                string fileName = Path.GetFileName(track.Album);
                if (fileName != null)
                {
                    if (distinct.ContainsKey(fileName))
                    {
                        return false;
                    }

                    distinct[fileName] = true;
                }
            }

            return true;
        }
    }
}