using Shared.Mathf;

namespace Client.Rendering.UI
{
    public class Font
    {
        private readonly ImageTexture imageTexture;

        private readonly int charWidth;
        private readonly int charHeight;

        private readonly int columns;
        private readonly int rows;

        public Font(string file, int charW, int charH)
        {
            imageTexture = ImageTexture.LoadFromPng(file);

            charWidth = charW;
            charHeight = charH;

            columns = imageTexture.width / charWidth;
            rows = imageTexture.height / charHeight;
        }

        public Vector2[] GetCharacterUv(int character)
        {
            if (character < 0 || character >= columns * rows)
                throw new ArgumentOutOfRangeException(nameof(character));

            int column = character % columns;
            int row = character / columns;

            float cellWidth = 1.0f / columns;
            float cellHeight = 1.0f / rows;

            float u0 = column * cellWidth;
            float v0 = row * cellHeight;

            float u1 = u0 + cellWidth;
            float v1 = v0 + cellHeight;

            return
            [
                new Vector2(u0, v0), // Top-left
                new Vector2(u1, v0), // Top-right
                new Vector2(u1, v1), // Bottom-right
                new Vector2(u0, v1)  // Bottom-left
            ];
        }

        public Vector2[] GetCharacterUv(char c)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"#$%&'()x+,-:;<=>?. ";

            string a = $"{c}";

            return GetCharacterUv(chars.IndexOf(a[0]));
        }

        public ImageTexture GetTexture()
        {
            return imageTexture;
        }

        public int GetCharacterWidth()
        {
            return charWidth;
        }

        public int GetCharacterHeight()
        {
            return charHeight;
        }
    }
}
