using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulateur
{

    public class Train
    {
        private string _description; //description (nombre de voiture + type)
        public string description
        {
            get { return _composition + " voitures de type " + type; }
            set {}
        }

        private string _composition; //composition (nombre de voiture)
        public string composition
        {
            get { return _composition; }
            set { _composition = value; }
        }

        private string _type;          // type de train (conduite CA/CC)
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

    }

        public class Voiture
    {
        string nom;             //Nom de la voiture
        string image;           //Description de la voiture
        string label;           //Label de la voiture (chaine figurant dans l'IHM)
        string instance;        //Instance de la voiture (chaine de caratères renseignée dans les variables configurables)
        List<List<Equipement>> listeEquipements;    //Liste de l'ensemble des équipements composant la voiture
        /// <summary>
        /// Constructeur
        /// </summary>
        public Voiture(string Nom, string Image, string Label, string Instance, List<List<Equipement>> ListeEquipements)
        {
            //Enregistrer les paramètres
            this.nom = Nom;
            this.image = Image;
            this.label = Label;
            this.instance = Instance;
            this.listeEquipements = ListeEquipements;
        }
        /// <summary>
        /// Lit le nom de la voiture
        /// </summary>
        /// <returns>Nom de la voiture</returns>
        public string lireNom()
        {
            return nom;
        }
        /// <summary>
        /// Lit le nom du fichier image correspondant à la voiture
        /// </summary>
        /// <returns>La description sous forme de string</returns>
        public string lireImage()
        {
            return image;
        }
        /// <summary>
        /// Récupère le label correspondant à la représentation graphique de la voiture 
        /// </summary>
        /// <returns>label: la chaine de caractère à afficher sur l'IHM </returns>
        public string lireLabel()
        {
            return label;
        }
        /// <summary>
        /// Récupère l'instance correspondant à la voiture 
        /// </summary>
        /// <returns>instance: la chaîne de caractères à intégrer dans le nom de la variable paramétrable</returns>
        public string lireInstance()
        {
            return instance;
        }
        /// <summary>
        /// Récupère la liste des équipements appartenant à la catégorie renseignée en paramètre
        /// </summary>
        /// <param name="Type">Le type d'équipement recherché</param>
        /// <returns>La liste des équipements correspondant au type renseigné</returns>
        public List<Equipement> lireEquipementSpecifique(string Type)
        {
            foreach (List<Equipement> lEqt in listeEquipements)
            {
                //Si le premier élément est du type voulu
                if (lEqt.First().lireType() == Type)
                {
                    //retourner la liste
                    return lEqt;
                }
            }
            return null;
        }
    }
}
