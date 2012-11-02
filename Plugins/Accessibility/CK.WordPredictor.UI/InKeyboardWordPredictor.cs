﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CK.Keyboard.Model;
using CK.Plugin;
using CK.Plugin.Config;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.UI
{
    [Plugin( "{1756C34D-EF4F-45DA-9224-1232E96964D2}", PublicName = "CK.Wordpredictor.UI | InKeyboard" )]
    public class InKeyboardWordPredictor : IPlugin
    {
        [RequiredService]
        public IKeyboardContext Context { get; set; }

        [RequiredService]
        public IWordPredictorService WordPredictorService { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        public const string CompatibilityKeyboardName = "Azerty";
        public const string PredictionZoneName = "Prediction";
        public const int DefaultMaxDisplayedWords = 10;

        public int MaxDisplayedWords
        {
            get { return Config.User.TryGet( "MaxDisplayedWords", DefaultMaxDisplayedWords ); }
        }

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            IKeyboard kb = GetAzertyKeyboard();
            if( kb != null )
                CreatePredictionZone( kb );

            if( WordPredictorService != null )
                WordPredictorService.Words.CollectionChanged += OnWordPredictedCollectionChanged;
        }

        protected void OnWordPredictedCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( Context.CurrentKeyboard.Name == CompatibilityKeyboardName )
            {
                var zone = Context.CurrentKeyboard.Zones[PredictionZoneName];
                if( zone != null )
                {
                    if( e.Action == NotifyCollectionChangedAction.Reset )
                    {
                        for( int i = 0; i < MaxDisplayedWords; ++i )
                        {
                            zone.Keys[i].CurrentLayout.Current.Visible = false;
                        }
                    }
                    else if( e.Action == NotifyCollectionChangedAction.Add )
                    {
                        int idx = e.NewStartingIndex;
                        IKey key = zone.Keys[idx];
                        if( key != null )
                        {
                            IWordPredicted wordPredicted = WordPredictorService.Words[e.NewStartingIndex];
                            if( wordPredicted != null )
                            {
                                key.Current.DownLabel = wordPredicted.Word;
                                key.Current.UpLabel = wordPredicted.Word;
                                key.Current.OnKeyPressedCommands.Commands.Clear();
                                key.Current.OnKeyPressedCommands.Commands.Add( CommandFromWord( wordPredicted ) );
                                key.CurrentLayout.Current.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        protected virtual string CommandFromWord( IWordPredicted wordPredicted )
        {
            return String.Format( @"sendPredictedWord""{0}""", wordPredicted.Word.ToLowerInvariant() );
        }

        public void Stop()
        {
            IKeyboard kb = GetAzertyKeyboard();
            if( kb != null )
            {
                IZone zone = kb.Zones[PredictionZoneName];
                if( zone != null ) zone.Destroy();
            }
        }

        public void Teardown()
        {
        }


        private IKeyboard GetAzertyKeyboard()
        {
            IKeyboard azertyKeyboard = Context.Keyboards[CompatibilityKeyboardName];
            if( azertyKeyboard != null )
            {
                return azertyKeyboard;
            }

            return null;
        }

        protected virtual void CreatePredictionZone( IKeyboard kb )
        {
            if( kb.Zones[PredictionZoneName] == null )
            {
                IZone predictionZone = kb.Zones.Create( PredictionZoneName );
                if( predictionZone != null )
                {
                    int wordWidth = Context.CurrentKeyboard.CurrentLayout.W / MaxDisplayedWords - 5;
                    int offset = 2;

                    for( int i = 0; i < MaxDisplayedWords; ++i )
                    {
                        IKey key = predictionZone.Keys.Create( i );
                        key.CurrentLayout.Current.Visible = false;
                        ConfigureKey( key.CurrentLayout.Current, i, wordWidth, offset );
                    }
                }
            }
        }

        protected virtual void ConfigureKey( ILayoutKeyModeCurrent layoutKeyMode, int idx, int wordWidth, int offset )
        {
            if( layoutKeyMode == null ) throw new ArgumentNullException( "layoutKeyMode" );
            layoutKeyMode.X = idx * 5 + (idx - 1) * wordWidth + offset;
            layoutKeyMode.Y = 5;
            layoutKeyMode.Width = wordWidth;
            layoutKeyMode.Height = 45;
        }


    }
}
