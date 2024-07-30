using DG.Tweening;
using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;


public class SettingsDrawerController : MonoBehaviour
{
    public RectTransform panel; // Panelin RectTransform bile�eni
    public float moveDistance = 500f; // Panelin hareket edece�i mesafe
    public float moveDuration = 1f; // Panelin hareket s�resi

    private Vector3 initialPosition; // Panelin ba�lang�� konumu
    private Vector3 targetPosition; // Panelin hedef konumu
    private bool isAtTarget = false; // Panelin hedefte olup olmad���n� kontrol eder
    private void Start()
    {
        initialPosition = panel.localPosition; // Panelin ba�lang�� konumunu al
        targetPosition = new Vector3(initialPosition.x - moveDistance, initialPosition.y, initialPosition.z); // Hedef konumu belirle
    }
    public void ShowPanel()
    {
        if (isAtTarget)
        {
            panel.DOLocalMove(initialPosition, moveDuration).SetEase(Ease.InOutSine); // Paneli ba�lang�� konumuna d�nd�r
        }
        else
        {
            panel.DOLocalMove(targetPosition, moveDuration).SetEase(Ease.InOutSine); // Paneli hedef konuma ta��
        }
        isAtTarget = !isAtTarget; // Panelin konum durumunu tersine �evir
    }
    

}

