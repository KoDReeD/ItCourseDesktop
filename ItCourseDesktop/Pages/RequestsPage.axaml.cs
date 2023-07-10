using System;
using System.Collections.Generic;
using System.Linq;
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

public partial class RequestsPage : UserControl
{
    private int page = 1;
    private int count = 10;

    private ListBox _listBoxRequest;
    private ComboBox _comboBoxStatus;
    private TextBox _textBoxFoundText;
    private Button _buttonOrderByDate;
    private TextBlock _textBlockNoContent;

    private Button _buttonLastPage;
    private Button _buttonNextPage;

    public RequestsPage()
    {
        InitializeComponent();
        Init();
    }

    private async Task Init()
    {
        await SetStatusComboBox();
        await SetRequestListBox();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _listBoxRequest = this.FindControl<ListBox>("ListBoxRequest");
        _comboBoxStatus = this.FindControl<ComboBox>("ComboBoxStatus");
        _textBoxFoundText = this.FindControl<TextBox>("TextBoxTextFound");
        _buttonOrderByDate = this.FindControl<Button>("ButtonOrderByDate");
        _textBlockNoContent = this.FindControl<TextBlock>("TextBlockNoContent");

        _buttonLastPage = this.FindControl<Button>("ButtonLastPage");
        _buttonNextPage = this.FindControl<Button>("ButtonNextPage");
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    public async Task SetRequestListBox()
    {
        string text = _textBoxFoundText.Text;
        bool sordByASC = true;

        int statusId;

        statusId = (_comboBoxStatus.SelectedItem as Status).Id;
        
        try
        {
            statusId = (_comboBoxStatus.SelectedItem as Status).Id;
        }
        catch (Exception e)
        {
            return;
        }

        if (_buttonOrderByDate.Tag.ToString() == "DESC")
        {
            sordByASC = false;
        }

        string apiUrl = $"{DataManager.HostUrl}/Request";

        if (!string.IsNullOrWhiteSpace(text))
        {
            apiUrl += $"/GetAllByPageByText?page={page}&count={count}&orderByASC={sordByASC}&statusId={statusId}&text={text}";
        }
        else
        {
            apiUrl += $"/GetAllByPage?page={page}&count={count}&orderByASC={sordByASC}&statusId={statusId}";
        }
        
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                try
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _textBlockNoContent.IsVisible = false;
                            _listBoxRequest.IsVisible = true;
                        });

                        var responseValue = response.Content.ReadAsStringAsync().Result;
                        var list = JsonConvert.DeserializeObject<List<RequestListBox>>(responseValue);
                        Dispatcher.UIThread.Post(() =>
                        {
                            _listBoxRequest.Items = list.Select(x => new
                            {
                                x.Id,
                                x.Username,
                                x.Course,
                                FIO = $"{x.Lastname} {x.Firstname} {x.Patronymic}",
                                x.RequestDate,
                                x.Status,
                                x.UserPhone,
                                x.UserEmail,
                                x.Comment
                            });
                        });
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                        if (flag == true)
                        {
                            SetRequestListBox();
                            return;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
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
                            _listBoxRequest.IsVisible = false;
                            _textBlockNoContent.IsVisible = true;
                        });
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
    }

    private async Task SetStatusComboBox()
    {
        string apiUrl = $"{DataManager.HostUrl}/Status/GetAll";
        //  получение категорий
        // await Task.Run(async () =>
        // {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<List<Status>>(responseValue);
                    list.Insert(0, new Status()
                    {
                        Id = 0,
                        Title = "Все"
                    });
                    // Dispatcher.UIThread.InvokeAsync(() => {  });
                    _comboBoxStatus.Items = list;
                    _comboBoxStatus.SelectedIndex = 0;
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
        // });
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
                    await SetRequestListBox();
                }

                break;

            case "ButtonNextPage":
                page++;
                _buttonLastPage.IsVisible = false;
                _buttonNextPage.IsVisible = false;
                await SetRequestListBox();
                break;
        }
    }

    private async void ComboBoxStatus_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await SetRequestListBox();
    }

    private async void TextBoxTextFound_OnKeyUp(object? sender, KeyEventArgs e)
    {
        await SetRequestListBox();
    }

    private async void ButtonOrderByDate_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;

        switch (btn.Tag.ToString())
        {
            case "ASC":
                btn.Tag = "DESC";
                await SetRequestListBox();
                break;

            case "DESC":
                btn.Tag = "ASC";
                await SetRequestListBox();
                break;
        }
    }

    private void ButtonChangeStatus_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        int reqId = int.Parse(btn.Tag.ToString());

        ChangeStatusWindow changeStatusWindow = new ChangeStatusWindow(reqId);
        changeStatusWindow.Closed += (o, args) => { SetRequestListBox(); };
        changeStatusWindow.ShowDialog(DataManager.mainWindow);
    }
}