using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Security.Cryptography;
using System.ComponentModel;




namespace Simulateur
{
    public class GestionSession : INotifyPropertyChanged
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string cheminVar_compo;              //Variable permettant la lecture du nombre de voiture
        string cheminVar_Orientation1;       //Variable permettant de lire l'etat de l'orientation du train cabine 1 dans le sens de la marche
        string cheminVar_Orientation2;       //Variable permettant de lire l'etat de l'orientation du train cabine 2 dans le sens de la marche
        string cheminVar_session_interdite;  //Variable permettant de lire l'etat de la session: interdite
        string cheminVar_session_autorisee;  //Variable permettant de lire l'etat de la session: autorisee
        string cheminVar_session_ouverte;    //Variable permettant de lire l'etat de la session: ouverte
        string cheminVar_MpuMaitre;          //Variable permettant d'identifier le MPU maître
        string cheminVar_type_train;         //Variable permettant la lecture du mode de conduite du train

        UInt16  train_compo;              //nombre de voiture
        Boolean train_orientation1;       //orientation du train cabine 1 dans le sens de la marche
        Boolean train_orientation2;       //orientation du train cabine 2 dans le sens de la marche
        Boolean train_session_interdite;  //état de la session: interdite
        Boolean train_session_autorisee;  //état de la session: autorisee
        Boolean train_session_ouverte;    //état de la session: ouverte
        Boolean train_mpu_maitre;         //état MPU est maitre
        UInt16  train_type;               //mode de conduite du train

        Boolean premiereLecture = false; //memorise première lecture des données calculateur

        //Définit les différents états possibles de la connexion
        public enum EnumEtatConnexion
        {
            non_init = 0,
            déconnecté = 1,
            connexion = 2,
            connecté = 3,
            interrompu = 4,
            verrouillé = 255
        }

        //Définit les différents états possibles de la session
        public enum EnumEtatSession
        {
            non_init = 0,
            fermé = 1,
            identification_PC = 2,
            identification_Train = 3,
            ouvert = 4,
            interrompu = 5,
            fermeture = 6,
            verrouillé = 255
        }

        Int16 NbreTentativeReconnexion = 0;

        Window1 ident;

        FenetreIdentification fenetreIdentification;    //Fenetre servant à entrer les identifiants de connexion
        Boolean deconnexionDemandee = false;            //Indique lorsqu'une demande de déconnexion a été initiée
        Boolean connexionPerdue = false;                //Flag permettant aux threads de surveillance de session de détecter une coupure de communication

        Boolean cloturerSession = false;  //Déclencheur d'une fermeture de session
        long tempoFermetureSession = 0;     //Durée maximale admissible de perte de communication avec le train
          
        // TODO: RTC: remettre en privé mais voir pour acceder à listeTrain depuis fenetre identification
           
       public ConfigurationTrain cfg;         //Représentation logicielle du train et de ses équipements
        Communication comObj;           //Pointe vers le module de communication du logiciel avec le train
        NameValueCollection readConfig = ConfigurationManager.AppSettings;  //Pointe vers App.config

        MainWindow IHM;
        MessageBoxResult mbResult;

       // static BackgroundWorker _bw;

        Thread Thread_Connexion;
        Thread Thread_MajEchanges;
        Thread Thread_surveillance_session;
        Thread Thread_surveillance_MPU_maitre;

        
        #region Propriétées
  
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

        //Version logicielle lue sur le MPU
        private string _versionLogicielLu;
        public string versionLogicielLu
        {
            get
            {
                return _versionLogicielLu;
            }
            set
            {
                _versionLogicielLu = value;
                lbVersionLogicielContent = _versionLogicielLu;
            }
        }

        //Version logicielle lue sur le MPU affichée à l'IHM
        private string _lbVersionLogicielContent;
        public string lbVersionLogicielContent
        {
            get
            {
                return _lbVersionLogicielContent;
            }
            set
            {
                _lbVersionLogicielContent = value;
                RaisePropertyChanged("lbVersionLogicielContent");
            }
        }

        // Définition des propriétés du label lbEtatConnexion
        private SolidColorBrush _lbEtatConnexionForeground;
        public SolidColorBrush lbEtatConnexionForeground
        {
            get
            {
                return _lbEtatConnexionForeground;
            }
            private set
            {
                _lbEtatConnexionForeground = value;
                RaisePropertyChanged("lbEtatConnexionForeground");
            }
        }

        private string _lbNbreTentativeReconnexionContent;
        public string lbNbreTentativeReconnexionContent
        {
            get
            {
                return _lbNbreTentativeReconnexionContent;
            }
            set
            {
                _lbNbreTentativeReconnexionContent = value;
                RaisePropertyChanged("lbNbreTentativeReconnexionContent");
            }
        }


        private string _recVieFill;
        public string recVieFill
        {
            get
            {
                return _recVieFill;
            }
            set
            {
                _recVieFill = value;
                RaisePropertyChanged("recVieFill");
            }
        }

        private string _lbOrientation1Content;
        public string lbOrientation1Content
        {
            get
            {
                return _lbOrientation1Content;
            }
            set
            {
                _lbOrientation1Content = value;
                RaisePropertyChanged("lbOrientation1Content");
            }
        }

        private string _lbOrientation2Content;
        public string lbOrientation2Content
        {
            get
            {
                return _lbOrientation2Content;
            }
            set
            {
                _lbOrientation2Content = value;
                RaisePropertyChanged("lbOrientation2Content");
            }
        }

        private string _lbEtatConnexionContent;
        public string lbEtatConnexionContent
        {
            get
            {
                return _lbEtatConnexionContent;
            }
            set
            {
                _lbEtatConnexionContent = value;
                RaisePropertyChanged("lbEtatConnexionContent");
            }
        }

        
        // Définition des propriétés du label lbEtatSimulateur
        private SolidColorBrush _lbEtatSimulateurForeground;
        public SolidColorBrush lbEtatSimulateurForeground
        {
            get
            {
                return _lbEtatSimulateurForeground;
            }
            private set
            {
                _lbEtatSimulateurForeground = value;
                RaisePropertyChanged("lbEtatSimulateurForeground");
            }
        }

        private string _lbEtatSimulateurContent;
        public string lbEtatSimulateurContent
        {
            get
            {
                return _lbEtatSimulateurContent;
            }
            set
            {
                _lbEtatSimulateurContent = value;
               RaisePropertyChanged("lbEtatSimulateurContent");
            }
        }

        // Définition des propriétés du groupbox gbCleConfirmation
        private Visibility _gbCleConfirmationVisible;
        public Visibility gbCleConfirmationVisible
        {
            get
            {
                return _gbCleConfirmationVisible;
            }
            private set
            {
                _gbCleConfirmationVisible = value;
                RaisePropertyChanged("gbCleConfirmationVisible");
            }
        }

