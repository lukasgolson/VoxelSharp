using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Abstractions.Window;

namespace VoxelSharp.Client.Input;

public enum KeyboardEvent
{
    KeyDown,
    KeyUp
}

public interface IKeyboardListener
{
    void Subscribe(Keys key, Action onEvent, KeyboardEvent keyboardEvent = KeyboardEvent.KeyDown);
}

/// <summary>
///     Cross-platform keyboard listener backed by the GLFW window's key events.
/// </summary>
public class KeyboardListener : IKeyboardListener
{
    private readonly Dictionary<(Keys, KeyboardEvent), Action> _subscriptions = new();

    public KeyboardListener(IWindow window)
    {
        var nativeWindow = (NativeWindow)window;
        nativeWindow.KeyDown += OnKeyDown;
        nativeWindow.KeyUp += OnKeyUp;
    }

    public void Subscribe(Keys key, Action onEvent, KeyboardEvent keyboardEvent = KeyboardEvent.KeyDown)
    {
        var subscription = (key, keyboardEvent);
        _subscriptions[subscription] = _subscriptions.TryGetValue(subscription, out var existing)
            ? existing + onEvent
            : onEvent;
    }

    private void OnKeyDown(KeyboardKeyEventArgs e)
    {
        // Ignore OS key repeat so handlers only fire once per physical press
        if (e.IsRepeat) return;

        Dispatch(e.Key, KeyboardEvent.KeyDown);
    }

    private void OnKeyUp(KeyboardKeyEventArgs e)
    {
        Dispatch(e.Key, KeyboardEvent.KeyUp);
    }

    private void Dispatch(Keys key, KeyboardEvent keyboardEvent)
    {
        if (_subscriptions.TryGetValue((key, keyboardEvent), out var handler)) handler();
    }
}
