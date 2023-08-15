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

var scale = .1f;

var chunkSize = 16;

HashSet<IntVector2> activatedChunks = new()
{
    new IntVector2(0, 0)
};

while (!WindowShouldClose())
{
    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
    {
        var pos = GetMousePosition();
        var chunk = new IntVector2((int)pos.X / chunkSize, (int)pos.Y / chunkSize);
        activatedChunks.Add(chunk);
    }

    if (GetMouseWheelMoveV().Y > 0)
    {
        scale += 0.01f;
    }
    if (GetMouseWheelMoveV().Y < 0)
    {
        scale -= 0.01f;
    }
    
    BeginDrawing();

    ClearBackground(Color.WHITE);

    foreach (var chunk in activatedChunks)
    {
        for (var x = 0; x < chunkSize; x++)
        {
            for (var y = 0; y < chunkSize; y++)
            {
                var globalX = x + chunk.X * chunkSize;
                var globalZ = y + chunk.Y * chunkSize;
                
                var res = mrPerlin.OctavePerlin(
                    globalX * scale, 
                    0, 
                    globalZ * scale, 1, 2);
                
                var dat = (int)(res * 255);
                
                DrawPixel(x + chunk.X * chunkSize, y + chunk.Y * chunkSize, new Color(dat, dat, dat, 255));
            }
        }
    }

    return;
    
    DrawText(scale.ToString(), 100, 100, 20, Color.RED);
    DrawText($"{0} - {screenWidth * scale}", 100, 200, 20, Color.RED);
    
    EndDrawing();
}

CloseWindow();