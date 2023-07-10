using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using ItCourseDesktop.Windows;
using Newtonsoft.Json;

namespace ItCourseDesktop.Pages;

public partial class TehnologiesPage : UserControl
{
    private ListBox _listBoxTehnology;
    private TextBox _textBoxTitleFound;
    private Button _buttonOrderByTitle;
    private TextBlock _textBlockNoContent;

    private Button _buttonLastPage;
    private Button _buttonNextPage;

    private int page = 1;
    private int count = 10;


    public TehnologiesPage()
    {
        InitializeComponent();
        SetTehnologiesListBox();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _listBoxTehnology = this.FindControl<ListBox>("TehnologyListBox");
        _textBoxTitleFound = this.FindControl<TextBox>("TextBoxTitleFound");
        _buttonOrderByTitle = this.FindControl<Button>("ButtonOrderByTitle");
        _textBlockNoContent = this.FindControl<TextBlock>("TextBlockNoContent");
        
        _buttonLastPage = this.FindControl<Button>("ButtonLastPage");
        _buttonNextPage = this.FindControl<Button>("ButtonNextPage");
    }

    private void ButtonOrderByTitle_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;

        switch (btn.Tag.ToString())
        {
            case "ASC":
                btn.Tag = "DESC";
                SetTehnologiesListBox();
                break;

            case "DESC":
                btn.Tag = "ASC";
                SetTehnologiesListBox();
                break;
        }
    }

    private void ButtonAddEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var context = btn.DataContext as Tehnology;

        AddEditTehnologiesWindow addEditTehnologiesWindow = new AddEditTehnologiesWindow(context);

        addEditTehnologiesWindow.Closed += (sender, e) => { SetTehnologiesListBox(); };
        addEditTehnologiesWindow.ShowDialog(DataManager.mainWindow);
    }

    private async void SetTehnologiesListBox()
    {
        string title = _textBoxTitleFound.Text;
        bool sordByASC = true;

        if (_buttonOrderByTitle.Tag.ToString() == "DESC")
        {
            sordByASC = false;
        }

        string apiUrl = $"{DataManager.HostUrl}/Tehnology";

        if (!string.IsNullOrWhiteSpace(title))
        {
            apiUrl += $"/GetAllByPageAndFound?page={page}&count={count}&sortByASC={sordByASC}&title={title}";
        }
        else
        {
            apiUrl += $"/GetAllByPage?page={page}&count={count}&sortByASC={sordByASC}";
        }

        await Task.Run(async () =>
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                try
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _textBlockNoContent.IsVisible = false;
                            _listBoxTehnology.IsVisible = true;
                        });

                        var responseValue = response.Content.ReadAsStringAsync().Result;
                        var list = JsonConvert.DeserializeObject<List<Tehnology>>(responseValue);
                        Dispatcher.UIThread.Post(() => { _listBoxTehnology.Items = list; });
                    }
                    else if(response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        if (page > 1)
                        {
                            --page;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _listBoxTehnology.IsVisible = false;
                            _textBlockNoContent.IsVisible = true;
                        });
                        if (page > 1)
                        {
                            --page;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        if (page > 1)
                        {
                            --page;
                        }
                    }
                }
                //  если нет интернета
                catch (Exception ex)
                {
                    if (page > 1)
                    {
                        --page;
                    }
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                _buttonLastPage.IsVisible = true;
                _buttonNextPage.IsVisible = true;
            });
        });
    }

    private async void ButtonPage_OnClick(object? sender, RoutedEventArgs e)
    {
        string bntName = (sender as Button).Name;

        switch (bntName)
        {
            case "ButtonLastPage":
                if (page > 1)
                {
                    --page;
                    _buttonLastPage.IsVisible = false;
                    _buttonNextPage.IsVisible = false;
                    SetTehnologiesListBox();
                }

                break;

            case "ButtonNextPage":
                page++;
                _buttonLastPage.IsVisible = false;
                _buttonNextPage.IsVisible = false;
                SetTehnologiesListBox();

                break;
        }
    }

    private async void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        MyMessageBox.MessageBoxResult result = await MyMessageBox.CreateDialog(DataManager.mainWindow,
            MyMessageBox.MessageBoxButtons.YesNo, "Безвозвратно удалить объект?",
            "Объект будет удалён, действие нельзя будет отменить");
        if (result == MyMessageBox.MessageBoxResult.No || result == MyMessageBox.MessageBoxResult.Cancel)
        {
            return;
        }

        Button btn = sender as Button;
        int tehnologyId = int.Parse(btn.Tag.ToString());

        string apiUrl = $"{DataManager.HostUrl}/Tehnology/Delete";
        string fullUrl = $"{apiUrl}?id={tehnologyId}";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.DeleteAsync(fullUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MyMessageBox.MessageBoxResult res = await MyMessageBox.CreateDialog(DataManager.mainWindow,
                        MyMessageBox.MessageBoxButtons.Ok, "Успех", "Технология успешно удалена");
                    SetTehnologiesListBox();
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        ButtonDelete_OnClick(sender, e);
                        return;
                    }
                }
                else
                {
                    string message = await GetResponseText(response);
                    MyMessageBox.MessageBoxResult res = await MyMessageBox.CreateDialog(DataManager.mainWindow,
                        MyMessageBox.MessageBoxButtons.Ok, "Ошибка", message);
                    return;
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
    }

    public async Task<string> GetResponseText(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return text;
    }

    private void TextBoxTitleFound_OnKeyUp(object? sender, KeyEventArgs e)
    {
        SetTehnologiesListBox();
    }
}