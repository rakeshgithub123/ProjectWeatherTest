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

namespace AutomationLib
{

    public class WeatherCheck
    {

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

        public enum eTextEncloser
        {
            Style,
            Value
        };

        public enum eByType
        {
            ID,
            NAME,
            XPATH,
            LINK_TEXT,
            CSS
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
            switch (byType)
            {
                case eByType.ID:
                    driver.FindElement(By.Id(locator)).Click();
                    return;
                case eByType.NAME:
                    driver.FindElement(By.Name(locator)).Click();
                    return;
                case eByType.XPATH:
                    driver.FindElement(By.XPath(locator)).Click();
                    return;
                case eByType.LINK_TEXT:
                    driver.FindElement(By.LinkText(locator)).Click();
                    return;
                default:
                    return;

            }

        }

        public void SendText(eByType byType, string locator, string textToEnter)
        {
            switch (byType)
            {
                case eByType.ID:
                    driver.FindElement(By.Id(locator)).SendKeys(textToEnter);
                    return;
                case eByType.NAME:
                    driver.FindElement(By.Name(locator)).SendKeys(textToEnter);
                    return;
                case eByType.XPATH:
                    driver.FindElement(By.XPath(locator)).SendKeys(textToEnter);
                    return;
                case eByType.LINK_TEXT:
                    driver.FindElement(By.LinkText(locator)).SendKeys(textToEnter);
                    return;
                default:
                    return;

            }
        }

        public string GetAttributeVal(eByType byType, string locator, eAttributeValue attribute,
            eTextEncloser textEncloser = eTextEncloser.Value)
        {
            IWebElement element = null;
            string attributeValue = "";

            switch (byType)
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
                default:
                    break;

            }
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

    }
}
