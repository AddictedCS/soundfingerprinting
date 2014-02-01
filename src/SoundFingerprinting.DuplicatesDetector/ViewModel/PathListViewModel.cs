namespace SoundFingerprinting.DuplicatesDetector.ViewModel
{
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

    using SoundFingerprinting.Data;
    using SoundFingerprinting.DuplicatesDetector.Model;
    using SoundFingerprinting.DuplicatesDetector.Services;

    /// <summary>
    ///   Path List VM
    /// </summary>
    public class PathListViewModel : ViewModelBase
    {
        #region Constants

        private const string StepSelectRootFolder = "Step 1/3 - Select root folders";
        private const string StepHashingMusicFiles = "Step 2/3 - Fingerprinting music files";
        private const string StepFindDuplicates = "Step 3/3 - Finding duplicates";
        private const string SetpGeneratingReport = "Generating report...";
        private const string StepAborting = "Aborting, please wait...";

        private static readonly object LockObject = new object();

        /// <summary>
        ///   Music file filters
        /// </summary>
        private readonly string[] musicFileFilters = new[] { "*.mp3", "*.ogg", "*.flac", "*.wav" };

        #endregion
        
        /// <summary>
        ///   Repository gateway
        /// </summary>
        private readonly DuplicatesDetectorFacade gate;

        /// <summary>
        ///   Add single music file command
        /// </summary>
        private RelayCommand addFileCommand;

        /// <summary>
        ///   Add more folders command
        /// </summary>
        private RelayCommand addMoreCommand;

        private int currentProgress;

        /// <summary>
        ///   Paths to folders to be processed
        /// </summary>
        private ObservableCollection<Item> paths;

        /// <summary>
        ///   Processed music files
        /// </summary>
        private int processedMusicItems;

        private HashSet<Item> processedPaths;

        /// <summary>
        ///   Flag for processing is possible
        /// </summary>
        private bool processing;

        /// <summary>
        ///   Processing step
        /// </summary>
        private string processingStep;

        /// <summary>
        ///   Start processing command
        /// </summary>
        private RelayCommand startCommand;

        /// <summary>
        ///   Stop processing command
        /// </summary>
        private RelayCommand stopCommand;

        /// <summary>
        ///   Total music items to process
        /// </summary>
        private int totalMusicItems;

        public PathListViewModel()
        {
            paths = new ObservableCollection<Item>();
            paths.CollectionChanged += CollectionChanged; /*Subscribe to the event when the collection gets changed*/
            processingStep = StepSelectRootFolder;
            TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            try
            {
                gate = new DuplicatesDetectorFacade();
            }
            catch (Exception ex)
            {
                IMessageBoxService msb = GetService<IMessageBoxService>(); // Show dialog if an exception occurred during processing
                if (msb != null)
                {
                    msb.Show(ex.Message, "Exception occurred during processing!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public ObservableCollection<Item> Paths
        {
            get
            {
                return paths ?? (paths = new ObservableCollection<Item>());
            }
        }

        public string ProcessingStep
        {
            get
            {
                return processingStep;
            }

            set
            {
                if (processingStep != value)
                {
                    processingStep = value;
                    OnPropertyChanged("ProcessingStep");
                }
            }
        }

        public bool IsProcessing
        {
            get
            {
                return processing;
            }

            set
            {
                if (processing != value)
                {
                    processing = value;
                    OnPropertyChanged("IsProcessing");
                }
            }
        }

        public int CurrentProgress
        {
            get
            {
                return currentProgress;
            }

            private set
            {
                if (currentProgress != value)
                {
                    currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }
        
        public TaskScheduler TaskScheduler { get; private set; }

        public ICommand AddMoreCommand
        {
            get
            {
                return addMoreCommand ?? (addMoreCommand = new RelayCommand(AddMoreFolders, CanAddMoreFolders));
            }
        }

        public ICommand StartCommand
        {
            get
            {
                return startCommand ?? (startCommand = new RelayCommand(StartProcessing, CanStartProcessing));
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return stopCommand ?? (stopCommand = new RelayCommand(StopProcessing, CanStopProcessing));
            }
        }

        public ICommand AddFileCommand
        {
            get
            {
                return addFileCommand ?? (addFileCommand = new RelayCommand(AddFile, CanAddFile));
            }
        }

        public void AddMoreFolders(object param)
        {
            IFolderBrowserDialogService fbg = GetService<IFolderBrowserDialogService>();
            if (fbg != null)
            {
                if (DialogResult.OK == fbg.Show())
                {
                    string selectedPath = fbg.SelectedPath;
                    
                    // Check if such directory exists, and check if such directory wasn't already introduced
                    if (!string.IsNullOrEmpty(selectedPath) && Paths.All(folder => selectedPath != folder.Path))
                    {
                        if (Directory.Exists(selectedPath))
                        {
                            Paths.Add(new Item { Path = selectedPath, Count = -1, IsFolder = true });
                        }
                        else if (File.Exists(selectedPath))
                        {
                            Paths.Add(new Item { Path = selectedPath, Count = 1, IsFolder = false });
                            return;
                        }
                        else
                        {
                            return;
                        }

                        // count the number of available music files asynchronously
                        Task.Factory.StartNew(
                            () =>
                            {
                                int count = Helper.CountNumberOfMusicFiles(selectedPath, musicFileFilters);

                                lock (LockObject)
                                {
                                    int index = -1;
                                    foreach (Item path in Paths)
                                    {
                                        index++;
                                        if (path.Path == selectedPath)
                                        {
                                            break;
                                        }
                                    }

                                    if (Paths != null && Paths.Count >= index && index >= 0)
                                    {
                                        totalMusicItems += count;
                                        Paths[index].Count = count;
                                    }
                                }
                            }).ContinueWith(
                                task => CommandManager.InvalidateRequerySuggested(), TaskScheduler);
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
            ProcessingStep = StepHashingMusicFiles;
            IsProcessing = true;
            int alreadyProcessedFiles = processedMusicItems;
            HashSet<Item> pathsToBeProcessed;

            if (processedPaths == null)
            {
                pathsToBeProcessed = new HashSet<Item>(Paths);
                processedPaths = new HashSet<Item>(Paths);
            }
            else
            {
                // if there are already paths that have been processed, use only new ones
                var except = Paths.Except(processedPaths).ToList();
                pathsToBeProcessed = new HashSet<Item>(except);
                if (pathsToBeProcessed.Count == 0)
                {
                    FindDuplicates();
                    return;
                }

                foreach (Item item in except)
                {
                    processedPaths.Add(item);
                }
            }

            gate.ProcessTracksAsync(
                pathsToBeProcessed,
                musicFileFilters,
                (tracks, exception) => /*processing ends*/
                {
                    if (exception != null) /*Exception occurred during processing*/
                    {
                        IMessageBoxService msg = GetService<IMessageBoxService>();
                        msg.Show("Error occurred during processing!\n" + exception.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (IsProcessing)
                        {
                            IsProcessing = false;
                        }

                        return;
                    }

                    if (tracks == null || tracks.Count == 0) /*Processing aborted*/
                    {
                        IsProcessing = false; /*successfully aborted continue*/
                        CurrentProgress = 0;
                        ProcessingStep = StepSelectRootFolder;
                        Task.Factory.StartNew(CommandManager.InvalidateRequerySuggested, CancellationToken.None, TaskCreationOptions.None, TaskScheduler);
                        return;
                    }

                    /*Processing ended*/
                    ProcessingStep = StepFindDuplicates;
                    CurrentProgress = 0;
                    FindDuplicates();
                },
                track => /*one file is processed*/
                {
                    // ReSharper disable AccessToModifiedClosure
                    Interlocked.Increment(ref alreadyProcessedFiles);
                    // ReSharper restore AccessToModifiedClosure
                    CurrentProgress = (int)Math.Ceiling((float)alreadyProcessedFiles / totalMusicItems * 100);
                    if (alreadyProcessedFiles >= totalMusicItems)
                    {
                        processedMusicItems = alreadyProcessedFiles; /*set processed unit items*/
                        alreadyProcessedFiles = 0;
                        CurrentProgress = 0;
                    }
                });
        }

        /// <summary>
        ///   Check if the system can start processing
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        public bool CanStartProcessing(object param)
        {
            return !IsProcessing && totalMusicItems > 1 && (totalMusicItems - processedMusicItems > 0); /*Processing can start if you have at least 2 files to play with*/
        }

        /// <summary>
        ///   Stop processing the songs
        /// </summary>
        /// <param name = "param">Parameter</param>
        public void StopProcessing(object param)
        {
            ProcessingStep = StepAborting;
            processedMusicItems = 0;
            processedPaths = null;
            gate.AbortProcessing();
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

        public void AddFile(object param)
        {
            IOpenFileDialogService fbg = GetService<IOpenFileDialogService>();
            if (fbg != null)
            {
                if (DialogResult.OK == fbg.Show("Please choose your awesome track", "Amazing Track", Helper.GetMultipleFilter("Supported Formats", musicFileFilters), true))
                {
                    string[] selectedPaths = fbg.SelectedPaths;
                    foreach (string selectedPath in selectedPaths)
                    {
                        // Check if such directory exists, and check if such directory wasn't already introduced
                        if (!string.IsNullOrEmpty(selectedPath) && Paths.All(folder => selectedPath != folder.Path))
                        {
                            if (Directory.Exists(selectedPath))
                            {
                                Paths.Add(new Item { Path = selectedPath, Count = -1, IsFolder = true });
                            }
                            else if (File.Exists(selectedPath))
                            {
                                Paths.Add(new Item { Path = selectedPath, Count = 1, IsFolder = false });
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
                    if (musicFileFilters.Where(filter => extension != null).Any(filter => filter.Contains(extension)))
                    {
                        Paths.Add(new Item { Path = param as string, Count = 1, IsFolder = false });
                    }
                }
            }
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    int c = ((Item)item).Count;
                    if (c > 0)
                    {
                        totalMusicItems += c;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    int c = ((Item)item).Count;
                    if (c > 0)
                    {
                        totalMusicItems -= c;
                    }
                }
            }

            OnPropertyChanged("Paths"); /*Signalize that the collection got changed*/
        }

        /// <summary>
        ///   Search the storage for duplicate files
        /// </summary>
        private void FindDuplicates()
        {
            HashSet<TrackData>[] duplicates = null;
            Task.Factory.StartNew(
                () =>
                {
                    duplicates = gate.FindAllDuplicates(
                        (track, total, current) =>
                            {
                                CurrentProgress = (int)Math.Ceiling((float)current / total * 100);
                                if (current >= total)
                                {
                                processedMusicItems = current; /*set processed unit items*/
                                CurrentProgress = 0;
                            }
                        });
                }).ContinueWith( /*Update the UI*/
                task =>
                {
                    ProcessingStep = SetpGeneratingReport;
                    ReportViewModel report = new ReportViewModel { Sets = duplicates };
                    GenericViewModel viewModel = new GenericViewModel();
                    viewModel.Workspaces.Add(report);
                    IGenericViewWindow view = GetService<IGenericViewWindow>();
                    IWindowService windowMediator = GetService<IWindowService>();
                    windowMediator.ShowDialog(view, viewModel, (o, args) => report.StopPlaying());
                    ProcessingStep = StepSelectRootFolder;
                    IsProcessing = false;
                },
                    TaskScheduler);
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