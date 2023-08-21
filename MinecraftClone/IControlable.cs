namespace RayLib3dTest;

public interface IControlable
{
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }

    public void Move(Vector3 posChangeInWorldSpace);
    public Vector3 Forward { get; }
    public Vector3 Right { get; }
    public Vector3 Up { get; }
}