namespace RayLib3dTest;

public class CameraManager
{
    public Camera3D Camera;
    private float _sens = 60;


    public CameraManager()
    {
        Camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE)
        {
            fovy = 100
        };
    }

    public void UpdateCamera(IControlable controlable)
    {
        var posChangeInWorldSpace = controlable.Position - Camera.position;

        var cameraRight = Vector3.Normalize(new Vector3(Camera.GetRight().X, 0, Camera.GetRight().Z));
        var cameraForward = Vector3.Normalize(new Vector3(-Camera.GetRight().Z, 0, Camera.GetRight().X));

        var localX = Vector3.Dot(cameraRight, posChangeInWorldSpace);
        var localZ = Vector3.Dot(cameraForward, posChangeInWorldSpace);

        var newlocalMoveDelta = new Vector3(localX, posChangeInWorldSpace.Y, localZ);

        var camera = Camera;
        UpdateCameraPro(ref camera,
            new Vector3(newlocalMoveDelta.X, newlocalMoveDelta.Z, newlocalMoveDelta.Y) * _sens * GetFrameTime(),
            new Vector3(GetMouseDelta() * 0.5f, 0), 0);
    }
}