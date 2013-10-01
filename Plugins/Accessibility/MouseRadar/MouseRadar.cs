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
        
        Radar _radar;
        
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {   
            return true;
        }

        public void Start()
        {
            _radar = new Radar( MouseDriver.Service );

            ExternalInput.Service.Triggered += TranslateRadar;
            MouseDriver.Service.PointerButtonUp += TranslateRadar;

            _radar.ScreenBoundCollide += ( o, e ) => {
                
                switch( e.ScreenBound )
                {
                    case ScreenBound.Left :
                        _radar.Model.AngleMin = 270;
                        _radar.Model.AngleMax = 90;
                        break;
                    case ScreenBound.Top:
                        _radar.Model.AngleMin = 0;
                        _radar.Model.AngleMax = 180;
                        break;
                    case ScreenBound.Right:
                        _radar.Model.AngleMin = 90;
                        _radar.Model.AngleMax = 270;
                        break;
                    case ScreenBound.Bottom:
                        _radar.Model.AngleMin = 90;
                        _radar.Model.AngleMax = 360;
                        break;
                    default :
                        _radar.Model.AngleMin = 0;
                        _radar.Model.AngleMax = 360;
                        break;
                }
                if( e.ScreenBound != ScreenBound.None ) _radar.StopTranslation();
            };
            
            _radar.StartRotation();
            _radar.Show();
        }

        void TranslateRadar(object o, EventArgs e)
        {
            if(_radar.IsTranslating()) _radar.StopTranslation();
            else _radar.StartTranslation();
        }

        public void Stop()
        {
            _radar.Dispose();
            MouseDriver.Service.PointerButtonUp -= TranslateRadar;
            ExternalInput.Service.Triggered -= TranslateRadar;
        }

        public void Teardown()
        {

        }

        #endregion
    }
}
