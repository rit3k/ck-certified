using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CommonServices;

namespace TextTemplate
{
    [Plugin( TextTemplate.PluginIdString,
           PublicName = TextTemplate.PluginPublicName,
           Version = TextTemplate.PluginIdVersion,
           Categories = new string[] { "Visual", "Accessibility" } )]
    public class TextTemplate : IPlugin
    {
        internal const string PluginIdString = "{F49A80AC-A38A-45B5-99B6-B95562C3B1E2}";
        Guid PluginGuid = new Guid( PluginIdString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "Text Template";

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<ITriggerService> ExternalInput { get; set; }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            ExternalInput.Service.Triggered += ( o, e ) =>
            {

            };
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
