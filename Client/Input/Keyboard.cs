
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Client.Input;

public class Keyboard
{
    public static Keyboard Current { get; private set; } = new Keyboard();

    private Keyboard()
    {
    }

    private Dictionary<Keys, bool> KeysStates = new Dictionary<Keys, bool>();

    // Keyss pressed during this frame
    private List<Keys> KeyssPressedThisFrame = new List<Keys>();

    // Keyss released during this frame
    private List<Keys> KeyssReleasedThisFrame = new List<Keys>();

    /// <summary>
    /// Update Keys state and track presses/releases per frame
    /// </summary>
    public void SetKeysState(Keys Keys, bool pressed)
    {
        bool wasPressed = KeysStates.ContainsKey(Keys) && KeysStates[Keys];

        KeysStates[Keys] = pressed;

        if (pressed && !wasPressed)
        {
            // Keys went down this frame
            if (!KeyssPressedThisFrame.Contains(Keys))
                KeyssPressedThisFrame.Add(Keys);

            AnyKeysPressed?.Invoke();
        }
        else if (!pressed && wasPressed)
        {
            // Keys was released this frame
            if (!KeyssReleasedThisFrame.Contains(Keys))
                KeyssReleasedThisFrame.Add(Keys);
        }
    }

    public Action? AnyKeysPressed;

    public float GetAxis(KeysboardAxis axis)
    {
        if (axis == KeysboardAxis.Vertical)
        {
            if (IsPressed(Keys.W))
                return 1;

            if (IsPressed(Keys.S))
                return -1;
        }

        if (axis == KeysboardAxis.Horizontal)
        {
            if (IsPressed(Keys.D))
                return 1;

            if (IsPressed(Keys.A))
                return -1;
        }

        return 0;
    }

    /// <summary>
    /// Cleanup pressed/released states at end of frame
    /// </summary>
    public void EndOfFrame()
    {
        KeyssPressedThisFrame.Clear();
        KeyssReleasedThisFrame.Clear();
    }

    /// <summary>
    /// Is Keys currently pressed?
    /// </summary>
    public bool IsPressed(Keys Keys)
    {
        return KeysStates.ContainsKey(Keys) && KeysStates[Keys];
    }

    /// <summary>
    /// Was Keys pressed this frame?
    /// </summary>
    public bool IsPressedThisFrame(Keys Keys)
    {
        return KeyssPressedThisFrame.Contains(Keys);
    }

    /// <summary>
    /// Was Keys released this frame?
    /// </summary>
    public bool IsReleasedThisFrame(Keys Keys)
    {
        return KeyssReleasedThisFrame.Contains(Keys);
    }
}

public enum KeysboardAxis
{
    Horizontal,
    Vertical
}
