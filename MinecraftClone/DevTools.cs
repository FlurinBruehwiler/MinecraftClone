namespace RayLib3dTest;

public struct Debug3dInstruction
{
    public Vector3 PointA;
    public Vector3 PointB;
    public Raylib_cs.Color Color;
    public float Scalar;
    public Debug3InstructionType Type;
}

public enum Debug3InstructionType
{
    Line,
    Sphere,
    Cube
}

public static class DevTools
{
    private static Dictionary<string, string> _printMessages = new();
    private static Dictionary<Plotable, Queue<int>> _plots = new();
    public static bool DevToolsEnabled = false;

    public static List<Debug3dInstruction> Debug3dInstructions = new List<Debug3dInstruction>();
    public static List<Action> RenderActions = new List<Action>();

    public static void Print(object? value, string name)
    {
        _printMessages[name] = value?.ToString() ?? "";
    }

    public static void Plot(int value, Plotable plotable)
    {
        return;
        if (!_plots.TryGetValue(plotable, out var queue))
        {
            queue = new Queue<int>();
            queue.Enqueue(value);
            _plots.Add(plotable, queue);
        }
        else
        {
            queue.Enqueue(value);
        }

        if (queue.Count > 100)
            queue.Dequeue();
    }

    public static void Draw3d()
    {
        if (!DevToolsEnabled)
            return;

        foreach (var ins in Debug3dInstructions)
        {
            switch (ins.Type)
            {
                case Debug3InstructionType.Line:
                    Raylib.DrawLine3D(ins.PointA, ins.PointB, ins.Color);
                    break;
                case Debug3InstructionType.Sphere:
                    Raylib.DrawSphere(ins.PointA, ins.Scalar, ins.Color);
                    break;
                case Debug3InstructionType.Cube:
                    Raylib.DrawCubeV(ins.PointA, new Vector3(ins.Scalar), ins.Color);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Debug3dInstructions.Clear();

        foreach (var renderAction in RenderActions)
        {
            renderAction();
        }

        // Debug3dInstructions.Clear();//this list is not actually needed right now, we would only need it if we would like to persist the instructions
    }

    public static void Tick()
    {
        RenderActions.Clear();
    }

    public static void Draw2d()
    {
        if (!DevToolsEnabled)
            return;

        DrawRectangle(0, 0, 500, 200, new Color(0, 0, 0, 50));
        int i = 0;
        foreach (var (name, value) in _printMessages)
        {
            DrawText($"{name}: {value}", 0, 30 * (i + 1), 20, Color.Black);
            i++;
        }


        DrawRectangle(500, 0, 400, _plots.Count * 200, new Color(0, 0, 0, 50));

        var plotIndex = 0;
        foreach (var (plotable, queue) in _plots)
        {
            var index = -1;
            float previousValue = 0;

            var sum = 0;

            foreach (var value in queue)
            {
                sum += value;

                if (index == -1)
                {
                    index++;
                    continue;
                }

                index++;

                float normalized = value - plotable.Min;
                float normalizedMax = plotable.Max - plotable.Min;

                var finalValue = normalized / normalizedMax;

                DrawLine(500 + 4 * index, plotIndex * 200 + (int)(200 - previousValue * 200), 500 + 4 * (index + 1),
                    plotIndex * 200 + (int)(200 - finalValue * 200), Color.Red);

                previousValue = finalValue;
            }

            var average = sum / queue.Count;

            DrawText(plotable.Name, 500, plotIndex * 200, 20, Color.Red);
            DrawText($"Average: {average.ToString()}", 500, plotIndex * 200 + 30, 20, Color.Red);

            plotIndex++;
        }
    }
}

public record struct Plotable(string Name, int Min, int Max);