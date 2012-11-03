﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.WordPredictor.Model;

namespace CK.WordPredictor.Engines
{
    internal class SybilleWordPredictorEngineFactory : IWordPredictorEngineFactory
    {
        Func<string> _pluginResourceDirectory;
        const string _sybileDataPath = "Engines\\Sybille\\Data";

        public SybilleWordPredictorEngineFactory( Func<string> pluginResourceDirectory )
        {
            _pluginResourceDirectory = pluginResourceDirectory;
        }

        public IWordPredictorEngine Create( string predictorName )
        {
            string p = _pluginResourceDirectory();
            return DoCreate( predictorName, p );
        }

        public Task<IWordPredictorEngine> CreateAsync( string predictorName )
        {
            string p = _pluginResourceDirectory();
            return Task.Factory.StartNew<IWordPredictorEngine>( () =>
            {
                return DoCreate( predictorName, p );
            } );
        }

        private static IWordPredictorEngine DoCreate( string predictorName, string pluginResourcePath )
        {
            switch( predictorName.ToLowerInvariant() )
            {
                case "sybille": return new SybilleWordPredictorEngine(
                    Path.Combine( pluginResourcePath, _sybileDataPath, "LMbin_fr.sib" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "UserPredictor_fr.sib" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "UserTexts_fr.txt" ) );
                case "sem-sybille": return new SybilleWordPredictorEngine(
                    Path.Combine( pluginResourcePath, _sybileDataPath, "LMbin_fr.sib" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "UserPredictor_fr.sib" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "UserTexts_fr.txt" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemMatrix_fr.bin" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemWords_fr.txt" ),
                    Path.Combine( pluginResourcePath, _sybileDataPath, "SemLambdas_fr.txt" ) );
                default: throw new ArgumentException( String.Format( "There is no word predictor engine for the name: {0}", predictorName ), "predictorName" );
            }
        }

        public void Release( IWordPredictorEngine engine )
        {
            var disposable = engine as IDisposable;
            if( disposable != null )
            {
                disposable.Dispose();
            }
        }
    }
}
