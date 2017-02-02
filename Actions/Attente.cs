using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulateur
{
    class Attente : Action
    {
        UInt32 duree;   //Durée d'attente prévu
        System.Diagnostics.Stopwatch timer; //Chronomètre
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="duree">Durée d'attente (en millisecondes)</param>
        public Attente(UInt32 duree)
        {
            this.duree = duree;
            timer = new System.Diagnostics.Stopwatch();
            //MàJ des données pour le suivi du scénario
            this.typeAction = "Attente";
            this.parametreAction_1 = duree.ToString();
            this.actionExecutee = false;
        }
        /// <summary>
        /// Attendre tant que la duree prédéfinie ne s'est pas écoulée
        /// </summary>
        public void executer()
        {
            timer.Start();
            //tant que la tempo n'est pas écoulée on reboucle
            while (timer.ElapsedMilliseconds < duree) { }
            timer.Stop();
            timer.Reset();
            this.actionExecutee = true;
        }
    }
}
