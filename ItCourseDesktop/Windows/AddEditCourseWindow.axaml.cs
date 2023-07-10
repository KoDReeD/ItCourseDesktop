using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using ItCourseDesktop.ViewModels;
using Newtonsoft.Json;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Net.Http.Headers;
using Avalonia.Controls.Templates;
using ItCourseAPI.DbModels;
using ItCourseAPI.DTO;

namespace ItCourseDesktop.Windows;

public partial class AddEditCourseWindow : Window
{
    private WrapPanel _wrapPanelCategory;
    private WrapPanel _wrapPanelTehnology;

    private ComboBox _comboBoxLevels;
    private List<Category> _categoriesList;
    private List<Tehnology> _tehnologiesList;

    private CourseAddVM _currentCourse = new CourseAddVM();

    private int category = 0;
    private int tehnology = 0;

    public AddEditCourseWindow()
    {
        InitializeComponent();
        this.AttachDevTools();
        
        InitializeAsync().ConfigureAwait(false);
        DataContext = _currentCourse;
    }

    public async Task InitializeAsync()
    {
        var task1 = SetComboboxLevel();
        var task2 = SetComboboxCategory();
        var task3 =  SetComboboxTehnology();

        Task.WhenAll(task1, task2, task3);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _wrapPanelCategory = this.FindControl<WrapPanel>("WrapPanelCategory");
        _wrapPanelTehnology = this.FindControl<WrapPanel>("WrapPanelTehnology");

        _comboBoxLevels = this.FindControl<ComboBox>("ComboBoxLevel");
    }

    private async Task SetComboboxLevel()
    {
        string apiUrl = $"{DataManager.HostUrl}/Level/GetAll";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    var list = JsonConvert.DeserializeObject<List<Level>>(responseValue);
                    _comboBoxLevels.Items = list;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
    }

    private async Task SetComboboxCategory()
    {
        string apiUrl = $"{DataManager.HostUrl}/Category/GetAll";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    _categoriesList = JsonConvert.DeserializeObject<List<Category>>(responseValue);
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
    }

