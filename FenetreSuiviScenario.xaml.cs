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
using System.ComponentModel; //RTC ajout pour raiseproperty

namespace Simulateur
{
    /// <summary>
    /// Interaction logic for SuiviScenario.xaml
    /// </summary>
    public partial class FenetreSuiviScenario : Window
    {
        MainWindow IHM;
        GestionScenario gScenario;
        GestionSession gSession;

        public FenetreSuiviScenario(GestionScenario gScenario, GestionSession gSession, MainWindow IHM)
        {
            //Enregistrement des pointeurs
            this.IHM = IHM;
            this.gScenario = gScenario;
            this.gSession = gSession;

            InitializeComponent();
            //tabScenario.RequestBringIntoView = this.tabScenarioSelectionne();
            gTabScenario.DataContext = gScenario;
            gTabInfo.DataContext = gSession;
            rTBConsole.DataContext = gScenario;

            Closing += masquerFenetre;

        }

        private void tabScenarioSelectionne(object sender, RequestBringIntoViewEventArgs e)
        {
            //lier le tableau de suivi avec la liste des actions du scénario courant
            this.rafraichirTableauSuivi();
        }

        public void rafraichirTableauSuivi()
        {
            if (gScenario != null)
            {
                if (gScenario.scenarioCourant != null)
                {
                    if (gScenario.scenarioCourant.listeActionDefinitive != null)
                    {
                        tableauDeSuivi.ItemsSource = gScenario.scenarioCourant.listeActionDefinitive;
                    }
                }
            }
        }

        #region BOUTONS
        private void Visualiser_Click(object sender, RoutedEventArgs e)
        {
            gScenario.visualiserScenario();
            //gScenario.btVisualiserEnabled = false;
            rafraichirTableauSuivi();
            
        }


        //Gestion du clic sur le bouton démarrer "►"
        private void btDemarrerClick(object sender, RoutedEventArgs e)
        {
            //si le scénario est prêt à être lancé (chargé ou déjà instancié d'une précédente exécution)
            if (gScenario.scenarioCourant.estCharge
            || gScenario.scenarioCourant.estInstancie) //TODO: vérifier cette condition si vraiment utile
            {
                gScenario.lancerScenario(); //lance le scénario
                ///fenetreSuiviScenario.rafraichirTableauSuivi();
            }
            //sinon s'il est pause
            else if (gScenario.scenarioCourant.estEnPause)
            {
                gScenario.reprendreScenario(); //relance le scénario
                gScenario.scenarioCourant.reglerModePasAPas(false); //désactive le mode pas à pas du scénario
            }

        }

        //Gestion du clic sur le bouton pas à pas "►ӏ"
        private void btDemarrerPauseClick(object sender, RoutedEventArgs e)
        {
            //si le scénario est en cours
            if (gScenario.scenarioCourant.estEnExecution)
            {
                gScenario.scenarioCourant.reglerModePasAPas(true); //active le mode pas à pas du scénario
                gScenario.interrompreScenario(); //met le scénario en pause à la fin de l'action en cours
            }
            //si le scénario est en pause
            else if (gScenario.scenarioCourant.estEnPause)
            {
                gScenario.reprendreScenario(); //relance le scénario
                gScenario.scenarioCourant.reglerModePasAPas(true); //laisse active le mode pas à pas du scénario
            }
        }

        //Gestion du clic sur le bouton pause "II"
        private void btPauseClick(object sender, RoutedEventArgs e)
        {

            if (gScenario.scenarioCourant.etatScenario == Scenario.EnumEtatScenario.exécution)
            {
                gScenario.interrompreScenario();
            }

        }

        //Gestion du clic sur le bouton arret "■" 
        private void btArreterClick(object sender, RoutedEventArgs e)
        {

            IHM.Afficheur.MessageAffiche = "Arrêt du scénario en cours";
            gScenario.terminerScenario();

        }



        private void masquerFenetre(object sender, CancelEventArgs e)
        {
            //annulation de l'évènement de fermeture de la fenetre (clic croix)
            e.Cancel = true;
            //masquage de la fenetre
            this.Hide();

        }

        //RTC
        /// <summary>
        /// Gestion des event pour raffraichir l'IHM
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)

                handler(this, new PropertyChangedEventArgs(name));

        }


        #endregion

    }
}
