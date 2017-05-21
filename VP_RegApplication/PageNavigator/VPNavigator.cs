using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using VP_RegApplication.MVVM;
using VP_RegApplication.Solver;

namespace VP_RegApplication
{
    /// <summary>
    /// Enum for page type
    /// </summary>
    public enum UrlType
    {
        Disclaimer = 0,
        Action = 1,
        Form = 2
    }

    //TODO:
    //Not the best implementation. Too much dependent on page layout
    /// <summary>
    /// Class that performs page navgation
    /// </summary>
    public class VPNavigator
    {
        private const int defaultDelayBetweenAttempts = 500;
        private int _counter = 0;
        private RegisterInfo _info;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Dictionary that hold page urls
        /// </summary>
        private readonly Dictionary<UrlType, string> UrlDictionary = new Dictionary<UrlType, string>()
            {
                {
                    UrlType.Disclaimer,
                    "https://visapoint.eu/disclaimer"
                },
                {
                    UrlType.Action,
                    "https://visapoint.eu/action"
                },
                {
                    UrlType.Form,
                    "https://visapoint.eu/form"
                }

            };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">All the personal data goes here</param>
        public VPNavigator(RegisterInfo info)
        {
            _info = info;
        }

        /// <summary>
        /// Initialize connection and perform registration
        /// </summary>
        public void InitializeAndStart()
        {
            var ct = _cts.Token;
            for (int i = 0; i < 1; i++)
            {
                var task = Task.Factory.StartNew(() => 
                {
                    Thread.Sleep(i * 1000);
                    StartProcess(); 
                }, ct).ContinueWith(
                    e =>
                    {
                        if (e.Exception != null)
                        {
                            MessageBox.Show(e.Exception.Message);
                        };
                    });
            }
        }

        /// <summary>
        /// Star the entire registration process
        /// </summary>
        private void StartProcess()
        {
            var remoteWebDriver = new ChromeDriver();
            remoteWebDriver.Navigate().GoToUrl(UrlDictionary[UrlType.Disclaimer]);
            WebDriverWait wait = new WebDriverWait(remoteWebDriver, new TimeSpan(0, 0, 5));
            ChooseLanguage(wait, remoteWebDriver);
            NavigateToFormPage(remoteWebDriver, wait);
            ChooseVisaType(wait, remoteWebDriver);

            if (!TryRegister(wait, remoteWebDriver))
            {
                remoteWebDriver.Close();
            }
            else
            {
                MessageBox.Show("Please, check registration on e-mail.");
            }
            _cts.Cancel();
        }

        /// <summary>
        /// Method tries to register new visit. Recursive
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="remoteWebDriver"></param>
        /// <returns>True if success</returns>
        private bool TryRegister(WebDriverWait wait, RemoteWebDriver remoteWebDriver)
        {
            _counter++;
            var result = false;
            var captchaSolver = new CaptchaSolver();

            while (!result)
            {
                result = GetAndTypeCaptcha(wait, remoteWebDriver, captchaSolver);
            }
            if (AnyDatesAvailable(remoteWebDriver))
            {
                result = PerformRegistration(remoteWebDriver, wait);
            }
            else
            {
                Thread.Sleep(defaultDelayBetweenAttempts);
                remoteWebDriver.FindElementById("ctl00_cp1_btnPrev").Click();
                Thread.Sleep(1000);
                result = TryRegister(wait, remoteWebDriver);
            }
            return result;
        }

        /// <summary>
        /// Choose page language
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="remoteWebDriver"></param>
        private static void ChooseLanguage(WebDriverWait wait, RemoteWebDriver remoteWebDriver)
        {
            //Select language
            wait.Until(ExpectedConditions.ElementExists(By.Id("ctl00_ddLocale")));
            var locale = remoteWebDriver.FindElementById("ctl00_ddLocale");
            locale.Click();
            Thread.Sleep(200);

            var localeMenu = remoteWebDriver.FindElementById("ctl00_ddLocale_DropDown");
            localeMenu.FindElement(By.XPath("div/ul/li[11]")).Click();
            Thread.Sleep(200);
        }