        // Définition des propriétés du groupbox gbCleConfirmation
        private Visibility _gbTrainVisible;
        public Visibility gbTrainVisible
        {
            get
            {
                return _gbTrainVisible;
            }
            private set
            {
                _gbTrainVisible = value;
                RaisePropertyChanged("gbTrainVisible");
            }
        }


        // Définition des propriétés du label lbEtatSession
        private SolidColorBrush _lbEtatSessionForeground;
        public SolidColorBrush lbEtatSessionForeground
        {
            get
            {
                return _lbEtatSessionForeground;
            }
            private set
            {
                _lbEtatSessionForeground = value;
                RaisePropertyChanged("lbEtatSessionForeground");
            }
        }

        private string _lbEtatSessionContent;
        public string lbEtatSessionContent
        {
            get
            {
                return _lbEtatSessionContent;
            }
            set
            {
                _lbEtatSessionContent = value;
                RaisePropertyChanged("lbEtatSessionContent");
            }
        }


        //calcul des états du simulateur
        public Boolean connexionEstEnCours      { get { return etatConnexion == EnumEtatConnexion.connexion; } }
        public Boolean connexionEstFerme         { get { return etatConnexion == EnumEtatConnexion.déconnecté; } }
        public Boolean connexionEstOuverte { get { return etatConnexion == EnumEtatConnexion.connecté; } }
        public Boolean connexionEstVerrouille { get { return etatConnexion == EnumEtatConnexion.verrouillé; } }

        public Boolean sessionEstFerme             { get { return etatSession == EnumEtatSession.fermé; } }
        public Boolean sessionEstIdentificationPC { get { return etatSession == EnumEtatSession.identification_PC; } }
        public Boolean sessionEstIdentificationTrain { get { return etatSession == EnumEtatSession.identification_Train; } }
        public Boolean sessionEstOuverte { get { return etatSession == EnumEtatSession.ouvert; } }
        public Boolean sessionEstInterrompu { get { return etatSession == EnumEtatSession.interrompu; } }
        public Boolean sessionEstVerrouille { get { return etatSession == EnumEtatSession.verrouillé; } }
        public Boolean sessionEstEnCoursFermeture { get { return etatSession == EnumEtatSession.fermeture; } }
        public Boolean sessionEstEnCoursOuverture { get { return sessionEstIdentificationPC || sessionEstIdentificationTrain; } }


        //Définit les différents états possibles de la connexion
        EnumEtatConnexion _etatConnexion;                  //Indique l'état de la connexion (string)
        /// <summary></summary>
        public EnumEtatConnexion etatConnexion             //Indique l'état de la connexion (0: déco, 1: en cours, 2: connecté)
        {
            get
            {
                return _etatConnexion;
            }
            private set
            {
                if (_etatConnexion != value)
                {
                    _etatConnexion = value;
                    majIHM(this);                  // demande la mise à jour de l'IHM
                }
            }
        }
        //Définit les différents états possibles de la session
        EnumEtatSession _etatSession;                    //Indique l'état de la session (0: fermé, 1: en cours, 2: ouvert)
        public EnumEtatSession etatSession
        {
            get
            {
                return _etatSession;
            }
            private set
            {
                if (_etatSession != value)
                {
                    _etatSession = value;
                    majIHM(this);                  // demande la mise à jour de l'IHM
                }
            }
        }           //Indique l'état de la session (string)


        public Boolean sessionOuvrir()
        {
            etatSession = EnumEtatSession.ouvert;
            return true;
        }



        //Disponibilité du bouton de connexion
        private Boolean _btConnexionEnabled;
        public Boolean btConnexionEnabled
        {
            get
            {
                return _btConnexionEnabled;
            }
            set
            {
                if(_btConnexionEnabled != value) {
                    _btConnexionEnabled = value;
                    RaisePropertyChanged("btConnexionEnabled");
                }
            }
        }

        //texte du bouton de connexion
        private String _btConnexionContent;
        public String btConnexionContent
        {
            get
            {
                return _btConnexionContent;
            }
            set
            {
                if (_btConnexionContent != value)
                {
                    _btConnexionContent = value;
                    RaisePropertyChanged("btConnexionContent");
                }
            }
        }

        //Disponibilité du bouton de Session
        private Boolean _btSessionEnabled;
        public Boolean btSessionEnabled
        {
            get
            {
                return _btSessionEnabled;
            }
            set
            {
                if (_btSessionEnabled != value)
                {
                    _btSessionEnabled = value;
                    RaisePropertyChanged("btSessionEnabled");
                }
            }
        }

        //texte du bouton de Session
        private String _btSessionContent;
        public String btSessionContent
        {
            get
            {
                return _btSessionContent;
            }
            set
            {
                if (_btSessionContent != value)
                {
                    _btSessionContent = value;
                    RaisePropertyChanged("btSessionContent");
                }
            }
        }

        //flag permettant d'abandonner la connexion en cours de l'appli en cas de problème
        private Boolean _abandonnerConnexion;
        public Boolean abandonnerConnexion
        {
            get
            {
                return _abandonnerConnexion;
            }
            set
            {
                _abandonnerConnexion = value;
            }
        }



        #endregion


