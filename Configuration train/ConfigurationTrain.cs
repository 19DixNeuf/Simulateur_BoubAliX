using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;
using System.Windows;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Simulateur
{
    public sealed class ConfigurationTrain
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ConfigurationTrain instance = new ConfigurationTrain();
        string composition;             //Nombre de voiture pour la configuration train actuelle
        string chemin;                  //Chemin vers le dossier contenant les fichiers XML
        short  orientation = 0;         //Indique le numero de l'orientation
        string typeTrain = "1";         //mode de conduite du train
        string varConfTrain;            //Nom de la variable à lire sur MPU pour connaitre la configuration du train
        string varOrientation1;         //Nom de la variable à lire sur MPU pour connaitre si la cabine 1 est active sur train
        string varOrientation2;         //Nom de la variable à lire sur MPU pour connaitre si la cabine 2 est active sur train
        string varMpuMaitre;            //Nom de la variable à lire sur MPU pour connaitre le mpu maitre sur train
        string varCleConfirmation;      //Nom de la variable à lire sur MPU pour connaitre la cle de confirmation via console
        string varModeFormation;        //Nom de la variable à lire sur MPU pour connaitre l'état du mode formation
        string varBitDeVie;             //Nom de la variable à écrire sur MPU indiquant 
        string varTypeTrain;            //Nom de la variable à lire sur MPU pour connaitre le mode du train
        string varSessionInterdite;     //Nom de la variable à lire sur MPU pour savoir si le train est bien initialisé
        string varSessionAuthorisee;    //Nom de la variable à lire sur MPU pour savoir si le train repecte les conditions pour une ouverture de session de formation
        string varSessionOuverte;       //Nom de la variable à lire sur MPU pour savoir si la session de formation est bien initiée
        string varFermerSession;        //Nom de la variable à lire sur MPU pour lancer la fermeture de session de formation
        Boolean estChargee = false;     //Indique que la configuration train est déjà connue

        public List<Train> listeTrain;  //liste des differentes configurations de train

        public List<Voiture> listeVoiture;     //Liste des voitures présentes sur le train
        XDocument xmlConfTrain;         //Instance du fichier XML chargé
        XDocument xmlVoiture;           //Pointeur sur le fichier XML de la voiture courante
        MainWindow IHM;                 //pointeur vers l'IHM

        //List<Voiture> liste_voitures;   //Liste des voitures composant le train actuel
        //Communication comObject = Communication.Instance;   //Récupérer l'interface de communication
        NameValueCollection readConfig = ConfigurationManager.AppSettings;

        static ConfigurationTrain() { }

        private ConfigurationTrain() { }
        /// <summary>
        /// Récupère l'objet ConfigurationTrain (objet unique)
        /// </summary>
        public static ConfigurationTrain Instance
        {
            get
            {
                return instance;
            }
        }
        /// <summary>
        /// Initialise la classe ConfigurationTrain avec les information contenues dans l'XML:
        ///     -l'adresse IP des MPU 1 et 2
        ///     -la variable contenant la configuration train
        ///     -la variable contenant l'identifiant de la cabine active
        /// Note: n'a besoin d'être utiliser qu'une seule fois, ConfigurationTrain étant une classe Unique (Singleton)
        /// </summary>
        public Boolean initialiser()
        {
            //Récupération du pointeur vers l'IHM
            IHM = (MainWindow)Application.Current.MainWindow;
            //Lecture du fichier App.config puis ouverture de l'XML contenant les infos train 
            this.chemin = readConfig["dossierXml"];
            try
            {
                xmlConfTrain = XDocument.Load(chemin + readConfig["ConfigTrainXML"]);
                // Chercher la balise <cible>
                var cible = (from n in xmlConfTrain.Descendants() where n.Name == "cible" select n).First();

                // Lire l'attribut contenant les informations sur la variable qui identifie la composition du train
                varConfTrain = cible.Attribute("composition").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie le type de train (1=conduite CA; 2=conduite CC)
                varTypeTrain = cible.Attribute("type").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie que la cabine 1 est en tête dans le sens de la marche
                varOrientation1 = cible.Attribute("orientation1").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie que la cabine 2 est en tête dans le sens de la marche
                varOrientation2 = cible.Attribute("orientation2").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie que le calculateur est le calculateur maitre
                varMpuMaitre = cible.Attribute("mpu_maitre").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui contient le code d'identification généré par le train
                varCleConfirmation = cible.Attribute("code_confirmation").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie le statut du mode formation
                varModeFormation = cible.Attribute("mode_formation").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui stocke le compteur de vie
                varBitDeVie = cible.Attribute("bit_de_vie").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie qu'il est interdit d'ouvrir une session jusqu'à repréparation du train
                varSessionInterdite = cible.Attribute("session_interdite").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie que le train autorise d'ouvrir une session
                varSessionAuthorisee = cible.Attribute("session_autorisee").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui identifie le statut de la session
                varSessionOuverte = cible.Attribute("session_ouverte").Value.ToString();

                // Lire l'attribut contenant les informations sur la variable qui permet la commande de fermeture de session
                varFermerSession = cible.Attribute("fermer_session").Value.ToString();

                return true;
            }
            catch (Exception exception)
            {
                MessageBoxResult r;
                //Si le fichier est absent: le signaler à l'utilisateur
                if (exception is System.IO.FileNotFoundException)
                {
                    r = MessageBox.Show("Le fichier " + readConfig["ConfigTrainXML"] + " contenant les configurations du train est manquant.",
                        "Fichier manquant", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //
                    r = MessageBox.Show("Un problème est survenu lors de la lecture du fichier "+ readConfig["ConfigTrainXML"] + " contenant les configurations du train.",
                        "Erreur fichier xml", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                log.Error("Erreur lecture fichier ConfigTrainXML.");
                log.Debug("Erreur lecture fichier ConfigTrainXML.", exception);

                return false;
            }


        }
        /// <summary>
        /// Lance le chargement de la configuration correspondant au train connecté
        /// </summary>
        /// <param name="Composition"></param>
        /// <param name="Orientation"></param>
        ///         public void lancerConfiguration(string Composition, short Orientation)
        public void lancerConfiguration(string Composition, short Orientation)
        {
            this.composition = Composition;
            this.orientation = Orientation;
            //fThread = new Thread(new ThreadStart(configurer));
            //fThread.Start();
            configurer();
            estChargee = true;
        }


        /// <summary>
        /// Récupère les fichiers décrivant le contenu des voitures et instancie l'ensemble des Voitures et équipements présents sur le train
        /// </summary>
        public void listerTrain()
        {
            listeTrain = new List<Train>();

            Train _train;

            //Parcourir l'ensemble des configurations
            foreach (XElement xe in xmlConfTrain.Descendants("conf"))
            {
                _train = new Train();

                if (xe.Attribute("composition").Value.ToString() != "")
                {
                    _train.composition = xe.Attribute("composition").Value.ToString();
                }

                if (xe.Attribute("type").Value.ToString() != "")
                {
                    _train.type = xe.Attribute("type").Value.ToString();

                }

                listeTrain.Add(_train);
            }


        }





        /// <summary>
        /// Récupère les fichiers décrivant le contenu des voitures et instancie l'ensemble des Voitures et équipements présents sur le train
        /// </summary>
        private void configurer()
        {
            XElement voiture;
            List<Equipement> listeEquipementSpecifique;
            List<List<Equipement>> listeEquipement = new List<List<Equipement>>();
            listeVoiture = new List<Voiture>();
            Equipement equipement;
            string fichierVoiture = "";

            int voitureNum = 0; //numero de voiture dans la composition

            IHM.GrilleConfig.Children.Clear(); //on efface la composition actuelle

            //Parcourir l'ensemble des configurations
            foreach (XElement xe in xmlConfTrain.Descendants("conf"))
            {
                //Récupérer la bonne config
                if (xe.Attribute("composition").Value.ToString() == composition)
                {
                    short NbreVoiture = 0;
                    bool result = short.TryParse(composition, out NbreVoiture);

                    //Pour chaque fichier lu, instancier la voiture qui lui correspond
                    foreach (XElement xelem in xe.Descendants("voiture"))
                    {
                        try
                        {
                            //Charger la description xml de la voiture
                            fichierVoiture = xelem.Attribute("file").Value.ToString();
                            xmlVoiture = XDocument.Load(chemin + fichierVoiture);
                            voiture = xmlVoiture.Descendants("voiture").First();

                            listeEquipement = new List<List<Equipement>>(); //RTC: ajout pour correction bug liste par réinitialisée à chaque voiture

                            //Créer la liste des équipements
                            foreach (XElement typeEqt in xmlVoiture.Descendants("equipements"))
                            {
                                //créer la liste spécifique au type d'équipements
                                listeEquipementSpecifique = new List<Equipement>();

                                foreach (XElement eqt in typeEqt.Descendants("equipement"))
                                {
                                    if (orientation == 1)
                                    {
                                        //instancie l'équipement et le paramètre (cabine 1 à gauche dans la représentation graphique)
                                        equipement = new Equipement(typeEqt.Attribute("label").Value,
                                                                    typeEqt.Attribute("type").Value,
                                                                    eqt.Attribute("instance").Value,
                                                                    eqt.Attribute("label").Value,
                                                                    eqt.Attribute("x1").Value,
                                                                    eqt.Attribute("y1").Value);
                                        //l'ajoute à la liste d'équipements spécifique
                                        listeEquipementSpecifique.Add(equipement);
                                    }
                                    else if (orientation == 2)
                                    {
                                        //instancie l'équipement et le paramètre (cabine 2 à gauche dans la représentation graphique)
                                        equipement = new Equipement(typeEqt.Attribute("label").Value,
                                                                    typeEqt.Attribute("type").Value,
                                                                    eqt.Attribute("instance").Value,
                                                                    eqt.Attribute("label").Value,
                                                                    eqt.Attribute("x2").Value,
                                                                    eqt.Attribute("y2").Value);
                                        //l'ajoute à la liste d'équipements spécifique
                                        listeEquipementSpecifique.Add(equipement);
                                    }


                                }
                                //Ajoute la liste d'équipements spécifique à la liste globale des équipements
                                listeEquipement.Add(listeEquipementSpecifique);
                            }

                            listeVoiture.Add(new Voiture(voiture.Attribute("name").Value,
                                                        voiture.Attribute("image").Value,
                                                        voiture.Attribute("label").Value,
                                                        voiture.Attribute("instance").Value,
                                                        listeEquipement));
                            //                             
                            //Instancier la Voiture correspondante (en renseignant la liste d'équipements)
                            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                      (System.Threading.ThreadStart)delegate
                                                    {
                            //gestion de l'affichage des voiture à l'IHM
                            voitureNum = listeVoiture.Count()-1;
                            String strAppDir = Environment.CurrentDirectory;

                            //récupération du nom de l'image défini dans <voiture>.xml
                            string imageNom = voiture.Attribute("image").Value;

                            //création et définition du container de l'image
                            Image simpleImage = new Image();

                            if (NbreVoiture > 0 )
                            { simpleImage.Width = Math.Min((1020 / NbreVoiture), 250); } //largeur variable (largeur fenetre / nombre de voiture) avec un maximum
                            else
                            { simpleImage.Width = 160; } //largeur fixe
                            
                            simpleImage.Height = 60; //hauteur max
                            simpleImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; //alignement à gauche dans la grille "grilleConfig"
                            
                            //définition de l'emplacement de l'image dans la grille
                            Thickness myThickness = new Thickness();
                            myThickness.Bottom = 0;
                            myThickness.Left = voitureNum * simpleImage.Width; //à chaque nouvelle voiture, la position de l'image est décalée de la largeur d'une image
                            myThickness.Right = 0;
                            myThickness.Top = 0;

                            simpleImage.Margin = myThickness;

                            BitmapImage bi = new BitmapImage();

                            bi.BeginInit();
                            bi.UriSource = new Uri(@strAppDir + "/images/" + imageNom, UriKind.RelativeOrAbsolute);
                            bi.EndInit();

                            simpleImage.Source = bi;

                            //ajout de l'image à la grille
                            IHM.GrilleConfig.Children.Add(simpleImage);
                        });
                    }
                        catch (Exception e)
                        {
                            MessageBoxResult r;
                            //Si le fichier est absent: le signaler à l'utilisateur
                            if (e is System.IO.FileNotFoundException)
                            {
                                r = MessageBox.Show("Le fichier " + fichierVoiture + " contenant la configuration de la voiture est manquant ou ne respecte pas le formalisme.",
                                    "Erreur du fichier", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                r = MessageBox.Show("Un problème est survenu lors de la lecture du fichier " + fichierVoiture + " contenant la configurations de la voiture.",
                                    "Erreur fichier xml", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            log.Error("Erreur lecture fichier " + fichierVoiture);
                            log.Debug("Erreur lecture fichier " + fichierVoiture, e);
                        }
                    }
                }
            }
            //En fonction de la cabine active, réorienter la liste des voitures
            if (orientation == 2 && listeVoiture.Last().lireNom().Contains("2"))
            {
                listeVoiture.Reverse();
            }

        }

        /// <summary>
        /// Lit le nombre de voiture composant le train dans sa configuration courante
        /// </summary>
        /// <returns>Nombre de voiture</returns>
        /// 
        public string lireComposition()
        {
            return composition;
        }
        /// <summary>
        /// Lit le nom de la variable indiquant la composition du train
        /// </summary>
        /// <returns>Nom de la variable contenant le nombre de voitures dans le train/returns>
        public string lireVariableComposition()
        {
            return varConfTrain;
        }
        /// <summary>
        /// Lit l'identifiant de la cabine active
        /// </summary>
        /// <returns>Numéro de la cabine active</returns>
        public short lireOrientation()
        {
            return orientation;
        }
        /// <summary>
        /// Récupère le nom de la variable indiquant l'état des cabines
        /// </summary>
        /// <returns>Nom de la variable 'Cabine active'</returns>
        public string lireVariableOrientation1Active()
        {
            return varOrientation1;
        }
        /// <summary>
        /// Récupère le nom de la variable indiquant l'état des cabines
        /// </summary>
        /// <returns>Nom de la variable 'Cabine active'</returns>
        public string lireVariableOrientation2Active()
        {
            return varOrientation2;
        }
        /// <summary>
        /// Récupère le nom de la variable indiquant le MPU maître
        /// </summary>
        /// <returns></returns>
        public string lireVariableMpuMaitre()
        {
            return varMpuMaitre;
        }
        /// <summary>
        /// Récupère le nom de la variable indiquant la cle de confirmation pour la console de conduite
        /// </summary>
        /// <returns></returns>
        public string lireVariableCleConfirmation()
        {
            return varCleConfirmation;
        }
        /// <summary>
        /// Récupere la variable contenant l'état du mode formation
        /// </summary>
        /// <returns></returns>
        public string lireVariableModeFormation()
        {
            return varModeFormation;
        }
        public string lireVariableBitDeVie()
        {
            return varBitDeVie;
        }
        /// <summary>
        /// Récupère la liste des voitures composants le train
        /// </summary>
        /// <returns>Liste des voitures</returns>
        public List<Voiture> lireListeVoiture()
        {
            return listeVoiture;//new List<Voiture>
        }
        /// <summary>
        /// Indique si la configuraiont train a déjà été chargée
        /// </summary>
        /// <returns>True lorsque configuration est déjà chargée</returns>
        public Boolean lireEtatConf()
        {
            return this.estChargee;
        }
        /// <summary>
        /// Indique le mode de conduite du train (pour MP14: CA ou CC)
        /// </summary>
        /// <returns></returns>
        public string lireTypeTrain()
        {
            return typeTrain;
        }
        /// <summary>
        /// Récupere la variable permettant de connaître l'état de la session: Interdite
        /// </summary>
        /// <returns></returns>
        public string lireVariableSessionInterdite()
        {
            return varSessionInterdite;
        }
        /// <summary>
        /// Récupere la variable permettant de connaître l'état de la session: Authorisee
        /// </summary>
        /// <returns></returns>
        public string lireVariableSessionAutorisee()
        {
            return varSessionAuthorisee;
        }
        /// <summary>
        /// Récupere la variable permettant de connaître l'état de la session: Ouverte
        /// </summary>
        /// <returns></returns>
        public string lireVariableSessionOuverte()
        {
            return varSessionOuverte;
        }
        /// <summary>
        /// Récupère la variable permettant de fermer la session de formation sur le MPU
        /// </summary>
        /// <returns></returns>
        public string lireVariableFermerSession()
        {
            return varFermerSession;
        }
        /// <summary>
        /// Récupere la variable permettant de connaître le mode de conduite du train
        /// </summary>
        /// <returns></returns>
        public string lireVariableTypeTrain()
        {
            return varTypeTrain;
        }
        /// <summary>
        /// Permet le réglage du mode de conduite du train
        /// </summary>
        /// <param name="mode"></param>
        public void reglerTypeTrain(string mode)
        {
            this.typeTrain = mode;
        }
    }
}
