using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ItCourseDesktop.Windows;

public partial class AddEditTehnologiesWindow : Window
{
    private TextBlock _textBlockTitle;
    private Button _buttonResult;
    
    private TextBox _textBoxTitle;

    private Tehnology _currentTehnology = new Tehnology();

    public AddEditTehnologiesWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public AddEditTehnologiesWindow(Tehnology tehnology)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();

        if (tehnology != null)
        {
            _currentTehnology = tehnology;
            _textBlockTitle.Text = "EDIT TEHNOLOGY";
            _buttonResult.Content = "SAVE";
        }

        DataContext = _currentTehnology;
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _textBlockTitle = this.FindControl<TextBlock>("TextBlockTitle");
        _buttonResult = this.FindControl<Button>("ButtonResult");
        _textBoxTitle = this.FindControl<TextBox>("TextBoxTitle");
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


    private void InputElement_OnTextInput(object? sender, TextInputEventArgs e)
    {
        var tb = sender as TextBox;
        tb.BorderBrush = Brushes.Red;
    }

    private async void CreateTehnology()
    {
        string url = $"{DataManager.HostUrl}/Tehnology/Create";

        _currentTehnology.Title = _currentTehnology.Title.Replace("\n", "");
        _currentTehnology.Title = _currentTehnology.Title.Replace("\r", "");
        
        var data = new
        {
            title = _currentTehnology.Title
        };

        var json = JsonSerializer.Serialize(data);
        
        // Создаем объект StringContent с JSON и указываем тип контента
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.PostAsync(url, content);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Успех",
                        "Новая технология успешно добавлена");
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        CreateTehnology();
                        return;
                    }
                }
                else
                {
                    var message = await GetResponseText(response);
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                        message);
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                    "Ошибка подключения к интернету");
            }
        }
    }
    
    public async Task<string> GetResponseText(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return text;
    }

    private async void EditTehnology()
    {
        string url = $"{DataManager.HostUrl}/Tehnology/Edit";
        
        _currentTehnology.Title = _currentTehnology.Title.Replace("\n", "");
        _currentTehnology.Title = _currentTehnology.Title.Replace("\r", "");
        
        var data = new
        {
            id = _currentTehnology.Id,
            title = _currentTehnology.Title
        };

        var json = JsonSerializer.Serialize(data);
        
        // Создаем объект StringContent с JSON и указываем тип контента
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.PutAsync(url, content);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Успех",
                        "Технология успешно изменена");
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        EditTehnology();
                        return;
                    }
                }
                else
                {
                    string message = await GetResponseText(response);
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                        message);
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                    "Ошибка подключения к интернету");
            }
        }
    }

    private async void ButtonResult_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_textBoxTitle.Text))
        {
            MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                "Заполните название");
            return;

        }

        Button btn = sender as Button;
        btn.IsEnabled = false;

        if (_currentTehnology.Id == 0)
        {
            CreateTehnology();
            btn.IsEnabled = true;
        }
        
        else if (_currentTehnology.Id != 0)
        {
            EditTehnology();
            btn.IsEnabled = true;
        }

    }
}