        /// <summary>
        /// Constructeur du gestionnaire de session:
        /// initialise la configuration du projet.
        /// </summary>
        public GestionSession()
        {

            IHM = (MainWindow)Application.Current.MainWindow;   //Récupération du "pointeur" vers l'IHM
            long.TryParse(readConfig["tempoFermetureSession"], out tempoFermetureSession); //Récupération dans le fichier de config de la durée maximale admissible de perte de communication avec le train

            // état connexion et session non initialisé
            etatConnexion = EnumEtatConnexion.non_init;
            etatSession = EnumEtatSession.non_init;

            //Création et initialisation de la configuration du train
            cfg = ConfigurationTrain.Instance;
            cfg.initialiser();

            //Récupération des variables et leur chemin pour les échanges avec la partie embarquée du simulateur
            cheminVar_compo = cfg.lireVariableComposition();                    // composition du train (nombre de voitures)
            cheminVar_Orientation1 = cfg.lireVariableOrientation1Active();      // orienation 1 (cabine 1 active ou non)
            cheminVar_Orientation2 = cfg.lireVariableOrientation2Active();      // orienation 2 (cabine 2 active ou non)
            cheminVar_session_interdite = cfg.lireVariableSessionInterdite();   // session interdite ou non
            cheminVar_session_autorisee = cfg.lireVariableSessionAutorisee();   // session autorisée ou non
            cheminVar_session_ouverte = cfg.lireVariableSessionOuverte();       // session en cours ou non
            cheminVar_type_train = cfg.lireVariableTypeTrain();                 // type de train (conduite CA ou CC)
            cheminVar_MpuMaitre = cfg.lireVariableMpuMaitre();                  // MPU est maitre

            gbTrainVisible = Visibility.Hidden;

            // TODO: il faudrait vérifier l'intégrité des données de config ?

            //Initialisation de la connexion          
            //Vérification de la présence des dossiers et fichiers ETR ==> TODO: ne fonctionne pas correctement: le chg du nom dossierETR en dossiertoto ne provoque rien
            if (!Directory.Exists(readConfig["cheminETR"] + readConfig["dossierETR"]))
            {
                mbResult = MessageBox.Show("Le dossier ETR contenant les paramètres de connexion du train est absent,\n "
                                                            + "Vérifier la présence du fichier à l'endroi spécifié dans le fichier App.config:"
                                                            + readConfig["cheminETR"] + readConfig["dossierETR"],
                                                            "Démarage de session", MessageBoxButton.OK, MessageBoxImage.Stop);
                etatConnexion = EnumEtatConnexion.verrouillé;
            }
            else if (!Directory.Exists(readConfig["cheminETR"] + readConfig["dossierETR"] + "\\" + readConfig["versionETR"]))
            {
                mbResult = MessageBox.Show("Le fichier ETR contenant les paramètres de connexion du train pour la version paramétré est absent,\n "
                                                            + "Vérifier la présence du fichier à l'endroi spécifié dans le fichier App.config.",
                                                            "Démarage de session", MessageBoxButton.OK, MessageBoxImage.Stop);
                etatConnexion = EnumEtatConnexion.verrouillé;
            }
            else
            {
                etatConnexion = EnumEtatConnexion.déconnecté;
                etatSession = EnumEtatSession.fermé;
            }

            cfg.listerTrain();
            cfg.listeTrain[0].ToString();
            ident = new Window1(this);
         //   ident.Show();  BSA

            majIHM(this);
          //  MessageBox.Show("Tests sans connexion, 4 voitures"); BSA 
            cfg.lancerConfiguration("4", 2); //conf istanbul
            IHM.afficherSynoptique(cfg);
            //btConnexionContent = "sans connexion";
            //btConnexionEnabled = false;
            gbTrainVisible = Visibility.Visible;/**/

        }

        /// <summary>
        /// Lance la connexion avec le train
        /// </summary>
        public void lancerConnexion()
        {
            if (connexionEstFerme)
            {
                //Lancer le processus de connexion connecter()
                //lancer le thread de surveillance de la connexion
                if (Thread_Connexion != null)
                {
                    Thread_Connexion.Abort();
                }

                Thread_Connexion = new Thread(() => Connecter(this));
                if (Thread_Connexion.Name == null)
                    Thread_Connexion.Name = "Connexion";
                Thread_Connexion.Start();

            }
            
        }
        /// <summary>
        /// Thread de connexion
        /// </summary>
        /// <param name="gSession"></param>
        static private void Connecter(GestionSession gSession)
        {


            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            (System.Threading.ThreadStart)delegate
            {
                gSession.majIHM(gSession);
                MessageBox.Show("Tests sans connexion, 8 voitures");
                gSession.cfg.lancerConfiguration("8", 1); //conf istanbul
                gSession.IHM.afficherSynoptique(gSession.cfg);
                //btConnexionContent = "sans connexion";
                //btConnexionEnabled = false;
                gSession.gbTrainVisible = Visibility.Visible;/**/
            });


            //Mettre à jour le statut
            gSession.etatConnexion = EnumEtatConnexion.connexion;

            Boolean NonclickCancel = true;
            
            MessageBoxResult mbResult;

            gSession.deconnexionDemandee = false;            //flag permettant de gérer l'affichage du status de la connexion à l'écran
            gSession.abandonnerConnexion = false;

            //Affiche le statut d'avancement de la connexion à l'IHM
            gSession.lbEtatSimulateurContent = "connexion en cours";

            //Etablissement de la commnunication avec le train
            gSession.comObj = Communication.Instance;

            //Affiche le statut d'avancement de la connexion à l'IHM "lecture configuration ETR"
            gSession.lbEtatSimulateurContent = "lecture configuration ETR";

            String strAppDir = Environment.CurrentDirectory;
            String cheminETR = gSession.readConfig["cheminETR"];
            String dossierETR = gSession.readConfig["dossierETR"];
            String versionETR = gSession.readConfig["versionETR"];
            try { 

                //initialisation
                if (!gSession.abandonnerConnexion)
                {
                   // gSession.comObj.init(cheminETR, dossierETR, versionETR);
                    gSession.comObj.init(cheminETR, dossierETR, versionETR);
                    Thread.Sleep(1000); // pause dans la connexion
                }

            }
                catch (Exception e)
                {
                    mbResult = MessageBox.Show("Echec lors de l'initialisation de la connexion\nVérifier la présence des fichiers DLL TrainTracer (atml.dll). ",
                        "Connexion au train", MessageBoxButton.OK, MessageBoxImage.Stop);
                    log.Error("La lecture de la version logicielle MPU a échouée.");
                    log.Error("Echec lors de l'initialisation de la connexion: vérifier présence des DLL TrainTracer (atml.dll).");
                    log.Debug("Echec lors de l'initialisation de la connexion: vérifier présence des DLL TrainTracer (atml.dll).", e);

                //TODO: quitter sur cette exception
                //Arreter le processus de connexion
                gSession.abandonnerConnexion = true;
            }


