global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static Raylib_CsLo.RayGui;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_CsLo;


const int screenWidth = 1800;
const int screenHeight = 900;


InitWindow(screenWidth, screenHeight, "Map Test");
SetTargetFPS(60);

var camera = new Camera2D
{
    zoom = 1
};

while (!WindowShouldClose())
{
    HandleNavigation();

    BeginDrawing();
        ClearBackground(WHITE);

        BeginMode2D(camera);


        EndMode2D();


    EndDrawing();
}

CloseWindow();


void HandleNavigation()
{
    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
    {
        var delta = GetMouseDelta();
        delta = Vector2Scale(delta, -1.0f/camera.zoom);

        camera.target += delta;
    }

    // Zoom based on mouse wheel
    var wheel = GetMouseWheelMove();
    if (wheel != 0)
    {
        // Get the world point that is under the mouse
        var mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), camera);

        // Set the offset to where the mouse is
        camera.offset = GetMousePosition();

        // Set the target to match, so that the camera maps the world space point
        // under the cursor to the screen space point under the cursor at any zoom
        camera.target = mouseWorldPos;

        // Zoom increment
        const float zoomIncrement = 0.125f;

        camera.zoom += (wheel*zoomIncrement);
        if (camera.zoom < zoomIncrement) camera.zoom = zoomIncrement;
    }
}
