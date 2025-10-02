using Flamui;

namespace RayLib3dTest;

public class RaylibUiTreeHost : IUiTreeHost
{
    public string GetClipboardText()
    {
        return Raylib.GetClipboardText_();
    }

    public void SetClipboardText(string text)
    {
        Raylib.SetClipboardText(text);
    }

    public void SetCursorStyle(CursorShape cursorShape)
    {
        Raylib.SetMouseCursor(cursorShape switch
        {
            CursorShape.Default => MouseCursor.Default,
            CursorShape.Arrow => MouseCursor.Arrow,
            CursorShape.IBeam => MouseCursor.IBeam,
            CursorShape.Crosshair => MouseCursor.Crosshair,
            CursorShape.Hand => MouseCursor.PointingHand,
            CursorShape.HResize => MouseCursor.ResizeEw,
            CursorShape.VResize => MouseCursor.ResizeNs,
            CursorShape.NwseResize => MouseCursor.ResizeNwse,
            CursorShape.NeswResize => MouseCursor.ResizeNesw,
            CursorShape.ResizeAll => MouseCursor.ResizeAll,
            CursorShape.NotAllowed => MouseCursor.NotAllowed,
            CursorShape.Wait => MouseCursor.Default,
            CursorShape.WaitArrow => MouseCursor.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(cursorShape), cursorShape, null)
        });
    }

    public void CloseWindow()
    {
        Raylib.CloseWindow();
    }
}