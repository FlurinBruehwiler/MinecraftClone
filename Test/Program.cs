global using static Raylib_cs.Raylib;
global using System.Numerics;
global using Raylib_cs;

InitWindow(1000, 1000, "");

var camera = new Camera3D(new Vector3(0, 10, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0), 60,
    CameraProjection.CAMERA_PERSPECTIVE);

DisableCursor(); 
SetTargetFPS(60);

while (!WindowShouldClose())
{
    UpdateCamera(ref camera, CameraMode.CAMERA_CUSTOM);
    
    BeginDrawing();
    
        ClearBackground(Color.WHITE);
        
        BeginMode3D(camera);

        DrawCylinderEx(Vector3.Zero, Vector3E.Forward * 10, .1f, .1f, 10, Color.BLUE); //z
        DrawCylinderEx(Vector3.Zero, Vector3E.Right * 10, .1f, .1f, 10, Color.RED); //x
        DrawCylinderEx(Vector3.Zero, Vector3E.Up * 10, .1f, .1f, 10, Color.GREEN); //y
        
        EndMode3D();
    
    EndDrawing();
}

CloseWindow();

public static class Vector3E
{
    public static Vector3 Right = new Vector3(1, 0, 0);
    public static Vector3 Forward = new Vector3(0, 0, 1);
    public static Vector3 Up = new Vector3(0, 1, 0);
    
}