    private async Task SetComboboxTehnology()
    {
        string apiUrl = $"{DataManager.HostUrl}/Tehnology/GetAll";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseValue = response.Content.ReadAsStringAsync().Result;
                    _tehnologiesList = JsonConvert.DeserializeObject<List<Tehnology>>(responseValue);
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                {
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
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

    //  Добавление категорий
    private void ButtonAddCategory_OnClick(object? sender, RoutedEventArgs e)
    {
        int children = _wrapPanelCategory.Children.Count;
        if (children == 6)
        {
            return;
        }

        DockPanel dockPanel = new DockPanel();
        dockPanel.LastChildFill = true;

        var comboBox = new ComboBox();
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBox.Items = _categoriesList;

        var template = new FuncDataTemplate<Category>((value, namescope) =>
            new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding("Title"),
            });
        comboBox.ItemTemplate = template;

        var button = new Button { Height = 40, Width = 40 };
        button.Click += ButtonMinusCategory_OnClick;
        button.Background = Brushes.Transparent;
        string path = @"..\..\..\Assets\delete.png";
        button.Content = new Image { Source = new Bitmap(path) };

        DockPanel.SetDock(button, Dock.Right);
        DockPanel.SetDock(comboBox, Dock.Right);

        dockPanel.Children.Add(button);
        dockPanel.Children.Add(comboBox);

        dockPanel.Margin = new Thickness(20, 0, 0, 0);

        _wrapPanelCategory.Children.Add(dockPanel);
    }

    private void ButtonMinusCategory_OnClick(object? sender, RoutedEventArgs e)
    {
        // Получаем родительский элемент DockPanel для кнопки
        DockPanel parent = (sender as Button).Parent as DockPanel;

        if (parent != null)
        {
            // Удаляем родительский элемент DockPanel из WrapPanel
            _wrapPanelCategory.Children.Remove(parent);
            category--;
        }
    }

    //  Добавление технологий
    private void ButtonAddTehnology_OnClick(object? sender, RoutedEventArgs e)
    {
        int children = _wrapPanelTehnology.Children.Count;
        if (children == 6)
        {
            return;
        }

        DockPanel dockPanel = new DockPanel();
        dockPanel.LastChildFill = true;

        var comboBox = new ComboBox();
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBox.Items = _tehnologiesList;


        var template = new FuncDataTemplate<Tehnology>((value, namescope) =>
            new TextBlock
            {
                [!TextBlock.TextProperty] = new Binding("Title"),
            });
        comboBox.ItemTemplate = template;

        var button = new Button { Height = 40, Width = 40 };
        button.Click += ButtonMinusTehnology_OnClick;
        button.Background = Brushes.Transparent;
        string path = @"..\..\..\Assets\delete.png";
        button.Content = new Image { Source = new Bitmap(path) };
// Задаем горизонтальное выравнивание кнопки в правый край докпанели
        DockPanel.SetDock(button, Dock.Right);
        DockPanel.SetDock(comboBox, Dock.Right);

        dockPanel.Children.Add(button);
        dockPanel.Children.Add(comboBox);

        dockPanel.Margin = new Thickness(20, 0, 0, 0);

        _wrapPanelTehnology.Children.Add(dockPanel);
    }

    private void ButtonMinusTehnology_OnClick(object? sender, RoutedEventArgs e)
    {
        // Получаем родительский элемент DockPanel для кнопки
        DockPanel parent = (sender as Button).Parent as DockPanel;

        if (parent != null)
        {
            // Удаляем родительский элемент DockPanel из WrapPanel
            _wrapPanelTehnology.Children.Remove(parent);
            tehnology--;
        }
    }

    private async void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        StringBuilder errors = new StringBuilder();
        
        Button btn = sender as Button;
        btn.IsEnabled = false;
        if (_currentCourse.Price <= 0)
        {
            errors.AppendLine("Неверное значение стоимости");
            // var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
            //     "Ошибка", "Неверное значение стоимости");
            // btn.IsEnabled = true;
            // return;
        }

        if (_currentCourse.Duration <= 0)
        {
            errors.AppendLine("Неверное значение длительности");
            // var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
            //     "Ошибка", "Неверное значение длительности");
            // btn.IsEnabled = true;
            // return;
        }

        if (string.IsNullOrWhiteSpace(_currentCourse.Title))
        {
            errors.AppendLine("Название не может быть пустым");
            // var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
            //     "Ошибка", "Название не может быть пустым");
            // btn.IsEnabled = true;
            // return;
        }
        
        if (string.IsNullOrWhiteSpace(_currentCourse.Description))
        {
            errors.AppendLine("Описание не может быть пустым");
            // var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
            //     "Ошибка", "Описание не может быть пустым");
            // btn.IsEnabled = true;
            // return;
        }
        
        if (_comboBoxLevels.SelectedItem == null)
        {
            errors.AppendLine("Выберите уровень");
            // var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
            //     "Ошибка", "Выберите уровень");
            // btn.IsEnabled = true;
            // return;
        }

        if (errors.Length > 0)
        {
            var res = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", errors.ToString());
            btn.IsEnabled = true;
            return;
        }
        
        _currentCourse.Title = _currentCourse.Title.Replace("\n", "");
        _currentCourse.Description = _currentCourse.Description.Replace("\n", "");
        
        _currentCourse.Title = _currentCourse.Title.Replace("\r", "");
        _currentCourse.Description = _currentCourse.Description.Replace("\r", "");

        _currentCourse.Tehnologies = new List<Tehnology>();
        _currentCourse.Categories = new List<Category>();


        //  категории
        if (_wrapPanelCategory.Children.Count > 0)
        {
            foreach (DockPanel dockPanel in _wrapPanelCategory.Children)
            {
                foreach (ComboBox comboBox in dockPanel.Children.OfType<ComboBox>())
                {
                    var item = comboBox.SelectedItem as Category;
                    if (item == null)
                    {
                        var result = await MyMessageBox.CreateDialog(this,
                            MyMessageBox.MessageBoxButtons.Ok,
                            "Ошибка", "Проверьте выбор категорий");
                        btn.IsEnabled = true;
                        return;
                    }

                    _currentCourse.Categories.Add(item);
                }
            }
        }
        else
        {
            var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", "Добавте хотя бы одну категорию");
            btn.IsEnabled = true;
            return;
        }

        //  технологии
        if (_wrapPanelTehnology.Children.Count > 0)
        {
            foreach (DockPanel dockPanel in _wrapPanelTehnology.Children)
            {
                foreach (ComboBox comboBox in dockPanel.Children.OfType<ComboBox>())
                {
                    var item = comboBox.SelectedItem as Tehnology;
                    if (item == null)
                    {
                        var result = await MyMessageBox.CreateDialog(this,
                            MyMessageBox.MessageBoxButtons.Ok,
                            "Ошибка", "Проверьте выбор технологии");
                        btn.IsEnabled = true;
                        return;
                    }

                    _currentCourse.Tehnologies.Add(item);
                }
            }
        }
        else
        {
            var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", "Добавте хотя бы одну технологию");
            btn.IsEnabled = true;

            return;
        }

        bool hasDuplicatesCategory = _currentCourse.Categories.GroupBy(x => x).Any(g => g.Count() > 1);
        if (hasDuplicatesCategory)
        {
            var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", "Категории не могут быть одинаковыми");
            btn.IsEnabled = true;

            return;
        }

        bool hasDuplicatesTehnology = _currentCourse.Tehnologies.GroupBy(x => x).Any(g => g.Count() > 1);
        
        if (hasDuplicatesTehnology)
        {
            var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                "Ошибка", "Технологии не могут быть одинаковыми");
            btn.IsEnabled = true;

            return;
        }


        string apiUrlCourse = $"{DataManager.HostUrl}/Course/Create";

        CourseAddDTO newCourse = new CourseAddDTO()
        {
            Title = _currentCourse.Title,
            Description = _currentCourse.Description,
            ImagePath = _currentCourse.ImagePath,
            LevelId = _currentCourse.SelectedLevel.Id,
            Price = _currentCourse.Price,
            DurationHours = _currentCourse.Duration
        };

        newCourse.Categories = _currentCourse.Categories;
        newCourse.Tehnologies = _currentCourse.Tehnologies;

        string apiUrl = $"{DataManager.HostUrl}/Course/Create";

        string jsonCategoryString = JsonConvert.SerializeObject(newCourse);
        var content = new StringContent(jsonCategoryString, Encoding.UTF8, "application/json");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.PostAsync(apiUrlCourse, content);
            try
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                    if (flag == true)
                    {
                        SetComboboxLevel();
                        SetComboboxCategory();
                        SetComboboxTehnology();
                        ButtonSave_OnClick(sender, e);
                        return;
                    }
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = await response.Content.ReadAsStringAsync();

                    var result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok,
                        "Ошибка", message);
                    btn.IsEnabled = true;
                    return;
                }
                else if(response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await MyMessageBox.CreateDialog(this,
                        MyMessageBox.MessageBoxButtons.Ok,
                        "Успех", "Курс добавлен");
                    this.Close();
                }
            }
            //  если нет интернета
            catch (Exception ex)
            {
            }
        }
        btn.IsEnabled = true;
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
}