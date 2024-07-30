using DG.Tweening;
using MalbersAnimations;
using System.Collections;
using TMPro;
using UnityEngine;


public class SettingsDrawerController : MonoBehaviour
{
    public RectTransform panel; // Panelin RectTransform bileþeni
    public float moveDistance = 500f; // Panelin hareket edeceði mesafe
    public float moveDuration = 1f; // Panelin hareket süresi

    private Vector3 initialPosition; // Panelin baþlangýç konumu
    private Vector3 targetPosition; // Panelin hedef konumu
    private bool isAtTarget = false; // Panelin hedefte olup olmadýðýný kontrol eder
    private void Start()
    {
        initialPosition = panel.localPosition; // Panelin baþlangýç konumunu al
        targetPosition = new Vector3(initialPosition.x - moveDistance, initialPosition.y, initialPosition.z); // Hedef konumu belirle
    }
    public void ShowPanel()
    {
        if (isAtTarget)
        {
            panel.DOLocalMove(initialPosition, moveDuration).SetEase(Ease.InOutSine); // Paneli baþlangýç konumuna döndür
        }
        else
        {
            panel.DOLocalMove(targetPosition, moveDuration).SetEase(Ease.InOutSine); // Paneli hedef konuma taþý
        }
        isAtTarget = !isAtTarget; // Panelin konum durumunu tersine çevir
    }
    

}

