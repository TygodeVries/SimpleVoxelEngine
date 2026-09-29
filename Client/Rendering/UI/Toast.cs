namespace Client.Rendering.UI
{
    public static class ToastManager
    {
        private static readonly List<Toast> toasts =
            new List<Toast>();

        public static void Send(string message, float time = 7)
        {
            Console.WriteLine($"New Toast -> {message}");

            Toast toast = new Toast(message, time);
            toasts.Add(toast);
        }

        public static void Update(float deltaTime)
        {
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                toasts[i].Update(deltaTime);
            }
        }

        public static void Clear()
        {
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                toasts[i].Destroy();
            }

            toasts.Clear();
        }

        public static void Remove(Toast toast)
        {
            toasts.Remove(toast);
        }
    }

    public class Toast
    {

        private const float StartX = -1.0f;
        private const float EndX = 0.02f;

        private const float SlideInTime = 1.4f;
        private const float SlideOutTime = 1.4f;

        private float DisplayTime = 7.0f;

        private readonly UITextRenderer textRenderer;

        private float timer = 0.0f;

        private enum State
        {
            SlidingIn,
            Displaying,
            SlidingOut
        }

        private State state = State.SlidingIn;

        public Toast(string message, float time = 7)
        {
            DisplayTime = time;

            textRenderer = new UITextRenderer(RenderData.DefaultFont);
            textRenderer.TextWidth = 40;
            textRenderer.SetText(message);

            GameCanvas.AddRenderer(textRenderer);

            SetPosition(StartX);
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;

            switch (state)
            {
                case State.SlidingIn:
                    {
                        float t = timer / SlideInTime;
                        t = Clamp01(t);

                        float x = Lerp(StartX, EndX, t);

                        SetPosition(x);

                        if (t >= 1.0f)
                        {
                            state = State.Displaying;
                            timer = 0.0f;
                        }

                        break;
                    }

                case State.Displaying:
                    {
                        if (timer >= DisplayTime)
                        {
                            state = State.SlidingOut;
                            timer = 0.0f;
                        }

                        break;
                    }

                case State.SlidingOut:
                    {
                        float t = timer / SlideOutTime;
                        t = Clamp01(t);

                        float x = Lerp(EndX, StartX, t);

                        SetPosition(x);

                        if (t >= 1.0f)
                        {
                            Destroy();
                            ToastManager.Remove(this);
                        }

                        break;
                    }
            }
        }

        private void SetPosition(float x)
        {
            Shared.Mathf.Vector2 position =
                new Shared.Mathf.Vector2(x, 0.02f);

            textRenderer.position = position;
        }

        public void Destroy()
        {
            GameCanvas.RemoveRenderer(textRenderer);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + ((b - a) * t);
        }

        private static float Clamp01(float value)
        {
            if (value < 0.0f)
                return 0.0f;

            if (value > 1.0f)
                return 1.0f;

            return value;
        }
    }
}
