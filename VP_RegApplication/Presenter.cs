using System;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace VP_RegApplication
{
    public class Presenter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public Presenter()
        {
            Info = new RegisterInfo()
            {
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
                DateOfBirth = DateTime.Now,
                FirstName = string.Empty,
                LastName = string.Empty,
                Passport = string.Empty,
                Email = string.Empty,
                PhoneNumber = string.Empty
            };
        }

        private ICommand _connect;
        public ICommand ConnectCommand
        {
            get
            {
                if (_connect == null)
                    _connect = new Connect(RegisterCommand_Execute, RegisterCommand_CanExecute);
                return _connect;
            }
            set { _connect = value; }
        }

        public void RegisterCommand_Execute()
        {
            logger.Info("Create Navigator");
            var navigator = new VPNavigator(Info);

            try
            {
                logger.Info("Run connection");
                navigator.InitializeConnection();
            }
            catch (Exception e)
            {
                logger.Error(e);
                MessageBox.Show(e.Message);
            }
        }

        public bool RegisterCommand_CanExecute()
        {
            return Info.IsFilled;
        }

        public RegisterInfo Info { get; set; }

        public class RegisterInfo
        {
            public DateTime StartDateTime { get; set; }
            public DateTime EndDateTime { get; set; }
            public DateTime DateOfBirth { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Passport { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }

            public bool IsFilled
            {
                get
                {
                    // ReSharper disable once ConvertPropertyToExpressionBody
                    return DateTime.Compare(StartDateTime, EndDateTime) < 0
                           && Email != string.Empty
                           && FirstName != string.Empty
                           && LastName != string.Empty
                           && Passport != string.Empty
                           && PhoneNumber != string.Empty;
                }
            }
        }

        private class Connect : ICommand
        {
            #region ICommand Members

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            private Action methodToExecute;
            private Func<bool> canExecuteEvaluator;

            public Connect(Action methodToExecute, Func<bool> canExecuteEvaluator)
            {
                this.methodToExecute = methodToExecute;
                this.canExecuteEvaluator = canExecuteEvaluator;
            }

            public Connect(Action methodToExecute)
                : this(methodToExecute, null) { }
             
            public bool CanExecute(object parameter)
            {
                if (this.canExecuteEvaluator == null)
                {
                    return true;
                }
                else
                {
                    bool result = this.canExecuteEvaluator.Invoke();
                    return result;
                }
            }

            public void Execute(object parameter)
            {
                this.methodToExecute.Invoke();
            }
            #endregion

        }

    }
}
