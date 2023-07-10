using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
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

public partial class CategoriesPage : UserControl
{
    private ListBox _listBoxCategory;
    private TextBox _textBoxFoundTitle;
    private Button _buttonSort;
    private TextBlock _textBlockNoContent;
    
    private Button _buttonLastPage;
    private Button _buttonNextPage;

    private int page = 1;
    private int count = 10;

    public CategoriesPage()
    {
        InitializeComponent();
        
        Init();
    }

    private async void Init()
    {
        await SetCategoriesListBox();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _listBoxCategory = this.FindControl<ListBox>("CategoryListBox");
        _textBoxFoundTitle = this.FindControl<TextBox>("TextBoxFoundTitle");
        _buttonSort = this.FindControl<Button>("ButtonOrderByTitle");
        _textBlockNoContent = this.FindControl<TextBlock>("TextBlockNoContent");
        
        _buttonLastPage = this.FindControl<Button>("ButtonLastPage");
        _buttonNextPage = this.FindControl<Button>("ButtonNextPage");
    }

    //  Установка ListBox
    private async Task SetCategoriesListBox()
    {
        string title = _textBoxFoundTitle.Text;
        bool sordByASC = true;

        if (_buttonSort.Tag.ToString() == "DESC")
        {
            sordByASC = false;
        }

        string apiUrl = $"{DataManager.HostUrl}/Category";

        if (!string.IsNullOrWhiteSpace(title))
        {
            apiUrl += $"/GetAllByPageAndFound?page={page}&count={count}&sortByASC={sordByASC}&title={title}";
        }
        else
        {
            apiUrl += $"/GetAllByPage?page={page}&count={count}&sortByASC={sordByASC}";
        }
        
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
                            _listBoxCategory.IsVisible = true;
                        });

                        var responseValue = response.Content.ReadAsStringAsync().Result;
                        var list = JsonConvert.DeserializeObject<List<Category>>(responseValue);
                        Dispatcher.UIThread.Post(() => { _listBoxCategory.Items = list; });
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
                            _listBoxCategory.IsVisible = false;
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

    //  Кнопки пагинации
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
                    await SetCategoriesListBox();
                }

                break;

            case "ButtonNextPage":
                ++page;
                _buttonLastPage.IsVisible = false;
                _buttonNextPage.IsVisible = false;
                await SetCategoriesListBox();
                break;
        }
    }

    //  сортировка
    private async void ButtonOrderByTitle_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;

        switch (btn.Tag.ToString())
        {
            case "ASC":
                btn.Tag = "DESC";
                await SetCategoriesListBox();
                break;

            case "DESC":
                btn.Tag = "ASC";
                await SetCategoriesListBox();
                break;
        }
    }

    //  Добавление и редактирование
    private async void ButtonAddEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var context = btn.DataContext as Category;

        AddEditCategoryWindow addEditCategoryWindow = new AddEditCategoryWindow(context);

        addEditCategoryWindow.Closed += async (sender, e) =>
        {
            await SetCategoriesListBox();
        };
        addEditCategoryWindow.ShowDialog(DataManager.mainWindow);
    }

    //  удаление
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
        int categoryId = int.Parse(btn.Tag.ToString());

        string apiUrl = $"{DataManager.HostUrl}/Category/Delete";
        string fullUrl = $"{apiUrl}?id={categoryId}";

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
                        MyMessageBox.MessageBoxButtons.Ok, "Успех", "Категория успешно удалена");
                    // SetDbPages();
                    await SetCategoriesListBox();
                    return;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        await SetCategoriesListBox();
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

    private async void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
        page = 1;

        await SetCategoriesListBox();
    }
}