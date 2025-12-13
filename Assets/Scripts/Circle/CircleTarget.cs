using UnityEngine;
using UnityEngine.EventSystems;

public class CircleTarget : MonoBehaviour, IPointerDownHandler
{
    public RectTransform Rect { get; private set; }

    CircleGameManager manager;
    float lifetime;
    float t;
    bool tapped;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    public void Init(CircleGameManager mgr, float life)
    {
        manager = mgr;
        lifetime = Mathf.Max(0.1f, life);
        t = 0f;
        tapped = false;
    }

    void Update()
    {
        if (tapped) return;

        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / lifetime);
        float s = 1f - p;                // shrink to zero in lifetime
        Rect.localScale = new Vector3(s, s, 1f);

        if (t >= lifetime)
        {
            manager.OnCircleExpired();
            Destroy(gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (tapped) return;
        tapped = true;
        manager.OnCircleTapped();
        Destroy(gameObject);
    }
}
