﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Plugin.Config;
using CK.Plugins.SendInput;
using CK.WordPredictor.Model;
using CommonServices;

namespace CK.WordPredictor
{
    [Plugin( "{8789CDCC-A7BB-46E5-B119-28DC48C9A8B3}", PublicName = "Simple TextualContext aware predicted word sender", Description = "Listens to a successful prediction and prints the word, according to the current textual context.", Categories = new string[] { "Prediction" } )]
    public class SimpleTextualContextAwarePredictedWordSender : IPlugin
    {
        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IWordPredictedService WordPredictedService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ITextualContextService TextualContextService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public ISendStringService SendStringService { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExist )]
        public IWordPredictorFeature Feature { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        protected virtual void OnWordPredictionSuccessful( object sender, WordPredictionSuccessfulEventArgs e )
        {
            if( TextualContextService != null )
            {
                int currentContextTokenLenth = TextualContextService.CurrentToken.Value.Length;
                string wordToSend = e.Word.Substring( currentContextTokenLenth, e.Word.Length - currentContextTokenLenth );

                if( Feature.InsertSpaceAfterPredictedWord ) wordToSend += " ";

                SendStringService.SendString( wordToSend );
            }
        }


        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            if( WordPredictedService != null )
                WordPredictedService.WordPredictionSuccessful += OnWordPredictionSuccessful;
        }

        public void Stop()
        {
            if( WordPredictedService != null )
                WordPredictedService.WordPredictionSuccessful -= OnWordPredictionSuccessful;
        }

        public void Teardown()
        {
        }
    }
}
