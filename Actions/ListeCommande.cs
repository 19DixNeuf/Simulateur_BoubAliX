using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulateur
{
    class ListeCommande : Action
    {
        //Attributs
        string type;    //type de la commande: ecriture/lecture/forçage/libération de variable
        string pattern; //pattern de la commande: chaine de caratères contenant la commande à instancier
        string valeur = "";   //valeur à écrire/forcer sur le MPU
        string typeEquipement="";   //type d'équipement concerné par la commande

        string groupLu = "";    //groupe de commande associé à la commande //RTC

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="type">Type de commande à envoyer: écriture, lecture, forçage, déforçage</param>
        /// <param name="pattern">variable controlbuild mise en oeuvre</param>
        /// <param name="typeEqt">Type d'equipement concerné par la commande</param>
        public ListeCommande(string type, string pattern, string typeEqt)
        {
            this.type = type;
            this.pattern = pattern;
            this.typeEquipement=typeEqt;
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="type">Type de commande à envoyer: écriture, lecture, forçage, déforçage</param>
        /// <param name="pattern">Variable mise en oeuvre par la commande</param>
        /// <param name="typeEqt">Type d'equipement concerné par la commande</param>
        /// <param name="value">Valeur à écrire ou forcer dans la variable</param>
        public ListeCommande(string type, string pattern, string typeEqt, string value)
        {
            this.type = type;
            this.pattern = pattern;
            this.typeEquipement = typeEqt;
            this.valeur = value;
        }

        /// <summary>
        /// Lire le type de commande à instancier
        /// </summary>
        /// <returns></returns>
        public string lireType()
        {
            return type;
        }
        /// <summary>
        /// Lire la variable à instancier
        /// </summary>
        /// <returns></returns>
        public string lirePattern()
        {
            return pattern;
        }
        /// <summary>
        /// Lire la valeur à appliquer à la variable (si il y a lieu)
        /// </summary>
        /// <returns></returns>
        public string lireValeur()
        {
            return valeur;
        }
        /// <summary>
        /// Lire le type d'équipement concerné par la commande instanciée
        /// </summary>
        /// <returns></returns>
        public string lireEquipement()
        {
            return typeEquipement;
        }        /*méthodes*/
        /// <summary>
        /// Lire le type de commande
        /// </summary>
        /// <returns></returns>
        public string lireGroup()
        {
            return groupLu;
        }
    }
    /*
    {
        List<Commande> listeCmd;
        List<Commande> historique;
        MainWindow IHM;

        public ListeCommande(List<Commande> l) 
        {
            this.IHM = (MainWindow)Application.Current.MainWindow;
            listeCmd = l;
            historique = new List<Commande>();
        }
        public Boolean executer(List<Equipement> eqtListe) 
        {
            Commande cmdTemp;
            //Vérifie si les listes ont bien été instanciées
            if ((eqtListe == null) || (listeCmd == null)) return false;
            
            //Prépare et envoie les commandes à chaque équipement sélectionné
            foreach(Equipement eqt in eqtListe)
            {
                foreach (Commande cmd in listeCmd) 
                {
                    //Instanciation de la commande suivant le type (read/write/force/release)
                    switch (cmd.getType())
                    {
                        case "read":
                            cmdTemp = new Commande(cmd.getType(), cmd.getPattern());
                            break;
                        case "write":
                            cmdTemp = new Commande(cmd.getType(), cmd.getPattern(),cmd.getValue()); 
                            break;
                        case "force":
                            cmdTemp = new Commande(cmd.getType(), cmd.getPattern(), cmd.getValue());
                            break;
                        case "release":
                            cmdTemp = new Commande(cmd.getType(), cmd.getPattern());
                            break;
                        default:
                            //afficher message
                            cmdTemp = new Commande(cmd.getType(), cmd.getPattern(), cmd.getValue());
                            IHM.TB1.Text += "Commande non reconnue:\n" + cmd.getType() + "\n" + cmd.getPattern() + "\n" + cmd.getValue() + "\n";
                            break;
                    }
                    //Modification de la chaine de caractères pour y insérer les informations voiture/équipement
                    if(cmdTemp != null)
                    {
                        string pattern = cmdTemp.getPattern();
                        //Tester si voiture est à identifier et inserer
                        if (pattern.Contains("[v]"))
                        {
                            pattern = pattern.Replace("[v]", eqt.lireVoiture());
                            cmdTemp.setPattern(pattern);
                        }
                        //Tester si équipement est à identifier et inserer
                        if (pattern.Contains("[i]"))
                        {
                            pattern = pattern.Replace("[i]", eqt.lireIdentifiant());
                            cmdTemp.setPattern(pattern);
                        }
                        //Tester si cabine est à identifier et inserer
                        if (pattern.Contains("[c]"))
                        {
                            pattern = pattern.Replace("[c]", eqt.lireIdentifiant());
                            cmdTemp.setPattern(pattern);
                        }
                        //Envoyer la commande
                        cmdTemp.Executer();
                        historique.Add(cmdTemp);
                    }
                }
            }
            return true;
        }
        public List<Commande> lireListe() 
        {
            return listeCmd;
        }
        public List<Commande> lireHistorique() 
        {
            return historique;
        }
        public void reinitialiser()
        {

        }

    }*/
}
