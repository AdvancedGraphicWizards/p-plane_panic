
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
    public float randOffset = 0f;

    // Generate points in a chunk according to poisson disc sampling and white noise mask
    // Rotation are assigned at random
    public Matrix4x4[] GeneratePoints(float offsetX, float offsetZ, float chunkSizeX, float chunkSizeZ) {
        // Generate uniform random points
		List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, new Vector2(chunkSizeX, chunkSizeZ), new Vector2(offsetX - chunkSizeX/2f,offsetZ - chunkSizeZ/2f),3);

        List<Vector2> maskedPoints = new List<Vector2>();
        List<Vector3> meshPoints = new List<Vector3>();
        List<Vector3> meshOrientations = new List<Vector3>();

        //Mask Points According to noise
        foreach (Vector2 point in points) {
            if (WhiteNoiseMask(point.x, point.y)){
                maskedPoints.Add(point);
            }
        }

        // Raycast to find point on mesh geometry
        //RaycastHit hit;
        foreach (Vector2 point in maskedPoints) {

            meshPoints.Add(new Vector3(point.x, 0, point.y));
            meshOrientations.Add(new Vector3(0,(point.x + point.y)*5236 % 360,0));

            // Vector3 pPos = new Vector3(point.x -regionSize.x/2f, maxTerrainHeight, point.y);
            // if (Physics.Raycast(pPos, Vector3.down, out hit, Mathf.Infinity, layerMask)) {
            //     // check if too steep here
            //     if (hit.transform.CompareTag(ignoreTag)) {
            //         Debug.Log("hitwater");
            //         continue;
            //     }
            //     if (Vector3.Dot(hit.normal, Vector3.up) <= maxSteepness) continue;

            //     meshPoints.Add(hit.point);
            //     meshOrientations.Add(Quaternion.Euler(0,(point.x + point.y)*5236 % 360,0));
            // }
        }
        Matrix4x4[] propMatrices = new Matrix4x4[meshPoints.Count];
        for (int i = 0; i < meshPoints.Count; i++) {
            propMatrices[i] = Matrix4x4.TRS(meshPoints[i], Quaternion.Euler(meshOrientations[i]), Vector3.one);
        }

        return propMatrices;
    }


    // Mask points based on white noise
    bool WhiteNoiseMask(float x, float y){
        float p = Mathf.PerlinNoise(x*noiseMaskScale +randOffset,y*noiseMaskScale +randOffset);
        return (p >= noiseThreshold);
    }


}
