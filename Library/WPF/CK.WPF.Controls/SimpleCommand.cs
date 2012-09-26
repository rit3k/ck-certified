#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.Controls\SimpleCommand.cs) is part of CiviKey. 
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

using System;
using System.Windows.Input;
using System.Diagnostics;

namespace CK.WPF.Controls
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
    /// From http://mvvmfoundation.codeplex.com/ open source project.
    /// </summary>
    /// <remarks>
    /// CanExecuteChanged and memory leaks.
    /// <para>
    /// This class uses the centralized <see cref="CommandManager.RequerySuggested"/> to implement <see cref="CanExecuteChanged"/> event
    /// when a <see cref="CanExecute"/> delegate is provided.
    /// </para>
    /// <para>
    /// Apparently, WPF expects this event to be implemented as a weak event. That means, the event should use a List&lt;WeakReference&gt; for 
    /// the delegates, so that objects don’t have to unregister (because they don’t!).
    /// </para>
    /// <para>
    /// Interestingly, this means a WeakReference to the delegate, not to the listening object – the listening object 
    /// must keep a reference to its own delegate instance so that the delegate doesn’t get garbage collected even 
    /// though the listening object is still reachable.
    /// CommandManager.RequerySuggested is such a weak event, so code below works fine.
    /// </para>
    /// The default interface implementation generated by VS – a default C# event (“public EventHandler CanExecuteChanged;”) – will cause a memory leak!
    /// So basically, there are only two easy options:
    /// <list type="">
    /// <item>
    /// If CanExecute never changes and you don’t intend to fire the event, then don’t use a default event, use an event that doesn’t store the delegates.
    /// <c>public event EventHandler CanExecuteChanged { add { } remove { } }</c>
    /// </item>
    /// <item>2) If CanExecute changes, use the CommandManager.</item>
    /// </list>
    /// </remarks>
    public class SimpleCommand : ICommand
    {
        readonly Action _execute;
        readonly Func<bool> _canExecute;
        
        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public SimpleCommand( Action execute )
            : this( execute, null )
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public SimpleCommand( Action execute, Func<bool> canExecute )
        {
            if( execute == null )
                throw new ArgumentNullException( "execute" );

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecute == null ? true : _canExecute();
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute( object parameter )
        {
            _execute();
        }

    }

    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
    /// From http://mvvmfoundation.codeplex.com/ open source project.
    /// </summary>
    public class SimpleCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Func<T,bool> _canExecute;

        public SimpleCommand( Action<T> execute )
            : this( execute, null )
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public SimpleCommand( Action<T> execute, Func<T,bool> canExecute )
        {
            if( execute == null )
                throw new ArgumentNullException( "execute" );

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecute == null ? true : _canExecute( (T)parameter );
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if( _canExecute != null )
                    CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute( object parameter )
        {
            _execute( (T)parameter );
        }

    }
    
}
