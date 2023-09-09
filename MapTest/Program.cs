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

ConcurrentDictionary<TileId, Texture> tiles = new();
ConcurrentBag<UnUploadedTile> tilesToUpload = new();
ConcurrentDictionary<TileId, int> loadingTiles = new();

var loadedPositions = new List<Pos>();

var client = new HttpClient();

var startX = 516;
var startY = 188;

var year = 2020;

for (var x = 0; x < 10; x++)
{
    for (var y = 0; y < 10; y++)
    {
        loadedPositions.Add(new Pos(x, y));
    }
}

var dragStart = new Vector2();
var currentDrag = new Vector2();
var isDraging = false;

while (!WindowShouldClose())
{
    HandleNavigation();

    CalculateDrag();

    if (IsKeyPressed(KeyboardKey.KEY_SPACE))
    {
        loadedPositions.Clear();
    }

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
                tiles.TryAdd(tile.Id, texture);
            }

            loadingTiles.Remove(tile.Id, out _);
        }
    }

    BeginDrawing();
        ClearBackground(WHITE);

        BeginMode2D(camera);

            // const int size = 100;
            // rlPushMatrix();
            //     rlTranslatef(0, size / 4 * resolution, 0);
            //     rlRotatef(90, 1, 0, 0);
            //     DrawGrid(size, resolution);
            // rlPopMatrix();

            foreach (var (x, y) in loadedPositions)
            {
                var tileId = new TileId(year, new Pos(x, y));
                if (tiles.TryGetValue(tileId, out var tile))
                {
                    DrawTexture(tile, x * resolution, y * resolution, WHITE);
                }
                else
                {
                    LoadImage(tileId);
                }
            }

            DrawDragRectangle();

        EndMode2D();

        var sliderValue = GuiSlider(new Rectangle(0, 0, screenWidth, 100), "1844", "2021", year, 1844, 2021);
        year = (int)(Math.Round(sliderValue / 5) * 5);

        DrawText(year.ToString(), 0, 100, 100, RED);

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
                loadedPositions.Add(new Pos(tileX, tileY));
            }
        }
    }
}

void LoadImage(TileId tileId)
{
    if (tiles.ContainsKey(tileId))
        return;

    var isAlreadyLoading = !loadingTiles.TryAdd(tileId, 0);

    if (isAlreadyLoading)
        return;

    var finalX = startX + tileId.Pos.X;
    var finalY = startY + tileId.Pos.Y;

    var directory = $"./tiles/{tileId.Year}";
    var path = Path.Combine(directory, $"{finalX}_{finalY}.png");

    if (File.Exists(path))
    {
        tiles.TryAdd(tileId, LoadTexture(path));
        return;
    }

    var url = $"https://wmts100.geo.admin.ch/1.0.0/ch.swisstopo.zeitreihen/default/{year}1231/2056/23/{finalX}/{finalY}.png";

    client.GetByteArrayAsync(url)
        .ContinueWith(task =>
        {
            var res = task.Result;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(path, res);

            tilesToUpload.Add(new UnUploadedTile(tileId, res));
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

record UnUploadedTile(TileId Id, byte[] RawBytes);

record Pos(int X, int Y);

record TileId(int Year, Pos Pos);

