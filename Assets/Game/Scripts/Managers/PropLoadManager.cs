using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LoadGridManager))]
public class PropLoadManager : MonoBehaviour
{
    [SerializeField] private Transform playerPos;
    [SerializeField] private Vector3 prevPlayerPos;
    [SerializeField] private float playerViewAngle = 60f;
    [SerializeField] private float playerLoadRadius = 5f;
    public Mesh[] objArray;
    public Material[] matArray;

    private LoadGridManager loadGridManager;


    void Start()
    {
        loadGridManager = GetComponent<LoadGridManager>();
        prevPlayerPos = playerPos.position;

        StartCoroutine(GenerateGrid());
    }

    IEnumerator GenerateGrid() {
        yield return new WaitForSeconds(1f);
        loadGridManager.GenerateGrid(new Vector2(playerPos.position.x, playerPos.position.z), playerLoadRadius);
    }

    void Update()
    {
        // Check if player has moved (should check x and y specifically not both picking the smallest diff)
        if (Vector3.Distance(prevPlayerPos, playerPos.position) > Mathf.Min(loadGridManager.chunkSize.x/2f, loadGridManager.chunkSize.y/2f))
        {
            // Generate grid of chunks around player
            loadGridManager.GenerateGrid(new Vector2(playerPos.position.x, playerPos.position.z), playerLoadRadius);
            prevPlayerPos = playerPos.position;
        }

        foreach (KeyValuePair<Vector2, LoadGridManager.Chunk> chunk in loadGridManager.chunks)
        {
            // Draw all points within the player's view angle
            DrawPointsInViewAngle(playerPos.position, playerPos.forward, chunk.Value.transformMatrices, chunk.Value.objType);
        }
    }


    // Draw all points within the player's view angle
    private void DrawPointsInViewAngle(Vector3 playerPos, Vector3 playerViewDirection, Matrix4x4[] points, int[] objType)
    {
        List<Matrix4x4>[] pointsInView = new List<Matrix4x4>[objArray.Length];
        for (int i = 0; i < objArray.Length; i++) {
            pointsInView[i] = new List<Matrix4x4>();
        }

        // Frustum Culling
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        for (int i = 0; i < points.Length; i++){
            if (GeometryUtility.TestPlanesAABB(cameraPlanes, new Bounds(points[i].GetPosition(), Vector3.one))) {
                // Draw point
                Debug.DrawLine(playerPos, points[i].GetPosition(), Color.red);

                // GPU instance meshes in view angle
                pointsInView[objType[i]].Add(points[i]);
            }
        }

        // Loop over possible mesh-mat pairs and draw them
        for (int i = 0; i < objArray.Length; i++){
            Graphics.DrawMeshInstanced(objArray[i], 0, matArray[i], pointsInView[i].ToArray());
        }
        
    }

}