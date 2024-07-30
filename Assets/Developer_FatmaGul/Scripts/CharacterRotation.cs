using UnityEngine;

public class CharacterRotation : MonoBehaviour
{

    public float hiz = 10f;
    private bool isTouched = false;

    private void Update()
    {
        // Dokunma varsa
        if (Input.touchCount > 0)
        {
            Touch dokunma = Input.GetTouch(0);
            Vector2 touchPosition = dokunma.position;

            // Dokunma noktas�yla ray olu�tur
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;

            // Ray'in karakteri vurup vurmad���n� kontrol et
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                isTouched = true;
            }
            else
            {
                isTouched = false;
            }

            // Karaktere dokunulmu�sa d�n��� yap
            if (isTouched)
            {
                // Karakterin mevcut d�n���n� al
                Quaternion currentRotation = transform.localRotation;

                // Yeni d�n�� hesapla
                Quaternion targetRotation = Quaternion.Euler(new Vector3(0, dokunma.position.x, 0));

                // D�n��� yumu�ak bir �ekilde uygulama
                transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * hiz);
            }
        }
        else
        {
            isTouched = false;
        }
    }
}
