using System;
using System.Collections.Generic;
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
using Avalonia.Threading;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ItCourseDesktop.Windows;

public partial class AddEditCategoryWindow : Window
{
    private TextBlock _textBlockTitle;
    private Button _buttonResult;

    private TextBox _textBoxTitle;

    private Category _currentCategory = new Category();

    public AddEditCategoryWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public AddEditCategoryWindow(Category category)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();

        if (category != null)
        {
            _currentCategory = category;
            _textBlockTitle.Text = "EDIT CATEGORY";
            _buttonResult.Content = "SAVE";
        }

        DataContext = _currentCategory;
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

    private async void CreateCategory()
    {
        string url = $"{DataManager.HostUrl}/Category/Create";
        _currentCategory.Title = _currentCategory.Title.Replace("\n", "");
        _currentCategory.Title = _currentCategory.Title.Replace("\r", "");

        var data = new
        {
            title = _currentCategory.Title
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
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok, "Успех",
                        "Новая категория успешно добавлена");
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        CreateCategory();
                        return;
                    }
                }
                else
                {
                    var message = await GetResponseText(response);
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                        message);
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                    MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                    "Ошибка подключения к интернету");
            }
        }
    }

    public async Task<string> GetResponseText(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return text;
    }

    private async void EditCategory()
    {
        string url = $"{DataManager.HostUrl}/Category/Edit";

        _currentCategory.Title = _currentCategory.Title.Replace("\n", "");
        _currentCategory.Title = _currentCategory.Title.Replace("\r", "");

        var data = new
        {
            id = _currentCategory.Id,
            title = _currentCategory.Title
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
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok, "Успех",
                        "Категория успешно изменена");
                    this.Close();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        EditCategory();
                        return;
                    }
                }
                else
                {
                    string message = await GetResponseText(response);
                    MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                        message);
                    return;
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
                MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(this,
                    MyMessageBox.MessageBoxButtons.Ok, "Ошибка",
                    "Ошибка подключения к интернету");
                return;
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

        if (_currentCategory.Id == 0)
        {
            CreateCategory();
            btn.IsEnabled = true;
        }

        else if (_currentCategory.Id != 0)
        {
            EditCategory();
            btn.IsEnabled = true;
        }
    }
}