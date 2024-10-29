using System.Collections.Generic;
using UnityEngine;

public class PoissonPlacement : MonoBehaviour
{
	public float radius = 1;
	public int rejectionSamples = 30;
	public float displayRadius =1;
	public float noiseMaskScale =1f;
	public float noiseThreshold =0.5f;
	public float waterLevel = 0f;
	public float maxSteepness = 0.5f; // 0 is default
    public float randOffset = 0f;

    // Generate points in a chunk according to poisson disc sampling and white noise mask
    // Rotation are assigned at random
    public Matrix4x4[] GeneratePoints(float offsetX, float offsetZ, float chunkSizeX, float chunkSizeZ) {
        // Generate uniform random points
		//List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, new Vector2(chunkSizeX, chunkSizeZ), new Vector2(offsetX - chunkSizeX/2f,offsetZ - chunkSizeZ/2f),3);

        List<Vector2> points = GridPoints.GetPointGrid(offsetX, offsetZ, chunkSizeX, chunkSizeZ, radius);

        List<Vector2> maskedPoints = new List<Vector2>();
        List<Vector3> meshPoints = new List<Vector3>();
        List<Vector3> meshOrientations = new List<Vector3>();

        //Mask Points According to noise
        foreach (Vector2 point in points) {
            if (WhiteNoiseMask(point.x, point.y)){
                maskedPoints.Add(point);
            }
        }

        // Find valid point on mesh geometry
        foreach (Vector2 point in maskedPoints) {

            Vector3 pointOnTerrain = new Vector3(point.x, WorldGeneration.SampleHeight(point.x, point.y), point.y);
            
            // check if point is above water
            if (pointOnTerrain.y < waterLevel) continue;

            // Get normal to Verify steepness
            //Vector3 normalAtPoint = WorldGeneration.SampleHeight(pointOnTerrain.x, pointOnTerrain.y, pointOnTerrain.z, 1f);

            //if (Vector3.Dot(normalAtPoint, Vector3.up) <= maxSteepness) continue;


            meshPoints.Add(new Vector3(point.x, WorldGeneration.SampleHeight(point.x, point.y), point.y));
            meshOrientations.Add(new Vector3(0,(point.x + point.y)*5236 % 360,0));
        
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
