using System.Windows;
using System.Windows.Controls;
using VoxelSegmentation.WPF;
using Button = System.Windows.Controls.Button;

namespace Downloader;

public partial class RetroMessageBox : ThemedWindow
{
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

    private MessageBoxResult _result = MessageBoxResult.Cancel;

    public RetroMessageBox()
    {
        InitializeComponent();
    }

    public static MessageBoxResult Show(Window owner, string message, string title = "Message",
        MessageBoxButtons buttons = MessageBoxButtons.Ok)
    {
        var msgBox = new RetroMessageBox
        {
            Owner = owner,
            Title = title
        };

        msgBox.txtMessage.Text = message;
        msgBox.CreateButtons(buttons);

        msgBox.ShowDialog();
        return msgBox._result;
    }

    private void CreateButtons(MessageBoxButtons buttons)
    {
        pnlButtons.Children.Clear();

        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                AddButton("OK", MessageBoxResult.Ok, isDefault: true);
                break;
            case MessageBoxButtons.OkCancel:
                AddButton("OK", MessageBoxResult.Ok, isDefault: true);
                AddButton("Cancel", MessageBoxResult.Cancel, isCancel: true);
                break;
            case MessageBoxButtons.YesNo:
                AddButton("Yes", MessageBoxResult.Yes, isDefault: true);
                AddButton("No", MessageBoxResult.No, isCancel: true);
                break;
        }
    }

    private void AddButton(string content, MessageBoxResult result, bool isDefault = false, bool isCancel = false)
    {
        var btn = new Button
        {
            Content = content,
            Width = 75,
            Margin = new Thickness(8, 0, 0, 0),
            IsDefault = isDefault,
            IsCancel = isCancel
        };

        btn.Click += (s, e) =>
        {
            _result = result;
            this.Close();
        };

        pnlButtons.Children.Add(btn);
    }
}