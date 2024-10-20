using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
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
	public float maxTerrainDepth = 1000f;
	public int layerMask = ~(1 << 8);

	private float randOffset =0f;

	List<Vector2> points;
	List<Vector2> maskedPoints;
	List<Vector3> meshPoints;

    void Start()
    {
        randOffset = Random.Range(0,99999);

        StartCoroutine(WaitALil());
    }
    IEnumerator WaitALil() {
        yield return new WaitForSeconds(1f);
        GeneratePoints();
    }


    void GeneratePoints() {
        // Generate uniform random points
		points = PoissonDiscSampling.GeneratePoints(radius, new Vector2(regionSize.x, regionSize.z));

        maskedPoints = new List<Vector2>();
        meshPoints = new List<Vector3>();

        //Mask Points According to noise
        foreach (Vector2 point in points) {
            if (WhiteNoiseMask(point.x, point.y)){
                maskedPoints.Add(point);
            }
        }

        // Raycast to find point on mesh geometry
        RaycastHit hit;
        foreach (Vector2 point in maskedPoints) {
            Vector3 pPos = new Vector3(point.x, maxTerrainHeight, point.y);
            if (Physics.Raycast(pPos, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
                // check if too steep here

                meshPoints.Add(hit.point);
            }
        }
        // instead of raycasting could use terrain at point...
    }


	void OnDrawGizmos() {
        Gizmos.color = Color.white;
		Gizmos.DrawWireCube(new Vector3(regionSize.z/2,maxTerrainHeight,regionSize.z/2),regionSize);
		if (meshPoints != null) {
            Gizmos.color = Color.blue;
			foreach (Vector2 point in maskedPoints) {
				Gizmos.DrawSphere(new Vector3(point.x, maxTerrainHeight, point.y), displayRadius);
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
