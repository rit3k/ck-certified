#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\SimpleSkin\ViewModels\VMContextSimple.cs) is part of CiviKey. 
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

using CK.WPF.ViewModel;
using CK.Keyboard.Model;
using CK.Plugin.Config;
using CK.Core;
using CK.Context;
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SimpleSkin.ViewModels
{
    internal class VMContextSimple : VMBase
    {
        Dictionary<object, VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>> _dic;
        EventHandler<CurrentKeyboardChangedEventArgs> _evCurrentKeyboardChanged;
        PropertyChangedEventHandler _evUserConfigurationChanged;
        EventHandler<KeyboardEventArgs> _evKeyboardDestroyed;
        EventHandler<KeyboardEventArgs> _evKeyboardCreated;
        ObservableCollection<VMKeyboardSimple> _keyboards;
        VMKeyboardSimple _currentKeyboard;
        IKeyboardContext _kbctx;
        IContext _ctx;

        public IKeyboardContext KeyboardContext { get { return _kbctx; } }

        public VMKeyboardSimple KeyboardVM
        {
            get { return _currentKeyboard; }
            set { _currentKeyboard = value; OnPropertyChanged( "KeyboardVM" ); }
        }

        public IContext Context { get { return _ctx; } }

        public ObservableCollection<VMKeyboardSimple> Keyboards
        {
            get { return _keyboards; }
        }

        public VMKeyboardSimple Obtain( IKeyboard keyboard )
        {
            VMKeyboardSimple k = FindViewModel<VMKeyboardSimple>( keyboard );
            if( k == null ) throw new Exception( "Context mismatch." );
            return k;
        }

        public VMZoneSimple Obtain( IZone zone )
        {
            VMZoneSimple z = FindViewModel<VMZoneSimple>( zone );
            if( z == null )
            {
                //if( zone.Context != _kbctx )
                //    throw new Exception( "Context mismatch." );
                z = CreateZone( zone );
                _dic.Add( zone, z );
            }
            return z;
        }

        public VMKeySimple Obtain( IKey key )
        {
            VMKeySimple k = FindViewModel<VMKeySimple>( key );
            if( k == null )
            {
                //if( key.Context != _kbctx )
                //    throw new Exception( "Context mismatch." );
                k = CreateKey( key );
                _dic.Add( key, k );
            }
            return k;
        }

        public VMKeyboardSimple Keyboard { get { return _currentKeyboard; } }

        T FindViewModel<T>( object m )
            where T : VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>
        {
            VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple> vm;
            _dic.TryGetValue( m, out vm );
            return (T)vm;
        }

        public void Dispose()
        {
            _kbctx.Keyboards.KeyboardCreated -= _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged -= _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed -= _evKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged -= _evUserConfigurationChanged;
            foreach( VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple> vm in _dic.Values ) vm.Dispose();
            _dic.Clear();
        }

        internal void OnModelDestroy( object m )
        {
            VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple> vm;
            if( _dic.TryGetValue( m, out vm ) )
            {
                vm.Dispose();
                _dic.Remove( m );
            }
        }

        #region OnXXXXXXXXX
        void OnKeyboardCreated( object sender, KeyboardEventArgs e )
        {
            VMKeyboardSimple k = CreateKeyboard( e.Keyboard );
            _dic.Add( e.Keyboard, k );
            _keyboards.Add( k );
        }

        void OnCurrentKeyboardChanged( object sender, CurrentKeyboardChangedEventArgs e )
        {
            if( e.Current != null )
            {
                _currentKeyboard = Obtain( e.Current );
                OnPropertyChanged( "KeyboardVM" );
                _currentKeyboard.TriggerPropertyChanged();

            }
        }

        void OnUserConfigurationChanged( object sender, PropertyChangedEventArgs e )
        {
            //If the CurrentContext has changed, but not because a new context has been loaded (happens when the userConf if changed but the context is kept the same).
            if( e.PropertyName == "CurrentContextProfile" )
            {
                OnPropertyChanged( "KeyboardVM" );
            }
        }

        void OnKeyboardDestroyed( object sender, KeyboardEventArgs e )
        {
            _keyboards.Remove( Obtain( e.Keyboard ) );
            OnModelDestroy( e.Keyboard );
        }
        #endregion

        IPluginConfigAccessor _config;
        public IPluginConfigAccessor Config
        {
            get
            {
                if( _config == null ) _config = Context.GetService<IPluginConfigAccessor>( true );
                return _config;
            }
        }

        public VMContextSimple( IContext ctx, IKeyboardContext kbctx )
        {
            _dic = new Dictionary<object, VMContextElement<VMContextSimple, VMKeyboardSimple, VMZoneSimple, VMKeySimple>>();
            _keyboards = new ObservableCollection<VMKeyboardSimple>();

            _kbctx = kbctx;
            _ctx = ctx;
            if( _kbctx.Keyboards.Count > 0 )
            {
                foreach( IKeyboard keyboard in _kbctx.Keyboards )
                {
                    VMKeyboardSimple kb = CreateKeyboard( keyboard );
                    _dic.Add( keyboard, kb );
                    _keyboards.Add( kb );
                }
                _currentKeyboard = Obtain( _kbctx.CurrentKeyboard );
            }

            _evKeyboardCreated = new EventHandler<KeyboardEventArgs>( OnKeyboardCreated );
            _evCurrentKeyboardChanged = new EventHandler<CurrentKeyboardChangedEventArgs>( OnCurrentKeyboardChanged );
            _evKeyboardDestroyed = new EventHandler<KeyboardEventArgs>( OnKeyboardDestroyed );
            _evUserConfigurationChanged = new PropertyChangedEventHandler( OnUserConfigurationChanged );

            _kbctx.Keyboards.KeyboardCreated += _evKeyboardCreated;
            _kbctx.CurrentKeyboardChanged += _evCurrentKeyboardChanged;
            _kbctx.Keyboards.KeyboardDestroyed += _evKeyboardDestroyed;
            _ctx.ConfigManager.UserConfiguration.PropertyChanged += _evUserConfigurationChanged;
        }

        protected VMKeySimple CreateKey( IKey k )
        {
            return new VMKeySimple( this, k );
        }

        protected VMZoneSimple CreateZone( IZone z )
        {
            return new VMZoneSimple( this, z );
        }

        protected VMKeyboardSimple CreateKeyboard( IKeyboard kb )
        {
            return new VMKeyboardSimple( this, kb );
        }
    }
}
