// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Soundfingerprinting.DuplicatesDetector.Themes
{
    /// <summary>
    ///   Converter which alternates the color on duplicate sets
    /// </summary>
    [ValueConversion(typeof (object), typeof (int))]
    public class SetIdConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Int32)
            {
                Int32 setId = (Int32) value;
                return setId%2 == 0 ?
                                        new SolidColorBrush(Color.FromArgb(100, 0, 100, 150)) :
                                                                                                  new SolidColorBrush(Colors.Transparent);
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