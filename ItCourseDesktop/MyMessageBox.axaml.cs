using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Microsoft.CodeAnalysis;

namespace ItCourseDesktop;

public partial class MyMessageBox : Window
{
    public TextBlock WinTitle { get; set; }
    public TextBlock MessageText { get; set; }

    private MessageBoxResult DialogResult { get; set; } = MessageBoxResult.Ok;

    public MyMessageBox()
    {
        AvaloniaXamlLoader.Load(this);

        InitializeComponent();
        this.AttachDevTools();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo
    }

    public enum MessageBoxResult
    {
        Ok,
        Cancel,
        Yes,
        No
    }


    public static Task<MessageBoxResult> CreateDialog(Window parent, MessageBoxButtons buttons, string title,
        string message)
    {
        var dialog = new MyMessageBox();

        dialog.WinTitle = dialog.FindControl<TextBlock>("TextBlockTitle");
        dialog.MessageText = dialog.FindControl<TextBlock>("TextBlockMessage");

        dialog.WinTitle.Text = title;
        dialog.MessageText.Text = message;

        var buttonPanel = dialog.FindControl<StackPanel>("StackPanelButtons");

        void AddButton(string caption, MessageBoxResult res)
        {
            var btn = new Button { Content = caption };
            btn.Classes.Add("result");
            btn.Click += (_, __) =>
            {
                dialog.DialogResult = res;
                dialog.Close();
            };
            buttonPanel.Children.Add(btn);
        }

        // Добавление кнопок в stack panel

        if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
        {
            AddButton("Ok", MessageBoxResult.Ok);
        }

        if (buttons == MessageBoxButtons.YesNo)
        {
            AddButton("Yes", MessageBoxResult.Yes);
            AddButton("No", MessageBoxResult.No);
        }

        if (buttons == MessageBoxButtons.OkCancel)
            AddButton("Cancel", MessageBoxResult.Cancel);

        var tcs = new TaskCompletionSource<MessageBoxResult>();
        dialog.Closed += delegate { tcs.TrySetResult(dialog.DialogResult); };

        dialog.ShowDialog(parent);


        return tcs.Task;
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = MessageBoxResult.Cancel;
        this.Close();
    }

    private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.BeginMoveDrag(e);
        }
    }
}