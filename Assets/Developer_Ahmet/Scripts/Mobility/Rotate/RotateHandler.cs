using UnityEngine;

public class RotateHandler : MonoBehaviour, IRotatable
{
    public RotateAngle RotateAngel { get; set; }
    public float RotateSpeed { get; set; } = 10f;

    public void RotateWithRotateAngel(Vector3 rotation)
    {
        //Vector3 rotation = new Vector3((_rotation.x * RotateSpeed * Time.deltaTime), (_rotation.y * RotateSpeed * Time.deltaTime), 0);
        //Debug.Log("RotateWithRotateAngel rotation => " + rotation);
        switch (RotateAngel)
        {
            case RotateAngle.All:
                Rotate(rotation);
                break;
            case RotateAngle.RotateX:
                RotateX(rotation.x);
                break;
            case RotateAngle.RotateY:
                RotateY(rotation.y);
                break;
            case RotateAngle.RotateZ:
                RotateZ(rotation.z);
                break;
            default:
                break;
        }
    }

    public void Rotate(Vector3 rotation)
    {
        Quaternion targetRotation = Quaternion.Euler(rotation) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotateSpeed);
    }

    public void RotateX(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(angle, 0, 0) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotateSpeed);
    }

    public void RotateY(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotateSpeed);
    }

    public void RotateZ(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotateSpeed);
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