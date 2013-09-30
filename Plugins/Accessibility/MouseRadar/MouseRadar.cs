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

            ExternalInput.Service.Triggered +=  (o, e) => {
                _radar.StartTranslation();
            };
            MouseDriver.Service.PointerButtonUp += ( o, e ) =>
            {
                _radar.StartTranslation();
            };
            _radar.ScreenBoundCollide += ( o, e ) => {
                _radar.StopTranslation();
                switch( e.ScreenBound )
                {
                    case ScreenBound.Left :
                        break;
                }
            };
            
            _radar.StartRotation();
            _radar.Show();
        }

        public void Stop()
        {

        }

        public void Teardown()
        {

        }

        #endregion
    }

    
}
