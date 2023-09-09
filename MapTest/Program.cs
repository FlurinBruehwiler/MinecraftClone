global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static Raylib_CsLo.RayGui;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_CsLo;

const int resolution = 256;

const int screenWidth = 1800;
const int screenHeight = 900;


InitWindow(screenWidth, screenHeight, "Map Test");
SetTargetFPS(60);

var camera = new Camera2D
{
    zoom = 1
};

ConcurrentDictionary<Pos, Texture> tiles = new();
ConcurrentBag<UnUploadedTile> tilesToUpload = new();

var client = new HttpClient();

var startX = 520;
var startY = 190;

for (var x = 0; x < 10; x++)
{
    for (var y = 0; y < 10; y++)
    {
        LoadImage(1861, x, y);
    }
}

var dragStart = new Vector2();
var currentDrag = new Vector2();
var isDraging = false;

while (!WindowShouldClose())
{
    HandleNavigation();

    CalculateDrag();

    while (!tilesToUpload.IsEmpty)
    {
        if (tilesToUpload.TryTake(out var tile))
        {
            unsafe
            {
                var pinnedArray = GCHandle.Alloc(tile.RawBytes, GCHandleType.Pinned);
                var pointer = (byte*)pinnedArray.AddrOfPinnedObject();
                var image = LoadImageFromMemory(".png", pointer, tile.RawBytes.Length * sizeof(byte));
                var texture = LoadTextureFromImage(image);
                tiles.TryAdd(tile.Pos, texture);
            }
        }
    }

    BeginDrawing();
        ClearBackground(WHITE);


        BeginMode2D(camera);

            const int size = 100;
            // rlPushMatrix();
            //     rlTranslatef(0, size / 4 * resolution, 0);
            //     rlRotatef(90, 1, 0, 0);
            //     DrawGrid(size, resolution);
            // rlPopMatrix();

            foreach (var (pos, texture) in tiles)
            {
                DrawTexture(texture, pos.X * resolution, pos.Y * resolution, WHITE);
            }

            DrawDragRectangle();

        EndMode2D();

        GuiSlider(new Rectangle(0, 0, screenWidth, 30), "1844", "2021", 1861, 1844, 2021);

    EndDrawing();
}

CloseWindow();

void DrawDragRectangle()
{
    if (!isDraging)
        return;

    var (bottomLeft, topRight) = GetTopLeftBottomRight(dragStart, currentDrag);

    DrawRectangleV(bottomLeft, topRight - bottomLeft, new Color(200, 200, 200, 200));
}

Pos GetTileCoord(Vector2 pos)
{
    return new Pos((int)Math.Floor(pos.X / resolution), (int)Math.Floor(pos.Y / resolution));
}

(Vector2 bottomLeft, Vector2 topRight) GetTopLeftBottomRight(Vector2 start, Vector2 end)
{
    var bottomLeft = new Vector2();
    var topRight = new Vector2();

    bottomLeft.X = Math.Min(start.X, end.X);
    bottomLeft.Y = Math.Min(start.Y, end.Y);

    topRight.X = Math.Max(start.X, end.X);
    topRight.Y = Math.Max(start.Y, end.Y);

    return (bottomLeft, topRight);
}

void CalculateDrag()
{
    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
    {
        dragStart = GetScreenToWorld2D(GetMousePosition(), camera);
        isDraging = true;
    }

    if(IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
    {
        currentDrag = GetScreenToWorld2D(GetMousePosition(), camera);
    }

    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
    {
        isDraging = false;

        var start = GetTileCoord(dragStart);
        var end = GetTileCoord(currentDrag);

        var minX = Math.Min(start.X, end.X);
        var minY = Math.Min(start.Y, end.Y);

        var maxX = Math.Max(start.X, end.X);
        var maxY = Math.Max(start.Y, end.Y);

        for (var tileX = minX; tileX <= maxX; tileX++)
        {
            for (var tileY = minY; tileY <= maxY; tileY++)
            {
                LoadImage(1861, tileX, tileY);
            }
        }
    }
}

void LoadImage(int year, int x, int y)
{
    if (tiles.ContainsKey(new Pos(x, y)))
        return;

    var finalX = startX + x;
    var finalY = startY + y;

    var directory = $"./tiles/{year}";
    var path = Path.Combine(directory, $"{finalX}_{finalY}.png");

    if (File.Exists(path))
    {
        tiles.TryAdd(new Pos(x, y), LoadTexture(path));
        return;
    }

    var url = $"https://wmts100.geo.admin.ch/1.0.0/ch.swisstopo.zeitreihen/default/18611231/2056/23/{finalX}/{finalY}.png";

    client.GetByteArrayAsync(url)
        .ContinueWith(task =>
        {
            var res = task.Result;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(path, res);

            tilesToUpload.Add(new UnUploadedTile(new Pos(x, y), res));
        });
}

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

record UnUploadedTile(Pos Pos, byte[] RawBytes);

record Pos(int X, int Y);

record TileId(int Year, Pos Pos);

