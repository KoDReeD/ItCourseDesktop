using System.Collections.Generic;
using System.Linq;
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
using ItCourseAPI.DbModels;
using ItCourseAPI.DTO;
using ItCourseDesktop.Data;
using ItCourseDesktop.Models;
using Newtonsoft.Json;

namespace ItCourseDesktop.Windows;

public partial class ChangeStatusWindow : Window
{
    private ComboBox _comboBoxStatus;
    private int _id;

    private RequestDTO _currentRequest = new RequestDTO();

    public ChangeStatusWindow()
    {
        InitializeComponent();
        this.AttachDevTools();
    }

    public ChangeStatusWindow(int id)
    {
        InitializeComponent();
        this.AttachDevTools();

        _id = id;

        Init();
    }

    private async void Init()
    {
        await GetRequest();
        await SetComboboxStatus();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _comboBoxStatus = this.FindControl<ComboBox>("ComboBoxStatuses");
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }

    private async Task SetComboboxStatus()
    {
        string url = $"{DataManager.HostUrl}/Status/GetAll";
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseValue = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Status>>(responseValue);
                _comboBoxStatus.Items = list;
                var curStatus = _comboBoxStatus.Items.OfType<Status>()
                    .FirstOrDefault(x => x.Id == _currentRequest.StatusId);
                _comboBoxStatus.SelectedItem = curStatus;
            }
        }
    }

    private async Task GetRequest()
    {
        string url = $"{DataManager.HostUrl}/Request/GetById?id={_id}";
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseValue = response.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<RequestDTO>(responseValue);
                _currentRequest = list;
                DataContext = _currentRequest;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                if (flag == true)
                {
                    await SetComboboxStatus();
                    await GetRequest();
                    return;
                }
            }
        }
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ButtonResult_OnClick(object? sender, RoutedEventArgs e)
    {
        string url = $"{DataManager.HostUrl}/Request/Edit";
        
        Button btn = sender as Button;
        btn.IsEnabled = false;

        _currentRequest.Comment = _currentRequest.Comment.Replace("\n", "");
        _currentRequest.Comment = _currentRequest.Comment.Replace("\r", "");

        _currentRequest.StatusId = (_comboBoxStatus.SelectedItem as Status).Id;

        string jsonCategoryString = JsonConvert.SerializeObject(_currentRequest);
        var content = new StringContent(jsonCategoryString, Encoding.UTF8, "application/json");


        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", DataManager.User.Token);
            HttpResponseMessage response = await client.PutAsync(url, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                    "Успех", "Статус изменён");
                this.Close();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool flag = await AuthorizeManager.RefreshRequest(DataManager.mainWindow);

                if (flag == true)
                {
                    ButtonResult_OnClick(sender, e);
                    return;
                }
            }
            else
            {
                var result = await MyMessageBox.CreateDialog(this, MyMessageBox.MessageBoxButtons.Ok,
                    "Ошибка", "Ошибка изменения статуса");
                btn.IsEnabled = true;
                return;
            }
        }
        btn.IsEnabled = true;
    }
}