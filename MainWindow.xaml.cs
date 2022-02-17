using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        private dynamic data;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void DisplayWeatherData()
        {
            WeatherData.Visibility = System.Windows.Visibility.Visible;

            Address.Content = (string)data["resolvedAddress"];

            switch (unitGroup)
            {
                case "us":
                    Temp.Content = (string)data["days"][0]["temp"] + "°F";
                    TempMax.Content = (string)data["days"][0]["tempmax"] + "°F";
                    TempMin.Content = (string)data["days"][0]["tempmin"] + "°F";
                    Humidity.Content = (string)data["days"][0]["humidity"] + "%";
                    WindSpeed.Content = (string)data["days"][0]["windspeed"] + "mp/h";
                    WindDir.Content = (string)data["days"][0]["winddir"] + "°";
                    Visibility.Content = (string)data["days"][0]["visibility"] + "mi";
                    UvIndex.Content = (string)data["days"][0]["uvindex"];
                    Sunrise.Content = (string)data["days"][0]["sunrise"];
                    Sunset.Content = (string)data["days"][0]["sunset"];
                    break;
                case "metric":
                    Temp.Content = (string)data["days"][0]["temp"] + "°C";
                    TempMax.Content = (string)data["days"][0]["tempmax"] + "°C";
                    TempMin.Content = (string)data["days"][0]["tempmin"] + "°C";
                    Humidity.Content = (string)data["days"][0]["humidity"] + "%";
                    WindSpeed.Content = (string)data["days"][0]["windspeed"] + "km/h";
                    WindDir.Content = (string)data["days"][0]["winddir"] + "°";
                    Visibility.Content = (string)data["days"][0]["visibility"] + "km";
                    UvIndex.Content = (string)data["days"][0]["uvindex"];
                    Sunrise.Content = (string)data["days"][0]["sunrise"];
                    Sunset.Content = (string)data["days"][0]["sunset"];
                    break;
                case "uk":
                    Temp.Content = (string)data["days"][0]["temp"] + "°C";
                    TempMax.Content = (string)data["days"][0]["tempmax"] + "°C";
                    TempMin.Content = (string)data["days"][0]["tempmin"] + "°C";
                    Humidity.Content = (string)data["days"][0]["humidity"] + "%";
                    WindSpeed.Content = (string)data["days"][0]["windspeed"] + "mp/h";
                    WindDir.Content = (string)data["days"][0]["winddir"] + "°";
                    Visibility.Content = (string)data["days"][0]["visibility"] + "mi";
                    UvIndex.Content = (string)data["days"][0]["uvindex"];
                    Sunrise.Content = (string)data["days"][0]["sunrise"];
                    Sunset.Content = (string)data["days"][0]["sunset"];
                    break;
            }

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
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            catch (Exception e)
            {
                MessageBox.Show($"Unknown exception occurred!\n{e}", "Weatherly", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);
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
                    $"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{city}/today?unitGroup={unitGroup}&include=days&key=J2HBVEN5P5DRPCYPHGED5VWM6&contentType=json");

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
    }
}