using UnityEngine;

public class CharacterRotation : MonoBehaviour
{
   
    private void Update()
    {
        float hiz = 10;
        
        if (Input.touchCount > 0)
        {
            Touch dokunma = Input.GetTouch(0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(new Vector3 (0, dokunma.position.x, 0)), Time.deltaTime * hiz);       
        }

    }
}
