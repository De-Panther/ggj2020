using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    public int BlobRadius = 40;
    private RenderTexture cracksMaskRenderTexture;
    private Texture2D cracksMaskTexture;
    public Vector2Int[] crackBlob;

    void Start()
    {        
        this.cracksMaskRenderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
        cracksMaskTexture = new Texture2D(this.cracksMaskRenderTexture.width, this.cracksMaskRenderTexture.height);
        Renderer renderer = GetComponent<Renderer>();
        renderer.materials[1].SetTexture("_OpacityTex", cracksMaskTexture);

        crackBlob = new Vector2Int[40];
        for (int i = 0; i < 40; i++)
        {
            crackBlob[i] = new Vector2Int(Random.Range(0, cracksMaskTexture.width), Random.Range(0, cracksMaskTexture.height));
        }
        refreshCracksMaskByBlobs();
    }

    float latestUpdateTime;
    void Update()
    {        
        if (Time.time - latestUpdateTime > 0.5f)
        {
            latestUpdateTime = Time.time;
            refreshCracksMaskByBlobs();
        }
    }

    private void refreshCracksMaskByBlobs()
    {
        RenderTexture.active = cracksMaskRenderTexture;
        cracksMaskTexture.ReadPixels(new Rect(0, 0, cracksMaskRenderTexture.width, cracksMaskRenderTexture.height), 0, 0);

        foreach (var blob in crackBlob)
        {
            for (int x = Mathf.Max(blob.x - BlobRadius, 0); x < Mathf.Min(blob.x + BlobRadius, cracksMaskTexture.width); x++)
            {
                for (int y = Mathf.Max(blob.y - BlobRadius, 0); y < Mathf.Min(blob.y + BlobRadius, cracksMaskTexture.height); y++)
                {
                    if (Vector2.Distance(new Vector2(x, y), blob) < BlobRadius)
                    {
                        cracksMaskTexture.SetPixel(x, y, new Color(1, 0, 0));
                    }
                }
            }
        }

        cracksMaskTexture.Apply();
        RenderTexture.active = null;
    }
}
