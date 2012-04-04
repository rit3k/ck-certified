﻿using System;
using System.Linq;
using CK.Plugin;
using System.Windows.Input;
using CK.Plugins.AutoClick.Views;
using CommonServices;
using CK.Plugins.AutoClick.Model;
using CK.WPF.ViewModel;
using System.Windows;
using CK.Plugin.Config;
using CK.Core;
using System.ComponentModel;

namespace CK.Plugins.AutoClick
{
    [Plugin( PluginGuidString, PublicName = PluginPublicName, Version = PluginIdVersion )]
    public class AutoClick : VMBase, IPlugin
    {
        const string PluginGuidString = "{989BE0E6-D710-489e-918F-FBB8700E2BB2}";
        Guid PluginGuid = new Guid( PluginGuidString );
        const string PluginIdVersion = "1.0.0";
        const string PluginPublicName = "AutoClick Plugin";
        public readonly INamedVersionedUniqueId PluginId;

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IPointerDeviceDriver> MouseDriver { get; set; }

        [DynamicService( Requires = RunningRequirement.MustExistAndRun )]
        public IService<IMouseWatcher> MouseWatcher { get; set; }

        public IPluginConfigAccessor Config { get; set; }

        private AutoClickEditorWindow _editorWindow;
        private WPFStdClickTypeWindow _wpfStandardClickTypeWindow;
        private MouseProgressPieWindow _mouseWindow;

        public bool IsEditorOpened { get { return _editorWindow.IsVisible; } }

        private ICommand _toggleEditorVisibilityCommand;
        public ICommand ToggleEditorVisibilityCommand
        {
            get
            {
                if( _toggleEditorVisibilityCommand == null )
                {
                    _toggleEditorVisibilityCommand = new VMCommand( () =>
                    {
                        //Ugly fix, waiting for me to really understand how Show/Close/Visiblity work
                        double editorWindowWidth = _editorWindow.ActualWidth == 0 ? _editorWindow.Width : _editorWindow.ActualWidth;
                        
                        double editorLeft = _wpfStandardClickTypeWindow.Left - editorWindowWidth;
                        if( editorLeft > 0 && editorLeft + _editorWindow.ActualWidth < System.Windows.SystemParameters.PrimaryScreenWidth )
                            _editorWindow.Left = editorLeft;
                        else
                            _editorWindow.Left = _wpfStandardClickTypeWindow.Left + _wpfStandardClickTypeWindow.ActualWidth;

                        _editorWindow.Top = _wpfStandardClickTypeWindow.Top;

                        if( !_editorWindow.IsVisible ) _editorWindow.Visibility = Visibility.Visible;
                        else _editorWindow.Visibility = Visibility.Hidden;

                        OnPropertyChanged( "IsEditorOpened" );
                    } );
                }
                return _toggleEditorVisibilityCommand;
            }
        }

        private ICommand _togglePauseCommand;
        public ICommand TogglePauseCommand
        {
            get
            {
                if( _togglePauseCommand == null )
                {
                    _togglePauseCommand = new VMCommand( () => { if( IsPaused ) Resume( this ); else Pause( this ); } );
                }

                return _togglePauseCommand;
            }
        }

        StdClickTypeSelector _selector;
        public StdClickTypeSelector Selector { get { return _selector; } }

        bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; OnPropertyChanged( "IsPaused" ); OnPropertyChanged( "ShowMousePanel" ); }
        }

        //true if fhe user wants to see the MousePanel
        bool _showMousePanelOption;
        public bool ShowMousePanelOption
        {
            get { return (bool)Config.User["ShowMousePanelOption"]; }
            set { Config.User["ShowMousePanelOption"] = value; }
        }

        //true if the user wants to see the MousePanel and the autoclick is not paused
        public bool ShowMousePanel
        {
            get { return !_isPaused && _showMousePanelOption; }
        }

        /// <summary>
        /// Progression of the click cycle ( 0 to 100)
        /// </summary>
        public int ProgressValue
        {
            get { return MouseWatcher.Service.ProgressValue; }
        }

        public int TimeBeforeCountDownStarts
        {
            get { return (int)Config.User["TimeBeforeCountDownStarts"]; }
        }

        public int CountDownDuration
        {
            get { return (int)Config.User["CountDownDuration"]; }
        }

        #region IPlugin Members

        public bool Setup( IPluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
            _isPaused = true;
            bool isFirstLaunch = false;
            _selector = new StdClickTypeSelector( this );
            _wpfStandardClickTypeWindow = new WPFStdClickTypeWindow() { DataContext = this };

            if( Config.User["ShowMousePanelOption"] == null )
                Config.User["ShowMousePanelOption"] = true;

            if( Config.User["AutoClickWindowLeft"] == null || Config.User["AutoClickWindowLeft"] == null )
                isFirstLaunch = true;

            if( Config.User["AutoClickWindowWidth"] == null )
                Config.User["AutoClickWindowWidth"] = 100;
            if( Config.User["AutoClickWindowHeight"] == null )
                Config.User["AutoClickWindowHeight"] = 420;

            _showMousePanelOption = (bool)Config.User["ShowMousePanelOption"];

            _wpfStandardClickTypeWindow.Width = (int)Config.User["AutoClickWindowWidth"];
            _wpfStandardClickTypeWindow.Height = (int)Config.User["AutoClickWindowHeight"];

            _mouseWindow = new MouseProgressPieWindow { DataContext = this };
            _editorWindow = new AutoClickEditorWindow { DataContext = this };

            Config.ConfigChanged += new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            ConfigureMouseWatcher();
            RegisterEvents();

            _mouseWindow.Show();

            if( isFirstLaunch )
            {
                Config.User["AutoClickWindowLeft"] = (int)_wpfStandardClickTypeWindow.Left;
                Config.User["AutoClickWindowTop"] = (int)_wpfStandardClickTypeWindow.Top;

                _wpfStandardClickTypeWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - _wpfStandardClickTypeWindow.Width;
                _wpfStandardClickTypeWindow.Top = ( System.Windows.SystemParameters.PrimaryScreenHeight - _wpfStandardClickTypeWindow.Height ) / 4;

                _wpfStandardClickTypeWindow.Show();

            }
            else
            {
                _wpfStandardClickTypeWindow.Left = (int)Config.User["AutoClickWindowLeft"];
                _wpfStandardClickTypeWindow.Top = (int)Config.User["AutoClickWindowTop"];
                _wpfStandardClickTypeWindow.Show();
            }

            Pause( this );
        }

        public void Stop()
        {
            Config.ConfigChanged -= new EventHandler<ConfigChangedEventArgs>( OnConfigChanged );
            UnregisterEvents();
            Config.User["AutoClickWindowWidth"] = (int)_wpfStandardClickTypeWindow.Width;
            Config.User["AutoClickWindowHeight"] = (int)_wpfStandardClickTypeWindow.Height;
            Config.User["AutoClickWindowLeft"] = (int)_wpfStandardClickTypeWindow.Left;
            Config.User["AutoClickWindowTop"] = (int)_wpfStandardClickTypeWindow.Top;
        }

        public void Teardown()
        {
            if( _mouseWindow != null )
            {
                if( _mouseWindow.IsVisible ) _mouseWindow.Close();
                if( _editorWindow.IsVisible ) _editorWindow.Close();
            }

            if( _wpfStandardClickTypeWindow != null )
            {
                if( _wpfStandardClickTypeWindow.IsVisible ) _wpfStandardClickTypeWindow.Close();
            }

            _selector = null;
            _wpfStandardClickTypeWindow = null;
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Launches OnPropertyChanged when needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnConfigChanged( object sender, ConfigChangedEventArgs e )
        {
            if( e.MultiPluginId.Any( ( c ) => String.Compare( "989BE0E6-D710-489e-918F-FBB8700E2BB2", c.UniqueId.ToString(), true ) == 0 ) && !String.IsNullOrEmpty( e.Key ) )
            {
                switch( e.Key )
                {
                    case "TimeBeforeCountDownStarts":
                        MouseWatcher.Service.TimeBeforeCountDownStarts = (int)e.Value;
                        break;
                    case "CountDownDuration":
                        MouseWatcher.Service.CountDownDuration = (int)e.Value;
                        break;
                    case "ShowMousePanelOption":
                        break;
                    default:
                        return;
                }

                OnPropertyChanged( e.Key );
            }
        }

        void OnMouseDriverServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == RunningStatus.Stopped )
            {
                MouseDriver.Service.PointerMove -= new PointerDeviceEventHandler( OnPointerMove );
            }
        }

        void OnPointerMove( object sender, PointerDeviceEventArgs e )
        {
            _mouseWindow.Left = e.X + 10;
            _mouseWindow.Top = e.Y - 20;
        }

        private void OnEditorWindowVisibilityChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            OnPropertyChanged( "IsEditorOpened" );
        }

        private void OnClickCancelled( object sender, EventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
            //TODO Nice animation to show that the click has failed ?
        }

        private void OnHasPaused( object sender, EventArgs e )
        {
            _isPaused = true;
            OnPropertyChanged( "ProgressValue" );
        }

        private void OnHasResumed( object sender, EventArgs e )
        {
            _isPaused = false;
        }

        private void OnProgressValueChanged( object sender, AutoClickProgressValueChangedEventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
        }

        private void OnClickAsked( object sender, EventArgs e )
        {
            OnPropertyChanged( "ProgressValue" );
            //Asking for a click, the IClickTypeSelector will respond via the ClickTypeChosenEvent
            _selector.AskClickType();
        }

        private void OnMouseWatcherPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            OnPropertyChanged( e.PropertyName );
        }

        #endregion

        #region Methods

        private void RegisterEvents()
        {
            MouseDriver.Service.PointerMove += new PointerDeviceEventHandler( OnPointerMove );

            MouseWatcher.Service.LaunchClick += new EventHandler( OnClickAsked );
            MouseWatcher.Service.ProgressValueChanged += new AutoClickProgressValueChangedEventHandler( OnProgressValueChanged );
            MouseWatcher.Service.ClickCanceled += new EventHandler( OnClickCancelled );
            MouseWatcher.Service.HasPaused += new EventHandler( OnHasPaused );
            MouseWatcher.Service.HasResumed += new EventHandler( OnHasResumed );
            MouseWatcher.Service.PropertyChanged += new PropertyChangedEventHandler( OnMouseWatcherPropertyChanged );
            _selector.AutoClickClickTypeChosen += new ClickTypeChosenEventHandler( SendClick );
            _selector.AutoClickResumeEvent += new AutoClickResumeEventHandler( Resume );
            _selector.AutoClickStopEvent += new AutoClickStopEventHandler( Pause );

            MouseDriver.ServiceStatusChanged += new EventHandler<ServiceStatusChangedEventArgs>( OnMouseDriverServiceStatusChanged );
        }
        private void UnregisterEvents()
        {
            if( MouseDriver.Status != RunningStatus.Stopped && MouseDriver.Status != RunningStatus.Disabled )
                MouseDriver.Service.PointerMove -= new PointerDeviceEventHandler( OnPointerMove );

            if( MouseWatcher.Status != RunningStatus.Stopped && MouseWatcher.Status != RunningStatus.Disabled )
            {
                MouseWatcher.Service.LaunchClick -= new EventHandler( OnClickAsked );
                MouseWatcher.Service.ProgressValueChanged -= new AutoClickProgressValueChangedEventHandler( OnProgressValueChanged );
                MouseWatcher.Service.ClickCanceled -= new EventHandler( OnClickCancelled );
                MouseWatcher.Service.HasPaused -= new EventHandler( OnHasPaused );
                MouseWatcher.Service.HasResumed -= new EventHandler( OnHasResumed );
                MouseWatcher.Service.PropertyChanged -= new PropertyChangedEventHandler( OnMouseWatcherPropertyChanged );
            }

            _selector.AutoClickClickTypeChosen -= new ClickTypeChosenEventHandler( SendClick );
            _selector.AutoClickResumeEvent -= new AutoClickResumeEventHandler( Resume );
            _selector.AutoClickStopEvent -= new AutoClickStopEventHandler( Pause );
            _editorWindow.IsVisibleChanged -= new System.Windows.DependencyPropertyChangedEventHandler( OnEditorWindowVisibilityChanged );
        }

        private void ConfigureMouseWatcher()
        {
            if( Config.User["TimeBeforeCountDownStarts"] == null )
                Config.User["TimeBeforeCountDownStarts"] = 1500;

            if( Config.User["CountDownDuration"] == null )
                Config.User["CountDownDuration"] = 2000;

            MouseWatcher.Service.TimeBeforeCountDownStarts = (int)Config.User["TimeBeforeCountDownStarts"];
            MouseWatcher.Service.CountDownDuration = (int)Config.User["CountDownDuration"];
        }

        internal void ModifyCountDownConfiguration( string property, int value )
        {
            if( property == "CountDownDuration" )
            {
                int newValue = CountDownDuration;
                if( CountDownDuration + value > 0 )
                    newValue = CountDownDuration + value;
                Config.User[property] = newValue;
            }
            else if( property == "TimeBeforeCountDownStarts" )
            {
                int newValue = TimeBeforeCountDownStarts;
                if( TimeBeforeCountDownStarts + value > 0 )
                    newValue = TimeBeforeCountDownStarts + value;
                Config.User[property] = newValue;
            }
            OnPropertyChanged( property );
        }

        /// <summary>
        /// Callback method that is called when the IClickTypeSelector has sent a ClickType (will send the actual click)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendClick( object sender, ClickTypeEventArgs e )
        {
            foreach( ClickInstruction instr in e.ClickVM )
            {
                switch( instr )
                {
                    case ClickInstruction.None:
                        break;
                    case ClickInstruction.RightButtonDown:
                        MouseDriver.Service.SimulateButtonDown( ButtonInfo.XButton, "Right" );
                        break;
                    case ClickInstruction.RightButtonUp:
                        MouseDriver.Service.SimulateButtonUp( ButtonInfo.XButton, "Right" );
                        break;
                    case ClickInstruction.LeftButtonDown:
                        MouseDriver.Service.SimulateButtonDown( ButtonInfo.DefaultButton, "" );
                        break;
                    case ClickInstruction.LeftButtonUp:
                        MouseDriver.Service.SimulateButtonUp( ButtonInfo.DefaultButton, "" );
                        break;
                    case ClickInstruction.WheelDown:
                        MouseDriver.Service.SimulateButtonDown( ButtonInfo.XButton, "Middle" );
                        break;
                    case ClickInstruction.WheelUp:
                        MouseDriver.Service.SimulateButtonUp( ButtonInfo.XButton, "Middle" );
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Method that is called when the AutoClickPlugin can start over its work
        /// </summary>
        /// <param name="sender"></param>
        public void Resume( object sender )
        {
            MouseWatcher.Service.Resume();
            IsPaused = false;
        }

        /// <summary>
        /// Method that is called when the AutoClickPlugin must stop
        /// </summary>
        /// <param name="sender"></param>
        public void Pause( object sender )
        {
            MouseWatcher.Service.Pause();
            IsPaused = true;
        }

        #endregion
    }
}