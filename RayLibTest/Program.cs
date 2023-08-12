using System.Globalization;
using Raylib_cs;
using RayLib3dTest;
using static Raylib_cs.Raylib;

const int screenWidth = 800;
const int screenHeight = 480;

InitWindow(screenWidth, screenHeight, "Hello World");
SetTargetFPS(60);

var frequency = 0.5f; //u, i
var amplitude = 1f; //o, p
float[] data;
Regenerate();

while (!WindowShouldClose())
{
    BeginDrawing();

    ClearBackground(Color.WHITE);

    for (var x = 0; x < screenWidth; x++)
    {
        for (var y = 0; y < screenHeight; y++)
        {
            var dat = (int)(Math.Clamp(data[y * screenWidth + x], 0f, 1f) * 255);
            DrawPixel(x, y, new Color(dat, dat, dat, 255));
        }
    }
    
    DrawText(frequency.ToString(CultureInfo.InvariantCulture), 100, 100, 12, Color.RED);
    DrawText(amplitude.ToString(CultureInfo.InvariantCulture), 100, 200, 12, Color.RED);

    if (IsKeyDown(KeyboardKey.KEY_U))
    {
        frequency += 0.1f;
        Regenerate();
    }
    if (IsKeyDown(KeyboardKey.KEY_I))
    {
        frequency -= 0.1f;
        Regenerate();
    }
    if (IsKeyDown(KeyboardKey.KEY_O))
    {
        amplitude += 0.1f;
        Regenerate();
    }
    if (IsKeyDown(KeyboardKey.KEY_P))
    {
        amplitude -= 0.1f;
        Regenerate();
    }

    EndDrawing();
}

CloseWindow();

void Regenerate()
{
    data = MrPerlin.GenerateNoiseMap(screenWidth, screenHeight, 1, frequency, amplitude);
}