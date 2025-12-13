using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CritterTarget : MonoBehaviour, IPointerDownHandler
{
    [Header("UI")]
    public Image img;

    [Header("Animations (assign in Inspector if you want)")]
    public Sprite[] walkFrames;       // Walk_0..Walk_5
    public float walkFps = 10f;
    public Sprite[] deathFrames;      // Death_0..Death_5
    public float deathFps = 16f;

    [Header("Auto-load fallback (optional)")]
    public bool autoLoadFromResources = false;
    public string resourcesSheetName = "Death";     // name of the sprite sheet file inside Assets/Resources (no extension)
    public string walkPrefix = "Walk_";
    public string deathPrefix = "Death_";

    [Header("Movement")]
    public float minSpeed = 40f;
    public float maxSpeed = 120f;

    [Header("Timing")]
    public float deathShowTime = 0.2f;

    public RectTransform Rect { get; private set; }

    CircleGameManager gm;
    RectTransform area;
    float padding;
    float lifeLeft;
    Vector2 dir;
    float speed;
    bool dead;
    Coroutine animCo;

    public void Init(CircleGameManager manager, RectTransform spawnArea, float lifetime, float spawnPadding)
    {
        gm = manager;
        area = spawnArea;
        padding = spawnPadding;
        lifeLeft = lifetime;

        Rect = transform as RectTransform;
        if (!img) img = GetComponent<Image>();

        // ✅ Make sure the UI Image is actually enabled/visible
        if (img)
        {
            img.enabled = true;
            img.color = Color.white;       // alpha 1
            img.raycastTarget = true;
            img.preserveAspect = true;
        }

        // ✅ Fallback: if frames weren’t carried into the clone, auto-load from Resources
        if (autoLoadFromResources && (walkFrames == null || walkFrames.Length == 0))
            TryLoadFramesFromResources();

        // ✅ Force a sprite immediately so it’s never “None”
        if (img && (walkFrames != null && walkFrames.Length > 0) && img.sprite == null)
            img.sprite = FirstNonNull(walkFrames);

        Debug.Log($"Pulpo Init -> walk={walkFrames?.Length ?? 0}, death={deathFrames?.Length ?? 0}, sprite={(img && img.sprite ? img.sprite.name : "NULL")}");

        // Random direction + speed
        dir = Random.insideUnitCircle.normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        speed = Random.Range(minSpeed, maxSpeed);

        // Start walk anim if we have frames
        if (animCo != null) StopCoroutine(animCo);
        if (img != null && walkFrames != null && walkFrames.Length > 0 && FirstNonNull(walkFrames) != null)
            animCo = StartCoroutine(PlayLoop(walkFrames, walkFps));
        else
            Debug.LogWarning("CritterTarget: walkFrames is empty (or all null). Assign frames on the prefab OR enable autoLoadFromResources.");
    }

    void TryLoadFramesFromResources()
    {
        // Put your sliced spritesheet PNG inside: Assets/Resources/<resourcesSheetName>.png
        // Unity will allow loading sub-sprites via LoadAll<Sprite>
        var all = Resources.LoadAll<Sprite>(resourcesSheetName);
        if (all == null || all.Length == 0)
        {
            Debug.LogWarning($"CritterTarget: Resources.LoadAll<Sprite>(\"{resourcesSheetName}\") returned 0. " +
                             $"If you want auto-load, move the spritesheet into Assets/Resources and set resourcesSheetName correctly.");
            return;
        }

        walkFrames = all.Where(s => s != null && s.name.StartsWith(walkPrefix)).OrderBy(s => s.name).ToArray();
        deathFrames = all.Where(s => s != null && s.name.StartsWith(deathPrefix)).OrderBy(s => s.name).ToArray();
    }

    static Sprite FirstNonNull(Sprite[] frames)
    {
        if (frames == null) return null;
        for (int i = 0; i < frames.Length; i++)
            if (frames[i] != null) return frames[i];
        return null;
    }

    void Update()
    {
        if (dead) return;
        if (gm == null || !gm.IsRunning) return;

        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f)
        {
            gm.OnTargetExpired(this);
            Destroy(gameObject);
            return;
        }

        // move + bounce inside spawn area
        Rect r = area.rect;

        Vector2 p = Rect.anchoredPosition;
        p += dir * speed * Time.deltaTime;

        float xMin = r.xMin + padding;
        float xMax = r.xMax - padding;
        float yMin = r.yMin + padding;
        float yMax = r.yMax - padding;

        if (p.x < xMin) { p.x = xMin; dir.x *= -1f; }
        if (p.x > xMax) { p.x = xMax; dir.x *= -1f; }
        if (p.y < yMin) { p.y = yMin; dir.y *= -1f; }
        if (p.y > yMax) { p.y = yMax; dir.y *= -1f; }

        Rect.anchoredPosition = p;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dead) return;
        if (gm == null || !gm.IsRunning) return;

        dead = true;
        if (img) img.raycastTarget = false;

        gm.OnTargetTapped(this);

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(PlayDeathAndDestroy());
    }

    IEnumerator PlayLoop(Sprite[] frames, float fps)
    {
        if (img == null || frames == null || frames.Length == 0) yield break;

        float dt = (fps <= 0f) ? 0.1f : 1f / fps;
        int i = 0;

        while (!dead)
        {
            var s = frames[i];
            if (s != null) img.sprite = s;

            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(dt);
        }
    }

    IEnumerator PlayDeathAndDestroy()
    {
        if (img == null || deathFrames == null || deathFrames.Length == 0 || FirstNonNull(deathFrames) == null)
        {
            yield return new WaitForSeconds(deathShowTime);
            Destroy(gameObject);
            yield break;
        }

        float dt = (deathFps <= 0f) ? 0.06f : 1f / deathFps;

        for (int i = 0; i < deathFrames.Length; i++)
        {
            if (deathFrames[i] != null) img.sprite = deathFrames[i];
            yield return new WaitForSeconds(dt);
        }

        Destroy(gameObject);
    }
}
