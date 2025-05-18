
using Raylib_cs;
using Color = Raylib_cs.Color;
using System.Drawing;
using System.Runtime.Intrinsics;
using System.Numerics;
using Rectangle = System.Drawing.Rectangle;
using System.Security.Cryptography;
using LALib;

namespace LA
{

    internal class Program
    {
        static Color BACKGROUND_COLOR = new() { A = 255, R = 0x18, G = 0x18, B = 0x18 };
        static void Main()
        {
            //LALib.Lalib.NumericalDifferentiation.VisualizeBasicMethod();
            int factor = 75;
            int w = 16 * factor;
            int h = 9 * factor;
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.AlwaysRunWindow);
            Raylib.InitWindow(w, h, "hello");
            Raylib.SetTargetFPS(60);
            List<float> function = [];
            int N = 80;
            float dx = 6.3f / N;
            for (int i = 0; i < N; i++)
            {
                float freq = 1.0f;
                float t = i * 2 * MathF.PI * freq / N;
                function.Add(MathF.Cos(t));
            }
            List<float> dir_function = Lalib.NumericalDifferentiation.BasicMethod(function, dx);
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(BACKGROUND_COLOR);
                w = Raylib.GetScreenWidth();
                h = Raylib.GetScreenHeight();

                int Countx = 2, County = 2;
                int padding = 20;
                for (int j = 0; j < County; j++)
                {
                    for (int i = 0; i < Countx; i++)
                    {
                        Color c = new() { A = 0xff, R = 0xff, G = (byte)(j * (0xff / Countx)), B = (byte)(i * (0xff / Countx)) };
                        float thickness = 2.5f;
                        Vector2 size = new(w / Countx - padding, h / County - padding);
                        Vector2 pos = new(i * size.X + ((i + 1) * padding / 2) + padding / 2, j * size.Y + ((j + 1) * padding / 2) + padding / 2);
                        Raylib_cs.Rectangle rect = new(pos, size);
                        Raylib.DrawRectangleLinesEx(rect, thickness, c);

                        float axisthick = 2.0f;
                        Vector2 xaxisStart = new(pos.X, pos.Y + size.Y / 2);
                        Vector2 xaxisEnd = new(pos.X + size.X, pos.Y + size.Y / 2);
                        Raylib.DrawLineEx(xaxisStart, xaxisEnd, axisthick, Color.Red);
                        
                        Vector2 yaxisStart = new(pos.X + size.X / 2, pos.Y);
                        Vector2 yaxisEnd = new(pos.X + size.X / 2, pos.Y + size.Y);
                        Raylib.DrawLineEx(yaxisStart, yaxisEnd, axisthick, Color.Red);

                        for (int p = 0; p < function.Count; p++)
                        {
                            float r = 3.5f;
                            Vector2 center = new(pos.X + p * (size.X / function.Count), xaxisStart.Y - (function[p] * size.Y / 2 + r / 2));
                            Raylib.DrawCircleV(center, r, c);
                        }
                        for (int p = 0; p < dir_function.Count; p++)
                        {
                            float r = 3.5f;
                            Vector2 center = new(pos.X + p * (size.X / function.Count), xaxisStart.Y - (dir_function[p] * size.Y / 2 + r / 2));
                            Raylib.DrawCircleV(center, r, c);
                        }
                    }
                }


                Raylib.EndDrawing();
            }

        }
    }
}
