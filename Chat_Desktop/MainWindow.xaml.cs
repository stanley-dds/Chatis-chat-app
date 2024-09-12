using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.AspNetCore.SignalR.Client;

namespace Chat_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _connection;

        public MainWindow()
        {
            InitializeComponent();
            SetupSignalR();
        }

        private async void SetupSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5159/chatHub") 
            .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessagesTextBlock.Text += $"{user}: {message}\n";
                });
            });

            try
            {
                await _connection.StartAsync();
                MessagesTextBlock.Text += "Connected to the chat hub.\n";
            }
            catch (Exception ex)
            {
                MessagesTextBlock.Text += $"Connectionnnn error: {ex.Message}\n";
            }

            
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var user = UserNameTextBox.Text;
            var message = MessageTextBox.Text;

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(message))
            {
                try
                {
                    await _connection.InvokeAsync("SendMessage", user, message);
                    MessageTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessagesTextBlock.Text += $"Error sending message: {ex.Message}\n";
                }
            }
            else
            {
                MessagesTextBlock.Text += "Username and message cannot be empty.\n";
            }
        }
    }
}
