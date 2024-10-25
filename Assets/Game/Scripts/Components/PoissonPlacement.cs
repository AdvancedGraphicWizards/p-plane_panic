
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class PoissonPlacement : MonoBehaviour
{
	public float radius = 1;
	public Vector3 regionSize = Vector3.one;
	public int rejectionSamples = 30;
	public float displayRadius =1;
	public float noiseMaskScale =1f;
	public float noiseThreshold =0.5f;
	public float maxTerrainHeight = 100f;
	public float maxTerrainDepth = 100f;
	public int layerMask = ~(1 << 8);
	public float maxSteepness = 0.5f; // 0 is default
    public GameObject[] objArray;
    public string ignoreTag = "Water";

	private float randOffset =0f;
    private ObjectPool objPool;
    private Vector2 offset;

    private Transform loadTarget;
    private Vector2 startPoint;
    public Vector2 loadRadius;



	List<Vector2> points;
	List<Vector2> maskedPoints;
	List<Vector3> meshPoints;

    void Start()
    {
        // Get world origin
        if (loadTarget == null )
            startPoint = Vector2.zero;
        else 
            startPoint = loadTarget.position;
    
        // Set object pool and rand offset for noise
        objPool = new ObjectPool(1000, objArray, transform);
        randOffset = Random.Range(0,99999);

        offset = startPoint;

        // Double check radius is odd
        if (loadRadius.x % 2 == 0) loadRadius.x++;
        if (loadRadius.y % 2 == 0) loadRadius.y++;

        StartCoroutine(WaitALil());
    }

    IEnumerator WaitALil() {
        for (int i = 0; i < loadRadius.x; i++)
        {
            for (int j = 0; j < loadRadius.y; j++)
            {
                yield return new WaitForSeconds(0.2f);
                offset = startPoint + new Vector2(regionSize.x*(i-(loadRadius.x-1)/2), regionSize.z*(j-(loadRadius.y-1)/2));
                GeneratePoints(offset.x, offset.y);
            }
        }
    }

    public (Vector3[], Quaternion[]) GeneratePoints(float offsetX, float offsetZ) {
        // Generate uniform random points
		points = PoissonDiscSampling.GeneratePoints(radius, new Vector2(regionSize.x, regionSize.z), new Vector2(offsetX,offsetZ),3);

        maskedPoints = new List<Vector2>();
        meshPoints = new List<Vector3>();
        meshPoints = new List<Vector3>();
        List<Quaternion> meshOrientations = new List<Quaternion>();

        //Mask Points According to noise
        foreach (Vector2 point in points) {
            if (WhiteNoiseMask(point.x, point.y)){
                maskedPoints.Add(point);
            }
        }

        // Raycast to find point on mesh geometry
        RaycastHit hit;
        foreach (Vector2 point in maskedPoints) {

            Vector3 pPos = new Vector3(point.x -regionSize.x/2f, maxTerrainHeight, point.y);
            if (Physics.Raycast(pPos, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
                // check if too steep here
                if (hit.transform.CompareTag(ignoreTag)) {
                    Debug.Log("hitwater");
                    continue;
                }
                if (Vector3.Dot(hit.normal, Vector3.up) <= maxSteepness) continue;

                meshPoints.Add(hit.point);
                meshOrientations.Add(Quaternion.Euler(0,(point.x + point.y)*5236 % 360,0));

                // objPool.SpawnObject(hit.point, 
                //                     Quaternion.Euler(0,(point.x + point.y)*5236 % 360,0), 
                //                     (int)(Mathf.Abs(((point.x + point.y)*5236) % objArray.Length)));
            }
        }

        return (meshPoints.ToArray(), meshOrientations.ToArray());
    }


	void OnDrawGizmos() {
        Gizmos.color = Color.white;
		Gizmos.DrawWireCube(new Vector3(0 + offset.x,maxTerrainHeight,regionSize.z/2 + offset.x),regionSize);
		if (meshPoints != null) {
            Gizmos.color = Color.blue;
			foreach (Vector2 point in maskedPoints) {
				Gizmos.DrawSphere(new Vector3(point.x -regionSize.x/2, maxTerrainHeight, point.y), displayRadius);
			}
            Gizmos.color = Color.red;
            foreach (Vector3 point in meshPoints) {
				Gizmos.DrawSphere(point, displayRadius);
            }
		}
	}


    bool WhiteNoiseMask(float x, float y){
        float p = Mathf.PerlinNoise(x*noiseMaskScale +randOffset,y*noiseMaskScale +randOffset);
        return (p >= noiseThreshold);
    }
}
