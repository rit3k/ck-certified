﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI
{
    [Plugin( "{1756C34D-EF4F-45DA-9224-1232E96964D2}", PublicName = "Word Prediction - In Keyboard", Categories = new string[] { "Prediction", "Visual" } )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IKeyboardContext Context { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IWordPredictorService> WordPredictorService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }


        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( Context != null )
            {
                Feature.PredictionContextFactory.CreatePredictionZone( Context.CurrentKeyboard, Feature.MaxSuggestedWords );
                Context.CurrentKeyboardChanged += OnCurrentKeyboardChanged;
            }

            if( WordPredictorService != null )
            {
                WordPredictorService.ServiceStatusChanged += OnWordPredictorServiceStatusChanged;
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }

            if( Feature != null )
            {
                Feature.PropertyChanged += OnFeaturePropertyChanged;
            }
        }

        void OnFeaturePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "WordPredictionMaxSuggestedWords" )
            {
                object newValue = Config.User["WordPredictionMaxSuggestedWords"];
                if( newValue != null )
                {
                    int newIntValue = (int)newValue;
                    if( newIntValue == Feature.MaxSuggestedWords ) return; //We don't remove and recreate the zone if the new value equals the previous one
                }

                DestroyPredictionZones();
                Feature.PredictionContextFactory.CreatePredictionZone( Context.CurrentKeyboard, Feature.MaxSuggestedWords );
            }
        }

        public void Stop()
        {
            if( WordPredictorService != null && WordPredictorService.Service != null )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
                WordPredictorService.ServiceStatusChanged -= OnWordPredictorServiceStatusChanged;
            }
            if( Context != null )
            {
                DestroyPredictionZones();
                Context.CurrentKeyboardChanged -= OnCurrentKeyboardChanged;
            }
        }

        private void DestroyPredictionZones()
        {
            foreach( IKeyboard k in Context.Keyboards )
            {
                IZone zone = k.Zones[Feature.PredictionContextFactory.PredictionZoneName];
                if( zone != null )
                {
                    for( int i = zone.Keys.Count - 1; i >= 0; i-- )
                    {
                        zone.Keys[i].Destroy();
                    }

                    zone.Destroy();
                }
            }
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            Feature.PredictionContextFactory.CreatePredictionZone( e.Current, Feature.MaxSuggestedWords );
        }

        void OnWordPredictorServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Stopping )
            {
                WordPredictorService.Service.Words.CollectionChanged -= OnWordPredictedCollectionChanged;
            }
            if( e.Current == InternalRunningStatus.Starting )
            {
                WordPredictorService.Service.Words.CollectionChanged += OnWordPredictedCollectionChanged;
            }
        }

        protected void OnWordPredictedCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            var zone = Context.CurrentKeyboard.Zones[Feature.PredictionContextFactory.PredictionZoneName];
            if( zone != null )
            {
                if( e.Action == NotifyCollectionChangedAction.Reset )
                {
                    for( int i = 0; i < Feature.MaxSuggestedWords; ++i )
                    {
                        zone.Keys[i].CurrentLayout.Current.Visible = false;
                    }
                }
                else if( e.Action == NotifyCollectionChangedAction.Add )
                {
                    int idx = e.NewStartingIndex;
                    if( idx < zone.Keys.Count )
                    {
                        IKey key = zone.Keys[idx];
                        if( key != null && e.NewStartingIndex < Feature.MaxSuggestedWords )
                        {
                            IWordPredicted wordPredicted = WordPredictorService.Service.Words[e.NewStartingIndex];
                            if( wordPredicted != null )
                            {
                                key.Current.DownLabel = wordPredicted.Word;
                                key.Current.UpLabel = wordPredicted.Word;
                            key.Current.OnKeyDownCommands.Commands.Clear();
                            key.Current.OnKeyDownCommands.Commands.Add( CommandFromWord( wordPredicted ) );
                                key.CurrentLayout.Current.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        protected virtual string CommandFromWord( IWordPredicted wordPredicted )
        {
            return String.Format( @"{0}:{1}", "sendPredictedWord", wordPredicted.Word );
        }

        public void Teardown()
        {
        }
    }
}
