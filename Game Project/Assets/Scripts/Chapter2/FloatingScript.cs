using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingScript : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text text;
    public float lifeSpan = 4f;
    public float dir = 0;
    public bool spawnPuzzleAfter = true;
    public Vector3[] path;
    public float duration;
    public float fadeSpeed = 2f;
    private float seed;
    private Tween myTween;
    private bool isFadingOut = false;
    void Start()
    {
        Destroy(gameObject, lifeSpan);
        seed = Random.Range(0f, 100f);
        for(int i = 0; i < path.Length; i++) {
            path[i] = new Vector3 (path[i].x, path[i].y - dir, path[i].z);
        }
        myTween = transform.DOPath(path, duration, PathType.CatmullRom);
        StartCoroutine(SpawnPuzzles());
    }

    // Update is called once per frame
    void Update()
    {
        text.ForceMeshUpdate();
        var textInfo = text.textInfo;
        for(int i=0; i< textInfo.characterCount; i++) {
            var charInfo = textInfo.characterInfo[i];
            if(!charInfo.isVisible) {
                continue;
            }
            var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            for(int j=0;j<4;j++) {
                var orig = vertices[charInfo.vertexIndex + j];
                vertices[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * 2f + orig.x * 0.01f + Time.time) * 10f, 0);
            }
        }
        for(int i=0; i<textInfo.meshInfo.Length; i++) {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
        if(isFadingOut) {
            Color c = text.color;
            c.a -= Time.deltaTime * fadeSpeed;
            if(c.a <= 0) {
                c.a = 0;
                isFadingOut = false;
            }
            text.color = c;
        }
    }

    public void StartFadingOut() {
        isFadingOut = true;
    }

    IEnumerator SpawnPuzzles() {
        yield return new WaitForSeconds(duration - 4f);
        if(spawnPuzzleAfter) {
            GetComponentInParent<FloatingTextManager>().SpawnPuzzle(transform.position);
        }
    }
}
