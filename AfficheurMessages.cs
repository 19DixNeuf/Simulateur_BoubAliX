using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Simulateur
{
    public class AfficheurMessages : INotifyPropertyChanged
    {

        private string _texteConsole;
        public string texteConsole
        {
            get
            {
                return _texteConsole;
            }
            set
            {
                _texteConsole = value;
                RaisePropertyChanged("texteConsole");
            }
        }

        private string _messageAffiche;
        public string MessageAffiche
        {
            get
            {
                return _messageAffiche;
            }
            set
            {
                _messageAffiche = value;
                texteConsole = value;
                RaisePropertyChanged("MessageAffiche");
                //TODO: gérer les files de messages
            }
        }

        public AfficheurMessages()
        {
            _messageAffiche = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string name)
        {

            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)

                handler(this, new PropertyChangedEventArgs(name));

        }
    }
}
