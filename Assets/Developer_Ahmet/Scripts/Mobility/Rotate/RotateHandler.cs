using UnityEngine;

public class RotateHandler : MonoBehaviour, IRotatable
{
    public RotateAngle RotateAngel { get; set; }
    public float RotateSpeed { get; set; }

    public void RotateWithRotateAngel(Vector3 _rotation)
    {
        switch (RotateAngel)
        {
            case RotateAngle.All:
                Rotate(_rotation);
                break;
            case RotateAngle.RotateX:
                RotateX(_rotation.x);
                break;
            case RotateAngle.RotateY:
                RotateY(_rotation.y);
                break;
            case RotateAngle.RotateZ:
                RotateZ(_rotation.z);
                break;
            default:
                break;
        }
    }
    public void Rotate(Vector3 _rotation)
    {
        transform.Rotate(_rotation * RotateSpeed);
    }
    public void RotateX(float _angle)
    {
        _angle = _angle * RotateSpeed;
        transform.Rotate(new Vector3(_angle,0,0), Space.Self);
    }

    public void RotateY(float _angle)
    {
        _angle = _angle * RotateSpeed;
        transform.Rotate(new Vector3(0, _angle, 0), Space.Self);
    }

    public void RotateZ(float _angle)
    {
        _angle = _angle * RotateSpeed;
        transform.Rotate(new Vector3(0, 0, _angle), Space.Self);
    }
}

public interface IRotatable
{
    public RotateAngle RotateAngel { get; set; }
    public float RotateSpeed { get; set; }
    void RotateWithRotateAngel(Vector3 _rotation);
    void Rotate(Vector3 _rotation);
    void RotateX(float _angle);
    void RotateY(float _angle);
    void RotateZ(float _angle);
}
public interface IRotateAnObject
{    
    public IRotatable RotatableObject{ get; set; } 
}
public enum RotateAngle
{
    All,
    RotateX,
    RotateY, 
    RotateZ
}