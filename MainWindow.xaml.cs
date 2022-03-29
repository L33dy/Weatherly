using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
            try
            {
                Icon = new BitmapImage(new Uri(path + "/images/icon.png"));
            }
            catch (DirectoryNotFoundException)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Directory containing image files has not been found!\nReinstall Weatherly.",
                    "Weatherly", MessageBoxButton.OK, MessageBoxImage.Error);

                if (result == MessageBoxResult.OK) Environment.Exit(1);
            }

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

                var sunsets = data["days"].Select(m => (DateTime)m.SelectToken("sunset")).ToList();
                var sunrises = data["days"].Select(m => (DateTime)m.SelectToken("sunrise")).ToList();
                DateTime localSunset = sunsets[dayIndex];
                DateTime localSunrise = sunrises[dayIndex];

                var isDay = localDateTime < localSunset && localDateTime > localSunrise;

                //Conditions
                DetermineConditions();


            }
            catch (Exception e)
            {
                MessageBox.Show($"Unknown exception occurred!\n{e}", "Weatherly", MessageBoxButton.OKCancel,
                    MessageBoxImage.Error);

                TextBox.Clear();
            }

            //Display weather data
            Address.Content = (string)data["resolvedAddress"];

            var datetimeList = data["days"].Select(m => (DateTime)m.SelectToken("datetime")).ToList();

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
            
            Datetime.Content =
                $"{datetimeList[dayIndex].Day:d2}.{datetimeList[dayIndex].Month:d2}.{datetimeList[dayIndex].Year}, {datetimeList[dayIndex].DayOfWeek}";

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

            PreviousDay.Visibility = System.Windows.Visibility.Visible;
            NextDay.Visibility = System.Windows.Visibility.Visible;
        }

        private void DetermineConditions()
        {
            var conditions = data["days"].Select(m => (string)m.SelectToken("icon")).ToList();
            string condition = conditions[dayIndex];

            switch (condition.ToLower())
            {
                case "clear-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/clear-day.png"));
                    break;
                case "clear-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/clear-night.png"));
                    break;
                case "cloudy":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/cloudy.png"));
                    break;
                case "fog":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/fog.png"));
                    break;
                case "hail":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/hail.png"));
                    break;
                case "partly-cloudy-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/partly-cloudy-day.png"));
                    break;
                case "partly-cloudy-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/partly-cloudy-night.png"));
                    break;
                case "rain-snow-showers-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/rain-snow-showers-day.png"));
                    break;
                case "rain-snow-showers-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/rain-snow-showers-night.png"));
                    break;
                case "rain-snow":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/rain-snow.png"));
                    break;
                case "rain":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/rain.png"));
                    break;
                case "showers-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/showers-day.png"));
                    break;
                case "showers-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/showers-night.png"));
                    break;
                case "sleet":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/sleet.png"));
                    break;
                case "snow-showers-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/snow-showers-day.png"));
                    break;
                case "snow-showers-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/snow-showers-night.png"));
                    break;
                case "snow":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/snow.png"));
                    break;
                case "thunder-rain":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/thunder-rain.png"));
                    break;
                case "thunder-showers-day":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/thunder-showers-day.png"));
                    break;
                case "thunder-showers-night":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/thunder-showers-night.png"));
                    break;
                case "thunder":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/thunder.png"));
                    break;
                case "wind":
                    ConditionImage.Source = new BitmapImage(new Uri(path + "/images/wind.png"));
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

                TextBox.Clear();
            }
            catch (Exception e)
            {
                MessageBox.Show("Unknown Exception error occurred! \n " + e, "Weatherly",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                TextBox.Clear();
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

            //Update weather data
            DisplayWeatherData();
        }

        private void NextDay_OnClick(object sender, RoutedEventArgs e)
        {
            if (dayIndex >= 14) return;

            dayIndex += 1;

            //Update weather data
            DisplayWeatherData();
        }

        private void ClearInput(object sender, RoutedEventArgs e)
        {
            TextBox.Clear();
        }
    }
}