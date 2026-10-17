namespace PartsEditor
{
    public struct Vector2F
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2F(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X} {Y})";
        }
    }
}
