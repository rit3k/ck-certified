﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CK.WordPredictor.UI
{
    /// <summary>
    /// Interaction logic for PredictionTextArea.xaml
    /// </summary>
    public partial class TextualContextPreviewWindow : Window
    {
        public TextualContextPreviewWindow( ViewModels.TextualContextPreviewViewModel vm )
        {
            this.DataContext = vm;
            InitializeComponent();
        }
    }
}
