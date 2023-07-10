using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ItCourseDesktop.Data;
using ItCourseDesktop.Pages;

namespace ItCourseDesktop.Windows;

public partial class MainWindow : Window
{
    public ICommand CategoryCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        DataManager.mainWindow = this;
        contentControl.Content = new CoursesPage();
        
    }

    public void InitializeUI()
    {
        
    }

    private void SetCoursesListBox()
    {
        
    }

    //  Перемещение окна
    private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }

    //  Закрытие окна
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    //  Маштаб окна
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

    private void MenuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var btn = sender as Button;

        switch (btn.Name)
        {
            case "ButtonCourse":
                contentControl.Content = new CoursesPage();
                break;
            
            case "ButtonCategory":
                contentControl.Content = new CategoriesPage();
                break;
                
            case "ButtonTehnology":
                contentControl.Content = new TehnologiesPage();
                break;
                
            case "ButtonUsers":
                contentControl.Content = new UsersPage();
                break;
            
            case "ButtonReviews":
                
                break;
            
            case "ButtonRequest":
                contentControl.Content = new RequestsPage();
                break;

            case "ButtonAccount":
                contentControl.Content = new AccountPage();
                break;
            
                
        }
    }
}