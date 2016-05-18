using System;
using System.Windows;
using System.Windows.Input;

namespace VP_RegApplication
{
    public class Presenter
    {
        private ICommand _connect;

        public ICommand ConnectCommand
        {
            get
            {
                if (_connect == null)
                    _connect = new Connect();
                return _connect;
            }
            set { _connect = value; }
        }

        private class Connect : ICommand
        {
              
            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            { 
                var navigator = new VPNavigator();

                try
                {
                    navigator.InitializeConnection();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            #endregion

        }

    }
}
