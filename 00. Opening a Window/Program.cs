
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Lesson
{
    internal class Program
    {
        private static IWindow? window;

        static void Main()
        {

            var options = WindowOptions.Default with
            {
                API = GraphicsAPI.None,             // <-- This is important as
                                                    // the window is handled by OpenGL
                                                    // by default
                Size = new Vector2D<int>(800, 800),
                Title = "00. Opening a Window"
            };

            Program.window = Window.Create(options);
            Program.window.Run();
        }
    }
}