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
using System.Windows.Shapes;

namespace Simulateur
{



    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        public Train trainSelectionne;  // train

        public Window1(GestionSession gSession)
        {
            InitializeComponent();
            
            cbListeTrain.ItemsSource = gSession.cfg.listeTrain;
            
            trainSelectionne = new Train();

        }


        private void cbListeTrain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ComboBoxItem lbi = ((sender as ComboBox).SelectedItem as ComboBoxItem);
            
           //trainSelectionne = lbi.s;
        }
    }
}
