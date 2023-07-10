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
using ItCourseDesktop.ViewModels;
using ItCourseDesktop.Windows;
using Newtonsoft.Json;

namespace ItCourseDesktop.Pages;

public partial class UsersPage : UserControl
{
    private ListBox _listBoxUsers;
    private TextBox _textBoxUsernameFound;
    private Button _buttonOrderBytitle;
    private TextBlock _textBlockNoContent;

    private Button _buttonLastPage;
    private Button _buttonNextPage;

    private int page = 1;
    private int count = 10;

    public UsersPage()
    {
        InitializeComponent();

        Init();
    }

    private async Task Init()
    {
        await SetUserListBox();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _listBoxUsers = this.FindControl<ListBox>("UserListBox");
        _textBoxUsernameFound = this.FindControl<TextBox>("TextBoxUsernameFound");
        _buttonOrderBytitle = this.FindControl<Button>("ButtonOrderByUsername");
        _textBlockNoContent = this.FindControl<TextBlock>("TextBlockNoContent");

        _buttonLastPage = this.FindControl<Button>("ButtonLastPage");
        _buttonNextPage = this.FindControl<Button>("ButtonNextPage");
    }

    private async void ButtonOrderByTitle_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;

        switch (btn.Tag.ToString())
        {
            case "ASC":
                btn.Tag = "DESC";
                await SetUserListBox();
                break;

            case "DESC":
                btn.Tag = "ASC";
                await SetUserListBox();
                break;
        }
    }

    private async Task SetUserListBox()
    {
        string title = _textBoxUsernameFound.Text;
        bool sordByASC = true;

        if (_buttonOrderBytitle.Tag.ToString() == "DESC")
        {
            sordByASC = false;
        }

        string apiUrl = $"{DataManager.HostUrl}/User";

        if (!string.IsNullOrWhiteSpace(title))
        {
            apiUrl += $"/GetAllByPageByUsername?page={page}&count={count}&sortByASC={sordByASC}&username={title}";
        }
        else
        {
            apiUrl += $"/GetAllByPage?page={page}&count={count}&sortByASC={sordByASC}";
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
                        _listBoxUsers.IsVisible = true;
                    });

                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<List<UserListBox>>(responseValue);
                    Dispatcher.UIThread.Post(() =>
                    {
                        _listBoxUsers.Items = list.Select(x => new
                        {
                            x.Id,
                            x.Username,
                            FIO = $"{x.Lastname} {x.Firsname} {x.Patronomic}",
                            x.Password,
                            x.RoleName,
                            x.Phone,
                            x.Email,
                            x.DateCreated
                        });
                    });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        await SetUserListBox();
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
                        _listBoxUsers.IsVisible = false;
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
                    await SetUserListBox();
                }

                break;

            case "ButtonNextPage":
                page++;
                _buttonLastPage.IsVisible = false;
                _buttonNextPage.IsVisible = false;
                await SetUserListBox();
                break;
        }
    }

    public async Task<string> GetResponseText(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return text;
    }

    private async void TextBoxUsernameFound_OnKeyUp(object? sender, KeyEventArgs e)
    {
        await SetUserListBox();
    }

    private void ButtonAddUser_OnClick(object? sender, RoutedEventArgs e)
    {
        AddUserWindow addUserWindow = new AddUserWindow();
        addUserWindow.Closed += async (o, args) =>
        {
            await SetUserListBox();
        };
        addUserWindow.ShowDialog(DataManager.mainWindow);
    }
}