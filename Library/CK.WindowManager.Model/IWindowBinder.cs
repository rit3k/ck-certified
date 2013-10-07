﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Plugin;

namespace CK.WindowManager.Model
{
    /// <summary>
    /// The window binder allows two <see cref="IWindwoElement"/> to bind together.
    /// </summary>
    public interface IWindowBinder : IDynamicService
    {
        /// <summary>
        /// Gets a readonly collection of attached <see cref="IWindowElement"/> from a referential <see cref="IWindowElement"/>.
        /// </summary>
        /// <param name="referential">The <see cref="IWindowElement"/> from where to retrieve all attached <see cref="IWindowElement"/>.</param>
        /// <returns></returns>
        ICKReadOnlyCollection<IWindowElement> GetAttachedElements( IWindowElement referential );

        /// <summary>
        /// Attach the both <see cref="IWindowElement"/> given
        /// </summary>
        /// <param name="me"></param>
        /// <param name="other"></param>
        void Attach( IWindowElement me, IWindowElement other );

        /// <summary>
        /// Removes the given <see cref="IBinding"/>
        /// </summary>
        /// <param name="me"></param>
        /// <param name="other"></param>
        void Detach( IWindowElement me, IWindowElement other );

        /// <summary>
        /// Raised before an attachment occurs. This event can be canceled.
        /// </summary>
        event EventHandler<WindowBindingEventArgs> BeforeBinding;

        /// <summary>
        /// Rasied after an attachment occurs.
        /// </summary>
        event EventHandler<WindowBindedEventArgs> AfterBinding;

    }

}
