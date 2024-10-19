using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour {

    // World settings
    [SerializeField] private WorldSettingsSO m_settings;
    public static WorldSettingsSO settings;

    // Generates LOD based on distance from main camera
    private Camera mainCamera;

    // QuadTree root nodes
    private BinaryTreeNode[] roots = new BinaryTreeNode[4];

    // Start is called before the first frame update
    void Start() {

        // Assign callback for settings change
        m_settings.OnSettingsChange += ReloadWorld;
    }

    // Update is called once per frame
    void Update() {
        mainCamera = Camera.main;

        // Update LOD based on distance from main camera
        Queue<BinaryTreeNode> queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            float distance = Vector3.Distance(mainCamera.transform.position, node.position);

            // Split 
            if (node.children == null && distance < settings.vertexDistance * settings.vertexCount * Mathf.Pow(Mathf.Sqrt(2), node.LODExp)) {//Mathf.Pow(2, node.LODExp)) {
                node.Split();

            // Merge
            } else if (node.children != null && distance > settings.vertexDistance * settings.vertexCount * Mathf.Pow(Mathf.Sqrt(2), node.LODExp + 1)) {// * Mathf.Pow(2, node.LODExp + 1)) {
                node.MarkMerge();
            }

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }

        // Merge marked nodes
        queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            if (node.LODExp != settings.levelsOfDetail) {
                node.Merge();
            }

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }

        // Update chunks
        queue = new Queue<BinaryTreeNode>( roots );
        while (queue.Count > 0) {
            BinaryTreeNode node = queue.Dequeue();
            if (node == null) continue;
            node.GenerateChunk();

            // Add children to queue
            if (node.children != null) {
                foreach (BinaryTreeNode child in node.children) {
                    queue.Enqueue(child);
                }
            }
        }
    }
    
    // Returns height for position x, z based on array of octaves and perlin noise
    public static float GetHeight(float x, float z) {
        float height = 0;
        // foreach (Octave octave in settings.octaves) {
        //     if (octave.enabled) {
        //         height += (Mathf.PerlinNoise(x * octave.scale + settings.seedX, z * octave.scale + settings.seedZ) - 0.5f) * 2f * octave.amplitude;
        //     }
        // }
        height = TerrainAtPosition(x, z).x;

        return height;
    }
    /// !!! ----------------------------------- !!!
    // MESSY TERRAIN TESTING
    /// !!! ----------------------------------- !!!
    /// 

    private static Vector4 TerrainAtPosition(float x, float y) {
        
        Vector3 sample = FBMErosion(new Vector2(x/300f,y/300f), settings.octaves.Length) *150f;

        //float spice = Noised(new Vector2(1,y)).x*50;

        float sinSDF = CanyonCarve(new Vector2(x, y), 40f, 50f, 40f, 0f, 2500f, 80f, 0f);
        sinSDF += CanyonCarve(new Vector2(x, y), 20f, 120f, 40f, 0f, 2500f, 80f, 0f);
        sinSDF += CanyonCarve(new Vector2(x, y), 10f, 160f, 40f, 0f, 2500f, 80f, 0f);

        sample.x *= Mathf.Min(1f, 60f/sinSDF);

        sample.x -= sinSDF;
        float baseOffset = 70f;

        return new Vector4(sample.x + baseOffset, 0,0,0) ;
    }

    // Carve canyon shape
    private static float CanyonCarve(Vector2 point, float canyonWidth, float canyonBaseWidth, float canyonDepth, float axisOffset, float period, float amplitude, float periodOffset) {
        
        float k = 2f;

        // Calc x-axis distance from sine wave (not currently sdf)
        float mod = Mathf.Sin(point.y*2*Mathf.PI/period + periodOffset)*amplitude;
        float sinSDF = Mathf.Abs(point.x -axisOffset - mod);

        sinSDF = Mathf.Pow(Smin(canyonWidth, Mathf.Max(sinSDF -canyonBaseWidth, 0f), k), 2) / Mathf.Pow(canyonWidth, 2);
        sinSDF = 1 - sinSDF;

        return sinSDF * canyonDepth;
    }

    static private float Smin( float a, float b, float k )
    {
        k *= 1.0f; // why
        float r = Mathf.Pow(2,-a/k) + Mathf.Pow(2,-b/k);
        return -k*Mathf.Log(r,2);
    }

    // Arbitrary rotation matrices
    static Vector2 m1 = new Vector2(0.8f,-0.6f);  
    static Vector2 m2 = new Vector2(0.6f,0.8f); 

    /// <summary>
    /// 2D brownian motion noise sampling
    /// Adapted from Blog post by Inigo Quilez: https://iquilezles.org/articles/morenoise/
    /// </summary>
    /// <param name="p"> x,y coordinates to sample the noise</param>
    /// <returns>Vector3 consisting of a value and x,y gradients</returns>
    private static Vector3 FBMErosion(Vector2 p , int octaves)
    {
        float a = 0.0f;
        float b = 1.0f;
        Vector2  d = Vector2.zero;
        for( int i=0; i< octaves; i++ )
        {
            Vector3 n = Noised(p);

            // Separate noise 2D-gradients
            d += new Vector2(n.y, n.z);

            // Accumulate noise values, 
            // dampens the contribution of noise based on the accumulated gradients (magnitude)^2
            a +=b*n.x/(1.0f+Vector2.Dot(d,d));

            // Half the amplitude of noise each octave
            b *=0.5f;

            // Apply magic vectors that rotate and scale the noise each octave to add variation each octave
            p.x=(m1.x*p.x + m1.y*p.y)*2.0f;
            p.y=(m2.x*p.x + m2.y*p.y)*2.0f;
        }

        // Normalize the noise
        //a /= (1-Mathf.Pow(0.5f,octaves)) * 2f;

        return new Vector3(a,d.x,d.y);
    }

    /// <summary>
    /// 2D brownian motion noise sampling
    /// Adapted from Blog post by Inigo Quilez: https://iquilezles.org/articles/morenoise/
    /// </summary>
    /// <param name="p"> x,y coordinates to sample the noise</param>
    /// <returns>Vector3 consisting of a value and x,y gradients</returns>
    private static Vector3 Noised( Vector2 p )
    {
        Vector2 i = new(Mathf.Floor(p.x), Mathf.Floor(p.y));
        Vector2 f = new(p.x - i.x, p.y - i.y);               // get fractional part of x

        // Calculate quintic interpolation and derivatives
        static float QuinticInterpolation(float x) => x*x*x*(x*(x*6.0f - 15.0f)+10.0f);
        static float QuinticInterpolationDerivative(float x) => 30.0f*x*x*(x*(x-2.0f)+1.0f);

        Vector2 u = new(QuinticInterpolation(f.x), QuinticInterpolation(f.y));  
        Vector2 du = new(QuinticInterpolationDerivative(f.x), QuinticInterpolationDerivative(f.y));  

        Vector2 ga = Hash( i + new Vector2(0.0f,0.0f));
        Vector2 gb = Hash( i + new Vector2(1.0f,0.0f) );
        Vector2 gc = Hash( i + new Vector2(0.0f,1.0f) );
        Vector2 gd = Hash( i + new Vector2(1.0f,1.0f) );

        float va = Vector2.Dot( ga, f - new Vector2(0.0f,0.0f) );
        float vb = Vector2.Dot( gb, f - new Vector2(1.0f,0.0f) );
        float vc = Vector2.Dot( gc, f - new Vector2(0.0f,1.0f) );
        float vd = Vector2.Dot( gd, f - new Vector2(1.0f,1.0f) );

        float c = (va-vb-vc+vd);
        Vector2 FBMDerivative = ga + u.x*(gb-ga) + u.y*(gc-ga) + u.x*u.y*(ga-gb-gc+gd) + du 
                                * (new Vector2(u.y,u.x) * new Vector2(c,c) + new Vector2(vb,vc) - new Vector2(va,va)); // note y,x instead of x,y on "u"
                        
        return new Vector3( va + u.x*(vb-va) + u.y*(vc-va) + u.x*u.y*(va-vb-vc+vd),   // value
                        FBMDerivative.x, FBMDerivative.y); // derivatives
    }

    //  Hash function Vector2 => Vector2
    //  (Potentially Replace with a better hash)
    private static Vector2 Hash( Vector2 x ) {
        Vector2 k = new( 0.3183099f, 0.3678794f);
        x = x*k + new Vector2(k.y, k.x); // note y,x instead of x,y
        Vector2 l = 16.0f * Fract( x.x*x.y*(x.x+x.y)) * k;
        return 2.0f * new Vector2(Fract( l.x), Fract( l.y)) - Vector2.one;
    }

    // Get fractional component of float p
    private static float Fract(float p) {
        return p - Mathf.Floor(p);
    }

    /// !!! ----------------------------------- !!!
    /// 
    /// !!! ----------------------------------- !!!

    
    // Reloads the world with new settings on settings change
    public void ReloadWorld() {

        // Remove old world
        foreach (BinaryTreeNode root in roots) {
            if (root != null) {
                root.Merge();
                ChunkManager.RemoveChunk(root);
                BinaryTreeNode.nodes.Remove(root.nodeID.node);
            }
        }

        // Assign new settings and create new world
        settings = m_settings;
        roots[0] = new BinaryTreeNode(
            new NeighborIDs { left = 0, right = 0, edge = 6, node = 4 }, 
            transform.position, 
            0, 
            settings.levelsOfDetail
        );
        // roots[1] = new BinaryTreeNode(
        //     new NeighborIDs { left = 0, right = 0, edge = 0, node = 5 }, 
        //     transform.position, 
        //     90, 
        //     settings.levelsOfDetail
        // );
        roots[2] = new BinaryTreeNode(
            new NeighborIDs { left = 0, right = 0, edge = 4, node = 6 }, 
            transform.position, 
            180, 
            settings.levelsOfDetail
        );
        // roots[3] = new BinaryTreeNode(
        //     new NeighborIDs { left = 0, right = 0, edge = 0, node = 7 }, 
        //     transform.position, 
        //     270, 
        //     settings.levelsOfDetail
        // );
    }

    // Validate settings on change
    private void OnValidate() {
        if (m_settings == null) {
            Debug.LogError("WorldSettingsSO not assigned in World");
            return;
        }

        ReloadWorld();
    }

    // Unsubscribe from event on destroy
    private void OnDestroy() {
        m_settings.OnSettingsChange -= ReloadWorld;
    }
}