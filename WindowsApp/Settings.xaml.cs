using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Shift.xaml
    /// </summary>
    public partial class Settings : Window
    {
        MasterLogic objMas = new MasterLogic();
        public Settings()
        {
            InitializeComponent();
            CloseAll();
        }

        public void CloseAll()
        {
            Window objWin = Window.GetWindow(this);
            foreach (Window openWin in System.Windows.Application.Current.Windows)
            {
                if (openWin != objWin)
                    openWin.Close();
            }
        }
    }
}
