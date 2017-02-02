using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simulateur
{
    public class Equipement
    {
        //Caratéristiques fonctionnelles de l'objet
        string labelGroupe; // label du groupe d'équipement
        string type;            //Type de l'équipement (cvs/brake/porte...)
        //Voiture voiture;      //Localisation de l'équipement sur le train  /!\ Pas nécessaire à priori
        string instance;        //Instance à insérer dans le pattern des listeCommandes

        //Caractéristiques graphique de l'objet
        string label;            //Nom affiché sur l'IHM
        Int32 position_x;        //Position x de l'équipement dan la voiture  
        Int32 position_y;        //Position y de l'équipement dan la voiture

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Id"></param>
        /// <param name="Label"></param>
        /// <param name="Position_X"></param>
        /// <param name="Position_Y"></param>
        public Equipement(string _labelGroupe, string _Type, string _Id, string _Label, string _Position_X, string _Position_Y)
        {
            this.labelGroupe = _labelGroupe;
            this.type = _Type;
            instance = _Id;
            label = _Label;
            if (!Int32.TryParse(_Position_X, out position_x))
            {   //Afficher message d'erreur
                log.Error("Le champ \"x\" correspondant à l'équipement " + type + " " + instance + " est erroné!\n");
            }
            if (!Int32.TryParse(_Position_Y, out position_y))
            {   //Afficher message d'erreur
                log.Error("Le champ \"y\" correspondant à l'équipement " + type + " " + instance + " est erroné!\n");
            }
        }
        /// <summary>
        /// Récupère le type d'équipement
        /// </summary>
        /// <returns>type: indique la nature de l'équipement (porte, cvs, traction...)</returns>
        public string lireType()
        {
            return type;
        }
        /// <summary>
        /// Récupère l'instance correspondant à l'équipement 
        /// </summary>
        /// <returns>instance: la chaîne de caractères à intégrer dans le nom de la variable paramétrable</returns>
        public string lireInstance()
        {
            return instance;
        }
        /// <summary>
        /// Récupère le label correspondant à la représentation graphique du groupe auquel apartient l'équipement
        /// </summary>
        /// <returns>labelGroup: la chaine de caractère à afficher sur l'IHM</returns>
        public string lireLabelGroupe()
        {
            return labelGroupe;
        }
        /// <summary>
        /// Récupère le label correspondant à la représentation graphique de l'équipement
        /// </summary>
        /// <returns>label: la chaine de caractère à afficher sur l'IHM</returns>
        public string lireLabel()
        {
            return label;
        }
        /// <summary>
        /// Récupère la position de l'équipement dans la voiture suivant l'axe X
        /// </summary>
        /// <returns>Position X de l'équipement dans la représentation de la voiture</returns>
        public Int32 lirePosition_X()
        {
            return position_x;
        }
        /// <summary>
        /// Récupère la position de l'équipement dans la voiture suivant l'axe Y
        /// </summary>
        /// <returns>Position Y de l'équipement dans la représentation de la voiture</returns>
        public Int32 lirePosition_Y()
        {
            return position_y;
        }
    }
}
