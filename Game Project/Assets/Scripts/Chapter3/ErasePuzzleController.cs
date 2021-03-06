using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ErasePuzzleController : MonoBehaviour
{
    private Texture2D m_Texture;
    private Color[] m_Colors;
    RaycastHit2D hit;
    SpriteRenderer spriteRend;
    Color zeroAlpha = Color.clear;
    public int erSize;
    public float autoEraseTime;
    public Vector3[] autoWaypoints;
    public Vector2Int lastPos;
    public Vector3 mousePosition;
    public float erasePersent;
    private bool Drawing = false;
    private bool isComplete = false;
    private bool drawable = false;
    private float startTime;
    private int currentIndex = 0;
    void Start ()
    {
        spriteRend = gameObject.GetComponent<SpriteRenderer>();
        var tex = spriteRend.sprite.texture;
        m_Texture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        m_Texture.filterMode = FilterMode.Bilinear;
        m_Texture.wrapMode = TextureWrapMode.Clamp;
        m_Colors = tex.GetPixels();
        m_Texture.SetPixels(m_Colors);
        m_Texture.Apply();
        spriteRend.sprite = Sprite.Create(m_Texture, spriteRend.sprite.rect, new Vector2(0.5f, 0.5f));
        startTime = Time.time;
	}

    void Update()
    {
        if (Input.GetMouseButton(0) && drawable)
        {
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null) {
                UpdateTexture();
                Drawing = true;
            }
        }
        else if(!drawable) {
            Vector3 mockMousePos = GetMockPos();
            hit = Physics2D.Raycast(mockMousePos, Vector2.zero);
            if(hit.collider != null) {
                UpdateTexture();
                Drawing = true;
            }
            if(Time.time - startTime >= autoEraseTime * autoWaypoints.Length) {
                drawable = true;
            }
        }
        else {
            Drawing = false;
        }
        if(!isComplete && CheckErased()) {
            StartCoroutine(FadeOut());
        }
    }

    private Vector3 GetMockPos() {
        if(currentIndex >= autoWaypoints.Length - 1) {
            return autoWaypoints[autoWaypoints.Length - 1];
        }
        Vector3 mockMousePos = Vector3.Lerp(autoWaypoints[currentIndex], autoWaypoints[currentIndex+1], 
             (Time.time - startTime) / autoEraseTime - currentIndex);
        if(Time.time - startTime >= autoEraseTime * (currentIndex + 1)){
            currentIndex++;
        }
        return mockMousePos;
    }

    public void UpdateTexture()
    {
        int w = m_Texture.width;
        int h = m_Texture.height;
        var mousePos = hit.point - (Vector2)hit.collider.bounds.min;
        mousePos.x *= w / hit.collider.bounds.size.x;
        mousePos.y *= h / hit.collider.bounds.size.y;
        Vector2Int p = new Vector2Int((int)mousePos.x, (int)mousePos.y);
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int start = new Vector2Int();
        Vector2Int end = new Vector2Int();
        if(!Drawing)
            lastPos = p;
        start.x = Mathf.Clamp(Mathf.Min(p.x, lastPos.x) - erSize, 0, w);
        start.y = Mathf.Clamp(Mathf.Min(p.y, lastPos.y) - erSize, 0, h);
        end.x = Mathf.Clamp(Mathf.Max(p.x, lastPos.x) + erSize, 0, w);
        end.y = Mathf.Clamp(Mathf.Max(p.y, lastPos.y) + erSize, 0, h);
        Vector2 dir = p - lastPos;
        for (int x = start.x; x < end.x; x++)
        {
            for (int y = start.y; y < end.y; y++)
            {
                Vector2 pixel = new Vector2(x, y);
                Vector2 linePos = p;
                if (Drawing)
                {
                    float d = Vector2.Dot(pixel - lastPos, dir) / dir.sqrMagnitude;
                    d = Mathf.Clamp01(d);
                    linePos = Vector2.Lerp(lastPos, p, d);
                }
                if ((pixel - linePos).sqrMagnitude <= erSize * erSize)
                {
                    m_Colors[x + y * w] = zeroAlpha;
                }
            }
        }
        lastPos = p;
        m_Texture.SetPixels(m_Colors);
        m_Texture.Apply();
        spriteRend.sprite = Sprite.Create(m_Texture, spriteRend.sprite.rect, new Vector2(0.5f, 0.5f));
    }

    private bool CheckErased() {
        Color[] m_Colors = spriteRend.sprite.texture.GetPixels();
        float averageAlpha = 0f;
        foreach(Color c in m_Colors) {
            averageAlpha += c.a;
        }
        if(averageAlpha/m_Colors.Length <= 1 - erasePersent) {
            return true;
        }
        return false;
    }

    IEnumerator FadeOut() {
        if(GameObject.FindGameObjectWithTag("Logger") != null) {
            GameObject.FindGameObjectWithTag("Logger").GetComponent<Logger>().LogData("Puzzle2-1", (Time.time - startTime).ToString());
        }
        isComplete = true;
        GetComponent<FadeInOut>().StartFadingOut();
        yield return new WaitForSeconds(3f);
        GetComponentInParent<FadeInOut>().StartFadingOut();
        foreach(FadeInOut f in transform.parent.GetComponentsInChildren<FadeInOut>()) {
            f.StartFadingOut();
        }
        yield return new WaitForSeconds(2f);
        GameObject.FindGameObjectWithTag("GameController").GetComponent<Chapter3Manager>().ChangeStage();
    }
}