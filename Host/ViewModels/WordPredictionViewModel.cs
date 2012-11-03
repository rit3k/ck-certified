﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;
using CK.Windows.Config;
using Host.Resources;

namespace Host.VM
{
    public class WordPredictionViewModel : ConfigBase
    {
        public WordPredictionViewModel( AppViewModel app )
            : base( "{4DC42B82-4B29-4896-A548-3086AA9421D7}", R.WordPredictionConfig, app )
        {
        }

        public int MaxSuggestedWords
        {
            get { return Config != null ? Config.GetOrSet( "MaxSuggestedWords", 10 ) : 10; }
            set
            {
                if( Config != null ) Config.Set( "MaxSuggestedWords", value );
            }
        }

        public bool InsertSpaceAfterPredictedWord
        {
            get { return Config != null ? Config.GetOrSet( "InsertSpaceAfterPredictedWord", true ) : true; }
            set
            {
                if( Config != null ) Config.Set( "InsertSpaceAfterPredictedWord", value );
            }
        }

        protected override void NotifyOfPropertiesChange()
        {
            base.NotifyOfPropertiesChange();
            NotifyOfPropertyChange( () => MaxSuggestedWords );
        }

        protected override void OnInitialize()
        {
            var g = this.AddActivableSection( R.WordPredictionSectionName, R.WordPredictionConfig );

            var c = new ConfigItemProperty<int>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.MaxSuggestedWords ) );
            c.DisplayName = R.WordPredictionMaxSuggestedWords;
            g.Items.Add( c );

            var p = new ConfigItemProperty<bool>( ConfigManager, this, CK.Reflection.ReflectionHelper.GetPropertyInfo( this, e => e.InsertSpaceAfterPredictedWord ) );
            p.DisplayName = R.WordPredictionInsertSpace;
            g.Items.Add( p );
            base.OnInitialize();
        }

        protected override void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            NotifyOfPropertyChange( () => MaxSuggestedWords );
        }
    }
}