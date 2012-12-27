﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ContextEditor.ViewModels
{
    public sealed class ImageConverter : IValueConverter
    {
        public object Convert( object value, Type targetType,
                              object parameter, CultureInfo culture )
        {
            try
            {
                return new BitmapImage( new Uri( (string)value ) );
            }
            catch
            {
                return new BitmapImage();
            }
        }

        public object ConvertBack( object value, Type targetType,
                                  object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
