namespace TUI.Base
{
    /// <summary>
    /// Represents a style of widget lighting.
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Wall ID to light with.
        /// </summary>
        public byte Wall { get; set; }
        /// <summary>
        /// Paint of lighting wall.
        /// </summary>
        public byte Color { get; set; }

        /// <summary>
        /// Represents a style of widget lighting.
        /// </summary>
        public Light(byte wall = 155, byte color = 0)
        {
            Wall = wall;
            Color = color;
        }
        /// <summary>
        /// Represents a style of widget lighting.
        /// </summary>
        public Light(Light light)
        {
            Wall = light.Wall;
            Color = light.Color;
        }
    }
}
