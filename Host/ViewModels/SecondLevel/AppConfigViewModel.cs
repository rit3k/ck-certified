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
    public class AppConfigViewModel : ConfigPage
    {
        Guid _keyboardEditorId;
        AppViewModel _app;
        SkinViewModel _sVm;
        AutoClickViewModel _acVm;
        WordPredictionViewModel _wpVm;
        AppAdvancedConfigViewModel _appAdvcVm;

        public AppConfigViewModel( AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = R.AppConfig;
            _app = app;
        }

        protected override void OnInitialize()
        {
            _keyboardEditorId = new Guid( "{66AD1D1C-BF19-405D-93D3-30CA39B9E52F}" );

            var profiles = this.AddCurrentItem( R.Profile, "", _app.CivikeyHost.Context.ConfigManager.SystemConfiguration, a => a.CurrentUserProfile, a => a.UserProfiles, false, "" );
            _app.CivikeyHost.Context.ConfigManager.SystemConfiguration.UserProfiles.CollectionChanged += ( s, e ) =>
            {
                profiles.RefreshValues( s, e );
            };

            this.AddLink( _appAdvcVm ?? ( _appAdvcVm = new AppAdvancedConfigViewModel( _app ) ) );
            this.AddLink( _sVm ?? ( _sVm = new SkinViewModel( _app ) ) );
            this.AddLink( _acVm ?? (_acVm = new AutoClickViewModel( _app )) );
            this.AddLink( _wpVm ?? (_wpVm = new WordPredictionViewModel( _app )) );

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartScrollEditor ) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.ScrollConfig;
                this.Items.Add( action );
            }

            {
                var action = new ConfigItemAction( this.ConfigManager, new SimpleCommand( StartKeyboardEditor ) );
                action.ImagePath = "Forward.png";
                action.DisplayName = R.SkinEditorSectionName;
                this.Items.Add( action );
            }

            this.AddAction( R.ObjectExplorer, R.AdvancedUserNotice, StartObjectExplorer );
            
            base.OnInitialize();
        }

        public void StartObjectExplorer()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{4BF2616D-ED41-4E9F-BB60-72661D71D4AF}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartKeyboardEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.PluginsStatus.SetStatus( _keyboardEditorId, ConfigPluginStatus.AutomaticStart );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }

        public void StartScrollEditor()
        {
            _app.CivikeyHost.Context.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( new Guid( "{48D3977C-EC26-48EF-8E47-806E11A1C041}" ), ConfigUserAction.Started );
            _app.CivikeyHost.Context.PluginRunner.Apply();
        }
    }
}
