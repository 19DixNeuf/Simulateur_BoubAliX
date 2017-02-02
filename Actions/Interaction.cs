using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulateur
{
    class Interaction : Action
    {
        //Attributs
        string titre = "";   //Titre de la fenêtre
        string contenu = ""; //Contenu textuel de la fenêtre
        string groupLu = "";    //groupe de commande associé à la commande //RTC
        string typeEqt = "";  //type d'équipement relatif à l'instanciation
        MessageBoxButton type;    //Type de réponse possible par l'utilisateur (OK, OKCancel, YesNo, YesNoCancel)
        MessageBoxImage icone;   //Type de l'icône affichée
        MessageBoxResult mbResult;  //Réponse reçue suite au clic utilisateur
        string yesLabel = "";         //Label ciblé lors d'un appuie sur bouton Yes ou Ok 
        string noLabel = "";          //Label ciblé lors d'un appuie sur bouton No
        string cancelLabel = "";      //Label ciblé lors d'un appuie sur bouton Cancel

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="titre">Titre de la fenêtre</param>
        /// <param name="contenu">Contenu textuel de la fenêtre</param>
        /// <param name="type">Type de réponse possible par l'utilisateur (OK, OKCancel, YesNo, YesNoCancel)</param>
        /// <param name="icone">Type de l'icône affichée</param>
        public Interaction(string titre, string contenu, string type, string icone, string yes, string no, string cancel, string typeEqt, string groupLu)
        {
            this.titre = titre;
            this.contenu = contenu;
            this.yesLabel = yes;
            this.noLabel = no;
            this.cancelLabel = cancel;
            this.typeEqt = typeEqt;
            this.groupLu = groupLu;
            switch (type)
            {
                case "OK":
                    this.type = MessageBoxButton.OK;
                    break;
                case "OKCancel":
                    this.type = MessageBoxButton.OKCancel;
                    break;
                case "YesNo":
                    this.type = MessageBoxButton.YesNo;
                    break;
                case "YesNoCancel":
                    this.type = MessageBoxButton.YesNoCancel;
                    break;
                default:
                    this.type = MessageBoxButton.OK;
                    break;
            }
            switch (icone)
            {
                case "asterisk":
                    this.icone = MessageBoxImage.Asterisk;
                    break;
                case "error":
                    this.icone = MessageBoxImage.Error;
                    break;
                case "exclamation":
                    this.icone = MessageBoxImage.Exclamation;
                    break;
                case "hand":
                    this.icone = MessageBoxImage.Hand;
                    break;
                case "information":
                    this.icone = MessageBoxImage.Information;
                    break;
                case "none":
                    this.icone = MessageBoxImage.None;
                    break;
                case "question":
                    this.icone = MessageBoxImage.Question;
                    break;
                case "stop":
                    this.icone = MessageBoxImage.Stop;
                    break;
                case "warning":
                    this.icone = MessageBoxImage.Warning;
                    break;
                default:
                    this.icone = MessageBoxImage.None;
                    break;
            }

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Interraction";
            this.parametreAction_1 = contenu;
            this.actionExecutee = false;
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="titre">Titre de la fenêtre</param>
        /// <param name="contenu">Contenu textuel de la fenêtre</param>
        /// <param name="type">Type de réponse possible par l'utilisateur (OK, OKCancel, YesNo, YesNoCancel)</param>
        /// <param name="icone">Type de l'icône affichée</param>
        public Interaction(string titre, string contenu, MessageBoxButton type, MessageBoxImage icone, string yes, string no, string cancel, string typeEqt, string groupLu)
        {
            this.titre = titre;
            this.contenu = contenu;
            this.type = type;
            this.icone = icone;
            this.yesLabel = yes;
            this.noLabel = no;
            this.cancelLabel = cancel;
            this.typeEqt = typeEqt;
            this.groupLu = groupLu;

            //MàJ des données pour le suivi du scénario
            decrireSuivi();//RTC
        }


        private Boolean decrireSuivi()//RTC
        {
            this.parametreAction_1 = "Afficher:\n" + contenu;

            if (this.yesLabel.ToString() != "")
            {
                this.parametreAction_1 = this.parametreAction_1 + "\nSi réponse OK/Oui aller au label '" + this.yesLabel.ToString() + "'";
            }
            if (this.yesLabel.ToString() != "")
            {
                this.parametreAction_1 = this.parametreAction_1 + "'.\nSi réponse Non aller au label '" + this.noLabel.ToString() + "'";
            }
            if (this.yesLabel.ToString() != "")
            {
                this.parametreAction_1 = this.parametreAction_1 + "\nSi réponse Annuler aller au label '" + this.cancelLabel.ToString() + "'";
            }

            this.typeAction = "boite de dialogue";
            this.actionExecutee = false;
            this.iteration = 0;

            return true;
        }

        /// <summary>
        /// Afficher une fenêtre d'interaction avec l'utilisateur
        /// </summary>
        public string executer()
        {
            mbResult = MessageBox.Show(contenu, titre, type, icone);

            //Traiter le résultat des boutons
            switch (mbResult)
            {
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    this.actionExecutee = true;
                    return yesLabel;
                case MessageBoxResult.No:
                    this.actionExecutee = true;
                    return noLabel;
                case MessageBoxResult.Cancel:
                case MessageBoxResult.None:
                    this.actionExecutee = true;
                    return cancelLabel;
                default:
                    return "";
            }
        }
        public string lireTitre()
        {
            return titre;
        }
        /// <summary>
        /// Permet la lecture du message contenu dans la boite de dialogue
        /// </summary>
        /// <returns>Contenu du message</returns>
        public string lireContenu()
        {
            return contenu;
        }
        public MessageBoxImage lireImage()
        {
            return icone;
        }
        public MessageBoxButton lireType()
        {
            return type;
        }
        public string lireLabelYes()
        {
            return yesLabel;
        }
        public string lireLabelNo()
        {
            return noLabel;
        }
        public string lireLabelCancel()
        {
            return cancelLabel;
        }
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
    }
}
