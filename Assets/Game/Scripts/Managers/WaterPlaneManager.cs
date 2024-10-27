using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class WaterPlaneManager : MonoBehaviour
{
    [Tooltip("Prefab of the water plane")]
    [SerializeField] private GameObject waterPlanePrefab;
    [Tooltip("How many planes wide the grid is")]
    [SerializeField] private int gridWidth = 10;
    [Tooltip("How many planes long the grid is")]
    [SerializeField] private int gridHeight = 100;
    [Tooltip("Size of each water plane")]
    [SerializeField] private float planeSize = 100f;
    [Tooltip("Reference to the player's transform")]
    [SerializeField] private Transform playerTransform;

    private GameObject[,] waterPlanes;
    private float lastShiftZ;

    void Start()
    {
        DeleteGrid();
        InitialiseGrid();
        lastShiftZ = playerTransform.position.z;
    }

    void OnEnable()
    {
        DeleteGrid();
        InitialiseGrid();
        lastShiftZ = playerTransform.position.z;
    }

    void OnDisable()
    {
        DeleteGrid();
    }

    void OnDestroy()
    {
        DeleteGrid();
    }

    void Update()
    {
        if (playerTransform)
        {
            float playerZ = playerTransform.position.z;

            if (playerZ - lastShiftZ >= planeSize)
            {
                ShiftGridForward();
                lastShiftZ = playerZ;
            }
            else if (lastShiftZ - playerZ >= planeSize)
            {
                ShiftGridBackward();
                lastShiftZ = playerZ;
            }
        }
    }

    public void InitialiseGrid()
    {
        if (waterPlanes == null || waterPlanes.Length == 0)
        {
            if (waterPlanePrefab == null) return;

            float offsetX = (gridWidth * planeSize) / 2f - planeSize / 2;

            waterPlanes = new GameObject[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Vector3 position = new Vector3(x * planeSize - offsetX, transform.position.y, z * planeSize);
                    waterPlanes[x, z] = Instantiate(waterPlanePrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    private void ShiftGridForward()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            GameObject lastPlane = waterPlanes[x, 0];

            float newZ = waterPlanes[x, gridHeight - 1].transform.position.z + planeSize;
            lastPlane.transform.position = new Vector3(lastPlane.transform.position.x, lastPlane.transform.position.y, newZ);

            for (int z = 0; z < gridHeight - 1; z++)
            {
                waterPlanes[x, z] = waterPlanes[x, z + 1];
            }

            waterPlanes[x, gridHeight - 1] = lastPlane;
        }
    }

    private void ShiftGridBackward()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            GameObject firstPlane = waterPlanes[x, gridHeight - 1];

            float newZ = waterPlanes[x, 0].transform.position.z - planeSize;
            firstPlane.transform.position = new Vector3(firstPlane.transform.position.x, firstPlane.transform.position.y, newZ);

            for (int z = gridHeight - 1; z > 0; z--)
            {
                waterPlanes[x, z] = waterPlanes[x, z - 1];
            }

            waterPlanes[x, 0] = firstPlane;
        }
    }

    public void DeleteGrid()
    {
        if (waterPlanes != null) waterPlanes = null;

        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

[CustomEditor(typeof(WaterPlaneManager))]
public class WaterPlaneManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaterPlaneManager waterManager = (WaterPlaneManager)target;

        if (GUILayout.Button("Reinitialise Grid"))
        {
            waterManager.DeleteGrid();
            waterManager.InitialiseGrid();
        }
    }
}
