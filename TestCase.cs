using System;
using AutomationLib;

namespace Test_WeatherAccuracy
{
    class Test_NDTVWeatherReport
    {
        static string sNDTVUrl = "https://www.ndtv.com/";
        static void Main(string[] args)
        {
            TestLibrary TestLib = new TestLibrary();
            AutomationLib.WeatherCheck autoLib = new AutomationLib.WeatherCheck();

            ConfigParmaeter configParameter = new ConfigParmaeter();
            configParameter = TestLib.GetConfigParameterFromExternalFile();
            int varianceForTempratureInCelsius = configParameter.TemperatureInCelsius;
            int varianceForHumidityInPercentage = configParameter.HumidityInPercentage;
            int variaceForWindSpeedInKMPH = configParameter.WindSpeedInKMPH;
            string city = configParameter.City;

            autoLib.LaunchBrowserAndNavigateToURL(sNDTVUrl, WeatherCheck.eBrowser.CHROME);

            TestLib.NavigateToWeatherPageAndSearchForCity(city);

            if (TestLib.VerifySearchedCityDisplayedOnMap(city) == "true")
                Console.WriteLine("PASS : Searched city is displayed on the map");
            else
                Console.WriteLine("FAIL : Searched city is not displayed on the map");

            WeatherDetails weatherDetailsOnNDTV = new WeatherDetails();
            weatherDetailsOnNDTV = TestLib.GetWeatherInfoFromNDTV(city);

            WeatherDetails weatherDetailsFromAPI = new WeatherDetails();
            weatherDetailsFromAPI = TestLib.GetWeatherInfoFromAPI(city);

            TestLib.VerifySearchedCityDisplayedOnMap(city);

            if (TestLib.VerifyWeatherInfoBetweenNDTVAndAPI(weatherDetailsFromAPI, weatherDetailsOnNDTV, varianceForTempratureInCelsius,
                varianceForHumidityInPercentage, variaceForWindSpeedInKMPH))
                Console.WriteLine("PASS : The weather info between NDTV and API matches");
            else
                Console.WriteLine("FAIL : The weather info between NDTV and API does not match");

            Console.ReadLine();

            autoLib.CloseBrowser();

        }
    }
}
