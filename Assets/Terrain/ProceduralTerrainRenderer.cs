using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralTerrainRenderer : MonoBehaviour {

    // References
    [Tooltip("The compute shader to use to modify the mesh.")]
    [SerializeField] private ComputeShader terrainComputeShader;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Last updated position.
    private Vector3 lastPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

    // Instantiated compute shader to modify mesh.
    private ComputeShader instantiatedTerrainComputeShader;
    private Mesh modifiedMesh;

    // Buffers that store the mesh data.
    public GraphicsBuffer indexBuffer;
    public GraphicsBuffer vertexBuffer;

    // Local instance compute shader variables.
    private bool _isInitialized = false;
    private int idVertexDisplacementKernel = 0;         // The kernel to use to displace the vertices.
    private int idNormalsKernel = 0;                    // The kernel to use to calculate the normals.
    private int idNormalizeNormalsKernel = 0;           // The kernel to use to normalize the normals.
    private Vector3Int vertexDisplacementDispatchSize = new Vector3Int(64, 1, 1);
    private Vector3Int calculateNormalsDispatchSize = new Vector3Int(64, 1, 1);
    private Vector3Int normalizeNormalsDispatchSize = new Vector3Int(64, 1, 1);

    void OnEnable() {
        Debug.Assert(terrainComputeShader != null, "Compute shader is not set.", this);

        // Get references.
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Deep copy the mesh from the mesh filter if it hasn't already been
        // Then upload mesh data to the GPU, and set the additional vertex streams.
        modifiedMesh = meshFilter.sharedMesh.name.Contains("Instance") ? meshFilter.sharedMesh : meshFilter.mesh;
        modifiedMesh.UploadMeshData(false);
        meshRenderer.additionalVertexStreams = modifiedMesh;
        meshFilter.mesh = modifiedMesh;
        
        // If initialized, call OnDisable to clean up.
        if (_isInitialized) { OnDisable(); }

        // Instantiate the compute shader and cache the kernel.
        instantiatedTerrainComputeShader = Instantiate(terrainComputeShader);
        idVertexDisplacementKernel = instantiatedTerrainComputeShader.FindKernel("DisplaceVertices");
        idNormalsKernel = instantiatedTerrainComputeShader.FindKernel("CalculateNormals");
        idNormalizeNormalsKernel = instantiatedTerrainComputeShader.FindKernel("NormalizeNormals");

        // Get the vertex and index buffers from the mesh.
        modifiedMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        modifiedMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        indexBuffer = modifiedMesh.GetIndexBuffer();
        vertexBuffer = modifiedMesh.GetVertexBuffer(0);
        
        // Set vertex buffer and other data for the compute shader.
        instantiatedTerrainComputeShader.SetBuffer(idVertexDisplacementKernel, "_VertexBuffer", vertexBuffer);
        instantiatedTerrainComputeShader.SetBuffer(idNormalsKernel, "_IndexBuffer", indexBuffer);
        instantiatedTerrainComputeShader.SetBuffer(idNormalsKernel, "_VertexBuffer", vertexBuffer);
        instantiatedTerrainComputeShader.SetBuffer(idNormalizeNormalsKernel, "_VertexBuffer", vertexBuffer);

        instantiatedTerrainComputeShader.SetInt("_IndexCount", (int)modifiedMesh.GetIndexCount(0));
        instantiatedTerrainComputeShader.SetInt("_VertexCount", modifiedMesh.vertexCount);
        instantiatedTerrainComputeShader.SetInt("_IndexStride", modifiedMesh.indexFormat == IndexFormat.UInt32 ? 4 : 2);
        instantiatedTerrainComputeShader.SetInt("_VertexStride", modifiedMesh.GetVertexBufferStride(0));

        instantiatedTerrainComputeShader.SetInt("_PositionOffset", modifiedMesh.GetVertexAttributeOffset(VertexAttribute.Position));
        instantiatedTerrainComputeShader.SetInt("_NormalOffset", modifiedMesh.GetVertexAttributeOffset(VertexAttribute.Normal));

        // Calculate the dispatch size.
        instantiatedTerrainComputeShader.GetKernelThreadGroupSizes(idVertexDisplacementKernel, out uint threadGroupSizeVertexDisplacement, out _, out _);
        instantiatedTerrainComputeShader.GetKernelThreadGroupSizes(idNormalsKernel, out uint threadGroupSizeNormals, out _, out _);
        instantiatedTerrainComputeShader.GetKernelThreadGroupSizes(idNormalizeNormalsKernel, out uint threadGroupSizeNormalizeNormals, out _, out _);
        vertexDisplacementDispatchSize = new Vector3Int(Mathf.CeilToInt((float)modifiedMesh.vertices.Length / threadGroupSizeVertexDisplacement), 1, 1);
        calculateNormalsDispatchSize = new Vector3Int(Mathf.CeilToInt((float)modifiedMesh.GetIndexCount(0) / threadGroupSizeNormals), 1, 1);
        normalizeNormalsDispatchSize = new Vector3Int(Mathf.CeilToInt((float)modifiedMesh.vertices.Length / threadGroupSizeNormalizeNormals), 1, 1);

        // Update bounds to avoid culling on the mesh.
        meshFilter.sharedMesh.bounds.Expand(new Vector3(0, 1000, 0));

        // Set the initialized flag.
        _isInitialized = true;
    }

    void OnDisable() {

        // Dispose of the vertex and index buffers.
        indexBuffer?.Release();
        vertexBuffer?.Release();

        // Destroy the instantiated compute shader and material.
        if (Application.isPlaying) {
            Destroy(instantiatedTerrainComputeShader);
        } else {
            DestroyImmediate(instantiatedTerrainComputeShader);
        }

        // Set the initialized flag.
        _isInitialized = false;
    }

    // LateUpdate is called after all Update functions have been called.
    void LateUpdate() {

        // If in editor mode, reinitialize the renderer to make sure changes are applied.
        if (!Application.isPlaying) {
            OnDisable();
            OnEnable();
        }

        // If not initialized, try to initialize the renderer.
        if (!_isInitialized) { OnEnable(); }
        if (!_isInitialized) { return; }

        // If the position has changed, update the compute shader.
        if (lastPosition != transform.position) {
            lastPosition = transform.position;

            // Update compute shader with frame specific data.
            instantiatedTerrainComputeShader.SetVector("_Time", new Vector4(0, Time.timeSinceLevelLoad, 0, 0));
            instantiatedTerrainComputeShader.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
            instantiatedTerrainComputeShader.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

            // Dispatch the compute shader.
            instantiatedTerrainComputeShader.Dispatch(idVertexDisplacementKernel, vertexDisplacementDispatchSize.x, vertexDisplacementDispatchSize.y, vertexDisplacementDispatchSize.z);
            //instantiatedTerrainComputeShader.Dispatch(idNormalsKernel, calculateNormalsDispatchSize.x, calculateNormalsDispatchSize.y, calculateNormalsDispatchSize.z);
            //instantiatedTerrainComputeShader.Dispatch(idNormalizeNormalsKernel, normalizeNormalsDispatchSize.x, normalizeNormalsDispatchSize.y, normalizeNormalsDispatchSize.z);
        }
    }
}
