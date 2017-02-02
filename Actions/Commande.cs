using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulateur
{
    class Commande : Action
    {
        /*Attributs*/
        string type;    //type de la commande: ecriture/lecture/forçage/libération de variable
        string pattern; //pattern de la commande: chaine de caratères contenant la commande à envoyer
        string valeur = "";   //valeur à écrire/forcer sur le MPU
        string groupLu = "";    //groupe de commande associé à la commande //RTC
        string typeEqt = "";//type d'équipement associé à la commande
        Communication comObj = Communication.Instance;  //récupère le lien vers l'interface de communication avec le train
        Boolean executee = false;//Etat de la commande: envoyée ou non
        object valeurInitiale;//Valeur sur train avant envoi d'une commande de type écriture ou forçage
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);//Entrée vers le data logger



        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="type">Type de commande à envoyer: écriture, lecture, forçage, déforçage</param>
        /// <param name="pattern">variable controlbuild mise en oeuvre</param>
        public Commande(string type, string pattern)
        {
            this.type = type;
            this.pattern = pattern;
            //MàJ des données pour le suivi du scénario
            this.typeAction = type;
            this.parametreAction_1 = pattern;
            this.actionExecutee = false;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="type">Type de commande à envoyer: écriture, lecture, forçage, déforçage</param>
        /// <param name="pattern">variable controlbuild mise en oeuvre</param>
        /// <param name="value">valeur à écrire ou forcer dans la variable</param>
        /// <param name="typeEqt">type d'équipement concerné (en cas de commande paramétrable)</param>
        public Commande(string type, string pattern, string value, string typeEqt, string groupLu)
        {
            this.type = type;
            this.pattern = pattern;
            this.valeur = value;
            this.groupLu = groupLu;
            this.typeEqt = typeEqt;
            //MàJ des données pour le suivi du scénario
            this.typeAction = type;
            this.parametreAction_1 = pattern;
            this.actionExecutee = false;
            decrireSuivi(); //RTC
        }


        private Boolean decrireSuivi()//RTC
        {
            this.typeAction = type;

            if(this.typeAction == "force")
            {
                this.parametreAction_1 = "Force à " + this.valeur.ToString() + "\nchemin: '" + this.pattern.ToString() + "'";
            }
            else if (this.typeAction == "write")
            {
                this.parametreAction_1 = "Ecrit à " + this.valeur.ToString() + "\nchemin: '" + this.pattern.ToString() + "'";
            }
            else if (this.typeAction == "release")
            {
                this.parametreAction_1 = "Relache forcage" + "\nchemin: '" + this.pattern.ToString() + "'";
            }

            this.actionExecutee = false;
            this.iteration = 0;
            return true;
        }

        /*méthodes*/
        /// <summary>
        /// Lire le type de commande
        /// </summary>
        /// <returns></returns>
        public string lireType()
        {
            return type;
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
        /// Lire le nom de la variable appliquée à la commande
        /// </summary>
        /// <returns></returns>
        public string lirePattern()
        {
            return pattern;
        }
        /// <summary>
        /// Lire la valeur appliquée à la commande (si il y a lieu)
        /// </summary>
        /// <returns></returns>
        public string lireValeur()
        {
            return valeur;
        }
        /// <summary>
        /// Permet de lire le type d'équipement lié à la commande (dans le cas d'une commande paramétrable)
        /// </summary>
        /// <returns>Le type d'équipement associé à la commande</returns>
        public string lireEquipement()
        {
            return typeEqt;
        }
        /// <summary>
        /// Lire l'état de la commmande
        /// </summary>
        /// <returns>True lorsque la commande a été envoyée, False sinon</returns>
        public Boolean lireEtat()
        {
            return executee;
        }
        /// <summary>
        /// Envoyer la commande au train
        /// </summary>
        public void executer()
        {
            //Envoyer Commande
            int entier;
            Boolean commandeEnvoyee = false;
            switch (type)
            {
                case "write":
                    //sauvegarder l'ancienne valeur
                    valeurInitiale = comObj.LireVariable(pattern);
                    //ecrire variable 
                    if ((valeur == "true") || (valeur == "false"))
                    {
                        commandeEnvoyee = comObj.EcrireVariable(pattern, valeur == "true");
                    }
                    else if (Int32.TryParse(valeur, out entier))
                    {
                        commandeEnvoyee = comObj.EcrireVariable(pattern, entier);
                    }
                    else
                    {
                        commandeEnvoyee = comObj.EcrireVariable(pattern, valeur);
                    }
                    if (commandeEnvoyee)
                    {
                        log.Info("Variable " + pattern + " forcée: " + valeur);
                    }
                    break;
                case "force":
                    //sauvegarder l'ancienne valeur
                    valeurInitiale = comObj.LireVariable(pattern);
                    //forcer variable 
                    if ((valeur == "true") || (valeur == "false"))
                    {
                        commandeEnvoyee = comObj.ForcerVariable(pattern, valeur == "true");
                    }
                    else if (Int32.TryParse(valeur, out entier))
                    {
                        commandeEnvoyee = comObj.ForcerVariable(pattern, entier);
                    }
                    else
                    {
                        commandeEnvoyee = comObj.ForcerVariable(pattern, valeur);
                    }
                    if (commandeEnvoyee)
                    {
                        log.Info("Variable " + pattern + " forcée: " + valeur);
                    }

                    break;
                case "release":
                    commandeEnvoyee = comObj.RelacherVariable(pattern);
                    if (commandeEnvoyee)
                    {
                        log.Info("Variable " + pattern + " relachée ");
                    }

                    break;
                default:
                    //Log message
                    log.Warn("Commande non reconnue: " + type + "\n" + pattern + "\n" + valeur + "\n");
                    break;
            }

            if (commandeEnvoyee)
            {
                executee = true;
                this.actionExecutee = true;
            }
        }
        /// <summary>
        /// Annuler l'effet de la commande sur l'équipement train
        /// </summary>
        public Boolean annuler()
        {
            Boolean cmdResult = false;
            //Si la commande est de type écrire
            if (type == "write")
            {
                cmdResult = comObj.EcrireVariable(pattern, valeurInitiale);
                executee = false;
                this.actionExecutee = false;
            }
            //Sinon si la commande est de type forçage
            else if (type == "force")
            {
                cmdResult = comObj.RelacherVariable(pattern) || comObj.EcrireVariable(pattern, valeurInitiale);
                executee = false;
                this.actionExecutee = false;
            }
            return cmdResult;
        }
    }
}
