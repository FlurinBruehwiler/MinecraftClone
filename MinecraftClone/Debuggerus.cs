namespace RayLib3dTest;

public class Debuggerus : I2DDrawable
{
    private List<PrintMessage> _printMessages = new();

    public void Print(object value, string name)
    {
        _printMessages.Add(new PrintMessage(value.ToString(), name));
    }

    public void Draw2d()
    {
        
    }
}