namespace Soundfingerprinting.DuplicatesDetector.Model
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///   Class for folder representation in the UI element
    /// </summary>
    [Serializable]
    public class Item : INotifyPropertyChanged
    {
        /// <summary>
        ///   Number of music files within the folder
        /// </summary>
        private int count;

        /// <summary>
        ///   Path to folder
        /// </summary>
        private string path;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public bool IsFolder { get; set; }

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                if (path != value)
                {
                    path = value;
                    OnPropertyChanged("Path");
                }
            }
        }

        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                if (count != value)
                {
                    count = value;
                    OnPropertyChanged("Count");
                }
            }
        }
        
        private void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler temp = PropertyChanged;
            if (temp != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}