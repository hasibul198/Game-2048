using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }

    public int number { get; private set; }
    public bool locked { get; set; }

    private Image background;
    private TextMeshProUGUI text;

    private void Awake()
    {
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;

        background.color = state.bagroundColor;
        text.color = state.tectColor;
        text.text = number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null) this.cell.tile = null;
        this.cell = cell;
        this.cell.tile = this;

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        transform.position = cell.transform.position;

        Vector3 localPos = transform.localPosition;
        localPos.z = 0f;
        transform.localPosition = localPos;

        transform.localScale = Vector3.zero;
        StartCoroutine(AnimateScale(Vector3.one));
    }

    
    public void MoveTo(TileCell cell)
    {
        if (this.cell != null) this.cell.tile = null;
        this.cell = cell;
        this.cell.tile = this;
        StartCoroutine(AnimateMove(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null) this.cell.tile = null;
        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(AnimateMove(cell.transform.position, true));
    }

    public void Bounce()
    {
        StopCoroutine(nameof(AnimateBounce));
        StartCoroutine(AnimateBounce());
    }

    private IEnumerator AnimateMove(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;

        if (merging) Destroy(gameObject);
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private IEnumerator AnimateBounce()
    {
        float elapsed = 0f;
        float duration = 0.15f;

        Vector3 initialScale = Vector3.one;
        Vector3 upScale = Vector3.one * 1.25f;

        Color originalColor = state.bagroundColor;
        Color flashColor = Color.white;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, upScale, t);
            background.color = Color.Lerp(originalColor, flashColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(upScale, initialScale, t);
            background.color = Color.Lerp(flashColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = initialScale;
        background.color = originalColor;
    }
}