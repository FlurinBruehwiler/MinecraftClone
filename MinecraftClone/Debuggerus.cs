namespace RayLib3dTest;

public class Debuggerus : I2DDrawable
{
    private List<PrintMessage> _printMessages = new();
    private Dictionary<Plotable, Queue<int>> _plots = new();

    public void Print(object value, string name)
    {
        _printMessages.Add(new PrintMessage(value.ToString(), name));
    }

    public void Plot(int value, Plotable plotable)
    {
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
    
    public void Draw2d()
    {
        DrawRectangle(0, 0, 300, 300, new Color(0, 0, 0, 50));
        for (var i = 0; i < _printMessages.Count; i++)
        {
            var printMessage = _printMessages[i];
            DrawText($"{printMessage.Name}: {printMessage.Value}", 0, 30 * (i + 1) , 20, Color.BLACK);
        }
        _printMessages.Clear();
        
        
        DrawRectangle(500, 0, 400,  _plots.Count * 200, new Color(0, 0, 0, 50));

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