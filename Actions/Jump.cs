using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulateur
{
    class Jump : Action
    {
        string label;
        string groupLu = "";    //groupe de commande associé à la commande //RTC

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="id">Le numéro du label ciblé</param>
        public Jump(string label, string groupLu)
        {
            this.label = label;
            this.groupLu = groupLu;

            //MàJ des données pour le suivi du scénario
            this.typeAction = "Jump";
            this.parametreAction_1 = "Aller au label '" + label + "'";
            this.actionExecutee = false;
        }
        /// <summary>
        /// Permet la lecture de l'identifiant du label
        /// </summary>
        /// <returns>Le numéro du label</returns>
        public string lireLabel(){
            return label;
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
