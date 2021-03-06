﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Windows.Config;

namespace Host.VM
{
    public abstract class ConfigBase : ConfigPage
    {
        AppViewModel _app;
        Guid _editedPluginId;
        IPluginProxy _plugin;
        IObjectPluginConfig _config;

        public IObjectPluginConfig Config
        {
            get { return _config; }
        }

        public ConfigBase( string editedPluginId, string displayName, AppViewModel app )
            : base( app.ConfigManager )
        {
            DisplayName = displayName;

            _editedPluginId = new Guid( editedPluginId );
            _app = app;
            _app.ConfigContainer.Changed += new EventHandler<ConfigChangedEventArgs>( OnConfigChangedWrapper );
            _app.PluginRunner.PluginHost.StatusChanged += ( o, e ) =>
            {
                if( e.PluginProxy.PluginKey.PluginId == _editedPluginId && _plugin == null )
                {
                    InitializePlugin();
                }

                NotifyOfPropertiesChange();
            };
        }

        void InitializePlugin()
        {
            _plugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _editedPluginId, true );
            if( _plugin != null ) _config = _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _plugin );
        }

        public bool ActivatePlugin
        {
            get { return _plugin != null ? _app.PluginRunner.PluginHost.IsPluginRunning( _plugin.PluginKey ) : false; }
            set
            {
                using( var wait = _app.ShowBusyIndicator() )
                {
                    if( value )
                    {
                        _app.StartPlugin( _editedPluginId );

                        if( _plugin == null ) _plugin = _app.PluginRunner.PluginHost.FindLoadedPlugin( _editedPluginId, true );
                        _config = _plugin != null ? _app.ConfigContainer.GetObjectPluginConfig( _app.CivikeyHost.Context.ConfigManager.UserConfiguration, _plugin ) : null;
                    }
                    else
                    {
                        _app.StopPlugin( _editedPluginId );
                    }
                }

                NotifyOfPropertiesChange();
            }
        }

        protected virtual void NotifyOfPropertiesChange()
        {
            NotifyOfPropertyChange( () => ActivatePlugin );
            OnConfigChanged( this, null );
        }

        private void OnConfigChangedWrapper( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Contains( _plugin ) )
            {
                OnConfigChanged( sender, e );
            }
        }

        protected abstract void OnConfigChanged( object sender, ConfigChangedEventArgs e );

        protected ConfigActivableSection AddActivableSection( string name, string description )
        {
            return this.AddActivableSection( name, description, this, h => h.ActivatePlugin, this );
        }

        protected override void OnInitialize()
        {
            InitializePlugin();
            base.OnInitialize();
        }
    }
}
