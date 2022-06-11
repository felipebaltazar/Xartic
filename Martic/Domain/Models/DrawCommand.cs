namespace Xartic.Core
{
    public class DrawCommand
    {
        public bool IsMouseDown { get; set; }

        public string Username { get; set; }

        public Color Color { get; set; }

        public Vector2 Position { get; set; }

        public int Radius { get; set; }
    }

    public struct Color
    {
        public string Hex { get; set; }

        public override string ToString() => Hex;
    }

    public struct Vector2
    {
        public double X { get; set; }

        public double Y { get; set; }

        public override string ToString()
        {
            return $"X:{X}, Y:{Y}";
        }
    }
}
