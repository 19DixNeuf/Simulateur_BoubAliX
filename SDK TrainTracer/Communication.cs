using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;

namespace Simulateur
{
    public sealed class Communication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Communication instance = new Communication();
        Alstom.eTrain.Services.ExecutionEnvironment FExecutionEnvironment;
        Alstom.eTrain.Services.DashboardService FDashboardService;
        Alstom.eTrain.Services.ControlService FControlService;
        Alstom.eTrain.Access.NetworkAccessor.OnBoardEquipment mpu;
        List<Alstom.eTrain.Access.NetworkAccessor.OnBoardEquipment> FListOnBoardEquipment;

        static Communication() { }

        private Communication() { }
        /// <summary>
        /// Constructeur: Création de l'interface de communication si innexistante.
        /// Si l'objet a déjà été créé, récupération de celui-ci
        /// </summary>
        public static Communication Instance
        {
            get
            {
                return instance;
            }
        }
        /// <summary>
        /// Initialise l'interface de communication avec le train
        /// </summary>
        /// <param name="repertoire">Répertoire contenant le dossier de l'ETR</param>
        /// <param name="dossier">Dossier contenant l'ETR</param>
        /// <param name="version">Verion de l'ETR</param>
        public void init(string repertoire, string dossier, string version)
        {

            try
            {
                
                Version versionETR = new System.Version(version); // RTC: si balise version pas définie dans app.config alors exception TODO --> à gérer

                // Récupération du projet TrainTracer (via fichier.etr)
                FExecutionEnvironment = new Alstom.eTrain.Services.ExecutionEnvironment(repertoire, dossier, versionETR);

                if (FExecutionEnvironment == null)
                {
                    MessageBoxResult result = MessageBox.Show("Echec de l'initialisation: vérifier la présence du fichier .etr !\n",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Stop);
                    log.Error("Echec de l'initialisation de l'environnement.");
                }
                else
                {
                    FDashboardService = new Alstom.eTrain.Services.DashboardService(FExecutionEnvironment, null);
                }
                FControlService = new Alstom.eTrain.Services.ControlService(FExecutionEnvironment, null);

            }
            catch (Exception e)
            {
                log.Error("Echec lors de l'initialisation de la connexion.");
                log.Debug("Echec lors de l'initialisation de la connexion.",e);
            }
            

        }
        /// <summary>
        /// Connecter le logiciel au réseau train
        /// </summary>
        /// <param name="ipAddress">@IP locale pour créer le socket</param>
        /// <returns></returns>
        public Boolean Connecter(string ipAddress)
        {

            // TODO: vérifier si presence d'un reseau
            if (FExecutionEnvironment == null)
            {
                return false;
            }
            //Démarrage du service de contrôle
            if (FControlService == null)
            {
                FControlService = new Alstom.eTrain.Services.ControlService(FExecutionEnvironment, null);
            }
            //Lancement de la connexion (et des services précédemment attaché à l'environnement)
            FExecutionEnvironment.Connect(ipAddress);
            //Lire la liste des cibles disponibles
            FListOnBoardEquipment = FExecutionEnvironment.GetTargetsInConnectionScope();
            return FExecutionEnvironment.Connected;
        }
        /// <summary>
        /// Déconnecter: ferme le socket de la connexion
        /// </summary>
        /// <returns></returns>
        public Boolean Deconnecter()
        {
            if (FExecutionEnvironment.Connected) { FExecutionEnvironment.Disconnect(); }
            return !FExecutionEnvironment.Connected;
        }
        /// <summary>
        /// Lire une variable
        /// </summary>
        /// <param name="nomVariable"></param>
        /// <returns></returns>
        public object LireVariable(string nomVariable)
        {
            object resultat;
            try
            {
                resultat = FDashboardService.ReadVariable(mpu.DeviceName + nomVariable);
                return resultat;
            }
            catch (System.Exception excep)
            {
                log.Error("Lecture impossible: " + nomVariable);
                log.Debug("Lecture impossible.", excep);
                return null;
            }

        }
        /// <summary>
        /// Ecrire dans une variable
        /// </summary>
        /// <param name="nomVariable"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Boolean EcrireVariable(string nomVariable, object value)
        {
            try
            {
                FDashboardService.WriteVariable(mpu.DeviceName + nomVariable, value);
            }
            catch (Exception excep)
            {
                log.Error("Ecriture impossible: " + nomVariable + " " + value.ToString());
                log.Debug("Ecriture impossible.", excep);

                return false;
            }

            return true;
        }
        /// <summary>
        /// Forcer une variable
        /// </summary>
        /// <param name="nomVariable"></param>
        /// <param name="valeur"></param>
        /// <returns></returns>
        public Boolean ForcerVariable(string nomVariable, object valeur)
        {
            try
            {
                FDashboardService.ForceVariable(mpu.DeviceName + nomVariable, valeur);
            }
            catch (Exception excep)
            {
                log.Error("Forçage impossible: " + nomVariable + " " + valeur.ToString());
                log.Debug("Forçage impossible.", excep);

                return false;
            }
            return true;
        }
        /// <summary>
        /// Relacher une variable
        /// </summary>
        /// <param name="nomVariable"></param>
        /// <returns></returns>
        public Boolean RelacherVariable(string nomVariable)
        {
            try
            {
                FDashboardService.ReleaseVariable(mpu.DeviceName + nomVariable);
            }
            catch (Exception excep)
            {
                log.Error("Déforçage impossible: " + nomVariable);
                log.Debug("Déforçage impossible.", excep);

                return false;
            }
            return true;
        }
        /// <summary>
        /// Lecture de la version logicielle
        /// </summary>
        /// <returns>Le numéro de version du logiciel embarqué sur la cible</returns>
        public string LireVersionLogicielMPU()
        {
            string version = "";
            try
            {
                version = FControlService.ReadGlobalSoftwareIdentification(mpu).SwPackageVersion;
            }
            catch (Exception exception)
            {
                /*MessageBoxResult result = MessageBox.Show("Erreur: " + exception.Message + "\n"
                                        + "La lecture de la version logicielle à échouée.\n",
                                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Stop);*/
                log.Error("La lecture de la version logicielle à échouée.");
                log.Debug("La lecture de la version logicielle à échouée.", exception);

            }

            return version;
        }
        /// <summary>
        /// Recherche le mpu maitre parmi les équipements disponibles
        /// </summary>
        /// <param name="variable">Chemin + Nom de la variable à lire pour connaitre le mpu maître</param>
        /// <returns>True lorsque le mpu maitre a été identifié, false sinon</returns>
        public Boolean rechercherMpuMaitre(string _cheminVar_MpuMaitre)
        {
            //réupération de la liste des équipements configurés dans l'ETR (settings.xml) -> en général 2 équipements type MPU/MCE
            int nbEqt = FListOnBoardEquipment.Count;

            int NbreMpuTrouve = 0;      //Compteur de MPU trouvé
            MessageBoxResult mbResult;

            //Créer un nouveau timer
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            //Chercher le MPU maitre dans la liste
            //pour chaque équipement dans la liste
            foreach (Alstom.eTrain.Access.NetworkAccessor.OnBoardEquipment eqt in FListOnBoardEquipment)
            {
                string _status="";
                //Vérifier s'il s'agit du MPU maître
                timer.Restart(); //on démarre un timer pour timeout

                try
                {
                    Boolean _mpu_ok = false;

                    do
                    {
                        _status = FExecutionEnvironment.GetTargetStatus(eqt).ToString();

                        if(_status == "OK") // le MPU répond présent
                        {
                            mpu = eqt;
                            _mpu_ok = true;

                        }
                        
                    }
                    while (!_mpu_ok && timer.ElapsedMilliseconds < 10000);
                    
                    // si le MPU est présent alors on vérifie s'il est maître                    
                    if (_mpu_ok)
                    {

                        //Lire le statut "est maitre" dans le MPU
                        Boolean varTemp = (Boolean)LireVariable(_cheminVar_MpuMaitre);
                        // Boolean varTemp = (Boolean)FDashboardService.ReadVariable(eqt.DeviceName + _cheminVar_MpuMaitre);
                        if (varTemp == true)
                        {
                            mpu = eqt; //retourne l'équipement identifié comme étant le MPU maitre
                            log.Info(mpu.DeviceName + " est identifié comme étant le MPU maitre.");
                            return true;
                        }
                        else
                        {
                            NbreMpuTrouve++; //flag indiquant qu'au moins un MPU a été trouvé
                        }

                    }


                }
                catch (Exception excep)
                {
                    log.Debug("Aucun MPU n'a été trouvé.", excep);
                }
            }
            if (NbreMpuTrouve > 0)
            {
                //Afficher: mpu trouvé mais pas de maitre.
                mbResult = MessageBox.Show( NbreMpuTrouve + " MPU(s) présent(s) mais pas de maître identifié\n "
                                           , "Démarage de session", MessageBoxButton.OK, MessageBoxImage.Stop);
                log.Error(NbreMpuTrouve + "MPU(s) présent(s) mais pas de maître identifié.");
            }
            else
            {
                //Afficher: Aucun MPU disponible
                mbResult = MessageBox.Show("Aucun MPU n'a été trouvé.\n "
                                   , "Connexion", MessageBoxButton.OK, MessageBoxImage.Stop);
                log.Error("Aucun MPU n'a été trouvé.");
                //log.Debug("Aucun MPU n'a été trouvé.", excep);
            }

            return false;
        }
        /// <summary>
        /// Vérifie l'intégritée de la connexion avec la cible
        /// </summary>
        /// <returns>Status de la connexion</returns>
        public string LireStatusConnexion()
        {
            return FExecutionEnvironment.GetTargetStatus(mpu).ToString();
        }

    }
}
