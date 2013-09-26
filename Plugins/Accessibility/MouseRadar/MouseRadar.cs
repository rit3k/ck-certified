using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CK.Plugin;
using CommonServices;

namespace MouseRadar
{
    [Plugin( MouseRadar.PluginIdString,
           PublicName = MouseRadar.PluginPublicName,
           Version = MouseRadar.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class MouseRadar : RadarViewModel, IPlugin
    {
        internal const string PluginIdString = "{390AFE83-C5A2-4733-B5BC-5F680ABD0111}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "MouseRadar";
        DispatcherTimer _timerRotate;
        DispatcherTimer _timerTranslate;
        Radar _radar;
        float _currentA;
        int direction = 1;
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            _timerRotate = new DispatcherTimer();
            _timerRotate.Interval = new TimeSpan( 5000 );
            _timerRotate.Tick += ( o, e ) =>
            {
                Angle+= 0.001f;
            };
            _timerTranslate = new DispatcherTimer();
            _timerRotate.Interval = new TimeSpan( 50000 );

            _timerTranslate.Tick += ( o, e ) =>
            {
                Angle += 0.001f;
                var p = GetTranslation( MouseDriver.Service.CurrentPointerXLocation + 1);
                MouseDriver.Service.MovePointer((int)p.X, (int)p.Y);
            };
            _radar = new Radar() { DataContext = this };

            return true;
        }

        public void Start()
        {

            _timerRotate.Start();
            MouseDriver.Service.PointerMove += ( o, e ) => { UpdateRadarLocation( e.X, e.Y ); };
            
            _radar.Show();
            TranslatePointerTo( 100, 10 );
        }

        public void Stop()
        {
        }

        public void Teardown()
        {

        }

        #endregion

        void UpdateRadarLocation(int x, int y)
        {
            _radar.Left = x;
            _radar.Top = y;
        }

        void TranslatePointerTo(int dx, int dy)
        {
            int sx = MouseDriver.Service.CurrentPointerXLocation;
            int sy = MouseDriver.Service.CurrentPointerYLocation;

            _currentA = (float)(dy - sy) / (float)(dx - sx);
            if( dx - sx < 0 ) direction = -1;
  
            _timerTranslate.Start();
        }

        Point GetTranslation(int x)
        {
            return new Point( x * direction, _currentA * x );
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
                _angle = value;
                FirePropertyChanged( "Angle" );
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void FirePropertyChanged(string propertyName)
        {
            if( PropertyChanged != null ) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
