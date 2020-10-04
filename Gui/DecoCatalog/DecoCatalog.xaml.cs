using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SunbirdMB.Gui
{
    /// <summary>
    /// Interaction logic for DecoCatalog.xaml
    /// </summary>
    public partial class DecoCatalog : UserControl
    {
        public DecoCatalog()
        {
            InitializeComponent();
        }
        private void TextBox_IntegerOnly(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Convert.ToDouble(e.Text);
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void TextBox_DoubleOnly(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != ".")
            {
                try
                {
                    Convert.ToDouble(e.Text);
                }
                catch
                {
                    e.Handled = true;
                }
            }
        }

    }

}
