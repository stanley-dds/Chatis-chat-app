using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace ChatDesktop
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private HubConnection _connection;
        private string _token;
        private string _userName;

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7024/")
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Registration Handler
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = RegisterUsername.Text;
            var password = RegisterPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var registerData = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    MessageBox.Show(error?.Message ?? "Registration error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Login Handler
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = LoginUsername.Text;
            var password = LoginPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loginData = new
            {
                Username = username,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/auth/login", content);
                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (contentType == "application/json")
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(data?.Token))
                    {
                        _token = data.Token;
                        _userName = username; // Save username
                        MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await InitializeSignalR();
                        ShowChat();
                    }
                    else
                    {
                        MessageBox.Show(data?.Message ?? "Login error.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var text = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Login error: unexpected server response.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine("Unexpected server response:", text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Initialize SignalR Connection
        private async Task InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7024/chatHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var msg = new TextBlock
                    {
                        Text = $"{user}: {message}",
                        TextWrapping = TextWrapping.Wrap
                    };
                    MessagesList.Children.Add(msg);
                });
            });

            _connection.Closed += async (error) =>
            {
                MessageBox.Show("Connection to chat was closed. Attempting to reconnect...", "Connection Closed", MessageBoxButton.OK, MessageBoxImage.Warning);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    MessageBox.Show("Connection restored.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Reconnection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            try
            {
                await _connection.StartAsync();
                Console.WriteLine("Connected to SignalR hub");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chat connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Send Message Handler
        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var message = MessageInput.Text;

            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Please enter a message.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_connection.State != HubConnectionState.Connected)
            {
                MessageBox.Show("Connection is not active. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                await _connection.InvokeAsync("SendMessage", _userName, message);

                // Add message to chat
                var msg = new TextBlock
                {
                    Text = $"You: {message}",
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Bold
                };
                MessagesList.Children.Add(msg);
                MessageInput.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Show Chat Section and Hide Authentication
        private void ShowChat()
        {
            AuthGrid.Visibility = Visibility.Collapsed;
            ChatGrid.Visibility = Visibility.Visible;
        }

        // Classes for Deserializing Responses
        private class LoginResponse
        {
            public string Token { get; set; }
            public string Message { get; set; }
        }

        private class SendMessageResponse
        {
            public string Message { get; set; }
        }

        private class ErrorResponse
        {
            public string Message { get; set; }
        }

        // Event Handlers for Managing Tooltip Visibility
        private void LoginUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Additional logic when text changes, if needed
        }

        private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Additional logic when password changes, if needed
        }

        private void RegisterUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Additional logic when text changes, if needed
        }

        private void RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Additional logic when password changes, if needed
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Additional logic when text changes, if needed
        }
    }

    //// Visibility Converters
    //public class TextEmptyToVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var text = value as string;
    //        return string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class PasswordEmptyToVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var password = value as string;
    //        return string.IsNullOrEmpty(password) ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
