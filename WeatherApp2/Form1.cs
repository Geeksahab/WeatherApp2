using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace WeatherApp2
{
    public partial class Form1 : Form
    {
        static string name = string.Empty;
        private static readonly string apiKey = "1e3e8f230b6064d27976e41163a82b77"; // Replace with your OpenWeatherMap API key
        private static readonly string apiBaseUrl = "http://api.openweathermap.org/data/2.5/weather";

        // UI Controls
        private Label lblTitle;
        private Label lblName;
        private Label lblCity;
        private TextBox txtName;
        private TextBox txtCity;
        private Button btnGetWeather;
        private Panel weatherPanel;
        private Label lblWeatherDetails;
        private PictureBox weatherIcon;
        private Label lblStatus;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Form properties
            this.Text = "Weather Forecast App";
            this.Size = new Size(500, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.AliceBlue;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Title
            lblTitle = new Label
            {
                Text = "Weather Forecast App",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.RoyalBlue,
                AutoSize = true,
                Location = new Point(25, 20)
            };
            Controls.Add(lblTitle);

            // Name Label
            lblName = new Label
            {
                Text = "Your Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(25, 70)
            };
            Controls.Add(lblName);

            // Name TextBox
            txtName = new TextBox
            {
                Location = new Point(130, 70),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtName);

            // City Label
            lblCity = new Label
            {
                Text = "City Name:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(25, 110)
            };
            Controls.Add(lblCity);

            // City TextBox
            txtCity = new TextBox
            {
                Location = new Point(130, 110),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtCity);

            // Get Weather Button
            btnGetWeather = new Button
            {
                Text = "Get Weather",
                Location = new Point(130, 150),
                Size = new Size(200, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGetWeather.Click += new EventHandler(btnGetWeather_Click);
            Controls.Add(btnGetWeather);

            // Status Label
            lblStatus = new Label
            {
                Text = "Enter your name and city to get weather information",
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(130, 195)
            };
            Controls.Add(lblStatus);

            // Weather Panel
            weatherPanel = new Panel
            {
                Location = new Point(25, 225),
                Size = new Size(440, 175),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Visible = false
            };
            Controls.Add(weatherPanel);

            // Weather Icon
            weatherIcon = new PictureBox
            {
                Size = new Size(64, 64),
                Location = new Point(20, 20),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            weatherPanel.Controls.Add(weatherIcon);

            // Weather Details Label
            lblWeatherDetails = new Label
            {
                AutoSize = true,
                Location = new Point(100, 20),
                Size = new Size(330, 150),
                Font = new Font("Segoe UI", 10F)
            };
            weatherPanel.Controls.Add(lblWeatherDetails);
        }

        private async void btnGetWeather_Click(object sender, EventArgs e)
        {
            name = string.IsNullOrEmpty(txtName.Text) ? "Guest" : txtName.Text;
            string city = txtCity.Text.Trim();

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show("Please enter a city name.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnGetWeather.Enabled = false;
                lblStatus.Text = "Fetching weather data...";
                lblStatus.ForeColor = Color.Blue;

                var weatherData = await GetWeatherDataAsync(city);
                DisplayWeatherInfo(weatherData);

                lblStatus.Text = "Weather data updated successfully!";
                lblStatus.ForeColor = Color.Green;
                weatherPanel.Visible = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Failed to get weather data.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Error fetching weather data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                weatherPanel.Visible = false;
            }
            finally
            {
                btnGetWeather.Enabled = true;
            }
        }

        static async Task<JObject> GetWeatherDataAsync(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{apiBaseUrl}?q={city}&appid={apiKey}&units=metric";
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JObject.Parse(responseBody);
            }
        }

        private void DisplayWeatherInfo(JObject weatherData)
        {
            string city = weatherData["name"].ToString();
            string country = weatherData["sys"]["country"].ToString();
            string description = weatherData["weather"][0]["description"].ToString();
            double temperature = (double)weatherData["main"]["temp"];
            double feelsLike = (double)weatherData["main"]["feels_like"];
            double humidity = (double)weatherData["main"]["humidity"];
            double windSpeed = (double)weatherData["wind"]["speed"];
            string weatherIconCode = weatherData["weather"][0]["icon"].ToString();

            // Format weather details with better layout
            lblWeatherDetails.Text = $"Hello, {name}!\n\n" +
                                     $"Weather in {city}, {country}\n" +
                                     $"Condition: {char.ToUpper(description[0]) + description.Substring(1)}\n" +
                                     $"Temperature: {temperature:F1}°C (Feels like: {feelsLike:F1}°C)\n" +
                                     $"Humidity: {humidity}%\n" +
                                     $"Wind Speed: {windSpeed} m/s";

            // Load weather icon from OpenWeatherMap
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string iconUrl = $"http://openweathermap.org/img/wn/{weatherIconCode}@2x.png";
                    byte[] imageBytes = client.GetByteArrayAsync(iconUrl).Result;
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        weatherIcon.Image = Image.FromStream(ms);
                    }
                }
            }
            catch
            {
                // If icon loading fails, don't show an icon
                weatherIcon.Image = null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set the default name if required
            txtName.Text = "Guest";
            txtCity.Focus();
        }
    }
}