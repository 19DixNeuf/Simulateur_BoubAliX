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
    /// Interaction logic for FenetreIdentification.xaml
    /// </summary>
    public partial class FenetreIdentification : Window
    {
        GestionSession gestionnaireSession; //point d'entrée vers le gestionnnaire de session

        public FenetreIdentification(GestionSession gSession)
        {
            InitializeComponent();
            this.gestionnaireSession = gSession;
            tbIdentifiant.Focus();
        }

        private void btValiderClick(object sender, RoutedEventArgs e)
        {
            if (gestionnaireSession.authentifier(this.tbIdentifiant.Text, this.tbMotDePasse.Password))
            {
                msgErreur.Content = "Authentification réussie !";
                gestionnaireSession.lancerSession();
                this.Close();
            }
            else
            {
                msgErreur.Content = "Echec de l'authentification !";
                this.tbMotDePasse.Clear();
            }
        }

        private void btAnnulerClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
