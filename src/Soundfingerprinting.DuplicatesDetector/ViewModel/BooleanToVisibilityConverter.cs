namespace Soundfingerprinting.DuplicatesDetector.ViewModel
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isprocessing = (bool) (value);
            return isprocessing ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility v = (Visibility) value;
            switch (v)
            {
                case Visibility.Hidden:
                    return false;
                case Visibility.Collapsed:
                    return false;
                case Visibility.Visible:
                    return true;
            }
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}