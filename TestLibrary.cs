using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AutomationLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        [Description("//div[@class='leaflet-top leaflet-right']/div/a[@class='leaflet-control-zoom-in']")]
        ZOOM_IN,
        [Description(".leaflet-popup-content .heading:nth-child(2) b")]
        WEATHER_DETAILS_CONDITION,
        [Description(".leaflet-popup-content .heading:nth-child(4) b")]
        WEATHER_DETAILS_WIND,
        [Description(".leaflet-popup-content .heading:nth-child(6) b")]
        WEATHER_DETAILS_HUMIDITY,
        [Description(".leaflet-popup-content .heading:nth-child(8) b")]
        WEATHER_DETAILS_TEMPERATURE,
    }

    public struct WeatherDetails
    {
        public string Condition;
        public double WindSpeed;
        public double WindSpeedGust;
        public int Humidity;
        public double Temperature;
    }

    public struct ConfigParmaeter
    {
        public int TemperatureInCelsius;
        public int HumidityInPercentage;
        public int WindSpeedInKMPH;
        public string City;
    }
    class TestLibrary
    {
        AutomationLib.WeatherCheck autoLib = new AutomationLib.WeatherCheck();

        #region Pulic methods

        public ConfigParmaeter GetConfigParameterFromExternalFile()
        {
            ConfigParmaeter configParmeter = new ConfigParmaeter();

            string json = "";
            using (StreamReader r = new StreamReader(@"C:\ProjectWeatherTest\config.json"))
            {
                json = r.ReadToEnd();
            }

            JObject jsonObj = JObject.Parse(json);
            configParmeter.TemperatureInCelsius = Convert.ToInt32(jsonObj.SelectToken("VarianceForTemperatureInCelsius"));
            configParmeter.HumidityInPercentage = Convert.ToInt32(jsonObj.SelectToken("VarianceForHumidityInPercentage"));
            configParmeter.WindSpeedInKMPH = Convert.ToInt32(jsonObj.SelectToken("VarianceForWindSpeedInKMPH"));
            configParmeter.City = (string)jsonObj.SelectToken("City");

            return configParmeter;
        }

        public void NavigateToWeatherPageAndSearchForCity(string city)
        {
            autoLib.Click(WeatherCheck.eByType.XPATH, ePageElements.SUBMENU.GetDescription());
            autoLib.Click(WeatherCheck.eByType.LINK_TEXT, ePageElements.WEATHER_PAGE.GetDescription());
            Thread.Sleep(2000);
            autoLib.SendText(WeatherCheck.eByType.ID, ePageElements.CITY_SEARCH.GetDescription(), city);
            if (autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, getLocatorForCitySearchList(city), WeatherCheck.eAttributeValue.Checked).ToLower() != "true")
                autoLib.Click(WeatherCheck.eByType.XPATH, getLocatorForCitySearchList(city));
        }


        public WeatherDetails GetWeatherInfoFromNDTV(string city)
        {
            WeatherDetails NDTVWeatherInfo = new WeatherDetails();

            autoLib.Click(WeatherCheck.eByType.XPATH, getLocatorForCityOnMap(city));

            //condition
            NDTVWeatherInfo.Condition = getWeatherCondition(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_CONDITION.GetDescription(), WeatherCheck.eAttributeValue.Text));

            //WindSpeed
            string windInfo = autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_WIND.GetDescription(), WeatherCheck.eAttributeValue.Text);
            NDTVWeatherInfo.WindSpeed = getWeatherWindMin(windInfo);
            NDTVWeatherInfo.WindSpeedGust = getWeatherWindMax(windInfo);

            //humidity
            NDTVWeatherInfo.Humidity = getWeatherHumidity(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_HUMIDITY.GetDescription(), WeatherCheck.eAttributeValue.Text));

            //temperature
            NDTVWeatherInfo.Temperature = getWeatherTemperature(autoLib.GetAttributeVal(WeatherCheck.eByType.CSS, ePageElements.WEATHER_DETAILS_TEMPERATURE.GetDescription(), WeatherCheck.eAttributeValue.Text));

            return NDTVWeatherInfo;
        }

        public WeatherDetails GetWeatherInfoFromAPI(string city)
        {
            WeatherDetails APIWeatherInfo = new WeatherDetails();

            string JsonResponse = autoLib.GetWeatherDataFromAPI(city.ToString());
            APIWeatherInfo = parseJsonResponse(JsonResponse);

            return APIWeatherInfo;
        }

        #endregion

        #region Verification methods

        public string VerifySearchedCityDisplayedOnMap(string city)
        {
            string cityAndWeatherDescriptionVisible = "";
            cityAndWeatherDescriptionVisible = autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, getLocatorForCityOnMap(city), WeatherCheck.eAttributeValue.Visible);
            if (cityAndWeatherDescriptionVisible.ToLower() == "true")
                cityAndWeatherDescriptionVisible = autoLib.GetAttributeVal(WeatherCheck.eByType.XPATH, getLocatorForCityWeatherDescripttion(city), WeatherCheck.eAttributeValue.Visible);
            return cityAndWeatherDescriptionVisible.ToLower();
        }

        public bool VerifyWeatherInfoBetweenNDTVAndAPI(WeatherDetails InfoFromAPI, WeatherDetails InfoFromNDTV,
            int varianceForTemp = 0, int varianceForHumidity = 0, int varianceForWindSpeed = 0)
        {
            bool finalResult = false;
            bool windSpeedResult = false;
            bool humidityResult = false;
            bool temperatureResult = false;

            //compare weather condition
            if (InfoFromAPI.Condition == InfoFromNDTV.Condition)
                Console.WriteLine("PASS : Weather condition is a match");
            else
            {
                Console.WriteLine("Warning : Weather condition is not a match");
                Console.WriteLine("Condition from API : " + InfoFromAPI.Condition);
                Console.WriteLine("Condition from NDTV : " + InfoFromNDTV.Condition);
            }

            //compare windSpeed
            if (Math.Abs(InfoFromAPI.WindSpeed - InfoFromNDTV.WindSpeed) <= varianceForWindSpeed)
            {
                Console.WriteLine("PASS : Weather windSpeed is a match");
                windSpeedResult = true;
            }
            else
            {
                Console.WriteLine("Fail : Weather windSpeed is not a match");
                Console.WriteLine("windSpeed from API : " + InfoFromAPI.WindSpeed);
                Console.WriteLine("windSpeed from NDTV : " + InfoFromNDTV.WindSpeed);
            }

            //compare humidity
            if ((Math.Abs(InfoFromAPI.Humidity - InfoFromNDTV.Humidity) / InfoFromAPI.Humidity) * 100 <= varianceForHumidity)
            {
                Console.WriteLine("PASS : Weather Humidity is a match");
                humidityResult = true;
            }
            else
            {
                Console.WriteLine("Fail : Weather Humidity is not a match");
                Console.WriteLine("Humidity from API : " + InfoFromAPI.Humidity);
                Console.WriteLine("Humidity from NDTV : " + InfoFromNDTV.Humidity);
            }

            //compare temperature
            if (Math.Abs(InfoFromAPI.Temperature - InfoFromNDTV.Temperature) <= varianceForTemp)
            {
                Console.WriteLine("PASS : Weather Temperature is a match");
                temperatureResult = true;
            }
            else
            {
                Console.WriteLine("Fail : Weather Temperature is not a match");
                Console.WriteLine("Temperature from API : " + InfoFromAPI.Temperature);
                Console.WriteLine("Temperature from NDTV : " + InfoFromNDTV.Temperature);
            }

            finalResult = windSpeedResult && humidityResult && temperatureResult;

            return finalResult;
        }

        #endregion

        #region Private Methods

        private string getLocatorForCityOnMap(string city)
        {
            //string locator = @"//div[@title='" + city + "']/div[@class='cityText']";
            string locator = @"//div[@title='" + city + "'and@class='outerContainer']";
            return locator;
        }

        private string getLocatorForCitySearchList(string city)
        {
            string locator = @"//div[@class='message']/label[@for='" + city + "']/input";
            return locator;
        }

        private string getLocatorForCityWeatherDescripttion(string city)
        {
            string locator = "//div[@title='" + city + "']//span[@class='tempRedText']";
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

        private WeatherDetails parseJsonResponse(string originalJson)
        {
            WeatherDetails weatherDetailsFromJson = new WeatherDetails();
            JObject jsonObj = JObject.Parse(originalJson);
            weatherDetailsFromJson.Condition = (string)jsonObj.SelectToken("weather[0].description");
            string windspeed = (string)jsonObj.SelectToken("wind.speed");
            weatherDetailsFromJson.WindSpeed = double.Parse(windspeed);
            weatherDetailsFromJson.WindSpeedGust = 0;
            weatherDetailsFromJson.Humidity = (int)jsonObj.SelectToken("main.humidity");
            weatherDetailsFromJson.Temperature = (int)jsonObj.SelectToken("main.temp");
            weatherDetailsFromJson.Temperature = weatherDetailsFromJson.Temperature - 273; //convert temperature in kelvin to celsius

            return weatherDetailsFromJson;
        }

        #endregion
    }

}
