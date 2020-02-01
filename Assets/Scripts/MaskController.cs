using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    public int BlobRadius = 14;
    private RenderTexture cracksMaskRenderTexture;
    private Texture2D cracksMaskTexture;
    private Vector3Int[] crackBlobs;

    void Start()
    {        
        this.cracksMaskRenderTexture = new RenderTexture(48, 27, 16, RenderTextureFormat.ARGB32);
        cracksMaskTexture = new Texture2D(this.cracksMaskRenderTexture.width, this.cracksMaskRenderTexture.height);
        Renderer renderer = GetComponent<Renderer>();
        renderer.materials[1].SetTexture("_OpacityTex", cracksMaskTexture);
    }

    void Update()
    {        
        
    }

    public void SetBlobs(List<Vector3> blobs, int blobRadius)
    {
        this.BlobRadius = blobRadius;
        List<Vector3Int> absoluteBlobs = new List<Vector3Int>();
        foreach (var blob in blobs)
        {
            absoluteBlobs.Add(new Vector3Int((int)(blob.x * cracksMaskTexture.width), (int)(blob.y * cracksMaskTexture.height), (int)blob.z));
        }
        crackBlobs = absoluteBlobs.ToArray();
        refreshCracksMaskByBlobs();
    }

    private void refreshCracksMaskByBlobs()
    {
        int[,] map = new int[cracksMaskRenderTexture.width, cracksMaskRenderTexture.height];
        foreach (var blob in crackBlobs)
        {
            map[blob.x, blob.y] += blob.z;
        }

        RenderTexture.active = cracksMaskRenderTexture;
        cracksMaskTexture.ReadPixels(new Rect(0, 0, cracksMaskRenderTexture.width, cracksMaskRenderTexture.height), 0, 0);

        for (int x = 0; x < cracksMaskRenderTexture.width; x++)
        {
            for (int y = 0; y < cracksMaskRenderTexture.height; y++)
            {
                cracksMaskTexture.SetPixel(x, y, new Color(1, 0, 0, map[x,y] / 100f));
            }
        }
        
        cracksMaskTexture.Apply();
        RenderTexture.active = null;
    }
}
