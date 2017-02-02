using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Simulateur
{
    public class Action : INotifyPropertyChanged
    {
        string _typeAction;
        string _parametreAction_1;
        Boolean _actionExecutee;
   

        #region PROPRIETEES

        public string typeAction
        {
            get
            {
                return _typeAction;
            }
            set
            {
                //update variable if needed
                if (value != _typeAction)
                {
                    _typeAction = value;
                    RaisePropertyChanged("typeAction");
                }

            }
        }
        public string parametreAction_1
        {
            get
            {
                return _parametreAction_1;
            }
            set
            {
                //update variable if needed
                if (value != _parametreAction_1)
                {
                    _parametreAction_1 = value;
                    RaisePropertyChanged("parametreAction_1");
                }

            }
        }

        public Boolean actionExecutee
        {
            get
            {
                return _actionExecutee;
            }
            set
            {
                //update variable if needed
                if (value != _actionExecutee)
                {
                    _actionExecutee = value;
                    RaisePropertyChanged("actionExecutee");
                }

            }
        }

        private int _iteration;
        public int iteration //RTC
        {
            get
            {
                return _iteration;
            }
            set
            {
                _iteration=value;
                RaisePropertyChanged("iteration"); //RTC
            }
        }
        #endregion

        public void executer() { }
        public void annuler() { }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Event de mise à jour des propriétées sur l'IHM
        /// </summary>
        /// <param name="name"></param>
        private void RaisePropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)

                handler(this, new PropertyChangedEventArgs(name));

        }
    }
}
