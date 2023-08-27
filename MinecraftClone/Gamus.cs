namespace RayLib3dTest;

public class Gamus
{
    private CameraManager _cameraManager;
    private readonly Chunker _chunker;
    private readonly CameraManager _manager;
    private Debuggerus _debuggerus;
    private readonly IControlable _player;


    public Gamus(CameraManager cameraManager, Chunker chunker, CameraManager manager, Debuggerus debuggerus, IControlable player)
    {
        _cameraManager = cameraManager;
        _chunker = chunker;
        _manager = manager;
        _debuggerus = debuggerus;
        _player = player;
    }

    public void GameLoop()
    {
        while (!WindowShouldClose())
        {
            MakeTheServus();
            
            BeginDrawing();
    
                ClearBackground(Color.RAYWHITE);

                BeginMode3D(_cameraManager.Camera);

                    Draw3d();
    
                EndMode3D();

                Draw2d();
            
            EndDrawing();
        }
    }

    private void MakeTheServus()
    {
        _cameraManager.Update();
    }

    private void Draw2d()
    {
        _debuggerus.Draw2d();
    }

    private void Draw3d()
    {
        _chunker.Draw3d();   
    }
}