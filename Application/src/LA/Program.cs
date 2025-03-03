
using static Raylib_cs.Raylib;
using Color = Raylib_cs.Color;
using System.Drawing;
using System.Numerics;


namespace LA
{

    internal class Program
    {
        // TODO:
        // - integrate raylib to draw the approximations for different methods, and various graphs like error, ...
        static Color BACKGROUND_COLOR = new() { A = 255, R = 0x18, G = 0x18, B = 0x18 };
        static void Main() 
        {
            //LALib.Lalib.NumericalDifferentiation.VisualizeBasicMethod();
            int WIDTH = 800;
            int HEIGHT = 600;
            InitWindow(WIDTH, HEIGHT, "hello");
            SetTargetFPS(60);
            while (!WindowShouldClose())
            {
                BeginDrawing();
                ClearBackground(BACKGROUND_COLOR);
                EndDrawing();
            }

        }
    }
}
