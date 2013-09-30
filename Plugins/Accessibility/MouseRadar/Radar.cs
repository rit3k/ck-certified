using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using CommonServices;

namespace MouseRadar
{
    public partial class Radar
    {
        IPointerDeviceDriver _mouseDriver;
        int _originX;
        int _originY;
        int _translationDirection = 1;
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
            DataContext = Model;
            _mouseDriver = pdd;
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
            _timerTranslate.Tick += ( o, e ) =>
            {
                var p = GetTranslation( _mouseDriver.CurrentPointerXLocation + TranslationSpeed * _translationDirection );
                if( CheckBoundCollision( p ) == ScreenBound.None)
                    _mouseDriver.MovePointer( (int)p.X, (int)p.Y );
            };

            _mouseDriver.PointerMove += (o, e) => {
                UpdateLocation( e.X, e.Y );
            };
        }

        private ScreenBound CheckBoundCollision( Point p )
        {
            ScreenBound collision = ScreenBound.None;
            if( p.X <= Screen.PrimaryScreen.Bounds.Left ) collision = ScreenBound.Left;
            if( p.X >= Screen.PrimaryScreen.Bounds.Right ) collision = ScreenBound.Right;
            if( p.Y <= Screen.PrimaryScreen.Bounds.Top ) collision = ScreenBound.Top;
            if( p.Y >= Screen.PrimaryScreen.Bounds.Bottom ) collision = ScreenBound.Bottom;

            if( collision != ScreenBound.None ) FireScreenBoundCollide( collision );
            return collision;
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

        public void UpdateLocation( int x, int y )
        {
            Left = x - Width / 2;
            Top = y - Height / 2;
        }

        Point GetTranslation( int x )
        {
            int xp = x - _originX;       //Place the coordinate system to the click zone
            double yp = Slope * xp;     //Equation y= ax + b with b = 0

            return new Point( x, yp + _originY ); //Get the computed screen coordinates
        }

        void FireScreenBoundCollide(ScreenBound bound)
        {
            if( ScreenBoundCollide != null ) 
                ScreenBoundCollide(this, new ScreenBoundCollideEventArgs(bound));
        }
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

        public float Angle
        {
            get { return _angle; }
            set
            {
                if( value >= 360 ) _angle = value - 360;
                else if( value < 0 ) _angle = value + 360;
                else _angle = value;
                FirePropertyChanged( "Angle" );
            }
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
