using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulateur
{
    class Message : Action
    {
        //Attributs
        string contenu = ""; //Contenu textuel du message
        string typeEqt = "";
		string groupLu = "";    //groupe de commande associé à la commande //RTC

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="valeur">Contenu textuel du message</param>
        public Message(string valeur)
        {
            this.contenu = valeur;

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Message";
            this.parametreAction_1 = valeur;
            this.actionExecutee = false;
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="valeur">Contenu textuel du message</param>
        /// /// <param name="typeEqt">Type d'équipement associé au message</param>
        public Message(string valeur, string typeEqt, string groupLu)
        {
            this.typeEqt = typeEqt;
            this.contenu = valeur;
            this.groupLu = groupLu;

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Message";
            this.parametreAction_1 = valeur;
            this.actionExecutee = false;
        }
        /// <summary>
        /// Lire le contenu du message
        /// </summary>
        /// <returns>Contenu textuel du message</returns>
        public string lire()
        {
            return contenu;
        }
        /// <summary>
        /// Permet de lire le type d'équipement lié au message (dans le cas d'un message paramétrable)
        /// </summary>
        /// <returns>Le type d'équipement associé à la commande</returns>
        public string lireEquipement()
        {
            return this.typeEqt;
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
        /// 
        /// </summary>
        public string executer()
        {
            //Afficher message sur l'IHM

            log.Info("Message affiché: " + contenu);
            this.actionExecutee = true;
            return contenu;
        }
    }
}
