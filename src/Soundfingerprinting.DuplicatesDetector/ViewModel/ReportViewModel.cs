// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Soundfingerprinting.AudioProxies;
using Soundfingerprinting.DuplicatesDetector.Infrastructure;
using Soundfingerprinting.DuplicatesDetector.Model;
using Soundfingerprinting.DuplicatesDetector.Services;

namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    /// <summary>
    ///   Report view-model
    /// </summary>
    /// <remarks>
    ///   Provides the report result to the view
    /// </remarks>
    public class ReportViewModel : ViewModelBase
    {
        #region Constants

        /// <summary>
        ///   Status of playing (play)
        /// </summary>
        private const string STATUS_PLAY = "Play";

        /// <summary>
        ///   Status of playing (stop)
        /// </summary>
        private const string STATUS_STOP = "Stop";

        #endregion

        #region Private fields

        /// <summary>
        ///   Proxy used in playing files
        /// </summary>
        private readonly BassProxy _proxy = new BassProxy();

        /// <summary>
        ///   Flag - if the player is playing something
        /// </summary>
        private bool _isPlaying;

        /// <summary>
        ///   Opens the location folder where the result item is located
        /// </summary>
        private RelayCommand _openFolderCommand;

        /// <summary>
        ///   Play the selected file
        /// </summary>
        private RelayCommand _playCommand;

        /// <summary>
        ///   Playing status (Stop/Play)
        /// </summary>
        private string _playingStatus = STATUS_PLAY;

        /// <summary>
        ///   Raw results
        /// </summary>
        private HashSet<Track>[] _rawResult;

        /// <summary>
        ///   Results displayed
        /// </summary>
        private ObservableCollection<ResultItem> _results;

        /// <summary>
        ///   Save the results into a file
        /// </summary>
        private RelayCommand _saveCommand;

        /// <summary>
        ///   Status of the player (Show which file is currently playing)
        /// </summary>
        private string _status;

        #endregion

        /// <summary>
        ///   Results displayed on the view
        /// </summary>
        public ObservableCollection<ResultItem> Results
        {
            get { return _results; }
            set
            {
                if (_results != value)
                {
                    _results = value;
                    OnPropertyChanged("Results");
                }
            }
        }

        /// <summary>
        ///   Selected item
        /// </summary>
        public ResultItem SelectedItem { get; set; }

        /// <summary>
        ///   Sets of duplicate files
        /// </summary>
        public HashSet<Track>[] Sets
        {
            get { return _rawResult; }
            set
            {
                _rawResult = value;
                _results = new ObservableCollection<ResultItem>(ProcessRawData(_rawResult));
            }
        }

        /// <summary>
        ///   Status of the player (show which file is currently playing
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        /// <summary>
        ///   Playing status (Play/Stop)
        /// </summary>
        public String PlayingStatus
        {
            get { return _playingStatus; }
            set
            {
                if (_playingStatus != value)
                {
                    _playingStatus = value;
                    OnPropertyChanged("PlayingStatus");
                }
            }
        }

        /// <summary>
        ///   Open explorer folder command
        /// </summary>
        public RelayCommand OpenFolderCommand
        {
            get
            {
                return _openFolderCommand ?? (_openFolderCommand =
                                              new RelayCommand(OpenFolder, CanOpenFolder));
            }
        }

        /// <summary>
        ///   Export results
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand =
                                        new RelayCommand(SaveResults, CanSaveResults));
            }
        }

        /// <summary>
        ///   Play sound command
        /// </summary>
        public RelayCommand PlayCommand
        {
            get
            {
                return _playCommand ?? (_playCommand =
                                        new RelayCommand(PlayStopAudioFile, CanPlayStopAudioFile));
            }
        }

        /// <summary>
        ///   Open containing folder
        /// </summary>
        /// <param name = "param">Selected item</param>
        private void OpenFolder(object param)
        {
            if (SelectedItem != null)
            {
                string path = SelectedItem.Path;
                Process.Start("explorer.exe", Path.GetDirectoryName(path));
            }
        }

        /// <summary>
        ///   Is there any item selected, if yes, allow opening the folder and listening to the music
        /// </summary>
        /// <param name = "param"></param>
        /// <returns></returns>
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
                int totalItems = Sets.Sum((set) => set.Count) + 1;
                object[][] array = new object[totalItems][];
                int i = 0;
                int setId = 0;
                array[i] = new object[4];
                array[i][0] = "Set ID";
                array[i][1] = "Path";
                array[i][2] = "Artist";
                array[i][3] = "Title";
                i++;
                foreach (HashSet<Track> set in Sets)
                {
                    foreach (Track track in set)
                    {
                        array[i] = new object[4];
                        array[i][0] = setId;
                        array[i][1] = track.Path;
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
                        if (!String.IsNullOrEmpty(path))
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
            if (!_isPlaying)
            {
                if (SelectedItem != null)
                {
                    string filename = SelectedItem.Path;
                    Status = "Playing: " + Path.GetFileName(filename);
                    _proxy.StopPlayingFile(); //stop previous play, if such
                    _proxy.PlayFile(filename); //play file
                    PlayingStatus = STATUS_STOP; //change playing status 
                    _isPlaying = !_isPlaying;
                }
                else
                    _proxy.StopPlayingFile(); //stop previous play, if such
            }
            else
            {
                Status = "";
                _proxy.StopPlayingFile(); //stop previous play
                _isPlaying = !_isPlaying;
                PlayingStatus = STATUS_PLAY; //change status
            }
        }

        /// <summary>
        ///   Can play/stop audio file
        /// </summary>
        /// <param name = "param">Parameter</param>
        /// <returns>True/False</returns>
        private bool CanPlayStopAudioFile(object param)
        {
            return SelectedItem != null || _isPlaying;
        }

        /// <summary>
        ///   Process raw results for suitable appearance
        /// </summary>
        /// <param name = "rawData">Raw data</param>
        /// <returns>List of items for view</returns>
        private static List<ResultItem> ProcessRawData(IEnumerable<HashSet<Track>> rawData)
        {
            List<ResultItem> results = new List<ResultItem>();
            int counter = 0;
            foreach (HashSet<Track> item in rawData)
            {
                results.AddRange(item.Select(track => new ResultItem(counter, track)));
                counter++;
            }
            return results;
        }

        /// <summary>
        ///   Stop playing (invoked
        /// </summary>
        public void StopPlaying()
        {
            if (_isPlaying)
            {
                _proxy.StopPlayingFile(); //stop previous play
                _isPlaying = !_isPlaying;
            }
        }
    }
}