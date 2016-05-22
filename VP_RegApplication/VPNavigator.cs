using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using DeathByCaptcha;
using NLog;

namespace VP_RegApplication
{
    public enum UrlType
    {
        Disclaimer = 0,
        Action = 1,
        Form = 2
    }

    public class VPNavigator
    {
        private int defaultDelayBetweenAttempts = 5000;
        private int _counter = 0;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private Presenter.RegisterInfo _info;
        private Dictionary<UrlType, string> UrlDictionary = new Dictionary<UrlType, string>()
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

        public VPNavigator(Presenter.RegisterInfo info)
        {
            _info = info;
            _logger.Info("Navigator created");
        }

        public void InitializeConnection()
        {
            using (var remoteWebDriver = new FirefoxDriver())
            {
                _logger.Info("Run browser");
                remoteWebDriver.Navigate().GoToUrl(UrlDictionary[UrlType.Disclaimer]);

                WebDriverWait wait = new WebDriverWait(remoteWebDriver, new TimeSpan(0, 0, 5));

                ChooseLanguage(wait, remoteWebDriver);

                NavigateToFormPage(remoteWebDriver, wait);

                ChooseVisaType(wait, remoteWebDriver);

                _logger.Info("Try to register");
                TryRegister(wait, remoteWebDriver);
            }
        }

        private void TryRegister(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
            _counter++;
            var result = false;

            while (!result)
            {
                result = GetAndTypeCaptcha(wait, remoteWebDriver);
            }

            if (result)
            {
                if (AnyDatesAvailable(remoteWebDriver))
                {
                    if (!PerformRegistration(remoteWebDriver, wait))
                    {
                        _logger.Warn("registration failed");
                        throw new System.Exception("Registration failed");
                    }
                }
                else
                {
                    Thread.Sleep(defaultDelayBetweenAttempts);
                    remoteWebDriver.FindElementById("ctl00_cp1_btnPrev").Click();
                    _logger.Info("perform next try: " + _counter);
                    TryRegister(wait, remoteWebDriver);
                }
            }
        }

