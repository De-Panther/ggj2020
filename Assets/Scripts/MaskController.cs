using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    public int BlobRadius = 14;
    private RenderTexture cracksMaskRenderTexture;
    private Texture2D cracksMaskTexture;

    private RenderTexture moldMaskRenderTexture;
    private Texture2D moldMaskTexture;

    private Vector3Int[] crackBlobs;

    void Start()
    {        
        Renderer renderer = GetComponent<Renderer>();

        this.cracksMaskRenderTexture = new RenderTexture(48, 27, 16, RenderTextureFormat.ARGB32);
        cracksMaskTexture = new Texture2D(this.cracksMaskRenderTexture.width, this.cracksMaskRenderTexture.height);
        renderer.materials[1].SetTexture("_OpacityTex", cracksMaskTexture);

        this.moldMaskRenderTexture = new RenderTexture(24, 14, 16, RenderTextureFormat.ARGB32);
        moldMaskTexture = new Texture2D(this.moldMaskRenderTexture.width, this.moldMaskRenderTexture.height);
        renderer.materials[2].SetTexture("_OpacityTex", moldMaskTexture);
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
        int[,] cracksMap = new int[cracksMaskRenderTexture.width, cracksMaskRenderTexture.height];
        int[,] moldMap = new int[moldMaskRenderTexture.width, moldMaskRenderTexture.height];
        foreach (var blob in crackBlobs)
        {
            cracksMap[blob.x, blob.y] += Mathf.Min(blob.z, 100);
            moldMap[blob.x / 2, blob.y / 2] += Mathf.Clamp(blob.z - 100, 0, 100);
        }

        RenderTexture.active = cracksMaskRenderTexture;
        cracksMaskTexture.ReadPixels(new Rect(0, 0, cracksMaskRenderTexture.width, cracksMaskRenderTexture.height), 0, 0);

        for (int x = 0; x < cracksMaskRenderTexture.width; x++)
        {
            for (int y = 0; y < cracksMaskRenderTexture.height; y++)
            {
                cracksMaskTexture.SetPixel(x, y, new Color(1, 0, 0, cracksMap[x,y] / 100f));
            }
        }
        
        cracksMaskTexture.Apply();
        RenderTexture.active = null;

        RenderTexture.active = moldMaskRenderTexture;
        moldMaskTexture.ReadPixels(new Rect(0, 0, moldMaskRenderTexture.width, moldMaskRenderTexture.height), 0, 0);

        for (int x = 0; x < moldMaskRenderTexture.width; x++)
        {
            for (int y = 0; y < moldMaskRenderTexture.height; y++)
            {
                moldMaskTexture.SetPixel(x, y, new Color(1, 0, 0, moldMap[x, y] / 100f));
            }
        }

        moldMaskTexture.Apply();
        RenderTexture.active = null;
    }
}
