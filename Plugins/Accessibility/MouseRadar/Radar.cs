using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using CommonServices;

namespace MouseRadar
{
    public partial class Radar : IDisposable
    {
        IPointerDeviceDriver _mouseDriver;
        int _originX;
        int _originY;
        int _translationDirection = 1;
        int _rotationDirection = 1;
        ScreenBound _previousCollision = ScreenBound.None;
        DispatcherTimer _timerRotate;
        DispatcherTimer _timerTranslate;
        public event EventHandler<ScreenBoundCollideEventArgs> ScreenBoundCollide;
        public double Slope { get; private set; }
        public int RotationSpeed { get; set; }
        public int TranslationSpeed { get; set; }
        public RadarViewModel Model { get; private set; }
        
        public Radar(IPointerDeviceDriver pdd) : this()
        {
            Model = new RadarViewModel();
            Model.SetCircleColor( 255, 0, 0, 0 );
            DataContext = Model;
            _mouseDriver = pdd;
            Left = pdd.CurrentPointerXLocation;
            Top = pdd.CurrentPointerYLocation;
            RotationSpeed = 1; 
            TranslationSpeed = 1;

            _timerRotate = new DispatcherTimer( DispatcherPriority.Send );
            _timerRotate.Interval = new TimeSpan( 10000 * 17 );  //60 fps
            _timerRotate.Tick += ( o, e ) =>
            {
                Model.Angle += (float)RotationSpeed / 1f;
            };

            _timerTranslate = new DispatcherTimer( DispatcherPriority.Send );
            _timerTranslate.Interval = new TimeSpan( 10000 * 17 ); //60 fps
            _timerTranslate.Tick += ProcessTranslation;

            _mouseDriver.PointerMove += UpdateLocation;
        }

        private ScreenBound CheckBoundCollision( Point p )
        {
            ScreenBound collision = ScreenBound.None;
            if( p.X <= Screen.PrimaryScreen.Bounds.Left ) collision = ScreenBound.Left;
            if( p.X >= Screen.PrimaryScreen.Bounds.Right ) collision = ScreenBound.Right;
            if( p.Y <= Screen.PrimaryScreen.Bounds.Top ) collision = ScreenBound.Top;
            if( p.Y >= Screen.PrimaryScreen.Bounds.Bottom ) collision = ScreenBound.Bottom;

            return collision;
        }

        void ProcessTranslation(object sender, EventArgs e)
        {
            int moveX = _mouseDriver.CurrentPointerXLocation;
            int moveY = _mouseDriver.CurrentPointerYLocation;

            var p = GetTranslation( moveX, moveY );
            ScreenBound collision = CheckBoundCollision( p );
            switch( collision )
            {
                default:
                    moveX = (int)p.X;
                    moveY = (int)p.Y;
                    break;
                case ScreenBound.Left:
                    moveX = 0;
                    moveY = (int)p.Y;
                    
                    break;
                case ScreenBound.Top:
                    moveY = 0;
                    moveX = (int)p.X;
                    break;
                case ScreenBound.Right:
                    moveX = Screen.PrimaryScreen.Bounds.Right;
                    moveY = (int)p.Y;
                    break;
                case ScreenBound.Bottom:
                    moveY = Screen.PrimaryScreen.Bounds.Bottom;
                    moveX = (int)p.X;
                    break;
            }

            if( collision != ScreenBound.None ) FireScreenBoundCollide( collision );
            _mouseDriver.MovePointer( moveX, moveY ); 
        }

        public void StartRotation()
        {
            _timerRotate.Start();
        }

        public void EndRotation()
        {
            _timerRotate.Stop();
        }

        public void StartTranslation()
        {
            _originX =  _mouseDriver.CurrentPointerXLocation;
            _originY = _mouseDriver.CurrentPointerYLocation;

            double rad = Model.Angle / 180 * Math.PI;
            Slope = Math.Tan( rad ); //Slope => pente

            _translationDirection = Math.Sign( Math.Cos( rad ) );
            _timerRotate.Stop();
            _timerTranslate.Start();
        }

        public void StopTranslation(bool startRotation = true)
        {
            _timerTranslate.Stop();
            if( startRotation ) _timerRotate.Start();
        }

        public void UpdateLocation( object sender, PointerDeviceEventArgs e )
        {
            Left = e.X - Width / 2;
            Top = e.Y - Height / 2;

            if( _previousCollision != ScreenBound.None && CheckBoundCollision( new Point(e.X, e.Y) ) == ScreenBound.None ) 
                FireScreenBoundCollide(ScreenBound.None);
        }

        Point GetTranslation( int x, int y )
        {
            double b = _originY - Slope * _originX; //Compute the origine
            int xp = x + TranslationSpeed * _translationDirection; //move x
            double yp = Slope * xp + b; //get y= ax + b

            return new Point( xp,  yp );
        }

        void FireScreenBoundCollide(ScreenBound bound)
        {
            Console.WriteLine("Colision : " + bound);
            _previousCollision = bound;
            if( ScreenBoundCollide != null ) 
                ScreenBoundCollide(this, new ScreenBoundCollideEventArgs(bound));
        }

        #region IDisposable Members

        public void Dispose()
        {
            _timerRotate.Stop();
            _timerTranslate.Stop();
            _mouseDriver.PointerMove -= UpdateLocation;
            this.Close();
        }

        #endregion
    }

    [Flags]
    public enum ScreenBound
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public class ScreenBoundCollideEventArgs : EventArgs
    {
        public ScreenBound ScreenBound { get; private set; }

        public ScreenBoundCollideEventArgs(ScreenBound b)
        {
            ScreenBound = b;
        }
    }

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
            }
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
                else if( value < AngleMax ) _angle = value + AngleMax;
                else _angle = value;
                FirePropertyChanged( "Angle" );
            }
        }

        public void SetCircleColor( int a, int r, int g, int b )
        {
            CircleColor = new SolidColorBrush( Color.FromArgb( (byte)a, (byte)r, (byte)g, (byte) b ) );//Color.FromArgb( a, r, g, b ) );
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
