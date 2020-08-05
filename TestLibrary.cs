using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AutomationLib;

namespace Test_WeatherAccuracy
{
    public static class Extensions
    {
        public static string GetDescription(this Enum e)
        {
            var attribute =
                e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                    as DescriptionAttribute;

            return attribute?.Description ?? e.ToString();
        }
    }

    
    public enum ePageElements
    {
        [Description("//a[@id='h_sub_menu']")]
        SUBMENU,
        [Description("WEATHER")]
        WEATHER_PAGE,
        [Description("searchBox")]
        CITY_SEARCH,
        [Description("//div[@id='messages']/div[1]/label")]
        CITY_SEARCH_RESULT,
        [Description(".leaflet-popup-content .heading:nth-child(2) b")]
        WEATHER_DETAILS_CONDITION,
        [Description(".leaflet-popup-content .heading:nth-child(4) b")]
        WEATHER_DETAILS_WIND,
        [Description(".leaflet-popup-content .heading:nth-child(6) b")]
        WEATHER_DETAILS_HUMIDITY,
        [Description(".leaflet-popup-content .heading:nth-child(8) b")]
        WEATHER_DETAILS_TEMPERATURE,
    }

    public enum eCity
    {
        Bengaluru,
        Mumbai,
        Chennai,
        Delhi,
        Ahmedabad
    }
    public struct WeatherDetails
    {
        public string Condition;
        public double WindSpeedMin;
        public double WindSpeedMax;
        public int Humidity;
        public int Temperature;
    }

   

    class TestLibrary
    {
        AutomationLib.WeatherCheck autoLib = new AutomationLib.WeatherCheck();
        
        public void NavigateToWeatherPageAndSearchForCity(eCity city)
        {
            autoLib.Click(WeatherCheck.eByType.XPATH, ePageElements.SUBMENU.GetDescription());
            autoLib.Click(WeatherCheck.eByType.LINK_TEXT, ePageElements.WEATHER_PAGE.GetDescription());
            autoLib.SendText(WeatherCheck.eByType.ID, ePageElements.CITY_SEARCH.GetDescription(), city.ToString());
            if (autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, ePageElements.CITY_SEARCH_RESULT.GetDescription(), WeatherCheck.eAttributeValue.Checked).ToLower() != "true")
                autoLib.Click(WeatherCheck.eByType.XPATH, ePageElements.CITY_SEARCH_RESULT.GetDescription());
        }

        public string VerifySearchedCityDisplayedOnMap(eCity city)
        {
            string cityAndWeatherDescriptionVisible = "";
            cityAndWeatherDescriptionVisible = autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, getLocatorForCityOnMap(city), WeatherCheck.eAttributeValue.Visible);
            if (cityAndWeatherDescriptionVisible.ToLower() == "true")
                cityAndWeatherDescriptionVisible = autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, getLocatorForCityWeatherDescripttion(city), WeatherCheck.eAttributeValue.Visible);
            return cityAndWeatherDescriptionVisible.ToLower();
        }
        public WeatherDetails GetWeatherInfoFromNDTV(eCity city)
        {
            WeatherDetails NDTVWeatherInfo = new WeatherDetails();

            autoLib.Click(WeatherCheck.eByType.XPATH, getLocatorForCityOnMap(city));

            //condition
            NDTVWeatherInfo.Condition = getWeatherCondition(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_CONDITION.GetDescription(), WeatherCheck.eAttributeValue.Text));

            //WindSpeed
            string windInfo = autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_WIND.GetDescription(), WeatherCheck.eAttributeValue.Text);
            NDTVWeatherInfo.WindSpeedMin = getWeatherWindMin(windInfo);
            NDTVWeatherInfo.WindSpeedMax = getWeatherWindMax(windInfo);

            //humidity
            NDTVWeatherInfo.Humidity = getWeatherHumidity(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_HUMIDITY.GetDescription(), WeatherCheck.eAttributeValue.Text));

            //temperature
            NDTVWeatherInfo.Temperature = getWeatherTemperature(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_TEMPERATURE.GetDescription(), WeatherCheck.eAttributeValue.Text));

            return NDTVWeatherInfo;
        }


        #region Private Methods

        private string getLocatorForCityOnMap(eCity city)
        {
            string locator = @"//div[@title='" + city.ToString() + "']/div[@class='cityText']";
            return locator;
        }

        private string getLocatorForCityWeatherDescripttion(eCity city)
        {
            string locator = "//div[@title='" + city.ToString() + "']//span[@class='tempRedText']";
            return locator;
        }

        private string getWeatherCondition(string weatherAsDisplayed)
        {
            string weatherCondition = "";
            weatherAsDisplayed = weatherAsDisplayed.Replace(" ", "");
            weatherCondition = weatherAsDisplayed.Substring(weatherAsDisplayed.IndexOf(':') + 1, weatherAsDisplayed.Length - weatherAsDisplayed.IndexOf(':') - 1);
            return weatherCondition;
        }
        private double getWeatherWindMin(string weatherAsDisplayed)
        {
            double windMin = 0;
            var doubleArray = Regex.Split(weatherAsDisplayed, @"[^0-9\.]+");
            doubleArray = doubleArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            windMin = double.Parse(doubleArray[0]);
            return windMin;
        }

        private double getWeatherWindMax(string weatherAsDisplayed)
        {
            double windMax = 0;
            var doubleArray = Regex.Split(weatherAsDisplayed, @"[^0-9\.]+");
            doubleArray = doubleArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            windMax = double.Parse(doubleArray[1]);
            return windMax;
        }
        private int getWeatherHumidity(string weatherAsDisplayed)
        {
            weatherAsDisplayed = weatherAsDisplayed.Replace(" ", "");
            weatherAsDisplayed = weatherAsDisplayed.Replace("%", "");
            string s = weatherAsDisplayed.Substring(weatherAsDisplayed.IndexOf(':') + 1, weatherAsDisplayed.Length - weatherAsDisplayed.IndexOf(':') - 1);
            int humidity = Convert.ToInt32(weatherAsDisplayed.Substring(weatherAsDisplayed.IndexOf(':') + 1, weatherAsDisplayed.Length - weatherAsDisplayed.IndexOf(':') - 1));
            return humidity;
        }
        private int getWeatherTemperature(string weatherAsDisplayed)
        {
            weatherAsDisplayed = weatherAsDisplayed.Replace(" ", "");
            int temperature = Convert.ToInt32(weatherAsDisplayed.Substring(weatherAsDisplayed.IndexOf(':') + 1, weatherAsDisplayed.Length - weatherAsDisplayed.IndexOf(':') - 1));
            return temperature;
        }

        #endregion
    }

}
