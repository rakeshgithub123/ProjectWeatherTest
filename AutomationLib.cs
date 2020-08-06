using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support;
using System.Net;
using System.IO;
using OpenQA.Selenium.Interactions;
using System.Drawing;

namespace AutomationLib
{

    public class WeatherCheck
    {
        #region Selenium

        //Global Variables
        private static IWebDriver driver = null;

        /// <summary>
        /// Mobile GUI Object Attribute Types
        /// </summary>
        public enum eAttributeValue
        {
            Checked,
            Visible,
            Text
        };

        public enum eByType
        {
            ID,
            NAME,
            XPATH,
            LINK_TEXT,
            CSS,
            CLASS
        }

        public enum eBrowser
        {
            CHROME,
            FIREFOX,
            IE
        }

        public void LaunchBrowserAndNavigateToURL(string sURL, eBrowser browser = eBrowser.CHROME)
        {
            switch (browser)
            {
                case eBrowser.CHROME:
                    System.Environment.SetEnvironmentVariable("webdriver.chrome.driver",
                        @"C:\ProjectWeatherTest\Dependencies\selenium.webdriver.chromedriver\84.0.4147.3001\driver\win32");
                    driver = new ChromeDriver();
                    break;
                case eBrowser.FIREFOX:
                    driver = new FirefoxDriver();
                    break;
                case eBrowser.IE:
                    driver = new InternetExplorerDriver();
                    break;
                default:
                    break;
            }
            driver.Navigate().GoToUrl(sURL);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public void Click(eByType byType, string locator)
        {
            IWebElement element = null;
            element = findElement(byType, locator);
            if (element != null)
            {
                element.Click();
            }
        }

        public void SendText(eByType byType, string locator, string textToEnter)
        {
            IWebElement element = null;
            element = findElement(byType, locator);
            if (element != null)
            {
                element.Clear();
                element.SendKeys(textToEnter);
            }
        }

        public string GetAttributeVal(eByType byType, string locator, eAttributeValue attribute)
        {
            IWebElement element = null;
            string attributeValue = "";
            element = findElement(byType, locator);
            if (element != null)
            {
                if (attribute == eAttributeValue.Checked)
                {
                    attributeValue = element.Selected.ToString();
                }
                else if (attribute == eAttributeValue.Visible)
                {
                    attributeValue = element.Displayed.ToString();
                }
                else if (attribute == eAttributeValue.Text)
                {
                    attributeValue = element.Text;
                }
            }
            return attributeValue;
        }

        public void CloseBrowser()
        {
            driver.Close();
        }

        private IWebElement findElement(eByType type, string locator)
        {
            IWebElement element = null;
            try
            {
                switch (type)
                {
                    case eByType.ID:
                        element = driver.FindElement(By.Id(locator));
                        break;
                    case eByType.NAME:
                        element = driver.FindElement(By.Name(locator));
                        break;
                    case eByType.XPATH:
                        element = driver.FindElement(By.XPath(locator));
                        break;
                    case eByType.LINK_TEXT:
                        element = driver.FindElement(By.LinkText(locator));
                        break;
                    case eByType.CSS:
                        element = driver.FindElement(By.CssSelector(locator));
                        break;
                    case eByType.CLASS:
                        element = driver.FindElement(By.CssSelector(locator));
                        break;
                    default:
                        break;

                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("OpenQA.Selenium.NoSuchElementException"))
                    Console.WriteLine("Element with locator - " + locator + " not found.");
            }
            return element;

        }

        #endregion

        #region API

        public string GetWeatherDataFromAPI(string city)
        {
            string APIKey = "7fe67bf08c80ded756e598d6f8fedaea";
            string sURL = @"https://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + APIKey;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);

            request.Method = "POST";
            request.ContentType = "application/json";

            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader
            StreamReader reader = new StreamReader(dataStream);
            // Read content.
            string JSONString = reader.ReadToEnd();
            // Display content.
            Console.WriteLine("/n********************************************API RESPONSE***************************************************");
            Console.WriteLine(JSONString);
            Console.WriteLine("/n*******************************************END OF RESPONSE*************************************************");
            // Cleanup streams, response.
            reader.Close();
            dataStream.Close();
            response.Close();

            return JSONString;
        }

        #endregion
    }
}
