using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using Newtonsoft.Json;
using PokemonsDesktop.Models;

namespace ItCourseDesktop.Windows;

public partial class AddUserWindow : Window
{
    private User _user = new User();

    private ComboBox _comboBoxRoles;
    private TextBox _textBoxUsername;
    private TextBox _textBoxPassword;


    public AddUserWindow()
    {
        InitializeComponent();
        this.AttachDevTools();

        this.DataContext = _user;
        Init();
    }

    private async void Init()
    {
        await SetComboRoles();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _comboBoxRoles = this.FindControl<ComboBox>("ComboBoxRoles");
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

    private async void ButtonAddUser_OnClick(object? sender, RoutedEventArgs e)
    {
        StringBuilder errors = new StringBuilder();

        Button btn = sender as Button;

        btn.IsEnabled = false;

        if (string.IsNullOrWhiteSpace(_user.Username))
        {
            errors.AppendLine("Укажите имя пользователя");
        }

        if (_user.Username.Contains("\n") || _user.Username.Contains("\r"))
        {
            errors.AppendLine("Удалите enter из username");
        }

        if (string.IsNullOrWhiteSpace(_user.Password))
        {
            errors.AppendLine("Укажите пароль");
        }

        if (_user.Password.Contains("\n") || _user.Password.Contains("\r"))
        {
            errors.AppendLine("Удалите enter из password");
        }

        if (_comboBoxRoles.SelectedItem == null)
        {
            errors.AppendLine("Укажите роль");
        }

        if (string.IsNullOrWhiteSpace(_user.Phone))
        {
            errors.AppendLine("Укажите телефон");
        }
        else
        {
            bool isDigitsOnly = _user.Phone.All(char.IsDigit);
            if (_user.Phone.Length != 11 || !isDigitsOnly)
            {
                errors.AppendLine("Телефон указан не верно");
            }
        }

        if (string.IsNullOrWhiteSpace(_user.Email))
        {
            errors.AppendLine("Укажите почту");
        }

        string pochtaPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        bool isEmail = Regex.IsMatch(_user.Email, pochtaPattern);
        if (!isEmail)
        {
            errors.AppendLine("формат почты не верный");
        }

        if (errors.Length > 0)
        {
            var messageBox = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Заполните поля",
                errors.ToString());
            btn.IsEnabled = true;
            return;
        }

        _user.Email = _user.Email.Replace("\n", "");
        _user.Phone = _user.Phone.Replace("\n", "");
        _user.Firstname = _user.Firstname.Replace("\n", "");
        _user.Lastname = _user.Lastname.Replace("\n", "");
        _user.Patronymic = _user.Patronymic.Replace("\n", "");

        _user.Email = _user.Email.Replace("\r", "");
        _user.Phone = _user.Phone.Replace("\r", "");
        _user.Firstname = _user.Firstname.Replace("\r", "");
        _user.Lastname = _user.Lastname.Replace("\r", "");
        _user.Patronymic = _user.Patronymic.Replace("\r", "");

        _user.RoleId = (_comboBoxRoles.SelectedItem as UserRole).Id;

        string apiUrl = $"{DataManager.HostUrl}/Account/CreateUser";
        string json = JsonConvert.SerializeObject(_user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            try
            {
                string message = GetResponseText(response).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Успех",
                        "Пользователь добавлен");
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        await SetComboRoles();
                        ButtonAddUser_OnClick(sender, e);
                        btn.IsEnabled = true;
                        return;
                    }
                }
                else
                {
                    var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                        $"{message}");
                    btn.IsEnabled = true;
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                btn.IsEnabled = true;
                await SetComboRoles();
            }
        }

        btn.IsEnabled = true;
    }

    private async Task SetComboRoles()
    {
        string apiUrl = $"{DataManager.HostUrl}/UserRole/GetAll";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                var responseValue = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var list = JsonConvert.DeserializeObject<List<UserRole>>(responseValue);

                    _comboBoxRoles.Items = list;
                    _comboBoxRoles.SelectedIndex = 2;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        await SetComboRoles();
                        return;
                    }
                }
                else
                {
                    await SetComboRoles();
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                await SetComboRoles();
            }
        }
    }

    public async Task<string> GetResponseText(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return text;
    }
}