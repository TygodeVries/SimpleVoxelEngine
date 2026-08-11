using OpenTK.Mathematics;

namespace Client.Input;

public class Mouse
{
    public static Mouse Current { get; private set; } = new Mouse();
    public Vector2 scroll;
    private Mouse()
    {

    }

    /// <summary>
    /// Cleanup at the end of a frame
    /// </summary>
    public void EndOfFrame()
    {
        mouseDelta = Vector2.Zero;
        scroll = Vector2.Zero;

        leftWasPressed = leftPressed;
        rightWasPressed = rightPressed;
    }
    private Vector2 lastFrameScroll = Vector2.Zero;
    public Vector2 mouseDelta;
    public Vector2 position;

    public bool leftPressed;
    public bool rightPressed;
    public bool middlePressed;

    private bool leftWasPressed;
    private bool rightWasPressed;
    public bool LeftPressedThisFrame()
    {
        return leftPressed && !leftWasPressed;
    }

    public bool RightPressedThisFrame()
    {
        return rightPressed && !rightWasPressed;
    }

    public bool RightReleasedThisFrame()
    {
        return !rightPressed && rightWasPressed;
    }

    public bool LeftReleasedThisFrame()
    {
        return !leftPressed && leftWasPressed;
    }
}