        private static void ChooseLanguage(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
            //Select language
            wait.Until(ExpectedConditions.ElementExists(By.Id("ctl00_ddLocale")));
            var locale = remoteWebDriver.FindElementById("ctl00_ddLocale");
            locale.Click();
            Thread.Sleep(200);

            var localeMenu = remoteWebDriver.FindElementById("ctl00_ddLocale_DropDown");
            localeMenu.FindElement(By.XPath("div/ul/li[11]")).Click();
            Thread.Sleep(200);
            _logger.Info("Language selected");
        }

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
                    _logger.Info("Date selected");
                    break;
                }
            }
            if (selectedDate == null)
            {
                _logger.Warn("No requested date");
                return false;
            }
            selectedDate.Click();

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(1000);

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_txtFirstName"))));

            remoteWebDriver.FindElementById("ctl00_cp1_txtFirstName").SendKeys(_info.FirstName.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtFamilyName").SendKeys(_info.LastName.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtBirthDate_dateInput").SendKeys(_info.DateOfBirth.ToShortDateString());

            var sexSelector = remoteWebDriver.FindElementById("ctl00_cp1_ddSex_Input");
            sexSelector.Click();
            Thread.Sleep(500);

            var sexMenu = remoteWebDriver.FindElementById("ctl00_cp1_ddSex_DropDown");
            sexMenu.FindElement(By.XPath("div/ul/li[1]")).Click();

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(500);

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_txtPassportNumber"))));
            remoteWebDriver.FindElementById("ctl00_cp1_txtPassportNumber").SendKeys(_info.Passport.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtEmail").SendKeys(_info.Email.ToUpper());
            remoteWebDriver.FindElementById("ctl00_cp1_txtPhone").Clear();
            remoteWebDriver.FindElementById("ctl00_cp1_txtPhone").SendKeys("+"+_info.PhoneNumber);

            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
            Thread.Sleep(500);

            remoteWebDriver.FindElementById("ctl00_cp1_btnSend").Click();
            _logger.Info("Registration done");
            return true;
        }

        private bool AnyDatesAvailable(FirefoxDriver remoteWebDriver)
        {
            try
            {
                remoteWebDriver.FindElementById("cp1_lblNoDates");
                return false;
            }
            catch
            {
                _logger.Warn("No dates found");
                return true;
            }
        }

        private bool GetAndTypeCaptcha(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
            _logger.Info("Get Captcha");
            Client client = (Client)new SocketClient(_info.Username, _info.Password);
            var captcha = GetCaptcha(wait, remoteWebDriver, client);

            if (captcha != null && captcha.Solved && captcha.Correct)
            {
                //Enter Captcha
                var textBox =
                    remoteWebDriver.FindElementById("cp1_pnlCaptchaBotDetect").FindElement(By.ClassName("riTextBox"));
                textBox.SendKeys(captcha.Text);

                //Navigate to dates
                remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();
                Thread.Sleep(500);

                try
                {
                    var captchaError = remoteWebDriver.FindElementById("cp1_lblCaptchaError");
                    client.Report(captcha);
                    _logger.Warn("Sorry, wrong captcha");
                    return false;
                }
                catch
                {
                    return true;
                }
            }
            _logger.Warn("Captcha recognition error occured");
            return false;
        }

        private void NavigateToFormPage(RemoteWebDriver driver, WebDriverWait driverWait)
        {
            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnAccept"))));
            driver.FindElementById("ctl00_cp1_btnAccept").Click();

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnNewAppointment"))));
            driver.FindElementById("ctl00_cp1_btnNewAppointment").Click();
        }

        private void ChooseVisaType(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
            wait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_ddCitizenship_Input"))));
            var citizenshipMenu = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_Input");
            citizenshipMenu.SendKeys("Ukraine (Україна)");
            citizenshipMenu.Click();
            Thread.Sleep(500);

            var citizenshipMenuItem = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_DropDown");
            citizenshipMenuItem.FindElement(By.XPath("div/ul/li[22]")).Click();

            var visaType = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_Input");
            visaType.Click();
            Thread.Sleep(500);

            var visaTypeItem = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_DropDown");
            visaTypeItem.FindElement(By.XPath("div/ul/li[4]")).Click();
            _logger.Info("Visa type selected");
        }

        private Captcha GetCaptcha(WebDriverWait wait, FirefoxDriver remoteWebDriver, Client client)
        {
            wait.Until(ExpectedConditions.ElementExists((By.Id("c_pages_form_cp1_captcha1_CaptchaImage"))));

            var arrScreen = remoteWebDriver.GetScreenshot().AsByteArray;
            using (var msScreen = new MemoryStream(arrScreen))
            {
                var bmpScreen = new Bitmap(msScreen);
                var cap = remoteWebDriver.FindElementById("c_pages_form_cp1_captcha1_CaptchaImage");
                var rcCrop = new Rectangle(cap.Location, cap.Size);
                Bitmap imgCap = bmpScreen.Clone(rcCrop, bmpScreen.PixelFormat);

                using (var msCaptcha = new MemoryStream())
                {
                    imgCap.Save(msCaptcha, ImageFormat.Png);
                    _logger.Info("Sending captcha for solving");

                    _logger.Info("Your balance is {0:F2} US cents", client.Balance);


                    Captcha captcha = client.Decode(msCaptcha, 20);
                    return captcha;

                    //// put your DeathByCaptcha credentials here
                    //var client = new SocketClient("user", "password");

                    //Console.WriteLine("Sending request to DeathByCaptcha...");
                    //var res = client.Decode(msCaptcha.GetBuffer(), 20);
                    //if (res != null && res.Solved && res.Correct)
                    //{
                    //    driver.FindElementByXPath("//input[@name='captcha_code']").SendKeys(res.Text);

                    //    driver.FindElementByXPath("//input[@name='submit']").Click();

                    //    var h4 = driver.FindElementByXPath("//div[@id='case_captcha']//h4");
                    //    if (!h4.Text.Contains("SUCCESSFULLY"))
                    //    {
                    //        Console.WriteLine("The captcha has been solved incorrectly!");
                    //        client.Report(res);
                    //    }
                    //    else
                    //        Console.WriteLine("The captcha has been solved correctly!");
                    //}
                    //else
                    //    Console.WriteLine("Captcha recognition error occured");
                }
            }
        }
    }
}
