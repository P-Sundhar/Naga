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
using System.Windows.Shapes;

namespace Naga
{
    /// <summary>
    /// Interaction logic for PrintingBox.xaml
    /// </summary>
    public partial class PrintingBox : Window
    {
        public PrintingBox()
        {
            InitializeComponent();
            btnOk.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
