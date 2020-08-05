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

            autoLib.LaunchBrowserAndNavigateToURL(sNDTVUrl, WeatherCheck.eBrowser.CHROME);

            TestLib.NavigateToWeatherPageAndSearchForCity(eCity.Ahmedabad);

            TestLib.VerifySearchedCityDisplayedOnMap(eCity.Ahmedabad);

            TestLib.GetWeatherInfoFromNDTV(eCity.Ahmedabad);

        }
    }
}
