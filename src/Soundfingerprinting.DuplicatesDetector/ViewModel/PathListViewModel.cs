// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Soundfingerprinting.DuplicatesDetector.Model;
using Soundfingerprinting.DuplicatesDetector.Services;

namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    /// <summary>
    ///   Path List VM
    /// </summary>
    public class PathListViewModel : ViewModelBase
    {
        #region Constants

        private const string STEP_SELECT_ROOT_FOLDER = "Step 1/3 - Selecting root folders";
        private const string STEP_HASHING_MUSIC_FILES = "Step 2/3 - Fingerprinting music files";
        private const string STEP_FIND_DUPLICATES = "Step 3/3 - Finding duplicates";
        private const string SETP_GENERATING_REPORT = "Generating report...";
        private const string STEP_ABORTING = "Aborting, please wait...";

        /// <summary>
        ///   Music file filters
        /// </summary>
        private readonly string[] _musicFileFilters = new[] {"*.mp3", "*.ogg", "*.flac", "*.wav"};

        #endregion

        /// <summary>
        ///   Locking object
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        ///   Repository gateway
        /// </summary>
        private readonly RepositoryGateway _gate;

        /// <summary>
        ///   Add single music file command
        /// </summary>
        private RelayCommand _addFileCommand;

        /// <summary>
        ///   Add more folders command
        /// </summary>
        private RelayCommand _addMoreCommand;

        private int _currentProgress;

        /// <summary>
        ///   Paths to folders to be processed
        /// </summary>
        private ObservableCollection<Item> _paths;

        /// <summary>
        ///   Processed music files
        /// </summary>
        private int _processedMusicItems;

        private HashSet<Item> _processedPaths;

        /// <summary>
        ///   Flag for processing is possible
        /// </summary>
        private bool _processing;

        /// <summary>
        ///   Processing step
        /// </summary>
        private string _processingStep;

        /// <summary>
        ///   Start processing command
        /// </summary>
        private RelayCommand _startCommand;

        /// <summary>
        ///   Stop processing command
        /// </summary>
        private RelayCommand _stopCommand;

        /// <summary>
        ///   Total music items to process
        /// </summary>
        private int _totalMusicItems;

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public PathListViewModel()
        {
            _paths = new ObservableCollection<Item>();
            _paths.CollectionChanged += CollectionChanged; /*Subscribe to the event when the collection gets changed*/
            _processingStep = STEP_SELECT_ROOT_FOLDER;
            TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            try
            {
                _gate = new RepositoryGateway();
            }
            catch (Exception ex)
            {
                IMessageBoxService msb = GetService<IMessageBoxService>(); //Show dialog if an exception occurred during processing
                if (msb != null)
                    msb.Show(ex.Message, "Exception occurred during processing!",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        ///   Paths to folders which contain music files
        /// </summary>
        public ObservableCollection<Item> Paths
        {
            get { return _paths ?? (_paths = new ObservableCollection<Item>()); }
            private set
            {
                _paths = value;
                OnPropertyChanged("Paths");
            }
        }

        /// <summary>
        ///   Processing step (1 out of 4)
        /// </summary>
        public string ProcessingStep
        {
            get { return _processingStep; }
            set
            {
                if (_processingStep != value)
                {
                    _processingStep = value;
                    OnPropertyChanged("ProcessingStep");
                }
            }
        }

        /// <summary>
        ///   Check if system is currently busy in processing the files
        /// </summary>
        public bool IsProcessing
        {
            get { return _processing; }
            set
            {
                if (_processing != value)
                {
                    _processing = value;
                    OnPropertyChanged("IsProcessing");
                }
            }
        }

        /// <summary>
        ///   Current progress on the progress bar
        /// </summary>
        public int CurrentProgress
        {
            get { return _currentProgress; }
            private set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }


        /// <summary>
        ///   UI Synchronization context obtained through UI task scheduler
        /// </summary>
        public TaskScheduler TaskScheduler { get; private set; }

        /// <summary>
        ///   Get the command to be executed when AddMore button on the View is clicked
        /// </summary>
        public ICommand AddMoreCommand
        {
            get
            {
                return _addMoreCommand ?? (_addMoreCommand = new RelayCommand(
                                                                 AddMoreFolders, CanAddMoreFolders));
            }
        }

        /// <summary>
        ///   Start processing the songs
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(
                                                             StartProcessing, CanStartProcessing));
            }
        }

        /// <summary>
        ///   Abort processing
        /// </summary>
        public ICommand StopCommand
        {
            get
            {
                return _stopCommand ?? (_stopCommand = new RelayCommand(
                                                           StopProcessing, CanStopProcessing));
            }
        }

        /// <summary>
        ///   Add single music file command
        /// </summary>
        public ICommand AddFileCommand
        {
            get
            {
                return _addFileCommand ?? (_addFileCommand = new RelayCommand(
                                                                 AddFile, CanAddFile));
            }
        }

        /// <summary>
        ///   The collection gets changed
        /// </summary>
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    int c = ((Item) item).Count;
                    if (c > 0)
                        _totalMusicItems += c;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    int c = ((Item) item).Count;
                    if (c > 0)
                        _totalMusicItems -= c;
                }
            }
            OnPropertyChanged("Paths"); /*Signalize that the collection got changed*/
        }

        /// <summary>
        ///   Add more folders to the list
        /// </summary>
        public void AddMoreFolders(object param)
        {
            IFolderBrowserDialogService fbg = GetService<IFolderBrowserDialogService>();
            if (fbg != null)
            {
                if (DialogResult.OK == fbg.Show())
                {
                    string selectedPath = fbg.SelectedPath;
                    //Check if such directory exists, and check if such directory wasn't already introduced
                    if (!String.IsNullOrEmpty(selectedPath) && Paths.All(folder => selectedPath != folder.Path))
                    {
                        if (Directory.Exists(selectedPath))
                        {
                            Paths.Add(new Item {Path = selectedPath, Count = -1, IsFolder = true});
                        }
                        else if (File.Exists(selectedPath))
                        {
                            Paths.Add(new Item {Path = selectedPath, Count = 1, IsFolder = false});
                            return;
                        }
                        else
                        {
                            return;
                        }
                        //count the number of available music files asynchronously

                        Task.Factory.StartNew(
                            () =>
                            {
                                int count = Helper.CountNumberOfMusicFiles(selectedPath, _musicFileFilters, true);

                                lock (LockObject)
                                {
                                    int index = -1;
                                    foreach (Item path in Paths)
                                    {
                                        index++;
                                        if (path.Path == selectedPath)
                                            break;
                                    }
                                    if (Paths != null && Paths.Count >= index && index >= 0)
                                    {
                                        _totalMusicItems += count;
                                        Paths[index].Count = count;
                                    }
                                }
                            }).ContinueWith(
                                (task) => CommandManager.InvalidateRequerySuggested(), TaskScheduler);
                    }
                }
            }
        }

        /// <summary>
        ///   Check if adding folders is allowed
        /// </summary>
        /// <param name = "param">Any parameter</param>
        /// <returns>True if allowed, otherwise false</returns>
        /// <remarks>
        ///   Can execute is called before Execute() method, thus if the method returns false, the button will be disabled
        /// </remarks>
        public bool CanAddMoreFolders(object param)
        {
            return !IsProcessing;
        }

        /// <summary>
        ///   Actual method which start processing
        /// </summary>
        /// <param name = "param">Parameter</param>
        public void StartProcessing(object param)
        {
            ProcessingStep = STEP_HASHING_MUSIC_FILES;
            IsProcessing = true;
            int alreadyProcessedFiles = _processedMusicItems;
            HashSet<Item> pathsToBeProcessed = null;

            if (_processedPaths == null)
            {
                pathsToBeProcessed = new HashSet<Item>(Paths);
                _processedPaths = new HashSet<Item>(Paths);
            }
            else
            {
                //if there are already paths that have been processed, use only new ones
                IEnumerable<Item> except = Paths.Except(_processedPaths);
                pathsToBeProcessed = new HashSet<Item>(except);
                if (pathsToBeProcessed.Count == 0)
                {
                    FindDuplicates();
                    return;
                }

                foreach (Item item in except)
                {
                    _processedPaths.Add(item);
                }
            }

            _gate.ProcessTracksAsync(
                pathsToBeProcessed,
                _musicFileFilters,
                (tracks, exception) => /*processing ends*/
                {
                    if (exception != null) /*Exception occurred during processing*/
                    {
                        IMessageBoxService msg = GetService<IMessageBoxService>();
                        msg.Show("Error occurred during processing!\n" + exception.Message, "Error!",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        if (IsProcessing) IsProcessing = false;
                        return;
                    }
                    if (tracks == null || tracks.Count == 0) /*Processing aborted*/
                    {
                        IsProcessing = false; /*successfully aborted continue*/
                        CurrentProgress = 0;
                        ProcessingStep = STEP_SELECT_ROOT_FOLDER;
                        Task.Factory.StartNew(CommandManager.InvalidateRequerySuggested, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
                        return;
                    }
                    /*Processing ended*/
                    ProcessingStep = STEP_FIND_DUPLICATES;
                    CurrentProgress = 0;
                    FindDuplicates();
                },
                (track) => /*one file is processed*/
                {
                    // ReSharper disable AccessToModifiedClosure
                    Interlocked.Increment(ref alreadyProcessedFiles);
                    // ReSharper restore AccessToModifiedClosure
                    CurrentProgress = (int) Math.Ceiling(((float) (alreadyProcessedFiles)/_totalMusicItems*100));
                    if (alreadyProcessedFiles >= _totalMusicItems)
                    {
                        _processedMusicItems = alreadyProcessedFiles; /*set processed unit items*/
                        alreadyProcessedFiles = 0;
                        CurrentProgress = 0;
                    }
                });
        }

        /// <summary>
        ///   Search the storage for duplicate files
        /// </summary>
        private void FindDuplicates()
        {
            HashSet<Track>[] duplicates = null;
            Task.Factory.StartNew(
                () =>
                {
                    duplicates = _gate.FindAllDuplicates(
                        (track, total, current) =>
                        {
                            CurrentProgress = (int) Math.Ceiling(((float) (current)/total*100));
                            if (current >= total)
                            {
                                _processedMusicItems = current; /*set processed unit items*/
                                CurrentProgress = 0;
                            }
                        });
                }).ContinueWith( /*Update the UI*/
                (task) =>
                {
                    ProcessingStep = SETP_GENERATING_REPORT;
                    ReportViewModel report = new ReportViewModel {Sets = duplicates};
                    GenericViewModel viewModel = new GenericViewModel();
                    viewModel.Workspaces.Add(report);
                    IGenericViewWindow view = GetService<IGenericViewWindow>();
                    IWindowService windowMediator = GetService<IWindowService>();
                    windowMediator.ShowDialog(view, viewModel,
                        ((o, args) => report.StopPlaying()));
                    ProcessingStep = STEP_SELECT_ROOT_FOLDER;
                    IsProcessing = false;
                }, TaskScheduler);
        }

        /// <summary>
        ///   Check if the system can start processing
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        public bool CanStartProcessing(object param)
        {
            return !IsProcessing && _totalMusicItems > 1 && (_totalMusicItems - _processedMusicItems > 0); /*Processing can start if you have at least 2 files to play with*/
        }

        /// <summary>
        ///   Stop processing the songs
        /// </summary>
        /// <param name = "param">Parameter</param>
        public void StopProcessing(object param)
        {
            ProcessingStep = STEP_ABORTING;
            _processedMusicItems = 0;
            _processedPaths = null;
            _gate.AbortProcessing();
        }

        /// <summary>
        ///   Check if the system can stop processing the songs
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        public bool CanStopProcessing(object param)
        {
            return IsProcessing;
        }

        /// <summary>
        ///   Add a single file to the union of files to be processed
        /// </summary>
        /// <param name = "param"></param>
        public void AddFile(object param)
        {
            IOpenFileDialogService fbg = GetService<IOpenFileDialogService>();
            if (fbg != null)
            {
                if (DialogResult.OK == fbg.Show("Please choose your awesome track", "Amazing Track", Helper.GetMultipleFilter("Supported Formats", _musicFileFilters), true))
                {
                    string[] selectedPaths = fbg.SelectedPaths;
                    foreach (string selectedPath in selectedPaths)
                    {
                        //Check if such directory exists, and check if such directory wasn't already introduced
                        if (!String.IsNullOrEmpty(selectedPath) && Paths.All(folder => selectedPath != folder.Path))
                        {
                            if (Directory.Exists(selectedPath))
                            {
                                Paths.Add(new Item {Path = selectedPath, Count = -1, IsFolder = true});
                            }
                            else if (File.Exists(selectedPath))
                            {
                                Paths.Add(new Item {Path = selectedPath, Count = 1, IsFolder = false});
                            }
                        }
                    }
                }
            }
            if (param != null)
            {
                if (File.Exists(param as string))
                {
                    string extension = Path.GetExtension(param as string);
                    if (_musicFileFilters.Where(filter => extension != null).Any(filter => filter.Contains(extension)))
                    {
                        Paths.Add(new Item {Path = param as string, Count = 1, IsFolder = false});
                    }
                }
            }
        }

        /// <summary>
        ///   See if adding files is allowed
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        private bool CanAddFile(object param)
        {
            return !IsProcessing;
        }
    }
}