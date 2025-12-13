using UnityEngine;
using UnityEngine.EventSystems;

public class CircleTapCatcher : MonoBehaviour, IPointerDownHandler
{
    public CircleGameManager manager;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (manager) manager.OnMissTap();
    }
}
