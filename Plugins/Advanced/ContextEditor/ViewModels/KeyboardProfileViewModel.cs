﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Keyboard.Model;
using CK.Storage;
using CK.Windows.App;

namespace ContextEditor.ViewModels
{
    /// <summary>
    /// The <see cref="WizardPage"/> enables editing a keyboards basic properties : its name, width and height.
    /// </summary>
    public class KeyboardProfileViewModel : WizardPage
    {
        SimpleKeyboardViewModel _viemModel;
        string _backupFileName;
        ContextEditor _root;
        IKeyboard _model;

        //Gets whether this step has already been passed. If so, and if the user wants to go back, 
        //we need to destroy/cancel what has been done by the user until then.
        //For keyboards are renamed/created after this step is finished.
        bool _stepAchieved;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        /// <param name="model">The keyboard to create or modify</param>
        public KeyboardProfileViewModel( ContextEditor root, WizardManager wizardManager, IKeyboard model )
            : base( wizardManager, false )
        {
            _root = root;
            _model = model;
            _viemModel = new SimpleKeyboardViewModel( model );

            _backupFileName = GenerateBackupFileName();

            IStructuredSerializable serializableModel = _model as IStructuredSerializable;
            if( serializableModel != null )
            {
                using( FileStream str = new FileStream( _backupFileName, FileMode.CreateNew ) )
                {
                    using( IStructuredWriter writer = SimpleStructuredWriter.CreateWriter( str, _root.Context.ServiceContainer ) )
                    {
                        writer.Xml.WriteStartElement( "Keyboard" );
                        serializableModel.WriteContent( writer );
                        writer.Xml.WriteEndElement();
                    }
                }
                _root.KeyboardBackup = new KeyboardBackup( model, _backupFileName );
            }

            Title = "Edition des propriétés de base d'un clavier";
            Description = "Cette page vous permet d'éditer les propriétés de base d'un clavier : son nom, sa hauteur et sa largeur.";
        }

        /// <summary>
        /// Ctor, creates a new <see cref="SimpleKeyboardViewModel"/>.
        /// </summary>
        /// <param name="wizardManager">The wizard manager</param>
        public KeyboardProfileViewModel( ContextEditor root, WizardManager wizardManager )
            : base( wizardManager, false )
        {
            _viemModel = new SimpleKeyboardViewModel();
            _backupFileName = "";
            _root = root;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the name of the keyboard
        /// </summary>
        public string Name
        {
            get { return _viemModel.Name; }
            set
            {
                if( value != _viemModel.Name )
                {
                    _viemModel.Name = value;
                    NotifyOfPropertyChange( () => Name );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        //TODO
        bool _keepRatio;
        public bool KeepRatio
        {
            get { return _keepRatio; }
            set { _keepRatio = value; NotifyOfPropertyChange( () => KeepRatio ); }
        }

        /// <summary>
        /// Gets or sets the height of the keyboard
        /// </summary>
        public int Height
        {
            get { return _viemModel.Height; }
            set
            {
                if( value != Height )
                {
                    _viemModel.Height = value;
                    NotifyOfPropertyChange( () => Height );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the keyboard
        /// </summary>
        public int Width
        {
            get { return _viemModel.Width; }
            set
            {
                if( value != this.Width )
                {
                    _viemModel.Width = value;
                    NotifyOfPropertyChange( () => Width );
                    NotifyOfPropertyChange( () => CanGoFurther );
                }
            }
        }

        #endregion

        /// <summary>
        /// Checks that all informations has been filled and that there are no keyboards that have the selected name.
        /// </summary>
        /// <returns></returns>
        public override bool CheckCanGoFurther()
        {
            return ( Width > 0
                && Height > 0
                && !String.IsNullOrWhiteSpace( Name ) );
        }

        public override bool OnBeforeGoBack()
        {
            if( _stepAchieved )
            {
                ModalViewModel mvm = new ModalViewModel( "Annuler les modifications", String.Format( "Revenir en arrière va effacer les modifications non sauvegardées.{0}Etes-vos sûr de vouloir continuer ?", System.Environment.NewLine ) );
                mvm.Buttons.Add( new ModalButton( mvm, "Oui, perdre les modifications", ModalResult.Yes ) );
                mvm.Buttons.Add( new ModalButton( mvm, "Non, rester sur cette fenêtre", ModalResult.No ) );
                CustomMsgBox msgBox = new CustomMsgBox( ref mvm );
                msgBox.ShowDialog();

                if( mvm.ModalResult != ModalResult.Yes )
                {
                    return false;
                }

                _root.CancelModifications();
                _stepAchieved = false;
            }
            return true;
        }

        public override bool OnBeforeNext()
        {
            //if the model is null, it means that we are creating a new keyboard.
            //So we do create it with the user's information.
            if( _model == null )
            {
                _model = _root.KeyboardContext.Service.Keyboards.Create( Name );
                _root.KeyboardBackup = new KeyboardBackup( _model, _backupFileName );
            }
            else if( _model.Name != Name ) _model.Rename( Name );

            Debug.Assert( _model != null, "The keyboard should be created even if the name is already used." );

            _model.CurrentLayout.H = Height;
            _model.CurrentLayout.W = Width;

            Next = new KeyboardEditionViewModel( _root, WizardManager, _model );
            _stepAchieved = true;

            return _model != null;
        }

        //outputs a filepath of the form : [tempfolder]/CiviKey/CK-[GUID].txt
        private string GenerateBackupFileName()
        {
            string fileName = String.Format( "CK-{0}.txt", Guid.NewGuid().ToString() );
            string folderPath = Path.Combine( System.IO.Path.GetTempPath(), "CiviKey", "KeyboardEditorBackup" );
            if( !Directory.Exists( folderPath ) ) Directory.CreateDirectory( folderPath );
            string keyboardFilePath = Path.Combine( folderPath, fileName );
            return keyboardFilePath;
        }
    }
}
