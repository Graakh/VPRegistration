using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;


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

        public void InitializeConnection()
        {
            using (var remoteWebDriver = new FirefoxDriver())
            {
                remoteWebDriver.Navigate().GoToUrl(UrlDictionary[UrlType.Disclaimer]);

                WebDriverWait wait = new WebDriverWait(remoteWebDriver, new TimeSpan(0, 0, 5));

                //Select language
                wait.Until(ExpectedConditions.ElementExists(By.Id("ctl00_ddLocale")));
                var locale = remoteWebDriver.FindElementById("ctl00_ddLocale");
                locale.Click();
                Thread.Sleep(2000);

                var localeMenu = remoteWebDriver.FindElementById("ctl00_ddLocale_DropDown");
                localeMenu.FindElement(By.XPath("div/ul/li[11]")).Click();
                Thread.Sleep(3000);
                
                NavigateToFormPage(remoteWebDriver, wait);

                ChooseVisaType(wait, remoteWebDriver);

                var result = GetAndTypeCaptcha(wait, remoteWebDriver);
            }
        }

        private bool GetAndTypeCaptcha(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
            var captcha = GetCaptcha(wait, remoteWebDriver).ToUpper();

            //Enter Captcha
            var textBox = remoteWebDriver.FindElementById("cp1_pnlCaptchaBotDetect").FindElement(By.ClassName("riTextBox"));
            textBox.SendKeys(captcha);

            //Navigate to dates
            remoteWebDriver.FindElementById("ctl00_cp1_btnNext").Click();

            try
            {
                remoteWebDriver.FindElementById("cp1_lblCaptchaError");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void NavigateToFormPage(RemoteWebDriver driver, WebDriverWait driverWait)
        {
            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnAccept"))));

            var acceptButton = driver.FindElementById("ctl00_cp1_btnAccept");
            acceptButton.Click();

            driverWait.Until(ExpectedConditions.ElementExists((By.Id("ctl00_cp1_btnNewAppointment"))));
            var newAppointmentButton = driver.FindElementById("ctl00_cp1_btnNewAppointment");

            newAppointmentButton.Click();
        }

        private void ChooseVisaType(WebDriverWait wait, FirefoxDriver remoteWebDriver)
        {
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
            visaTypeItem.FindElement(By.XPath("div/ul/li[4]")).Click();
        }

        private string GetCaptcha(WebDriverWait wait, FirefoxDriver remoteWebDriver)
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
                    imgCap.Save("C:\\captcha.png", ImageFormat.Png);

                    var request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1/gsa_test.gsa");

                    var postData = "file=C:\\captcha.png";
                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    return "CAPTCH";
                    //imgCap.Save(msCaptcha, ImageFormat.Png);

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
