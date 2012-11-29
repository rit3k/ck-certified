#region LGPL License
/*----------------------------------------------------------------------------
* This file (Plugins\Accessibility\EditableSkin\ViewModels\VMZoneEditable.cs) is part of CiviKey. 
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
using CK.Core;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Windows.Input;
using CK.Windows.App;

namespace ContextEditor.ViewModels
{
    public class VMZoneEditable : VMZone<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable>
    {
        public VMZoneEditable( VMContextEditable ctx, IZone zone )
            : base( ctx, zone )
        {
            _ctx = ctx;
        }

        public void Initialize()
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.Initialize();
            }
        }

        public override VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable> Parent
        {
            get { return Context.Obtain( Model.Keyboard ); }
        }

        public override IKeyboardElement LayoutElement
        {
            get { return Model.CurrentLayout; }
        }

        bool _isSelected;
        /// <summary>
        /// Gets whether this element is being edited.
        /// </summary>
        public override bool IsBeingEdited
        {
            get { return IsSelected || Parent.IsBeingEdited; }
        }

        public override bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Context.SelectedElement = this;
                OnPropertyChanged( "IsBeingEdited" );
                OnPropertyChanged( "IsSelected" );
                foreach( var item in Keys )
                {
                    item.TriggerOnPropertyChanged( "IsBeingEdited" );
                }
            }
        }

        internal void TriggerOnPropertyChanged( string propertyName )
        {
            OnPropertyChanged( propertyName );
        }

        internal void TriggerOnPropertyChanged( string propertyName, bool propagate )
        {
            OnPropertyChanged( propertyName );
            if( !propagate ) return;

            foreach( var item in Keys )
            {
                item.TriggerOnPropertyChanged( propertyName );
            }
        }

        VMContextEditable _ctx;

        ICommand _createKeyCommand;
        public ICommand CreateKeyCommand
        {
            get
            {
                if( _createKeyCommand == null )
                {
                    _createKeyCommand = new CK.WPF.ViewModel.VMCommand( () =>
                    {
                        IKey key = Model.Keys.Create();
                        key.KeyModes.First().UpLabel = "New key";
                        key.CurrentLayout.Current.X = X;
                        key.CurrentLayout.Current.Y = Y;
                        key.CurrentLayout.Current.Width = 40;
                        key.CurrentLayout.Current.Height = 40;
                        key.CurrentLayout.Current.Visible = true;
                        OnPropertyChanged( "Keys" );
                    } );
                }
                return _createKeyCommand;
            }
        }


        ICommand _deleteZoneCommand;
        public ICommand DeleteZoneCommand
        {
            get
            {
                if( _deleteZoneCommand == null )
                {
                    _deleteZoneCommand = new CK.WPF.ViewModel.VMCommand( () =>
                    {
                        ModalViewModel mvm = new ModalViewModel( "Supprimer une zone", "Vous êtes sur le point de supprimer une zone. Voulez-vous que les touches contenues dans cette zone soient stockées dans la zone par défaut ? Dans le cas contraire, elles seront détruites." );
                        mvm.Buttons.Add( new ModalButton( mvm, "Sauver les touches", ModalResult.Yes ) );
                        mvm.Buttons.Add( new ModalButton( mvm, "Détruire les touches", ModalResult.No ) );
                        mvm.Buttons.Add( new ModalButton( mvm, "Annuler", ModalResult.Cancel ) );
                        CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                        msgBox.ShowDialog();

                        if( mvm.ModalResult == ModalResult.Cancel ) return;

                        if( mvm.ModalResult == ModalResult.Yes ) //Saving the key
                        {
                            foreach( var key in Model.Keys )
                            {
                                IKey newKey = key.Keyboard.Zones[""].Keys.Create();
                                foreach( var keyMode in key.KeyModes )
                                {
                                    InsertKeyMode( keyMode, newKey ); //TODO : check the todos of this method
                                }

                                //TODOJL : copy all the LayoutKeyModes of the key (+ plugindatas)
                                //TODOJL : copy the plugindatas of the key

                            }
                        }

                        for( int i = Model.Keys.Count - 1; i >= 0; i-- )
                        {
                            if( Model.Keys[i] != null )
                                Model.Keys[i].Destroy();
                        }

                        Context.SelectedElement = Parent;
                        Model.Destroy(); //Deleting the key

                    } );
                }
                return _deleteZoneCommand;
            }
        }

        private IKey InsertKeyMode( IKeyMode keyMode, IKey newKey )
        {
            IKeyMode newKeyMode = newKey.KeyModes.Create( keyMode.Mode );
            newKeyMode.UpLabel = keyMode.UpLabel;
            newKeyMode.DownLabel = keyMode.DownLabel;
            newKeyMode.Description = keyMode.Description;
            newKeyMode.Enabled = keyMode.Enabled;

            foreach (string keyProgram in keyMode.OnKeyDownCommands.Commands)
	        {
                newKeyMode.OnKeyDownCommands.Commands.Add( keyProgram );    
	        }

            foreach( string keyProgram in keyMode.OnKeyPressedCommands.Commands )
            {
                newKeyMode.OnKeyPressedCommands.Commands.Add( keyProgram );
            }

            foreach( string keyProgram in keyMode.OnKeyUpCommands.Commands )
            {
                newKeyMode.OnKeyUpCommands.Commands.Add( keyProgram );
            }

            //TODOJL : copy all the plugindatas of the keymode onto the new keymode

            return newKey;
        }

        ICommand _selectZoneCommand;
        public ICommand SelectZoneCommand
        {
            get
            {
                if( _selectZoneCommand == null )
                {
                    _selectZoneCommand = new CK.WPF.ViewModel.VMCommand( () =>
                    {
                        _ctx.SelectedElement = this;
                    } );
                }
                return _selectZoneCommand;
            }
        }

        /// <summary>
        /// Gets or sets the Name of the underlying <see cref="IZone"/>
        /// </summary>
        public new string Name
        {
            get { return Model.Name; }
            set { Model.Rename( value ); }
        }

        /// <summary>
        /// Gets the X position of the key contained in this zone that is the nearer to the Top Left corner;
        /// </summary>
        public int X
        {
            get { return Keys.Min( k => k.X ); }
        }

        /// <summary>
        /// Gets the Y position of the key contained in this zone that is the nearer to the Top Left corner;
        /// </summary>
        public int Y
        {
            get { return Keys.Min( k => k.Y ); }
        }

        /// <summary>
        /// Gets the Width between the two keys contained in this zone that are the farther from each other (width-wise)
        /// </summary>
        public int Width
        {
            get { return Keys.Max( k => k.X + k.Width ) - X; }
        }

        /// <summary>
        /// Gets the Height between the two keys contained in this zone that are the farther from each other (height-wise)
        /// </summary>
        public int Height
        {
            get { return Keys.Max( k => k.Y + k.Height ) - Y; }
        }
    }
}