        /// <summary>
        /// Performs registration
        /// </summary>
        /// <param name="remoteWebDriver"></param>
        /// <param name="driverWait"></param>
        /// <returns></returns>
        private bool PerformRegistration(RemoteWebDriver remoteWebDriver, WebDriverWait driverWait)
        {
            IWebElement selectedDate = null;
            var datesList = remoteWebDriver.FindElementById("cp1_rblDate");
            var datesCollection = datesList.FindElements(By.XPath("id('cp1_rblDate')/tbody/tr"));
            foreach (var date in datesCollection)
            {
                DateTime dateTime;
                DateTime.TryParse(date.Text, out dateTime);
                if (DateTime.Compare(_info.StartDateTime, dateTime) < 0 &&
                    DateTime.Compare(dateTime, _info.EndDateTime) < 0)
                {
                    selectedDate = date;
                    break;
                }
            }
            if (selectedDate == null)
            {
                return false;
            }
            selectedDate.Click();

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(300);

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_txtFirstName"))));

            remoteWebDriver.FindElementById("ctl00_cp1_txtFirstName").SendKeys(_info.FirstName.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtFamilyName").SendKeys(_info.LastName.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtBirthDate_dateInput").SendKeys(_info.DateOfBirth.ToShortDateString());

            var sexSelector = remoteWebDriver.FindElementById("ctl00_cp1_ddSex_Input");
            sexSelector.Click();
            Thread.Sleep(300);

            var sexMenu = remoteWebDriver.FindElementById("ctl00_cp1_ddSex_DropDown");
            sexMenu.FindElement(By.XPath("div/ul/li[1]")).Click();

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(300);

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_txtPassportNumber"))));
            remoteWebDriver.FindElementById("ctl00_cp1_txtPassportNumber").SendKeys(_info.Passport.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtEmail").SendKeys(_info.Email.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtPhone").Clear();
            remoteWebDriver.FindElementById("ctl00_cp1_txtPhone").SendKeys("+"+_info.PhoneNumber);

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(300);

            remoteWebDriver.FindElementById("ctl00_cp1_btnSend").Click();
            return true;
        }

        /// <summary>
        /// Check if any dates are available
        /// </summary>
        /// <returns>returns true if available</returns>
        private bool AnyDatesAvailable(RemoteWebDriver remoteWebDriver)
        {
            try
            {
                remoteWebDriver.FindElementById("cp1_lblNoDates");
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Gets captcha from captcha solver and enters it.
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="remoteWebDriver"></param>
        /// <param name="captchaSolver"></param>
        /// <returns>true if success</returns>
        private bool GetAndTypeCaptcha(WebDriverWait wait, RemoteWebDriver remoteWebDriver, CaptchaSolver captchaSolver)
        {
            var captcha = captchaSolver.GetCaptcha(GetCaptchaImage(wait, remoteWebDriver));

            if (captcha != null && captcha.Solved && captcha.Correct)
            {
                //Enter Captcha
                var textBox =
                    remoteWebDriver.FindElementById("cp1_pnlCaptchaBotDetect").FindElement(By.ClassName("riTextBox"));
                textBox.Clear();
                textBox.SendKeys(" "+ captcha.Text);

                //Navigate to dates
                remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
                Thread.Sleep(500);

                try
                {
                    var captchaError = remoteWebDriver.FindElementById("cp1_lblCaptchaError");
                    captchaSolver.Report(captcha);
                    return false;
                }
                catch
                {
                    //if element was not present, then exception will be thrown. Reconsider what exception type to catch
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// navigate to form page
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="driverWait"></param>
        private void NavigateToFormPage(RemoteWebDriver driver, WebDriverWait driverWait)
        {
            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnAccept"))));
            driver.FindElementById("ctl00_cp1_btnAccept").Click();

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnNewAppointment"))));
            driver.FindElementById("ctl00_cp1_btnNewAppointment").Click();
        }

        /// <summary>
        /// Performs visa type selection
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="remoteWebDriver"></param>
        private void ChooseVisaType(WebDriverWait wait, RemoteWebDriver remoteWebDriver)
        {
            wait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_ddCitizenship_Input"))));
            var citizenshipMenu = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_Input");
            citizenshipMenu.SendKeys("Ukraine (Україна)");
            citizenshipMenu.Click();
            Thread.Sleep(500);

            var citizenshipMenuItem = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_DropDown");
            citizenshipMenuItem.FindElement(By.XPath("div/ul/li[23]")).Click();

            var visaType = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_Input");
            visaType.Click();
            Thread.Sleep(500);

            var visaTypeItem = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_DropDown");
            visaTypeItem.FindElement(By.XPath("div/ul/li[7]")).Click();
        }

        /// <summary>
        /// Get captha image to send it later to solving service
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="remoteWebDriver"></param>
        /// <returns>captcha bitmap</returns>
        private Bitmap GetCaptchaImage(WebDriverWait wait, RemoteWebDriver remoteWebDriver)
        {
            wait.Until(ExpectedConditions.ElementExists((By.Id("c_pages_form_cp1_captcha1_CaptchaImage"))));

            var arrScreen = remoteWebDriver.GetScreenshot().AsByteArray;
            using (var msScreen = new MemoryStream(arrScreen))
            {
                var bmpScreen = new Bitmap(msScreen);
                var cap = remoteWebDriver.FindElementById("c_pages_form_cp1_captcha1_CaptchaImage");
                var rcCrop = new Rectangle(cap.Location, cap.Size);
                Bitmap imgCap = bmpScreen.Clone(rcCrop, bmpScreen.PixelFormat);
                return imgCap;
            }
        }
    }
}
