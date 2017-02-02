using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulateur
{
    class Test : Action
    {
        string pattern = "";    //pattern de la variable à lire
        string valeur = "";     //valeur à écrire/forcer sur le MPU
        string groupLu = "";    //groupe de commande associé à la commande //RTC
        string typeEqt = "";    //type d'équipement associé à la commande
        string label = "";      //Label ciblé si la lecture de donnée n'a pas été concluante
        UInt32 duree = 0;       //Durée d'attente maximum pendant laquelle la commande est envoyée
        Boolean execute = false;//Etat du test: exécuté ou non

        System.Diagnostics.Stopwatch timer; //Chronomètre
        Communication comObj = Communication.Instance;  //récupère le lien vers l'interface de communication avec le train
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);//Entrée vers le data logger

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">Variable controlbuild mise en oeuvre</param>
        /// <param name="value">Valeur attendue dans la variable</param>
        /// <param name="timeout">Durée d'attente maximum pendant laquelle la commande est envoyée (en ms)</param>
        /// <param name="label">Label cible lorsque la durée du timeout est écoulée</param>
        public Test(string pattern, string value, UInt32 timeout, string label)
        {
            this.pattern = pattern;
            if ((value == "false") || (value == "FALSE"))
            {
                this.valeur = "False";
            }
            else if ((value == "true") || (value == "TRUE"))
            {
                this.valeur = "True";
            }
            else
            {
                this.valeur = value;
            }
            this.duree = timeout;
            this.label = label;
            timer = new System.Diagnostics.Stopwatch();

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Test";
            this.parametreAction_1 = pattern;
            this.actionExecutee = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern">Variable controlbuild mise en oeuvre</param>
        /// <param name="value">Valeur attendue dans la variable</param>
        /// <param name="timeout">Durée d'attente maximum pendant laquelle la commande est envoyée (en ms)</param>
        /// <param name="typeEqt">Type d'équipement concerné (en cas de commande paramétrable)</param>
        /// <param name="label">Label cible lorsque la durée du timeout est écoulée</param>
        public Test(string pattern, string value, UInt32 timeout, string typeEqt, string label, string groupLu)
        {

            value = value.ToLower();

            //si la valeur à lire est un booleen au format false/true on la convertit au format 0/1

            //si la valeur lue est un booleen au format false/true on la convertit au format 0/1
            switch (value.ToLower())
            {
                case "true":
                    value = "1";
                    break;
                case "false":
                    value = "0";
                    break;
                default:
                    break;
            }

            this.valeur = value;

            this.pattern = pattern;


            this.duree = timeout;
            this.label = label;
            this.typeEqt = typeEqt;
            this.groupLu = groupLu;
            timer = new System.Diagnostics.Stopwatch();

            //MàJ des données pour le suivi du scénario
            decrireSuivi(); //RTC
        }


        private Boolean decrireSuivi()//RTC
        {
            this.typeAction = "Test";
            this.parametreAction_1 = "Test à " + this.valeur.ToString() + "\nchemin: '" + this.pattern.ToString() + "'Si test ok alors continuer.\nSinon aller au label '" + label + "'";
            this.actionExecutee = false;
            this.iteration = 0;
            return true;
        }


        /// <summary>
        /// Lire la donnée tant que la variable n'a pas la valeur attendue ou que la duree prédéfinie ne s'est pas écoulée
        /// </summary>
        /// <returns>le label cible si le test n'a pas été concluant, une chaîne vide sinon</returns>
        public string executer()
        {
            object valeurLu = null;
            timer.Start();
            try
            {
                //tant que la tempo n'est pas écoulée ET variable n'est pas lue: on reboucle
                do
                {
                    valeurLu = comObj.LireVariable(pattern).ToString();
                }
                while (timer.ElapsedMilliseconds < duree && ((valeurLu == null) || (valeurLu.ToString() != valeur)));

                timer.Stop();
                timer.Reset();
                log.Info("Variable " + pattern + " testée: " + valeurLu + ", attendue: " + valeur);
                this.actionExecutee = true;
            }
            catch (Exception exe)
            {
                log.Error("Echec du test: variable " + pattern + ", valeur attendue valeur");
                log.Debug("Echec du test: variable " + pattern + ", valeur attendue valeur", exe);
                return null;
            }
            
            //si la valeur lue est un booleen au format false/true on la convertit au format 0/1
            switch (valeurLu.ToString().ToLower())
            {
                case "true":
                    valeurLu = "1";
                    break;
                case "false":
                    valeurLu = "0";
                    break;
                default:
                    break;
            }

            //-------------------------------------------------------------

            if (valeurLu.ToString() == valeur)
            {
                execute = true;
                return "";
            }else if((valeurLu == null))
            {
                //traitement de l'erreur
                return "err";
            }
            else
            {
                return label;
            }
        }
        /// <summary>
        /// Remet à zéro le status d'exécution du test
        /// </summary>
        public void annuler()
        {
            execute = false;
            this.actionExecutee = false;
        }
        /// <summary>
        /// Lire le nom de la variable appliquée à la commande
        /// </summary>
        /// <returns></returns>
        public string lirePattern()
        {
            return pattern;
        }
        /// <summary>
        /// Permet de lire le type d'équipement lié à la commande (dans le cas d'une commande paramétrable)
        /// </summary>
        /// <returns>Le type d'équipement associé à la commande</returns>
        public string lireEquipement()
        {
            return typeEqt;
        }
        /*méthodes*/
        /// <summary>
        /// Lire le type de commande
        /// </summary>
        /// <returns></returns>
        public string lireGroup()
        {
            return groupLu;
        }
        /// <summary>
        /// Permet la lecture de la durée du test
        /// </summary>
        /// <returns>Durée maximum de la requête</returns>
        public UInt32 lireDuree()
        {
            return duree;
        }
        /// <summary>
        /// Lire la valeur appliquée au test (si il y a lieu)
        /// </summary>
        /// <returns></returns>
        public string lireValeur()
        {
            return valeur;
        }
        /// <summary>
        /// Permet de connaitre le label ciblé en cas de timeout expiré
        /// </summary>
        /// <returns></returns>
        public string lireLabel()
        {
            return label;
        }
    }
}
