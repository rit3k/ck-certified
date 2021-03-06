﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using CK.Keyboard.Model;
using CK.WPF.Controls;

namespace Host.ViewModels
{
    public class ContextModel : VMBase, IDisposable
    {
        IKeyboardContext _ctx;
        AppViewModel _app;

        ObservableCollection<KeyboardModel> _keyboards;
        public ObservableCollection<KeyboardModel> Keyboards
        {
            get { return _keyboards; }
        }

        public ContextModel( AppViewModel app )
        {
            _app = app;
            _ctx = app.KeyboardContext;

            _keyboards = new ObservableCollection<KeyboardModel>();
            foreach( IKeyboard keyboard in _ctx.Keyboards )
            {
                _keyboards.Add( new KeyboardModel( keyboard ) );
            }

            RegisterEvents();
        }

        public KeyboardModel Current
        {
            get
            {
                return _ctx.CurrentKeyboard != null ? _keyboards.Where( k => k.Name == _ctx.CurrentKeyboard.Name ).Single() : null;
            }
            set
            {
                if( value != null )
                    _ctx.CurrentKeyboard = value.Model;
                else _ctx.CurrentKeyboard = null;

                OnPropertyChanged( "Current" );
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            var destroyedKeyboard = _keyboards.Where( k => k.Name == e.Keyboard.Name ).Single();
            destroyedKeyboard.Dispose();
            
            _keyboards.Remove( destroyedKeyboard );
        }

        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            _keyboards.Add( new KeyboardModel( e.Keyboard ) );
        }

        private void RegisterEvents()
        {
            _ctx.Keyboards.KeyboardCreated += OnKeyboardCreated;
            _ctx.Keyboards.KeyboardDestroyed += OnKeyboardDestroyed;
            _ctx.Keyboards.CurrentChanged += OnCurrentChanged;
        }

        void OnCurrentChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            OnPropertyChanged( "Current" );
        }

        private void UnregisterEvents()
        {
            _ctx.Keyboards.KeyboardCreated -= OnKeyboardCreated;
            _ctx.Keyboards.KeyboardDestroyed -= OnKeyboardDestroyed;
            _ctx.Keyboards.CurrentChanged -= OnCurrentChanged;
        }

        public void Dispose()
        {
            UnregisterEvents();

            foreach( KeyboardModel keyboard in Keyboards )
            {
                keyboard.Dispose();
            }
        }
    }

    public class KeyboardModel : VMBase, IDisposable
    {
        IKeyboard _keyboard;
        public KeyboardModel( IKeyboard keyboard )
        {
            _keyboard = keyboard;
            RegisterEvents();
        }

        public IKeyboard Model { get { return _keyboard; } }
        public string Name { get { return _keyboard.Name; } }

        private void RegisterEvents()
        {
            _keyboard.Context.Keyboards.KeyboardRenamed += Keyboards_KeyboardRenamed;
        }

        void Keyboards_KeyboardRenamed( object sender, KeyboardRenamedEventArgs e )
        {
            OnPropertyChanged( "Name" );
        }

        private void UnregisterEvents()
        {
            _keyboard.Context.Keyboards.KeyboardRenamed -= Keyboards_KeyboardRenamed;
        }

        public void Dispose()
        {
            UnregisterEvents();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
