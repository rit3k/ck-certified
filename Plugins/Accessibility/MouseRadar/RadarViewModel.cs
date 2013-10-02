using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MouseRadar
{
    public class RadarViewModel : INotifyPropertyChanged
    {
        float _angle;
        SolidColorBrush _circleColor;
        int _radarSize;

        public float AngleMin { get; set; }
        public float AngleMax { get; set; }

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
            get { return RadarSize + 16; }
        }

        public int PointerPositionY
        {
            get { return RadarSize / 2 - 8; }
        }

        public int PointerPositionX
        {
            get { return RadarSize - 8; }
        }

        public int RotationOriginX
        {
            get { return -RadarSize / 2 + 8; }
        }

        public int RotationOriginY
        {
            get { return 8; }
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

        public float Angle
        {
            get { return _angle; }
            set
            {
                if( value >= AngleMax ) _angle = value - AngleMax + AngleMin;
                else if( value < AngleMin ) _angle = value + AngleMax;
                else _angle = value;
                FirePropertyChanged( "Angle" );
            }
        }

        public void SetCircleColor( int a, int r, int g, int b )
        {
            CircleColor = new SolidColorBrush( Color.FromArgb( (byte)a, (byte)r, (byte)g, (byte)b ) );//Color.FromArgb( a, r, g, b ) );
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
