using Flamui;

namespace RayLib3dTest;

public class RaylibUiTreeHost : IUiTreeHost
{
    public string GetClipboardText()
    {
        return "";
    }

    public void SetClipboardText(string text)
    {

    }

    public void SetCursorStyle(CursorShape cursorShape)
    {
    }

    public void CloseWindow()
    {
        CloseWindow();
    }

    public (int width, int height) GetSize()
    {
        return (GetScreenWidth(), GetScreenHeight());
    }
}