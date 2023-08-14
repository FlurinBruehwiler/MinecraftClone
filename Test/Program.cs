global using static Raylib_cs.Raylib;
global using static Raylib_cs.Rlgl;
using System.Globalization;
using Raylib_cs;
using RayLib3dTest;


const int screenWidth = 800;
const int screenHeight = 480;

var mrPerlin = new MrPerlin();

InitWindow(screenWidth, screenHeight, "Hello World");
SetTargetFPS(60);

var scale = .1;

var chunkSize = screenWidth / 1;

HashSet<IntVector2> activatedChunks = new();

while (!WindowShouldClose())
{
    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
    {
        var pos = GetMousePosition();
        var chunk = new IntVector2((int)pos.X / chunkSize, (int)pos.Y / chunkSize);
        activatedChunks.Add(chunk);
    }
    
    BeginDrawing();

    ClearBackground(Color.WHITE);

    foreach (var chunk in activatedChunks)
    {
        for (var x = 0; x < chunkSize; x++)
        {
            for (var y = 0; y < chunkSize; y++)
            {
                var res = mrPerlin.OctavePerlin((float)(x + chunk.X * chunkSize) * scale, 0, (float)(y + chunk.Y * chunkSize) * scale, 1, 2);
                Console.WriteLine(res);
                var dat = (int)(Math.Clamp(res, 0f, 1f) * 255);
                DrawPixel(x + chunk.X * chunkSize, y + chunk.Y * chunkSize, new Color(dat, dat, dat, 255));
            }
        }
    }
    
    EndDrawing();
}

CloseWindow();