﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Configuration;

namespace CiviKeyPostInstallScript
{
    class Program
    {
        public static string _distributionName { get; set; }
        static string _appName;

        /// <summary>
        /// Full path of application-specific data repository, for the current roaming user.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        static string _commonAppDataPath;
        public static string CommonApplicationDataPath
        {
            get { return _commonAppDataPath ?? GetFilePath( Environment.SpecialFolder.CommonApplicationData, out _commonAppDataPath ); }
        }

        /// <summary>
        /// Gets the full path of application-specific data repository, for all users.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// The directory is created if it does not exist.
        /// </summary>
        static string _appDataPath;
        public static string ApplicationDataPath
        {
            get { return _appDataPath ?? GetFilePath( Environment.SpecialFolder.ApplicationData, out _appDataPath ); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">0 - Version
        ///                    1 - AppName
        ///                    2 - DistributionName
        ///                    3 - AupdateServerUrl
        ///                    4 - UpdaterGUID
        ///                    5 - RemoveExistingCtx
        ///                    6 - IsStandAloneInstance
        ///                    7 - ApplicationDirectory
        ///                    8 - ApplicationExePath</param>
        static void Main( string[] args )
        {
            if( args == null || args.Length != 9 )
                OnError( @"There is not the right number of parameters, there should be 9 of them :
                            0 - Version
                            1 - AppName
                            2 - DistributionName
                            3 - AupdateServerUrl
                            4 - UpdaterGUID
                            5 - RemoveExistingCtx
                            6 - IsStandAloneInstance
                            7 - ApplicationDirectory
                            8 - ApplicationExePath" );

            string systemConfPath;
            string systemConfDir;
            string userConfPath;
            string userConfDir;
            string contextPath;
            string contextDir;

            Console.Out.WriteLine( "Version :" + args[0] );
            Console.Out.WriteLine( "AppName :" + args[1] );
            Console.Out.WriteLine( "DistributionName :" + args[2] );
            Console.Out.WriteLine( "AupdateServerUrl :" + args[3] );
            Console.Out.WriteLine( "UpdaterGUID :" + args[4] );
            Console.Out.WriteLine( "RemoveExistingCtx :" + args[5] );
            Console.Out.WriteLine( "IsStandAloneInstance :" + args[6] );
            Console.Out.WriteLine( "standAloneConfigDir :" + args[7] );
            Console.Out.WriteLine( "ApplicationExePath :" + args[8] );
            Console.ReadKey();

            string version = args[0];
            _appName = args[1];
            _distributionName = args[2];
            string updateServerUrl = args[3];
            string updaterGuid = args[4];  //"11C83441-6818-4A8B-97A0-1761E1A54251";

            bool removeExistingCtx;
            bool.TryParse( args[5], out removeExistingCtx );
            bool _isStandAloneInstance;
            bool.TryParse( args[6], out _isStandAloneInstance );

            string standAloneConfigDir = args[7];
            if( !Directory.Exists( standAloneConfigDir ) ) return;

            string applicationExePath = args[8];

            if( _isStandAloneInstance )
            {
                systemConfDir = standAloneConfigDir;
                userConfDir = standAloneConfigDir;
                contextDir = standAloneConfigDir;
            }
            else
            {
                systemConfDir = CommonApplicationDataPath;
                userConfDir = ApplicationDataPath;
                contextDir = ApplicationDataPath;
            }

            systemConfPath = Path.Combine( systemConfDir, "System.config.ck" );
            userConfPath = Path.Combine( userConfDir, "User.config.ck" );
            contextPath = Path.Combine( contextDir, "Context.xml" );

            string hostGuid = "1A2DC25C-E357-488A-B2B2-CD2D7E029856";
            string updateDoneDir = Path.Combine( Path.GetTempPath(), _appName + Path.DirectorySeparatorChar + _distributionName );
            string updateDone = Path.Combine( updateDoneDir, "UpdateDone" );
            Directory.CreateDirectory( updateDoneDir );
            File.Create( updateDone );

            if( File.Exists( systemConfPath ) )
            {
                XmlDocument xPathDoc = new XmlDocument();
                xPathDoc.Load( systemConfPath );
                XPathNavigator xPathNav = xPathDoc.CreateNavigator();

                AddEntryToSystemConf( "UpdateServerUrl", updateServerUrl, updaterGuid, "Update Checker", xPathNav );
                AddEntryToSystemConf( "DistributionName", _distributionName, updaterGuid, "Update Checker", xPathNav );
                AddEntryToSystemConf( "Version", version, hostGuid, "Host", xPathNav );

                xPathDoc.Save( systemConfPath );

                if( removeExistingCtx )
                {
                    if( File.Exists( contextPath ) )
                    {
                        string renamedFilePath = Path.Combine( contextDir, "Context.RenamedBefore_" + version + ".xml" );
                        if( !File.Exists( renamedFilePath ) ) //in case this version has already been installed, don't overwrite the saved context
                        {
                            File.Move( contextPath, renamedFilePath );
                        }
                    }
                }

                Configuration appConf = ConfigurationManager.OpenExeConfiguration( applicationExePath );
                if( appConf != null )
                {
                    if( appConf.AppSettings.Settings["IsStandAloneInstance"] != null )
                    {
                        appConf.AppSettings.Settings.Remove( "IsStandAloneInstance" );
                    }
                    if( appConf.AppSettings.Settings["ConfigurationDirectory"] != null )
                    {
                        appConf.AppSettings.Settings.Remove( "ConfigurationDirectory" );
                    }

                    appConf.AppSettings.Settings.Add( "IsStandAloneInstance", _isStandAloneInstance.ToString() );
                    if( _isStandAloneInstance )
                    {
                        Debug.Assert( systemConfDir == userConfDir && systemConfDir == contextDir );
                        appConf.AppSettings.Settings.Add( "ConfigurationDirectory", systemConfDir );
                    }
                }
                appConf.Save( ConfigurationSaveMode.Full );
            }
            else 
            {
                OnError( "Problem while reading the system configuration. Path : " + systemConfPath );
            }
        }

        public static void OnError( string message )
        {
            Console.Out.WriteLine( message );
            Console.Out.WriteLine( "The installation is not complete, please try again or contact us at contact@invenietis.com" );
            Console.Out.WriteLine( "Press any key to exit." );
            Console.ReadKey();
            return;
        }

        private static void AddEntryToSystemConf( string key, string value, string pluginId, string pluginName, XPathNavigator xPathNav )
        {
            bool updateServerUrlSet = false;

            string xPathExp = "//p[@guid='" + pluginId + "']";
            XPathNodeIterator iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
            if( iterator.Count == 1 )
            {
                iterator.MoveNext();

                if( !iterator.Current.MoveToFirstChild() )
                {
                    InsertUpdateElementAfter( iterator, value, key );
                }
                else
                {
                    do
                    {
                        if( iterator.Current.GetAttribute( "key", String.Empty ) == key )
                        {
                            iterator.Current.SetValue( value );
                            updateServerUrlSet = true;
                            break;
                        }
                    } while( iterator.Current.MoveToNext() );

                    if( !updateServerUrlSet )
                    {
                        InsertUpdateElementAfter( iterator, value, key );
                    }
                }
            }//At the end of this if statement, we are either on the right data element, or a the end of the pluginsdata
            else //if the updater entry does not exist, create it
            {
                xPathExp = "//System/Plugins";
                iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );

                if( iterator.Count == 0 ) //if there is no pluginsData child
                {
                    xPathExp = "//System";
                    iterator = xPathNav.Select( xPathNav.Compile( xPathExp ) );
                    iterator.MoveNext();

                    iterator.Current.AppendChildElement( null, "Plugins", null, null );
                    iterator.Current.MoveToFirstChild();

                    while( iterator.Current.Name != "Plugins" ) iterator.Current.MoveToNext();
                }
                else
                {
                    iterator.MoveNext();
                }

                bool pFound = false;
                //Adding the <p>
                if( iterator.Current.MoveToFirstChild() )
                {
                    //at least one "<p>" exists
                    do
                    {
                        if( iterator.Current.GetAttribute( "guid", String.Empty ) == pluginId )
                        {
                            //if the <p> corresponds to the updater data
                            pFound = true;
                            break;
                            //We are on the right <p>
                        }
                    } while( iterator.Current.MoveToNext() );

                    if( !pFound )
                    {
                        iterator.Current.InsertElementAfter( null, "p", null, null );
                        iterator.Current.MoveToNext();
                        iterator.Current.CreateAttribute( null, "guid", null, pluginId );
                        iterator.Current.CreateAttribute( null, "version", null, "1.0.0" );
                        iterator.Current.CreateAttribute( null, "name", null, pluginName );
                    }
                }
                else
                {
                    //There is nothing in PluginsData, we add the updater's <p>

                    iterator.Current.AppendChildElement( null, "p", null, null );
                    iterator.Current.MoveToFirstChild();
                    iterator.Current.CreateAttribute( null, "guid", null, pluginId );
                    iterator.Current.CreateAttribute( null, "version", null, "1.0.0" );
                    iterator.Current.CreateAttribute( null, "name", null, pluginName );
                    //We are on the right <p>
                }

                //Last step : adding the <data>
                if( iterator.Current.MoveToFirstChild() )
                {
                    InsertUpdateElementAfter( iterator, value, key );
                }
                else
                {
                    //There are no children yet
                    AppendChildUpdateElement( iterator, value, key );
                }
            }
        }

        static void InsertUpdateElementAfter( XPathNodeIterator iterator, string value, string key )
        {
            iterator.Current.InsertElementAfter( null, "data", null, value );
            iterator.Current.MoveToNext();
            iterator.Current.CreateAttribute( null, "key", null, key );
            iterator.Current.CreateAttribute( null, "type", null, "String" );
        }

        static void AppendChildUpdateElement( XPathNodeIterator iterator, string value, string key )
        {
            iterator.Current.AppendChildElement( null, "data", null, value );
            iterator.Current.MoveToFirstChild();
            iterator.Current.CreateAttribute( null, "key", null, key );
            iterator.Current.CreateAttribute( null, "type", null, "String" );
        }

        static string GetFilePath( Environment.SpecialFolder specialFolder, out string savePath )
        {
            string p = Environment.GetFolderPath( specialFolder )
                    + Path.DirectorySeparatorChar
                    + _appName
                    + Path.DirectorySeparatorChar
                    + _distributionName
                    + Path.DirectorySeparatorChar;

            if( !Directory.Exists( p ) ) Directory.CreateDirectory( p );
            savePath = p;
            return p;
        }
    }
}