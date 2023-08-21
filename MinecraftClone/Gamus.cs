namespace RayLib3dTest;

public class Gamus
{
    private List<I2DDrawable> _2dDrawables = new();
    private List<I3DDrawable> _3dDrawables = new();
    private List<IServus> _services = new();
    private CameraManager _cameraManager = new();

    public void RegisterDraw2d(I2DDrawable instance)
    {
        _2dDrawables.Add(instance);
    }
    
    public void RegisterDraw3d(I3DDrawable instance)
    {
        _3dDrawables.Add(instance);
    }

    public void RegisterServus(IServus instance)
    {
        _services.Add(instance);
    }

    public void GameLoop()
    {
        while (!WindowShouldClose())
        {
            BeginDrawing();
    
                ClearBackground(Color.RAYWHITE);

                BeginMode3D(_cameraManager.Camera);

                    Draw3d();
    
                EndMode3D();

                Draw2d();
            
            EndDrawing();
        }
    }

    private void Draw2d()
    {
        
    }

    private void Draw3d()
    {
        foreach (var drawable in _3dDrawables)
        {
            drawable.Draw3d();
        }
    }
}