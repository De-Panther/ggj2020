using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    public GameObject Ceiling;
    public Transform BlobsContainer;
    public GameObject BlobPrefab;
    public Texture2D Cracks;

    private int blobRadius;

    private Dictionary<GameObject, Vector2> blobToRelativePosition = new Dictionary<GameObject, Vector2>();
        
    void Start()
    {
        CreateBlobGameObjects();

        InvokeRepeating("UpdateBlobsMask", 0f, 0.05f);
        InvokeRepeating("AddRandomCrack", 0f, 5f);
    }

    private void AddRandomCrack()
    {
        List<GameObject> blobs = new List<GameObject>(blobToRelativePosition.Keys);
        var blobScript = blobs[Random.Range(0, blobs.Count - 1)].GetComponent<BlobScript>();

        if (blobScript.CrackState == 0)
        {
            blobScript.CrackState = 1;
        }
    }

    private void CreateBlobGameObjects()
    {
        Color[] pixels = Cracks.GetPixels();

        List<Vector2Int> blobs = new List<Vector2Int>();

        int blobSize = 30;
        blobRadius = (int)(blobSize / 1.5f);
        {
            float alphaSum;
            bool hasFound;
            for (int centerX = blobSize / 2; centerX < Cracks.width; centerX += blobSize)
            {
                for (int centerY = blobSize / 2; centerY < Cracks.height; centerY += blobSize)
                {
                    hasFound = false;
                    for (int x = centerX - blobSize / 2; x < centerX + blobSize / 2; x++)
                    {
                        alphaSum = 0;
                        for (int y = centerY - blobSize / 2; y < centerY + blobSize / 2; y++)
                        {
                            alphaSum += pixels[x + y * Cracks.width].a;

                            if (alphaSum > Mathf.Pow(blobSize, 2) / 300f)
                            {
                                hasFound = true;
                                break;
                            }
                        }

                        if (hasFound) break;
                    }

                    if (hasFound)
                    {
                        blobs.Add(new Vector2Int(centerX, centerY));
                    }
                }
            }
        }

        foreach (var blob in blobs)
        {
            var blobObject = Instantiate(BlobPrefab, BlobsContainer);
            float relativeX = blob.x / (float)Cracks.width;
            float relativeY = blob.y / (float)Cracks.height;
            blobToRelativePosition.Add(blobObject, new Vector2(relativeX, relativeY));
            blobObject.transform.localPosition = new Vector3((relativeX - .5f) * 10 * Ceiling.transform.localScale.x, 0, (relativeY - .5f) * 10);
            blobObject.GetComponent<BlobScript>().ShouldInfect += BlobScript_ShouldInfect;
        }
    }

    private void BlobScript_ShouldInfect(GameObject blob)
    {
        InfectNearbyBlob(blob);
    }
        
    private void UpdateBlobsMask()
    {
        List<Vector3> blobsForMask = new List<Vector3>();
        foreach (Transform blob in BlobsContainer.transform)
        {
            var blobForMask = new Vector3();
            blobForMask.x = blobToRelativePosition[blob.gameObject].x;
            blobForMask.y = blobToRelativePosition[blob.gameObject].y;
            blobForMask.z = blob.GetComponent<BlobScript>().CrackState;
            blobsForMask.Add(blobForMask);
        }
        Ceiling.GetComponent<MaskController>().SetBlobs(blobsForMask, blobRadius);
    }

    private void InfectNearbyBlob(GameObject sourceBlob)
    {
        GameObject minDistanceNotInfectedBlob = null;
        float minDistanceNotInfectedBlobDistance = -1;

        Vector2 sourceBlobRelativePosition = blobToRelativePosition[sourceBlob];
        foreach (var blob in blobToRelativePosition.Keys)
        {
            if (blob == sourceBlob) continue;
            if (blob.GetComponent<BlobScript>().CrackState > 0) continue;

            float distance = Vector2.Distance(blobToRelativePosition[blob], sourceBlobRelativePosition);

            if (distance > 0.1f) continue;

            if (minDistanceNotInfectedBlobDistance == -1 || 
                (distance < minDistanceNotInfectedBlobDistance))
            {
                minDistanceNotInfectedBlobDistance = distance;
                minDistanceNotInfectedBlob = blob;
            }
        }

        if (minDistanceNotInfectedBlob)
        {
            minDistanceNotInfectedBlob.GetComponent<BlobScript>().CrackState = 0.1f;
        }
    }
}
