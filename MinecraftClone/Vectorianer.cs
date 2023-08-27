namespace RayLib3dTest;

public static class Vectorianer
{
    public static Vector3 GetForward(this Camera3D camera)
    {
        return Vector3.Normalize(camera.target - camera.position);
    }
    
    public static Vector3 GetLeft(this Camera3D camera)
    {
        var forward = GetForward(camera);
        var up = GetUp(camera);
        return -Vector3.Cross(forward, up);
    }
    
    public static Vector3 GetUp(this Camera3D camera)
    {
        return Vector3.Normalize(camera.up);
    }
}