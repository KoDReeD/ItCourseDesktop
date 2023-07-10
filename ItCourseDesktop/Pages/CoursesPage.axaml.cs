using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using ItCourseDesktop.ViewModels;
using ItCourseDesktop.Windows;
using Newtonsoft.Json;  
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace ItCourseDesktop.Pages;

public partial class CoursesPage : UserControl
{
    private ListBox _listBoxCourses;
    private ComboBox _comboBoxCategories;
    private TextBox _textBoxFoundTitle;
    private Button _buttonOrderByTitle;
    private TextBlock _textBlockNoContent;

    private Button _buttonLastPage;
    private Button _buttonNextPage;

    private int page = 1;
    private int count = 10;

    private bool _isAuthorizationTriggered = false;

    public CoursesPage()
    {
        InitializeComponent();

        Init();
    }

    public async void Init()
    {
        await SetComboBoxCategories();
        await SetCourseListBox();
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _listBoxCourses = this.FindControl<ListBox>("CourseCard");
        _comboBoxCategories = this.FindControl<ComboBox>("ComboBoxCategories");
        _textBoxFoundTitle = this.FindControl<TextBox>("TextBoxFoundTitle");
        _buttonOrderByTitle = this.FindControl<Button>("ButtonOrderByTitle");
        _textBlockNoContent = this.FindControl<TextBlock>("TextBlockNoContent");

        _buttonLastPage = this.FindControl<Button>("ButtonLastPage");
        _buttonNextPage = this.FindControl<Button>("ButtonNextPage");
    }


    public async Task SetCourseListBox()
    {
        string title = _textBoxFoundTitle.Text;
        bool sordByASC = true;

        if (_buttonOrderByTitle.Tag.ToString() == "DESC")
        {
            sordByASC = false;
        }

        int categoryId = (_comboBoxCategories.SelectedItem as Category).Id;
        string apiUrl = $"{DataManager.HostUrl}/Course";

        if (!string.IsNullOrWhiteSpace(title))
        {
            apiUrl +=
                $"/GetAllByPageAndFound?page={page}&count={count}&sortByASC={sordByASC}&title={title}&categoryId={categoryId}";
        }
        else
        {
            apiUrl += $"/GetAllByPage?page={page}&count={count}&sortByASC={sordByASC}&categoryId={categoryId}";
        }

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);

            var response = await client.GetAsync(apiUrl);

            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _textBlockNoContent.IsVisible = false;
                    _listBoxCourses.IsVisible = true;

                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<List<CourseListBox>>(responseValue);

                    foreach (var course in list)
                    {
                        try
                        {
                            using (var httpClient = new HttpClient())
                            {
                                byte[] imageData = await httpClient.GetByteArrayAsync(course.ImagePath);
            
                                using (var stream = new MemoryStream(imageData))
                                {
                                    course.Image = new Bitmap(stream);
                                }
                            }
                        }
                        catch
                        {
                            course.Image = new Bitmap("../../../Assets/noImage.jpg");
                        }
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        _listBoxCourses.Items = list;
                    });
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        await SetComboBoxCategories();
                        await SetCourseListBox();
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
                    _listBoxCourses.IsVisible = false;
                    _textBlockNoContent.IsVisible = true;
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

            Dispatcher.UIThread.Post(() =>
            {
                _buttonLastPage.IsVisible = true;
                _buttonNextPage.IsVisible = true;
            });
        }
    }

    private async void ButtonOrderByTitle_OnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;

        switch (btn.Tag.ToString())
        {
            case "ASC":
                btn.Tag = "DESC";
                await SetCourseListBox();
                break;

            case "DESC":
                btn.Tag = "ASC";
                await SetCourseListBox();
                break;
        }
    }

    private async Task SetComboBoxCategories()
    {
        string apiUrl = $"{DataManager.HostUrl}/Category/GetAll";
        //  получение категорий
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<List<Category>>(responseValue);
                    list.Insert(0, new Category()
                    {
                        Id = 0,
                        Title = "Все"
                    });
                    _comboBoxCategories.Items = list;
                    _comboBoxCategories.SelectedIndex = 0;
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
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
                    await SetCourseListBox();
                }

                break;

            case "ButtonNextPage":
                page++;
                _buttonLastPage.IsVisible = false;
                _buttonNextPage.IsVisible = false;
                await SetCourseListBox();
                break;
        }
    }

    private async void ComboBoxCategories_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await SetCourseListBox();
    }

    private void ButtonAddCourse_OnClick(object? sender, RoutedEventArgs e)
    {
        AddEditCourseWindow addEditCourseWindow = new AddEditCourseWindow();
        addEditCourseWindow.Closed += (o, args) => { SetCourseListBox(); };
        addEditCourseWindow.ShowDialog(DataManager.mainWindow);
    }

    private void TextBoxFoundTitle_OnKeyUp(object? sender, KeyEventArgs e)
    {
        SetCourseListBox();
    }
}