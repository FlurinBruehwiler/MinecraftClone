global using static Raylib_cs.Raylib;
global using static Raylib_cs.Rlgl;
global using static Raylib_cs.Raymath;
global using Raylib_cs;
using System.Numerics;
using Test;
using rlImGui_cs;
using ImGuiNET;

InitWindow(1000, 1000, "");

SetTargetFPS(60);

var camera = new Camera2D
{
    zoom = 1
};

rlImGui.Setup(true);

var nodes = new List<Node>();

var layouter = new Layouter(nodes);

Node? dragStartNode = null;

while (!WindowShouldClose())
{
    Navigation.HandleNavigation(ref camera);

    var pos = GetScreenToWorld2D(GetMousePosition(), camera);

    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
    {

        var clickedNode = nodes.FirstOrDefault(x => Vector2.Distance(x.Position, pos) < 10);

        if (clickedNode is null)
        {
            nodes.Add(new Node
            {
                Position = pos
            });
        }
        else
        {
            dragStartNode = clickedNode;
        }
    }

    if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
    {
        if (dragStartNode is not null)
        {
            var clickedNode = nodes.FirstOrDefault(x => Vector2.Distance(x.Position, pos) < 10);

            if (clickedNode is not null)
            {
                clickedNode.ConnectedNodes.Add(dragStartNode);
                dragStartNode.ConnectedNodes.Add(clickedNode);
            }

            dragStartNode = null;
        }
    }

    if (IsKeyPressed(KeyboardKey.KEY_SPACE))
    {
        layouter.Layout().GetAwaiter().GetResult();
    }

    BeginDrawing();

    ClearBackground(Color.BLACK);


    BeginMode2D(camera);

    foreach (var node in nodes)
    {
        var color = Color.RED;

        if(dragStartNode is not null)
        {
            if (Vector2.Distance(node.Position, pos) < 10)
            {
                color = Color.BLUE;
            }
        }

        DrawCircle((int)node.Position.X, (int)node.Position.Y, 10, color);
    }

    var connections = nodes.SelectMany(x => x.ConnectedNodes.Select(y => new Connection(x, y))).Distinct();

    foreach (var (a, b) in connections)
    {
        DrawLine((int)a.Position.X, (int)a.Position.Y, (int)b.Position.X, (int)b.Position.Y, Color.BLUE);
    }

    if (dragStartNode is not null)
    {
        DrawLine((int)dragStartNode.Position.X, (int)dragStartNode.Position.Y, (int)pos.X, (int)pos.Y, Color.BLUE);;
    }

    EndMode2D();

    rlImGui.Begin();

    

    rlImGui.End();

    EndDrawing();
}

rlImGui.Shutdown();
CloseWindow();

public record struct Connection(Node A, Node B)
{
    public bool Equals(Connection? other)
    {
        if (!other.HasValue)
            return false;

        if (A.Equals(other.Value.A) && B.Equals(other.Value.B))
            return true;


        if (A.Equals(other.Value.B) && B.Equals(other.Value.A))
            return true;

        return false;
    }
}
