using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Net;
using System.Configuration;
using System.ComponentModel;
using System.Reflection;
using System.Net.NetworkInformation;


namespace Simulateur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        //Scenario scenario;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /*Création de la liste d'action*/
        List<Action> listeAct = new List<Action>();
        List<Voiture> listeVoiture = new List<Voiture>();
        List<Equipement> listeEqt = new List<Equipement>();
        List<List<Equipement>> listeEqtComplete = new List<List<Equipement>>();
        Communication comObj;
        ConfigurationTrain confTrain;
        object test;
        public static MainWindow AppWindow;
        NameValueCollection readConfig = ConfigurationManager.AppSettings;
        MessageBoxResult mbResult;
        List<Scenario> listeScenarios;
        public GestionScenario gScenario;
        public GestionSession gSession;
        FenetreSuiviScenario fenetreSuiviScenario;

        public SolidColorBrush colorGreen;
        public SolidColorBrush colorBlue;
        public SolidColorBrush colorBlack;
        public SolidColorBrush colorOrange;
        public SolidColorBrush colorRed;
        public SolidColorBrush colorGray;

        public Visibility visible;

        public AfficheurMessages Afficheur;

        private Boolean _btQuitterEnabled;
        public Boolean btQuitterEnabled
        {
            get
            {
                return _btQuitterEnabled;
            }
            set
            {
                if (_btQuitterEnabled != value)
                {
                    _btQuitterEnabled = value;
                    RaisePropertyChanged("btQuitterEnabled");
                }
            }
        }


        //****************************************************************************************************

        /// Constructeur: Création de l'interface de communication si innexistante.
        /// Si l'objet a déjà été créé, récupération de celui-ci
        /// </summary>
        public MainWindow()
        {

            // initialise les couleurs utilisables
            colorGreen = new SolidColorBrush(Colors.Green);
            colorBlue = new SolidColorBrush(Colors.Blue);
            colorBlack = new SolidColorBrush(Colors.Black);
            colorOrange = new SolidColorBrush(Colors.Orange);
            colorRed = new SolidColorBrush(Colors.Red);
            colorGray = new SolidColorBrush(Colors.Gray);

            InitializeComponent();
            AppWindow = this;
            Afficheur = new AfficheurMessages();

            Closing += fermetureFenetre;

            //Création des fonctions principales
            confTrain = ConfigurationTrain.Instance;
            comObj = Communication.Instance;

            gSession = new GestionSession();
            gScenario = new GestionScenario();

            fenetreSuiviScenario = new FenetreSuiviScenario(gScenario,gSession,this);

            //Binding des fonctions avec les éléments graphiques
            Grille1.DataContext = gSession;
            Grille2.DataContext = gScenario;
            Grille3.DataContext = this;
            cadreZoneAffichage.DataContext = gSession;

           // tbDescriptionScenario.DataContext = gScenario.scenarioCourant;

            //Binding de l'afficheur de message
            msgInfo.DataContext = Afficheur;

            //Initialisation
            btQuitterEnabled = true;
            
            Assembly thisAssem = typeof(MainWindow).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            
            Version ver = thisAssemName.Version;
            this.Title = this.Title + " - v" + ver;

        }

        //Bouton: Connecter au train
        private void btConnecterClick(object sender, RoutedEventArgs e)
        {
            Button bouton = sender as Button;

            //le simulateur est en statut déconnecté, bouton "connecter"
            if (gSession.connexionEstFerme)
            {
                gSession.lancerConnexion();
            }
            else if (gSession.connexionEstEnCours)
            {
                //Arreter le processus de connexion
                gSession.abandonnerConnexion = true;
            }
            //le simulateur est en statut connexion, bouton "déconnecter"
            else
            {
                gSession.lancerDeconnexion();
            }
            
        }

        //Bouton: Ouverture de session
        private void btOuvrirSessionClick(object sender, RoutedEventArgs e)
        {
            Button bouton = sender as Button;
            //Lancer la bonne fonction en cas d'ouverture et en cas de fermeture de session
            if (gSession.sessionEstFerme)
            {
                gSession.lancerIdentificationPC();
            }
            else
            {
                gSession.fermerSession();                
            }
        }

        //Bouton 2: Chargement du scénario et instanciation des commandes en fonction de la liste des équipements
        private void btChargerClick(object sender, RoutedEventArgs e)
        {   
            gScenario.chargerScenario();
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
            if (gScenario.scenarioCourant.estEnExecution) {
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

            Afficheur.MessageAffiche = "Arrêt du scénario en cours";
            gScenario.terminerScenario();

        }

        //Quitter l'application
        private void btQuitterClick(object sender, RoutedEventArgs e)
        {

            MessageBoxResult mb = MessageBox.Show("Etes vous certain de vouloir fermer l'application ?", "Fermeture du simulateur", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (mb == MessageBoxResult.OK)
            {
                if (gSession != null)
                {
                    //Fermer la session
                    if (gSession.sessionEstOuverte)
                    {
                        gSession.fermerSession();
                    }
                    //Fermer la connexion
                    if (gSession.connexionEstOuverte)
                    {
                        gSession.lancerDeconnexion();
                    }

                    gSession.terminer();
                }
                Application.Current.Shutdown();
            }

        }
        private void fermetureFenetre(object sender, CancelEventArgs e)
        {
            Boolean pasDeScenarioEnCours = false;
            
            if (gScenario.scenarioCourant == null)
            {
                pasDeScenarioEnCours = true;
            }
            else if(gScenario.scenarioCourant.estPrecharge || gScenario.scenarioCourant.estCharge)
            {
                pasDeScenarioEnCours = true;
            }
               

            if (pasDeScenarioEnCours)
            {

                MessageBoxResult mb = MessageBox.Show("Etes vous certain de vouloir fermer l'application ?", "Fermeture du simulateur", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (mb == MessageBoxResult.OK)
                {
                    if (gSession != null)
                    {
                        //Fermer la session
                        if (gSession.sessionEstOuverte)
                        {
                            gSession.fermerSession();
                        }
                        //Fermer la connexion
                        if (gSession.connexionEstOuverte)
                        {
                            gSession.lancerDeconnexion();
                        }

                        gSession.terminer();
                    }
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel=true;
                }

            }
            else
            {

                MessageBoxResult mb = MessageBox.Show("Un scénario est en cours d'exécution. Veuillez d'abord l'arrêter.", "Fermeture du simulateur", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;

            }
        }
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

        private void btDetailsClick(object sender, RoutedEventArgs e)
        {
            fenetreSuiviScenario.Show();
            fenetreSuiviScenario.rafraichirTableauSuivi();
        }


        public Boolean afficherSynoptique(ConfigurationTrain cfg)
        {

            Grid grille;
            GroupBox gb;

            Afficheur.MessageAffiche = "composition du train affichée";

            string _labelGroupe = "train";
            
            List<Voiture> synoptique;       //Liste des voitures composant le train

            synoptique = cfg.lireListeVoiture();

            zoneAffichage.Children.Clear();

            int NbreVoiture = synoptique.Count();

            foreach (Voiture voiture in synoptique)
            {
                grille = new Grid();
                grille.Width = Math.Min((936 / NbreVoiture), 235);
                grille.Height = 14 * 5;//20

                //ajout de la grille dans le stackPanel (Encapsulé dans une GroupBox)
                gb = new GroupBox() { Header = voiture.lireLabel() };
                gb.Content = grille;
                gb.Name = voiture.lireNom();
                zoneAffichage.Children.Add(gb);
            }
            cadreZoneAffichage.Header = _labelGroupe;
            cadreZoneAffichage.Visibility = Visibility.Visible;
            return true;

        }

    }
}
