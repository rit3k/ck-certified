#region LGPL License
/*----------------------------------------------------------------------------
* This file (Host\ViewModels\AppConfigViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CK.WPF.Controls;
using CK.Plugin.Config;
using CK.Core;
using CK.Reflection;
using Host.Resources;
using System.ComponentModel;
using CK.Windows.Config;
using CK.Windows;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Security.Principal;
using System.Security.AccessControl;

namespace Host.VM
{
    public class AppAdvancedConfigViewModel : ConfigPage
    {
        string _stopReminderPath;
        AppViewModel _app;

        public AppAdvancedConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AdvancedAppConfig;
            _app = app;
        }

        protected override void OnInitialize()
        {
            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            var g = this.AddGroup();
            g.AddProperty( R.ShowTaskbarIcon, _app, a => a.ShowTaskbarIcon );
            g.AddProperty( R.ShowSystrayIcon, _app, a => a.ShowSystrayIcon );
            g.AddProperty( R.RemindMeOfNewUpdates, this, a => a.RemindMeOfNewUpdates );

            string stopReminderFolderPath = Path.Combine( _app.CivikeyHost.ApplicationDataPath, "Updates" );
            if( !Directory.Exists( stopReminderFolderPath ) ) Directory.CreateDirectory( stopReminderFolderPath );
            _stopReminderPath = Path.Combine( stopReminderFolderPath, "StopReminder" );
            _remindMeOfNewUpdates = !File.Exists( _stopReminderPath );

            base.OnInitialize();
        }

        bool _remindMeOfNewUpdates;
        public bool RemindMeOfNewUpdates
        {
            get { return _remindMeOfNewUpdates; }
            set
            {
                if( value )
                {
                    File.Delete( _stopReminderPath );
                }
                else
                {
                    File.Create( _stopReminderPath ).Close();
                    FileSecurity f = File.GetAccessControl( _stopReminderPath );
                    var sid = new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null );
                    NTAccount account = (NTAccount)sid.Translate( typeof( NTAccount ) );
                    f.AddAccessRule( new FileSystemAccessRule( account, FileSystemRights.Modify, AccessControlType.Allow ) );
                    File.SetAccessControl( _stopReminderPath, f );
                }

                _remindMeOfNewUpdates = value;
                NotifyOfPropertyChange( () => RemindMeOfNewUpdates );
            }
        }
    }
}
