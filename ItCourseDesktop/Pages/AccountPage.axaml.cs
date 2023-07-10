using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ItCourseDesktop.Data;

namespace ItCourseDesktop.Pages;

public partial class AccountPage : UserControl
{
    private TextBlock _textBlockUsername;
    public AccountPage()
    {
        InitializeComponent();

        _textBlockUsername.Text = "Hello, " + DataManager.User.Username;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _textBlockUsername = this.FindControl<TextBlock>("TextBlockUsername");
    }
}