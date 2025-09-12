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
    private static List<PrintMessage> _printMessages = new();
    private static Dictionary<Plotable, Queue<int>> _plots = new();

    public static List<Debug3dInstruction> Debug3dInstructions = new List<Debug3dInstruction>();

    public static void Print(object value, string name)
    {
        _printMessages.Add(new PrintMessage(value.ToString(), name));
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

        // Debug3dInstructions.Clear();//this list is not actually needed right now, we would only need it if we would like to persist the instructions
    }

    public static void Draw2d()
    {
        DrawRectangle(0, 0, 300, 300, new Color(0, 0, 0, 50));
        for (var i = 0; i < _printMessages.Count; i++)
        {
            var printMessage = _printMessages[i];
            DrawText($"{printMessage.Name}: {printMessage.Value}", 0, 30 * (i + 1), 20, Color.BLACK);
        }

        _printMessages.Clear();


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
                    plotIndex * 200 + (int)(200 - finalValue * 200), Color.RED);

                previousValue = finalValue;
            }

            var average = sum / queue.Count;

            DrawText(plotable.Name, 500, plotIndex * 200, 20, Color.RED);
            DrawText($"Average: {average.ToString()}", 500, plotIndex * 200 + 30, 20, Color.RED);

            plotIndex++;
        }
    }
}

public record struct Plotable(string Name, int Min, int Max);