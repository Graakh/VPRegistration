using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace VP_RegApplication
{
    public class Presenter
    {
        private ICommand _connect;

        public Presenter()
        {

        }

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
                
            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            { 
                var remoteWebDriver = new FirefoxDriver();
                remoteWebDriver.Navigate().GoToUrl(UrlDictionary[UrlType.Disclaimer]);

                WebDriverWait wait = new WebDriverWait(remoteWebDriver, new TimeSpan(0, 0, 5));
                NavigateToFormPage(remoteWebDriver, wait);

                wait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_ddCitizenship_Input"))));
                var citizenshipMenu = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_Input");
                citizenshipMenu.SendKeys("Ukraine (Україна)");
                citizenshipMenu.Click();
                
                Thread.Sleep(2000);
                var citizenshipMenuItem = remoteWebDriver.FindElementById("ctl00_cp1_ddCitizenship_DropDown");
                citizenshipMenuItem.FindElement(By.XPath("div/ul/li[22]")).Click();

                var visaType = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_Input");
                visaType.SendKeys("Short-term visa - Schengen");
                visaType.Click();

                Thread.Sleep(2000);
                var visaTypeItem = remoteWebDriver.FindElementById("ctl00_cp1_ddVisaType_DropDown");
                visaTypeItem.FindElement(By.XPath("div/ul/li[5]")).Click();
            }

            private void NavigateToFormPage(RemoteWebDriver driver, WebDriverWait driverWait)
            {
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
                driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnAccept"))));

                var acceptButton = driver.FindElementById("ctl00_cp1_btnAccept");
                acceptButton.Click();

                driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnNewAppointment"))));
                var newAppointmentButton = driver.FindElementById("ctl00_cp1_btnNewAppointment");

                newAppointmentButton.Click();
            }

            #endregion

        }

        enum UrlType
        {
            Disclaimer = 0,
            Action = 1,
            Form = 2
        }
    }
}
