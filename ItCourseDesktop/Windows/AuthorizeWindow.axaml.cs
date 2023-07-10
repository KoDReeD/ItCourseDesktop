using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ItCourseDesktop.Data;
using ItCourseDesktop.ViewModels;
using Newtonsoft.Json;

namespace ItCourseDesktop.Windows;

public partial class AuthorizeWindow : Window
{
    public TextBox _textBoxPass;
    public TextBox _textBoxUsername;
    public Image ImageHidePass;

    public AuthorizeWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _textBoxPass = this.FindControl<TextBox>("TextBoxPassword");
        _textBoxUsername = this.FindControl<TextBox>("TextBoxUsername");
        ImageHidePass = this.FindControl<Image>("ImagePassHide");
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }


    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ButtonWinSize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
        }
        else
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }
    }

    private void ButtonHidePassword_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_textBoxPass.PasswordChar == '\0')
        {
            string path = @"..\..\..\Assets\eye.png";
            ImageHidePass.Source = new Bitmap(path);
            _textBoxPass.PasswordChar = '*';
        }
        else
        {
            string path = @"..\..\..\Assets\hide_eye.png";
            ImageHidePass.Source = new Bitmap(path);
            _textBoxPass.PasswordChar = '\0';
        }
    }

    private async void ButtonLogIn_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        btn.IsEnabled = false;

        string pass = _textBoxPass.Text;
        string username = _textBoxUsername.Text;

        if (string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(username))
        {
            var message = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", "Заполните все поля");
            btn.IsEnabled = true;
            return;
        }

        var json = new
        {
            Username = username,
            Password = pass
        };

        // using (HttpClient httpClient = new HttpClient())
        // {
        //     string apiUrl = "";
        //     HttpResponseMessage response = await httpClient.PostAsJsonAsync(apiUrl, json);
        //     try
        //     {
        //         var responseValue = response.Content.ReadAsStringAsync().Result;
        //         if (response.StatusCode == HttpStatusCode.OK)
        //         {
        //             var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseValue);
        //         }
        //         else if (response.StatusCode == HttpStatusCode.BadRequest)
        //         {
        //             
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         btn.IsEnabled = true;
        //     }
        // }

        using (HttpClient client = new HttpClient())
        {
            string apiUrl = $"{DataManager.HostUrl}/Account/Authorization?";

            HttpResponseMessage response = await client.PostAsJsonAsync(apiUrl, json);
            try
            {
                var responseValue = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseValue);
                    DataManager.User = authResponse;

                    if (DataManager.User.RoleId != 1)
                    {
                        var message = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                            "Ошибка доступа", "Доступ запрещён вы не администратор");
                        btn.IsEnabled = true;
                        return;
                    }

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                        "Ошибка", responseValue);
                    btn.IsEnabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                btn.IsEnabled = true;
            }
        }
    }
}