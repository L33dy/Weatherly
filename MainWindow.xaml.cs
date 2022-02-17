using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Weatherly
{
    public partial class MainWindow
    {
        private string unitGroup;

        private WebClient wc = new WebClient();

        private JObject data;

        private int dayIndex;
        
        string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public MainWindow()
        {
            Icon = new BitmapImage(new Uri(path + "/images/icon.png"));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void DisplayWeatherData()
        {
            //Weather Condition
            try
            {
                //Figure out if locally there is day or night
                DateTime currentDateTime = DateTime.UtcNow;

                string timeZOffset = (string)data["tzoffset"];

                DateTime localDateTime =
                    currentDateTime.AddHours(double.Parse(timeZOffset, CultureInfo.InvariantCulture));

                DateTime localSunset = (DateTime)data["days"][0]["sunset"];
                DateTime localSunrise = (DateTime)data["days"][0]["sunrise"];

                var isDay = localDateTime < localSunset && localDateTime > localSunrise;

                //Conditions itself
                string conditions = (string)data["days"][0]["conditions"];

                if (conditions.ToLower().Contains("clear"))
                {
                    if (isDay)
                    {
                        ConditionImage.Source =
                            new BitmapImage(new Uri(path + "/images/clear_day.png"));
                    }
                    else
                    {
                        ConditionImage.Source =
                            new BitmapImage(new Uri(path + "/images/clear_night.png"));
                    }
                }

                if (conditions.ToLower().Contains("partially"))
                {
                    if (isDay)
                    {
                        ConditionImage.Source =
                            new BitmapImage(new Uri(path + "/images/partially_day.png"));
                    }
                    else
                    {
                        ConditionImage.Source =
                            new BitmapImage(new Uri(path + "/images/partially_night.png"));
                    }
                }

                if (conditions.ToLower().Contains("overcast"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/overcast.png"));
                }

                if (conditions.ToLower().Contains("thunderstorm"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/thunderstorm.png"));
                }

                if (conditions.ToLower().Contains("rain"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/rain.png"));
                }

                if (conditions.ToLower().Contains("snow"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/snow.png"));
                }

                if (conditions.ToLower().Contains("snow") && conditions.ToLower().Contains("rain"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/snowandrain.png"));
                }

                if (conditions.ToLower().Contains("fog"))
                {
                    ConditionImage.Source =
                        new BitmapImage(new Uri(path + "/images/fog.png"));
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Directory containing image files has not been found!\nReinstall Weatherly.",
                    "Weatherly", MessageBoxButton.OK, MessageBoxImage.Error);

                switch (result)
                {
                    case MessageBoxResult.OK:
                        Environment.Exit(1);
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unknown exception occurred!\n{e}", "Weatherly", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);
            }

            //Display weather data
            Address.Content = (string)data["resolvedAddress"];

            /*ConsoleAllocator.ShowConsoleWindow();
            Console.WriteLine(data);*/

            var datetimeList = data["days"].Select(m => (string)m.SelectToken("datetime")).ToList();
            
            var tempList = data["days"].Select(m => (string)m.SelectToken("temp")).ToList();
            var tempMaxList = data["days"].Select(m => (string)m.SelectToken("tempmax")).ToList();
            var tempMinList = data["days"].Select(m => (string)m.SelectToken("tempmin")).ToList();
            var humidityList = data["days"].Select(m => (string)m.SelectToken("humidity")).ToList();
            var windSpeedList = data["days"].Select(m => (string)m.SelectToken("windspeed")).ToList();
            var windDirList = data["days"].Select(m => (string)m.SelectToken("winddir")).ToList();
            var visibilityList = data["days"].Select(m => (string)m.SelectToken("visibility")).ToList();
            var uvIndexList = data["days"].Select(m => (string)m.SelectToken("uvindex")).ToList();
            var sunriseList = data["days"].Select(m => (string)m.SelectToken("sunrise")).ToList();
            var sunsetList = data["days"].Select(m => (string)m.SelectToken("sunset")).ToList();

            Datetime.Content = datetimeList[dayIndex];

            switch (unitGroup)
            {
                case "us":
                    Temp.Content = tempList[dayIndex] + "°F";
                    TempMax.Content = tempMaxList[dayIndex] + "°F";
                    TempMin.Content = tempMinList[dayIndex] + "°F";
                    Humidity.Content = humidityList[dayIndex] + "%";
                    WindSpeed.Content = windSpeedList[dayIndex] + "mp/h";
                    WindDir.Content = windDirList[dayIndex] + "°";
                    Visibility.Content = visibilityList[dayIndex] + "mi";
                    UvIndex.Content = uvIndexList[dayIndex];
                    Sunrise.Content = sunriseList[dayIndex];
                    Sunset.Content = sunsetList[dayIndex];
                    break;
                case "metric":
                    Temp.Content = tempList[dayIndex] + "°C";
                    TempMax.Content = tempMaxList[dayIndex] + "°C";
                    TempMin.Content = tempMinList[dayIndex] + "°C";
                    Humidity.Content = humidityList[dayIndex] + "%";
                    WindSpeed.Content = windSpeedList[dayIndex] + "km/h";
                    WindDir.Content = windDirList[dayIndex] + "°";
                    Visibility.Content = visibilityList[dayIndex] + "km";
                    UvIndex.Content = uvIndexList[dayIndex];
                    Sunrise.Content = sunriseList[dayIndex];
                    Sunset.Content = sunsetList[dayIndex];
                    break;
                case "uk":
                    Temp.Content = tempList[dayIndex] + "°C";
                    TempMax.Content = tempMaxList[dayIndex] + "°C";
                    TempMin.Content = tempMinList[dayIndex] + "°C";
                    Humidity.Content = humidityList[dayIndex] + "%";
                    WindSpeed.Content = windSpeedList[dayIndex] + "mp/h";
                    WindDir.Content = windDirList[dayIndex] + "°";
                    Visibility.Content = visibilityList[dayIndex] + "mi";
                    UvIndex.Content = uvIndexList[dayIndex];
                    Sunrise.Content = sunriseList[dayIndex];
                    Sunset.Content = sunsetList[dayIndex];
                    break;
            }
        }

        private void LoadWeatherData(object sender, RoutedEventArgs e)
        {
            if (TextBox.Text == "") return;

            unitGroup = "";

            if (UsSwitch.IsChecked != null && (bool)UsSwitch.IsChecked)
            {
                unitGroup = "us";
            }
            else if (MetricSwitch.IsChecked != null && (bool)MetricSwitch.IsChecked)
            {
                unitGroup = "metric";
            }

            if (UkSwitch.IsChecked != null && (bool)UkSwitch.IsChecked)
            {
                unitGroup = "uk";
            }

            PreviousDay.Visibility = System.Windows.Visibility.Visible;
            NextDay.Visibility = System.Windows.Visibility.Visible;

            DownloadData(TextBox.Text, unitGroup);

            if (data != null)
            {
                DisplayWeatherData();
            }
        }

        private void LoadWeatherData_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadWeatherData(sender, e);
            }
        }

        private void DownloadData(string city, string unitGroup)
        {
            data = null;

            try
            {
                var downloadedData = wc.DownloadData(
                    $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{city}?unitGroup={unitGroup}&key=J2HBVEN5P5DRPCYPHGED5VWM6&contentType=json");

                var convertedData = Encoding.UTF8.GetString(downloadedData);

                data = JObject.Parse(convertedData);
            }
            catch (WebException we)
            {
                MessageBox.Show("Unknown WebException error occurred! \n " + we, "Weatherly",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show("Unknown Exception error occurred! \n " + e, "Weatherly",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit the application?", "Weatherly",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.OK:
                    Environment.Exit(0);
                    break;
            }
        }

        private void PreviousDay_OnClick(object sender, RoutedEventArgs e)
        {
            if (dayIndex == 0) return;
            
            dayIndex -= 1;

            DisplayWeatherData();
        }

        private void NextDay_OnClick(object sender, RoutedEventArgs e)
        {
            if (dayIndex >= 14) return;
            
            dayIndex += 1;

            DisplayWeatherData();
        }
    }
}