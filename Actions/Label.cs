using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulateur
{
    class Label : Action
    {
        string id;
        string groupLu = "";    //groupe de commande associé à la commande //RTC
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="id">Le numéro du label</param>
        public Label(string id, string groupLu)
        {
            this.id = id;
            this.groupLu = groupLu;

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Label";
            this.parametreAction_1 = id;
            this.actionExecutee = false;
		}
        /// <summary>
        /// Permet la lecture de l'identifiant du label
        /// </summary>
        /// <returns>Le numéro du label</returns>
        public string lireLabel(){
            return id;
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
