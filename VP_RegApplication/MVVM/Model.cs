using System;

namespace VP_RegApplication.MVVM
{
    /// <summary>
    /// Class that holds personal info and search conditions
    /// </summary>
    public class RegisterInfo
    {
        #region Search coditions
        /// <summary>
        /// Earliest selection date
        /// </summary>
        public DateTime StartDateTime { get; set; }
        
        /// <summary>
        /// Latest selection date
        /// </summary>
        public DateTime EndDateTime { get; set; }
        #endregion

        #region Personal data
        /// <summary>
        /// Date of birth
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Id number
        /// </summary>
        public string Passport { get; set; }

        /// <summary>
        /// E-mail
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        #endregion

                /// <summary>
        /// Property that indicated that all data has been filled
        /// </summary>
        /// <returns>True if all data filled</returns>
        public bool IsFilled
        {
            get
            {
                return DateTime.Compare(StartDateTime, EndDateTime) < 0
                       && Email != string.Empty
                       && FirstName != string.Empty
                       && LastName != string.Empty
                       && Passport != string.Empty
                       && PhoneNumber != string.Empty;
            }
        }
    }
}
