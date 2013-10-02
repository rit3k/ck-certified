﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MouseRadar
{
    public class RadarViewModel : INotifyPropertyChanged
    {
        public const int ARROW_LENGTH = 15;

        float _angle;
        SolidColorBrush _circleColor;
        SolidColorBrush _arrowColor;
        int _radarSize;
        float _opacity;
        public float AngleMin { get; set; }
        public float AngleMax { get; set; }

        public RadarViewModel()
        {
            _opacity = 1;
            AngleMin = 0;
            AngleMax = 360;

        }

        public float Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                FirePropertyChanged( "Opacity" );
            }
        }
        public int RadarSize
        {
            get { return _radarSize; }
            set
            {
                _radarSize = value;
                FirePropertyChanged( "RadarSize" );
                FirePropertyChanged( "WindowSize" );
                FirePropertyChanged( "PointerPositionX" );
                FirePropertyChanged( "PointerPositionY" );
                FirePropertyChanged( "RotationOriginX" );
                FirePropertyChanged( "RotationOriginY" );
            }
        }

        public int WindowSize
        {
            get { return RadarSize + 50; }
        }

        public int PointerPositionY
        {
            get { return RadarSize / 2 - ARROW_LENGTH / 2; }
        }

        public int PointerPositionX
        {
            get { return RadarSize - ARROW_LENGTH / 2; }
        }

        public int RotationOriginX
        {
            get { return -RadarSize / 2 + ARROW_LENGTH / 2; }
        }

        public int RotationOriginY
        {
            get { return ARROW_LENGTH / 2; }
        }

        public SolidColorBrush CircleColor
        {
            get { return _circleColor; }
            set
            {
                _circleColor = value;
                FirePropertyChanged( "CircleColor" );
            }
        }

        public SolidColorBrush ArrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                FirePropertyChanged( "ArrowColor" );
            }
        }

        public float Angle
        {
            get { return _angle; }
            set
            {
                if( AngleMin > AngleMax )
                {
                    if( value >= AngleMax && value <= AngleMin) _angle = value - AngleMax + AngleMin;
                    else if( value >= 360 ) _angle = value - 360;
                    else _angle = value;
                }
                else
                {
                    if( value >= AngleMax ) _angle = value - AngleMax + AngleMin;
                    else if( value < AngleMin ) _angle = value + AngleMax;
                    else _angle = value;
                }
                
                FirePropertyChanged( "Angle" );
            }
        }

        public void SetCircleColor( int a, int r, int g, int b )
        {
            CircleColor = new SolidColorBrush( Color.FromArgb( (byte)a, (byte)r, (byte)g, (byte)b ) ); //Color.FromArgb( a, r, g, b ) );
        }

        public void SetArrowColor( int a, int r, int g, int b )
        {
            ArrowColor = new SolidColorBrush( Color.FromArgb( (byte)a, (byte)r, (byte)g, (byte)b ) ); //Color.FromArgb( a, r, g, b ) );
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void FirePropertyChanged( string propertyName )
        {
            if( PropertyChanged != null ) PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
