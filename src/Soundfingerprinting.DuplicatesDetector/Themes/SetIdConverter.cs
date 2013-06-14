namespace SoundFingerprinting.DuplicatesDetector.Themes
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    ///   Converter which alternates the color on duplicate sets
    /// </summary>
    [ValueConversion(typeof(object), typeof(int))]
    public class SetIdConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int setId = (int)value;
                return setId % 2 == 0 ? new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)) : new SolidColorBrush(Colors.Transparent);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}