﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Context;
using CK.Core;
using CK.Plugin;
using CK.Plugin.Config;
using CommonServices.Accessibility;

namespace OnlineHelp
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class OnlineHelp : IPlugin, IHelpService
    {
        const string PluginGuidString = "{1DB78D66-B5EC-43AC-828C-CCAB91FA6210}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "OnlineHelp";
        public readonly INamedVersionedUniqueId PluginId = new SimpleNamedVersionedUniqueId( PluginGuidString, PluginIdVersion, PluginPublicName );

        public string _defaultCulture = "en";

        [RequiredService]
        public IHostInformation HostInformations { get; set; }

        private string LocalHelpBaseDirectory { get { return Path.Combine( HostInformations.ApplicationDataPath, "HelpContents" ); } }

        private string NoContentFilePath { get { return Path.Combine( LocalHelpBaseDirectory, "Default", "nocontent.html" ); } }

        string GetHelpLocalContentFilePath( INamedVersionedUniqueId pluginName, string culture = null )
        {
            if( culture == null ) culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant();
            
            // try to load the help of the plugin in the good culture (current or given)
            string localhelp = Path.Combine( LocalHelpBaseDirectory, pluginName.UniqueId.ToString( "B" ), pluginName.Version.ToString(), culture, "index.html" );
            if( !File.Exists( localhelp ) )
            {
                // if the help does not exists, and if the given culture is already the default, there is no help content
                if( culture == _defaultCulture )
                    localhelp = NoContentFilePath;
                else
                {
                    // if the given culture is still not the default, and a specialized culture, try to load the help for the base culture
                    if( culture.Contains( '-' ) ) return GetHelpLocalContentFilePath( pluginName, culture.Substring( culture.IndexOf( '-' ) ) );
                    else return GetHelpLocalContentFilePath( pluginName, _defaultCulture );
                }

            }

            return localhelp;
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Teardown()
        {
        }

        public CancellationTokenSource GetHelpContentFor( INamedVersionedUniqueId pluginName, Action<Task<string>> onComplete )
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task<string> getContent = new Task<string>( () =>
            {
                using( StreamReader rdr = new StreamReader( GetHelpLocalContentFilePath( pluginName ) ) )
                {
                    return rdr.ReadToEnd();
                }
            }, cancellationToken.Token );

            getContent.ContinueWith( onComplete, cancellationToken.Token );

            getContent.Start();

            return cancellationToken;
        }

        public bool ShowHelpFor( INamedVersionedUniqueId pluginName, bool force = false )
        {
            string url = GetHelpLocalContentFilePath(pluginName);
            bool found = url != NoContentFilePath;
            if( found || force )
            {
                var startinfo = new System.Diagnostics.ProcessStartInfo( "IExplore.exe", url );
                System.Diagnostics.Process.Start( startinfo );
            }
            return found;
        }
    }
}
