using System;
using System.Windows.Input;
//using NLog;

namespace VP_RegApplication.MVVM
{
    /// <summary>
    /// MainViewModel
    /// </summary>
    public class MainViewModel
    {
        //TODO:
        //Implement logger later
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Info = new RegisterInfo()
            {
                StartDateTime = new DateTime(day:20, month: 6, year: 2017),
                EndDateTime = new DateTime(day: 20, month: 7, year: 2017),
                DateOfBirth = new DateTime(day: 9, month: 10, year: 1985),
                FirstName = "First Name",
                LastName = "Last Name",
                Passport = "AA123123",
                Email = "test@test.com",
                PhoneNumber = "380123123123",
            };
        }

        private ICommand _connect;

        /// <summary>
        /// Connect command
        /// </summary>
        public ICommand ConnectCommand
        {
            get
            {
                if (_connect == null)
                    _connect = new RelayCommand(RegisterCommand_Execute, RegisterCommand_CanExecute);
                return _connect;
            }
            set { _connect = value; }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        public void RegisterCommand_Execute()
        {
            //logger.Info("Create Navigator");
            var navigator = new VPNavigator(Info);
            //logger.Info("Run connection");
            navigator.InitializeAndStart();
        }

        /// <summary>
        /// Returns true if command can be executed
        /// </summary>
        /// <returns>True if personal information if filled</returns>
        public bool RegisterCommand_CanExecute()
        {
            return Info.IsFilled;
        }

        /// <summary>
        /// Propersty that holds person info
        /// </summary>
        public RegisterInfo Info { get; set; }
    }
}
