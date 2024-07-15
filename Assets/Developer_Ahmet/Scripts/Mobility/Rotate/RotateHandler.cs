using UnityEngine;

public class RotateHandler : MonoBehaviour, IRotatable
{
    public void Rotate(Vector3 _rotation)
    {
        transform.Rotate(_rotation);
    }

    public void RotateX(float _angle)
    {
        transform.Rotate(new Vector3(_angle,0,0), Space.Self);
    }

    public void RotateY(float _angle)
    {
        transform.Rotate(new Vector3(0, _angle, 0), Space.Self);
    }

    public void RotateZ(float _angle)
    {
        transform.Rotate(new Vector3(0, 0, _angle), Space.Self);
    }
}

public interface IRotatable
{
    void Rotate(Vector3 _rotation);
    void RotateX(float _angle);
    void RotateY(float _angle);
    void RotateZ(float _angle);
}
public interface IRotateAnObject
{    
    public IRotatable RotatableObject{ get; set; } 
}
