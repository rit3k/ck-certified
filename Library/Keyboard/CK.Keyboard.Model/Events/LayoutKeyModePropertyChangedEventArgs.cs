#region LGPL License
/*----------------------------------------------------------------------------
* This file (Library\Keyboard\CK.Keyboard.Model\Events\LayoutKeyModePropertyChangedEventArgs.cs) is part of CiviKey. 
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

namespace CK.Keyboard.Model
{
    /// <summary>
    /// Defines a key property changed event argument when the <see cref="PropertyHolder"/>
    /// is one of the <see cref="ILayoutKeyMode"/> of the <see cref="KeyEventArgs.Key"/>.
    /// </summary>
    public class LayoutKeyModePropertyChangedEventArgs : LayoutKeyPropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the <see cref="ILayoutKeyMode"/> that holds the property.
        /// </summary>
        public new ILayoutKeyMode PropertyHolder 
        {
            get { return (ILayoutKeyMode)base.PropertyHolder; } 
        }

        public LayoutKeyModePropertyChangedEventArgs( ILayoutKeyMode k, string propertyName )
            : base( k.LayoutKey, propertyName )
        {
        }
    }
}
