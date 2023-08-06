using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Rlgl;
using Color = Raylib_cs.Color;

const int screenWidth = 800;
const int screenHeight = 480;

InitWindow(screenWidth, screenHeight, "Hello World");
SetTargetFPS(60);

const float speed = 1f;
var offset = new Vector2();
var objs = new List<Obj>();
Obj? selectedObj = null;
var draggedObjOffset = new Vector2();
Obj? draggedObj = null;
var lines = new List<ValueTuple<Vector2, Vector2>>();
var lineStart = new Vector2();
var isDrawingLine = false;
var cameraDragStart = new Vector2();
var initialCameraPos = new Vector2();

var camera = new Camera2D
{
    zoom = 1,
    rotation = 0,
};

while (!WindowShouldClose())
{
    var wheel = GetMouseWheelMove();
    if (wheel != 0)
    {
        var previousPos = GetMouseWorldPos();
        camera.offset = GetMousePosition();
        camera.target = previousPos;

        const float zoomIncrement = 0.125f;
        camera.zoom += wheel * zoomIncrement;
        if (camera.zoom < zoomIncrement)
        {
            camera.zoom = zoomIncrement;
        }
    }
    
    if (IsKeyDown(KeyboardKey.KEY_A))
        offset.X -= speed;
    if (IsKeyDown(KeyboardKey.KEY_D))
        offset.X += speed;
    if (IsKeyDown(KeyboardKey.KEY_W))
        offset.Y -= speed;
    if (IsKeyDown(KeyboardKey.KEY_S))
        offset.Y += speed;

    
    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
    {
        var clickedObj = objs.FirstOrDefault(x => x.ContainsPoint(GetMouseWorldPos()));

        if (clickedObj is not null)
        {
            selectedObj = clickedObj;
            draggedObj = clickedObj;
            draggedObjOffset = draggedObj.Pos - GetMouseWorldPos();
        }
        else
        {
            lineStart = GetMouseWorldPos();
            isDrawingLine = true;
        }
    }

    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_MIDDLE))
    {
        cameraDragStart = GetMousePosition();
        initialCameraPos = camera.offset;
    }

    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
    {
        var dragVec = GetMousePosition() - cameraDragStart;
        camera.offset = initialCameraPos + dragVec;
    }

    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
    {
        if (draggedObj is not null)
        {
            draggedObj.Pos = GetMouseWorldPos() + draggedObjOffset;
        }
    }

    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
    {
        draggedObj = null;

        if (isDrawingLine)
        {
            lines.Add((lineStart, GetMouseWorldPos()));
            isDrawingLine = false;
        }
    }

    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
    {
        var clickedObj = objs.FirstOrDefault(x => x.ContainsPoint(GetMouseWorldPos()));

        if (clickedObj is not null)
        {
            if (clickedObj == selectedObj)
            {
                selectedObj = null;
            }

            if (clickedObj == draggedObj)
            {
                draggedObj = null;
            }

            objs.Remove(clickedObj);
        }
        else
        {
            var newObj = new Obj(50)
            {
                Pos = GetMouseWorldPos()
            };
            objs.Add(newObj);
            selectedObj = newObj;
        }
    }
    
    BeginDrawing();
    ClearBackground(Color.RED);
    
    BeginMode2D(camera);

    rlPushMatrix();
    rlTranslatef(0, 25*50, 0);
    rlRotatef(90, 1, 0, 0);
    DrawGrid(100, 50);
    rlPopMatrix();
    
    foreach (var obj in objs)
    {
        DrawCircleV(obj.Pos, obj.Radius, obj == selectedObj ? Color.BLUE : Color.GREEN);
    }
    
    foreach (var (start, end) in lines)
    {
        DrawLineEx(start, end, 10, Color.PINK);
    }

    if (isDrawingLine)
    {
        DrawLineEx(lineStart, GetMouseWorldPos(), 10, Color.PINK);
    }
    EndMode2D();
    EndDrawing();
}

CloseWindow();

Vector2 ToWorld(Vector2 vec)
{
    return GetScreenToWorld2D(vec, camera);
}

Vector2 GetMouseWorldPos()
{
    return ToWorld(GetMousePosition());
}

record Obj(float Radius)
{
    public Vector2 Pos { get; set; }
    
    public bool ContainsPoint(Vector2 vec)
    {
        return CheckCollisionPointCircle(vec, Pos, Radius);
    }
}