            if (!gSession.abandonnerConnexion)
            {   
                //Affiche le statut d'avancement de la connexion à l'IHM "Connexion au train"
                gSession.lbEtatSimulateurContent = "connexion au train";
                gSession.comObj.Connecter(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                Thread.Sleep(1000); // pause dans la connexion
            }

            /* Rechercher le MPU maître */


            //Lance le processus d'identification du MPU maitre
            if (!gSession.abandonnerConnexion)
            {
                //Affiche le statut d'avancement de la connexion à l'IHM "Recherche MCE Maitre"
                gSession.lbEtatSimulateurContent = "identification du calculateur maitre";

                Boolean mpuMaitreTrouve = gSession.comObj.rechercherMpuMaitre(gSession.cheminVar_MpuMaitre);

                if (!mpuMaitreTrouve)
                {
                    //gSession.mbResult = MessageBox.Show("La connexion avec la cible a échouée!\n "
                    //                           , "Connexion au train", MessageBoxButton.OK, MessageBoxImage.Stop);
                    //log.Warn("Aucun MPU trouvé.");

                    //Arreter le processus de connexion
                    gSession.abandonnerConnexion = true;
                }
            }


            /* Lire la version du logiciel du MPU */
            if (!gSession.abandonnerConnexion)
            {
                //Affiche le statut d'avancement de la connexion à l'IHM "Lecture version logiciel train"
                gSession.lbEtatSimulateurContent = "lecture version logiciel train";

                //Vérifie la version logicielle
                gSession.versionLogicielLu = gSession.comObj.LireVersionLogicielMPU();
                if (gSession.versionLogicielLu == "")
                {
                    mbResult = MessageBox.Show("La lecture de la version logicielle de la cible a échouée!\n ",
                                            "Connexion au train", MessageBoxButton.OK, MessageBoxImage.Stop);
                    log.Warn("La lecture de la version logicielle MPU a échouée.");

                    //Arreter le processus de connexion
                    gSession.abandonnerConnexion = true;
                }
            }

            /* Rechercher la correspondance avec la version chargée dans le simulateur */
            if (!gSession.abandonnerConnexion)
            {
                //Affiche le statut d'avancement de la connexion à l'IHM "Recherche correspondance version dans simulateur"
                gSession.lbEtatSimulateurContent = "recherche correspondance version dans simulateur";

                //compare la version embarquée dans le MPU avec la version chargée dans le simulateur
                if (gSession.versionLogicielLu != gSession.readConfig["versionETR"])
                {
                    //chercher dans le dossier courant un dossier du nom de la version MPU lue
                    if (Directory.Exists(gSession.readConfig["cheminETR"] + gSession.readConfig["dossierETR"] + "\\" + gSession.versionLogicielLu))
                    {
                        //S'il est bien présent, la connexion est réinitialisée avec la bonne version
                        gSession.comObj.init(gSession.readConfig["cheminETR"], gSession.readConfig["dossierETR"], gSession.versionLogicielLu);
                        gSession.comObj.Connecter(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString());
                    }
                    else
                    {
                        //Afficher un message indiquant que le fichier est manquant
                        mbResult = MessageBox.Show("Le dossier ETR contenant les paramètres de connexion du train ne correspond pas à la version actuelle du train,\n "
                                                                    + "Version configurée: " + gSession.readConfig["versionETR"] + "\nVersion train: " + gSession.versionLogicielLu
                                                                    , "Connexion au train", MessageBoxButton.OK, MessageBoxImage.Stop);
                        log.Warn("Le dossier ETR contenant les paramètres de connexion du train ne correspond pas à la version actuelle du train,\n "
                                                                    + "Version configurée: " + gSession.readConfig["versionETR"] + "\nVersion train: " + gSession.versionLogicielLu);

                        //Arreter le processus de connexion
                        gSession.abandonnerConnexion = true;
                    }
                }
            }


            /* Lancement de la surveillance de la connexion avec le MPU */
            if (!gSession.abandonnerConnexion)
            {
                //Affiche le statut d'avancement de la connexion à l'IHM "Lancement surveillance connexion train"
                gSession.lbEtatSimulateurContent = "Lancement maj données train";
                gSession.etatConnexion = EnumEtatConnexion.connecté; //RTC

                //lancer le tread de surveillance de la connexion
                if (gSession.Thread_MajEchanges != null)
                {
                    gSession.Thread_MajEchanges.Abort();
                }

                gSession.Thread_MajEchanges = new Thread(() => gSession.MajEchanges(gSession));
                if (gSession.Thread_MajEchanges.Name == null)
                    gSession.Thread_MajEchanges.Name = "MajEchanges";
                gSession.Thread_MajEchanges.Start();
            }


            /* Déterminer l'orientation du train */
            if (!gSession.abandonnerConnexion)
            {

                gSession.IHM.Afficheur.MessageAffiche = "récupération de la configuration du train";
                //Affiche le statut d'avancement de la connexion à l'IHM "identification orientation du train"
                gSession.lbEtatSimulateurContent = "identification orientation du train";

                Boolean _orientationOk = false;

                do
                {
                    // TODO: à simplifier
                    if (gSession.train_orientation1)
                    {
                        gSession.lbEtatSimulateurContent = "Orientation 1 est actif"; //RTC                                            
                                             
                       // if (!gSession.cfg.lireEtatConf())
                        //{

                            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                          (System.Threading.ThreadStart)delegate
                          {
                              gSession.cfg.lireEtatConf();
                              gSession.cfg.lancerConfiguration(gSession.train_compo.ToString(), 1);
                              gSession.IHM.afficherSynoptique(gSession.cfg); // affiche le synoptique du train
                          });

                            gSession.gbTrainVisible = Visibility.Visible;
                        //}
                        _orientationOk = true;
                    }
                    else if (gSession.train_orientation2)
                    {
                        gSession.lbEtatSimulateurContent = "Orientation 2 est actif"; //RTC

                        if (!gSession.cfg.lireEtatConf())
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                          (System.Threading.ThreadStart)delegate
                          {
                              gSession.cfg.lancerConfiguration(gSession.train_compo.ToString(), 1);
                              gSession.IHM.afficherSynoptique(gSession.cfg); // affiche le synoptique du train
                          });
                        }
                        
                        gSession.gbTrainVisible = Visibility.Visible;

                        _orientationOk = true;
                    }
                    else if(gSession.premiereLecture == true) // TODO: prévoir de relacher si trop long
                    {
                        mbResult = MessageBox.Show("Orientation indéterminée !\n "
                                                + "Veuillez activer une cabine puis appuyer sur OK",
                                                "Connexion au train", MessageBoxButton.OKCancel, MessageBoxImage.Stop);

                        NonclickCancel = mbResult != MessageBoxResult.Cancel;
                    }


                } while (!_orientationOk && NonclickCancel && gSession.etatConnexion != EnumEtatConnexion.interrompu);

                if (!NonclickCancel)
                {
                    mbResult = MessageBox.Show("Une cabine doit être active pour utiliser le simulateur!\n "
                                                + "La connexion à la cible est annulée.\n",
                                                "Connexion au train", MessageBoxButton.OK, MessageBoxImage.Warning);
                    gSession.lancerDeconnexion();
                    _orientationOk = false;
                }
                else
                {
                    //Afficher le synoptique train
                    //gSession.vueTrain.lancer();
                    gSession.IHM.Afficheur.MessageAffiche = "Connexion réussie";

                    //lire le mode de conduite du train
                    gSession.cfg.reglerTypeTrain(gSession.train_type.ToString());

                    //lancer le thread de surveillance de la session
                    if (gSession.Thread_surveillance_session != null)
                    {
                        gSession.Thread_surveillance_session.Abort();
                    }
                    
                    gSession.Thread_surveillance_session = new Thread(() => gSession.surveillerSession(gSession));
                    gSession.Thread_surveillance_session.Start();
                    if (gSession.Thread_surveillance_session.Name == null)
                        gSession.Thread_surveillance_session.Name = "Surveillance session";
               
                }

            }

            if (gSession.abandonnerConnexion)
            {
                gSession.lancerDeconnexion();
            }

                             
        }
        /// <summary>
        /// Déconnecte le logiciel du train
        /// </summary>
        public void lancerDeconnexion()
        {
            if (!connexionEstFerme)
            {
                abandonnerConnexion = true; // n'a aucun effet sur le processus de connexion
                deconnexionDemandee = true;

                if (comObj != null)
                    comObj.Deconnecter();
                IHM.Afficheur.MessageAffiche = "Train déconnecté";
                etatConnexion = EnumEtatConnexion.déconnecté;
            }
        }
        /// <summary>
        /// Démarrer la fenêtre d'identification PC
        /// </summary>
        public void lancerIdentificationPC()
        {
            etatSession = EnumEtatSession.identification_PC;
            //Demander identification
            fenetreIdentification = new FenetreIdentification(this);
            fenetreIdentification.Show();
        }
        /// <summary>
        /// Lancer la session de formation
        /// </summary>
        public void lancerSession()
        {
            etatSession = EnumEtatSession.identification_Train;
            if (!sessionEstOuverte)
            {
                string cleConsole = string.Empty;      //clé permettant la confirmation de l'ouverture de session via la console
                Boolean lecture_cle_OK = false;
                try
                {
                    //Lire clé sur MPU
                    cleConsole = comObj.LireVariable(cfg.lireVariableCleConfirmation()).ToString();
                    //Afficher clé à l'écran
                    //IHM.visible = Visibility.Visible;
                    //gbCleConfirmationVisible = IHM.visible;
                    IHM.cleConfirmation.Content = cleConsole;
                    //Affiher message 
                    IHM.Afficheur.MessageAffiche = "Entrer la clé de confirmation sur la console de conduite";
                    lecture_cle_OK = true;
                }
                catch (Exception ex)
                {
                    log.Error("Echec de la lecture de la clé de confirmation");
                    log.Debug("Echec de la lecture de la clé de confirmation", ex);
                    IHM.Afficheur.MessageAffiche = "Echec de la lecture de la clé de confirmation";
                }
                /*
                if (lecture_cle_OK)
                {
                    Thread_surveillance_session = new Thread(() => surveillerSession(this));//(new ThreadStart(surveillerSession));
                    Thread_surveillance_session.Start();
                }*/
            }
        }



        //RTC
        public void majIHM(GestionSession gSession)
        {
            // ici, il faudra mettre à jour les propriétés des éléments graphiques en fonction du tableau excel
     
            // gestion du bouton de connexion en fonction de l'état du simulateur
            if (connexionEstFerme)
            {
                btConnexionContent = "Connecter";
            }

            if(connexionEstFerme || connexionEstEnCours || sessionEstVerrouille || (connexionEstOuverte && sessionEstFerme))
            {
                btConnexionEnabled = true;
            }
            else
//            if (gSession.etatConnexion == EnumEtatConnexion.non_init || etatSession == EnumEtatSession.identification_PC || etatSession == EnumEtatSession.identification_Train)
            {
                btConnexionEnabled = false;
            }

            if (connexionEstVerrouille)
            {
                btConnexionContent = "Connecter";
                
            }

            if (connexionEstEnCours)
            {
                btConnexionContent = "Annuler";
                //btConnexionEnabled = true;
            }

            if (connexionEstOuverte)
            {
                btConnexionContent = "Déconnecter";
                //btConnexionEnabled = true;
                btSessionEnabled = true;
            }

            // gestion du bouton de session en fonction de l'état du simulateur
            if (sessionEstOuverte || (connexionEstOuverte && sessionEstFerme) || sessionEstIdentificationTrain)
            {
                btSessionEnabled = true;
            }

            if (sessionEstEnCoursOuverture || connexionEstFerme || sessionEstVerrouille)
            {
                btSessionEnabled = false;
            }

            if (sessionEstIdentificationTrain)
            {
                gbCleConfirmationVisible = Visibility.Visible;
            }
            else
            {
                gbCleConfirmationVisible = Visibility.Hidden;
            }

            if (sessionEstIdentificationTrain)
            {
                btSessionContent = "Annuler";
            }

            if (sessionEstFerme)
            {
                btSessionContent = "Ouvrir";
            }

            if (sessionEstOuverte)
            {
                btSessionContent = "Fermer";
            }

            // configure l'apparence du label EtatSession en fonction de l'état de la session
            switch (etatSession)
               {
                   case EnumEtatSession.ouvert:
                        lbEtatSessionForeground = IHM.colorGreen;
                        lbEtatSessionContent = "ouvert";
                        break;

                case EnumEtatSession.fermeture:
                    lbEtatSessionForeground = IHM.colorOrange;
                    lbEtatSessionContent = "fermeture";
                    break;

                case EnumEtatSession.fermé:
                        lbEtatSessionForeground = IHM.colorBlack;
                        lbEtatSessionContent = "fermé";
                        break;

                   case EnumEtatSession.identification_PC:
                        lbEtatSessionForeground = IHM.colorOrange;
                        lbEtatSessionContent = "identification PC";
                        break;

                    case EnumEtatSession.identification_Train:
                        lbEtatSessionForeground = IHM.colorOrange;
                        lbEtatSessionContent = "identification Train";
                        break;

                    case EnumEtatSession.interrompu:
                        lbEtatSessionForeground = IHM.colorRed;
                        lbEtatSessionContent = "interrompu";
                        break;

                    case EnumEtatSession.verrouillé:
                        lbEtatSessionForeground = IHM.colorRed;
                        lbEtatSessionContent = "verrouillé";
                        break;

                   default:
                        lbEtatSessionForeground = IHM.colorBlue;
                        lbEtatSessionContent = "???";
                        break;
               }


            // configure l'apparence du label etatConnexion en fonction de l'état de la connexion
            switch (gSession.etatConnexion)
              {
                  case EnumEtatConnexion.connecté:
                    lbEtatConnexionForeground = IHM.colorGreen;
                    gSession.lbEtatConnexionContent = "connecté";
                  break;

                  case EnumEtatConnexion.déconnecté:
                    lbEtatConnexionForeground = IHM.colorBlack;
                    gSession.lbEtatConnexionContent = "déconnecté";
                  break;

                  case EnumEtatConnexion.connexion:
                    lbEtatConnexionForeground = IHM.colorOrange;
                    gSession.lbEtatConnexionContent = "en cours";
                  break;

                case EnumEtatConnexion.interrompu:
                    lbEtatConnexionForeground = IHM.colorBlue;
                    gSession.lbEtatConnexionContent = "essais de reconnexion";
                    break;

                case EnumEtatConnexion.verrouillé:
                    lbEtatConnexionForeground = IHM.colorRed;
                    gSession.lbEtatConnexionContent = "interdit";
                    break;

                default:
                    lbEtatConnexionForeground = IHM.colorBlue;
                    gSession.lbEtatConnexionContent = "???";
                  break;

              }

        }

        /// <summary>
        /// Fermer la session de formation
        /// </summary>
        public void fermerSession()
        {
            cloturerSession = true;

            etatSession = EnumEtatSession.fermeture;

            lbEtatSimulateurContent = "session en cours de fermeture";
            IHM.Afficheur.MessageAffiche = "session en cours de fermeture";
            
            if (connexionEstOuverte)
            {
                //etatSession = EnumEtatSession.fermé;
                lbEtatSimulateurContent = "fermeture session";
                IHM.Afficheur.MessageAffiche = "fermeture session";

                try
                {
                    comObj.EcrireVariable(cfg.lireVariableFermerSession(), true);
                    comObj.EcrireVariable(cfg.lireVariableFermerSession(), false);//RTC

                    //vérification immédiate état de la session
                    train_session_ouverte = (Boolean)comObj.LireVariable(cheminVar_session_ouverte);    //Variable permettant de lire l'état de la session: ouverte

                    log.Info("Demande fermeture de la session de formation.");
                }
                catch (Exception exe)
                {
                    //Log
                    log.Error("Echec de la demande de fermeture de session");
                    log.Debug("Echec de la demande de fermeture de session", exe);
                }
            }
            
        }

 
        /// <summary>
        /// Surveillance de la session active
        /// </summary>
        /// 

        public void surveillerSession(GestionSession gSession)
        {

            Boolean messageSelectionScenario= false;

            //laisser tourner le thread tant que la session est initialisée et la connexion active
            while (gSession.etatSession != EnumEtatSession.non_init && !connexionEstFerme)
            {

                //la session est fermée mais le train la voit ouverte
                if ((gSession.sessionEstFerme && connexionEstOuverte && train_session_ouverte)
                || (gSession.sessionEstIdentificationTrain && train_session_ouverte)
                )
                {
                    gSession.etatSession = EnumEtatSession.ouvert;
                }

                //la session est ouverte mais la connexion est interrompue
                if (gSession.sessionEstOuverte && gSession.etatConnexion == EnumEtatConnexion.interrompu)
                {
                    gSession.etatSession = EnumEtatSession.interrompu;
                }

                //la session est interrompue et la connexion est fermée
                if ((gSession.sessionEstInterrompu && connexionEstFerme)
                ||  (gSession.sessionEstEnCoursFermeture && !train_session_ouverte)
                ||  (gSession.sessionEstOuverte && !train_session_ouverte))
                {
                    gSession.etatSession = EnumEtatSession.fermé;
                }

                //la session est interrompue et la connexion est récupérée
                if (gSession.sessionEstInterrompu && connexionEstOuverte)
                {
                    gSession.etatSession = EnumEtatSession.ouvert;
                }

                //la session est interdite par le train
                if ((gSession.sessionEstInterrompu && train_session_interdite)
                || (gSession.sessionEstFerme && train_session_interdite)
                || (gSession.sessionEstIdentificationTrain && train_session_interdite))
                {
                    gSession.etatSession = EnumEtatSession.verrouillé;
                }

                if(gSession.sessionEstOuverte && !messageSelectionScenario) {
                    IHM.Afficheur.MessageAffiche = "Sélectionner un scénario";
                    messageSelectionScenario = true; //remis à zéro au prochain appel du Thread (prochaine ouverture de session)
                }
                
            }

        }


  /*      public void surveillerSession(GestionSession gSession)
        {
            Boolean enAttenteDuModeFormation = true;
            //Boolean sessionOuverte = false;
            gSession.connexionPerdue = false;
            gSession.cloturerSession = false;
            UInt16 compteur = 0;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            Boolean session_interdite = false;
            Boolean session_autorisee = false;

            try
            {
                //lecture des jalons de l'ouverture session
                session_interdite = (Boolean)comObj.LireVariable(cfg.lireVariableSessionInterdite());
                session_autorisee = (Boolean)comObj.LireVariable(cfg.lireVariableSessionAutorisee());

            }
            catch (Exception ex)
            {
                log.Error("Echec de la lecture des conditions d'ouverture de session");
                log.Debug("Echec de la lecture des conditions d'ouverture de session", ex);
                IHM.Afficheur.MessageAffiche = "Echec de la lecture des conditions d'ouverture de session";
            }
            //Lancement de la fonction de suivie si les conditions sont remplies
            if (session_interdite)
            {
                gSession.etatSession = EnumEtatSession.verrouillé; // RTC ajout du statut session verrouillé
                log.Info("Ouverture de session interdite: repréparation du train nécessaire.");
                IHM.Afficheur.MessageAffiche = "Ouverture de session interdite: repréparation du train nécessaire.";
            }
            else if (!session_autorisee)
            {
                log.Info("Ouverture de session non autorisée: vérifier les conditions d'états du train.");
                IHM.Afficheur.MessageAffiche = "Ouverture de session non autorisée: vérifier les conditions d'états du train.";
            }
            else
            {
                //Initialiser thread de surveillance du MPU
                if (gSession.Thread_surveillance_MPU_maitre != null)
                {
                    gSession.Thread_surveillance_MPU_maitre.Abort();
                }
                gSession.Thread_surveillance_MPU_maitre = new Thread(() => gSession.surveiller_MPU_maitre(gSession));

                do
                {
                    //Si la connexion vient d'être perdue
                    if ((!gSession.SimuEstConnecte) && !timer.IsRunning)    
                    {
                        //démarrer timer
                        timer.Start();

                        //Interrompre le scénario en cours
                        if (gSession.IHM.gScenario.scenarioCourant != null)
                        {
                            if (!gSession.IHM.gScenario.scenarioCourant.estInterrompu())
                            {
                                gSession.IHM.gScenario.scenarioCourant.interrompre();
                                //empêcher l'utilisateur d'interagir avec le scénario
                                gSession.IHM.gScenario.btPauseEnabled = false;
                                gSession.IHM.gScenario.btStopEnabled = false;
                                gSession.IHM.gScenario.btPauseContent = "Continuer";

                                //Avertir l'utilisateur
                                gSession.IHM.Afficheur.MessageAffiche = "Connexion interrompue: scénario mis en pause";
                                log.Info("Connexion interrompue: scénario mis en pause");
                            }

                        }
                        else
                        {
                            gSession.IHM.Afficheur.MessageAffiche = "Connexion interrompue: tentative de reconnexion...";
                            log.Info("Connexion interrompue: tentative de reconnexion...");
                        }
                        
                    }
                    //Si la perte de connexion est trop longue
                    else if (!gSession.SimuEstConnecte && (timer.ElapsedMilliseconds >= tempoFermetureSession) && timer.IsRunning) //parametrable via App.config
                    {
                        //fermer session 
                        gSession.cloturerSession = true;
                        //Stopper le timer
                        timer.Stop();
                        //Reset le timer
                        timer.Reset();

                        //Arreter le scénario en cours
                        if (gSession.IHM.gScenario.scenarioCourant != null)
                        {
                            if(!gSession.IHM.gScenario.scenarioCourant.estArrete() || !IHM.gScenario.scenarioCourant.estInterrompu())//Si le scénario est en cours
                            {
                                gSession.IHM.gScenario.arreterScenario();
                                //Inhiber les boutons concernant le scénario
                                gSession.IHM.gScenario.btPauseEnabled=false;
                                gSession.IHM.gScenario.btPauseContent = "Suspendre";
                                gSession.IHM.gScenario.btStopEnabled = false;
                                gSession.IHM.Bouton_Quitter = true;
                                //Avertir l'utilisateur
                                gSession.IHM.Afficheur.MessageAffiche = "La connexion avec le train est perdue: Repréparation du train nécessaire";
                                MessageBox.Show("La connexion avec le train est perdue: session interrompue\n "
                                                + "L'intégrité des forçages ne peux plus être assuré:\n"
                                                + "une repréparation du train est nécessaire.\n",
                                                "Interruption de la session", MessageBoxButton.OK, MessageBoxImage.Stop);
                            }

                        }
                        else
                        {
                            gSession.IHM.Afficheur.MessageAffiche = "La tentative de reconnexion a échouée.";
                        }
                        
                    }
                    //Si la connexion est rétablie
                    else if (gSession.SimuEstConnecte && timer.IsRunning)
                    {
                        //Stopper le timer
                        timer.Stop();
                        //Reset le timer
                        timer.Reset();
                        //Réhabiliter l'exécution du code
                        gSession.connexionPerdue = false;

                        //si un scénario est sélectionné
                        if (gSession.IHM.gScenario.scenarioCourant != null)
                        { //Si le scénario est en cours
                            if (!gSession.IHM.gScenario.scenarioCourant.estArrete())
                            {
                                //Réautoriser les actions sur les boutons concernant le scénario
                                gSession.IHM.gScenario.btPauseEnabled = true;      //Réabiliter le bouton pause
                                gSession.IHM.gScenario.btPauseContent = "Continuer";
                                gSession.IHM.gScenario.btStopEnabled = true;    //Réabiliter le bouton arreter
                                gSession.IHM.Bouton_Quitter = false; //empêcher de quitter le logiciel sans s'être déconnecté
                            }
/*
                        }
                        //Avertir l'utilisateur
                        gSession.IHM.Afficheur.MessageAffiche = "Connexion avec le train rétablie";

                    }
                    //Si la connexion est bonne
                    else if (gSession.etatConnexion==EnumEtatConnexion.connecté)
                    {
                        if (timer.IsRunning)
                        {
                            //reset timer
                            timer.Stop();
                            timer.Reset();
                        }
                        //Lecture de l'état de la session
                        try
                        {
                            if ((Boolean)comObj.LireVariable(cfg.lireVariableSessionOuverte())) {
                                gSession.etatSession = EnumEtatSession.ouvert;
                            }
                            else if (gSession.etatSession != EnumEtatSession.identification_PC && gSession.etatSession != EnumEtatSession.verrouillé) // RTC : si la session n'est pas encore ouverte coté train et qu'on est pas en phase d'identification coté PC ni en session interdite, alors on est session fermée
                            {
                                gSession.etatSession = EnumEtatSession.fermé;
                            }
                            
                        }
                        catch (SystemException excep)
                        {
                            if (excep is System.NullReferenceException)
                            {
                                gSession.connexionPerdue = true;
                            }
                        }

   /*                     

                        if (gSession.SimuEstConnecte && !gSession.connexionPerdue)
                        {
                            if (gSession.sessionEstOuverte)
                            //if(etatSession == EnumEtatSession.ouvert)
                            {
                                //incrémenter et envoyer bit de vie de la session
                                if (compteur != 65535)
                                {
                                    compteur += 1;
                                }
                                else
                                {
                                    compteur = 0;
                                }
                                if (!comObj.EcrireVariable(cfg.lireVariableBitDeVie(), compteur)) // RTC TODO mettre à jour toutes les 1 secondes
                                {
                                    gSession.connexionPerdue = true;
                                }
                                if (enAttenteDuModeFormation)
                                {
                                    enAttenteDuModeFormation = false;
                                    //Masquer la clé de confirmation à l'écran
                                   // IHM.visible = Visibility.Hidden;
                                    //gbCleConfirmationVisible = IHM.visible;
                                    //gSession.IHM.Afficheur.MessageAffiche = "Session de formation ouverte";
                                    gSession.etatSession = EnumEtatSession.ouvert;

                                    //démarer la surveillance du MPU
                                    gSession.Thread_surveillance_MPU_maitre.Start();
                                }


                            }
                            else if (!gSession.sessionEstOuverte && !enAttenteDuModeFormation)
                            //else if (etatSession != EnumEtatSession.ouvert && !enAttenteDuModeFormation)
                            {
                                //Afficher "Mode formation interrompu."
                                mbResult = MessageBox.Show("Mode formation interrompu !\n "
                                                           + "Le scénario et la session de formation ont été arrêté. ",
                                                           "Interruption de la session", MessageBoxButton.OK, MessageBoxImage.Stop);
                                gSession.etatSession = EnumEtatSession.interrompu;
                                //gSession.sessionOuverte = false;
                            }
                        }

                    }



                } while (!gSession.cloturerSession);
            }

            if (!gSession.sessionEstFerme) { //RTC: ajout condition session pas deja fermée
                gSession.fermerSession();
                gSession.IHM.Afficheur.MessageAffiche = "Session fermée."; // TODO: à revoir...
            }



            if (gSession.SimuEstConnecte)
            {
               // gSession.IHM.Afficheur.MessageAffiche = "Session fermée."; // TODO: à revoir...
            }
            

        }
        */
  /// <summary>
  /// Surveiller le MPU maître
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
        private void surveiller_MPU_maitre(GestionSession gSession)
        {
            //Créer un nouveau timer
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            //Tant que la session de formation est ouverte
            while (gSession.sessionEstOuverte)
            //while (etatSession == EnumEtatSession.ouvert)
            {
                //Surveiller si le MPU est toujours Maître
                try
                {
                    if (timer.ElapsedMilliseconds >= 5000) //toutes les 5 secondes
                    {
                        if (!(Boolean)gSession.comObj.LireVariable(gSession.cheminVar_MpuMaitre))
                        {
                            MessageBox.Show("Mode formation interrompu !\n "
                                                       + "Le MPU associé à la session de formation n'est plus l'équipement maître.\n "
                                                       + "L'intégrité des forçages ne peux plus être assuré:\n"
                                                       + "Veuiller redémarrer les équipements concernés ou repréparer le train.\n",
                                                       "Interruption de la session", MessageBoxButton.OK, MessageBoxImage.Stop);

                            gSession.cloturerSession = true;
                        }
                        timer.Restart();
                    }

                }
                catch (SystemException excep)
                {
                    if (excep is System.NullReferenceException)
                    {
                        gSession.connexionPerdue = true;
                    }
                }
            }
        }


        /// <summary>
        /// Surveiller le MPU maître
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MajEchanges(GestionSession gSession)
        {
            //Créer un nouveau timer
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            
            UInt16 compteurDeVie = 0;

            //Tant que la connexion est active
            while (connexionEstOuverte || gSession.etatConnexion == EnumEtatConnexion.interrompu )
            {
                //Mettre à jour les données échangées avec le MPU
                try
                {
                    if (timer.ElapsedMilliseconds >= 5000 || !premiereLecture ) //toutes les 1 secondes
                    {
                        string _status = gSession.comObj.LireStatusConnexion();
                        gSession.lbEtatSimulateurContent = _status;

                        if (_status == "OK")
                        {                        
                            gSession.recVieFill = "Blue";    //indicateur de vie bleu
                            
                            if(gSession.etatConnexion == EnumEtatConnexion.interrompu)
                            {
                                Thread.Sleep(3000);
                                premiereLecture = false;
                            }

                            // à surveiller au démarrage
                            if (premiereLecture == false)
                            {
                                Boolean _train_orientation1 = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_Orientation1);       //Variable permettant de lire l'état de la cabine 1
                                Boolean _train_orientation2 = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_Orientation2);       //Variable permettant de lire l'état de la cabine 2
                                UInt16 _train_compo = (UInt16)gSession.comObj.LireVariable(gSession.cheminVar_compo);              //Variable permettant la lecture du nombre de voiture
                                UInt16 _train_type = (UInt16)gSession.comObj.LireVariable(gSession.cheminVar_type_train);         //Variable permettant la lecture du mode de conduite du train

                                gSession.train_compo = _train_compo;              //nombre de voiture
                                gSession.train_orientation1 = _train_orientation1;       //orientation du train cabine 1 dans le sens de la marche
                                gSession.train_orientation2 = _train_orientation2;       //orientation du train cabine 1 dans le sens de la marche
                                gSession.train_type = _train_type;               //mode de conduite du train
                            }

                            // à surveiller systématiquement:
                            Boolean _train_session_interdite = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_session_interdite);  //Variable permettant de lire l'état de la session: interdite
                            Boolean _train_session_autorisee = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_session_autorisee);  //Variable permettant de lire l'état de la session: autorisée
                            Boolean _train_session_ouverte = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_session_ouverte);    //Variable permettant de lire l'état de la session: ouverte
                            Boolean _train_mpu_maitre = (Boolean)gSession.comObj.LireVariable(gSession.cheminVar_MpuMaitre);          //Variable permettant d'identifier le MPU maître

                            gSession.train_session_interdite = _train_session_interdite;  //état de la session: interdite
                            gSession.train_session_autorisee = _train_session_autorisee;  //état de la session: autorisée
                            gSession.train_session_ouverte = _train_session_ouverte;    //état de la session: ouverte
                            gSession.train_mpu_maitre = _train_mpu_maitre;         //état MPU est maitre
                            
                            premiereLecture = true;

                            //incrémenter et envoyer bit de vie de la session seulement si ouverte
                            if (gSession.sessionEstOuverte) {
                                if (compteurDeVie != 65535)
                                {
                                    compteurDeVie += 1;
                                }
                                else
                                {
                                    compteurDeVie = 0;
                                }
                                if (!comObj.EcrireVariable(cfg.lireVariableBitDeVie(), compteurDeVie)) // RTC TODO mettre à jour toutes les 1 secondes
                                {
                                    gSession.connexionPerdue = true;
                                }
                            }

                            gSession.recVieFill = "Transparent"; //indicateur de vie transparent

                            // si la connexion est rétabli, alors on repasse en statut connecté
                            if (gSession.etatConnexion == EnumEtatConnexion.interrompu)
                            {
                                gSession.etatConnexion = EnumEtatConnexion.connecté;
                            }



                        }
                        /*else
                        {
                            gSession.NbreTentativeReconnexion++;
                            lbNbreTentativeReconnexionContent = gSession.NbreTentativeReconnexion.ToString();
                            gSession.recVieFill = "Orange";    //indicateur de vie orange
                        }*/
                                                
                        timer.Restart();
                    }

                }
                catch (SystemException excep)
                {
                    if (excep is System.NullReferenceException)
                    {
                        gSession.recVieFill = "Red";    //indicateur de vie rouge
                        gSession.connexionPerdue = true;
                        gSession.etatConnexion = EnumEtatConnexion.interrompu; //la connexion est interrompue, tentative de reconnexion

                        gSession.NbreTentativeReconnexion++;
                        lbNbreTentativeReconnexionContent = gSession.NbreTentativeReconnexion.ToString();
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">identifiant entré par l'utilisateur</param>
        /// <param name="pwd">mot de passe entré par l'utilisateur</param>
        /// <returns>vrai lorsque l'authentification est réussi</returns>
        public Boolean authentifier(string id, string pwd)
        {
            string motdepasse = readConfig[id];
            if (motdepasse == null)
            {
                return false;
            }

            /* //En attente de validation de la méthode d'encryptage
            
            byte[] bytesPwd = System.Text.Encoding.Unicode.GetBytes(pwd);                   //Mot de passe entré
            byte[] bytesPepper = System.Text.Encoding.UTF8.GetBytes(readConfig["pepper"]);  //Code d'entropie
            byte[] bytesConfPwd = System.Text.Encoding.UTF8.GetBytes(readConfig[id]);       //Mot de passe encodé dans BDD
            byte[] securedPwd = ProtectedData.Protect(bytesPwd, bytesPepper, DataProtectionScope.CurrentUser);  //Encodage du mot de passe entré

            //--------------------------------------------------------------------
            string[] lines = { Convert.ToBase64String(securedPwd) };
            System.IO.File.WriteAllLines(@"C:\Users\e_gquill\Documents\visual studio 2013\Projects\GestionSession_latest\hash.txt", lines);
            IHM.errorTextBox.Text = Convert.ToBase64String(securedPwd);
            //--------------------------------------------------------------------
            */
            //Compare both keys
            if (pwd == motdepasse)//bytesConfPwd == securedPwd)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Terminer les threads actifs dans le gestionnaire de session
        /// </summary>
        public void terminer()
        {
            //Fermer les threads de surveillance
            if (Thread_Connexion != null)
                Thread_Connexion.Abort();

            if (Thread_surveillance_session != null)
                Thread_surveillance_session.Abort();

            if (Thread_surveillance_MPU_maitre != null)
                Thread_surveillance_MPU_maitre.Abort();

            if (Thread_MajEchanges != null)
                Thread_MajEchanges.Abort();

        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)

                handler(this, new PropertyChangedEventArgs(name));

        }


    }

}


