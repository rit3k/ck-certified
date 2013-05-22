#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\WPF\CK.WPF.ViewModel\VMZone.cs) is part of CiviKey. 
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

using System.Collections.ObjectModel;
using CK.Keyboard.Model;
using CK.Core;

namespace KeyboardEditor.ViewModels
{
    public abstract class VMZone<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable> : VMContextElement<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMContextEditable : VMContext<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMKeyboardEditable : VMKeyboard<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMZoneEditable : VMZone<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMKeyEditable : VMKey<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMKeyModeEditable : VMKeyMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
        where VMLayoutKeyModeEditable : VMLayoutKeyMode<VMContextEditable, VMKeyboardEditable, VMZoneEditable, VMKeyEditable, VMKeyModeEditable, VMLayoutKeyModeEditable>
    {
        IZone _zone;
        ObservableSortedArrayKeyList<VMKeyEditable,int> _keys;

        public IZone Model { get { return _zone; } }

        public ObservableSortedArrayKeyList<VMKeyEditable, int> Keys { get { return _keys; } }

        public string Name { get { return _zone.Name; } }

        public VMZone( VMContextEditable context, IZone zone )
            : base( context )
        {
            _zone = zone;
            _keys = new ObservableSortedArrayKeyList<VMKeyEditable, int>( k => k.Index );

            foreach( IKey key in _zone.Keys )
            {
                VMKeyEditable k = Context.Obtain( key );
                Keys.Add( k );
            }
        }

        protected override void OnDispose()
        {
            foreach( VMKeyEditable key in Keys )
            {
                key.Dispose();
            }

            base.OnDispose();
        }
    }
}