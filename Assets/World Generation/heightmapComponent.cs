using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class heightmapComponent : MonoBehaviour
{

    [SerializeField] private int m_textureWidth = 1024;
    [SerializeField] private int m_textureHeight = 1024;

    [SerializeField] private float m_textureScale = 2f;
    [SerializeField] private Vector2 m_textureOffset = Vector2.zero;

    public Texture2D m_noiseTexture;


    private void Start() {
        // Set random offset
        m_textureOffset = GenerateRandomVector2();
        // generate the texture
        m_noiseTexture = GenerateTerrainTexture();

        // Just for testing/visualization
        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.mainTexture = m_noiseTexture;
    }


    Vector2 GenerateRandomVector2() {
        return new Vector2(Random.Range(0,999999), Random.Range(0,999999));
    }

    // Generate visualization of terrain function
    Texture2D GenerateTerrainTexture() {
        Texture2D texture = new Texture2D(m_textureWidth, m_textureHeight);

        // call terrain function for each pixel in texture
        for (int x = 0; x < m_textureWidth; x++)
        {
            for (int y = 0; y < m_textureHeight; y++)
            {
                // Sample terrain at point to generate texture

                float terrainVal = TerrainAtPosition(x,y,1).x;

                // For visualization set colour to be height value
                Color color = new(terrainVal, terrainVal, terrainVal);
                texture.SetPixel(x,y,color);
            }
        }

        // Update texture with changes
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Magnitude of a line segment
    /// </summary>
    /// <param name="p"></param> line starting point
    /// <param name="a"></param> point to be measured
    /// <param name="b"></param> line ending point
    /// <returns></returns>
    private float SDFSegment(Vector2 p, Vector2 a, Vector2 b )
    {
        Vector3 pa = p-a, ba = b-a;
        float h = Mathf.Clamp01(Vector2.Dot(pa,ba)/Vector2.Dot(ba,ba));
        return ( pa - ba*h ).magnitude;
    }


    /// <summary>
    /// Get terrain height and normal at given point according to Noise Function
    /// </summary>
    /// <param name="x"></param> x coordinate
    /// <param name="y"></param> y coordinate
    /// <returns> Return height value (float) and normal vector (Vector3) </returns>
    public Vector4 TerrainAtPosition(int x, int y, float height) {
        // As perlin repeats on whole numbers convert to decimal
        float xCrd = (float)x / m_textureWidth * m_textureScale;
        float yCrd = (float)y / m_textureWidth * m_textureScale;
        
        // Calculate color based on perlin noise function (NEEDS TO BE NORMALIZED!!!)
        Vector3 sample = FBMErosion(new Vector2(xCrd,yCrd), 15);

        // Normal Calculation using derivatives (! Currently Completely wrong !)
        Vector3 xTangent = new(1, sample.z* height*sample.x, 0);
        Vector3 yTangent = new(0, sample.y* height*sample.x, 1);
        Vector3 pointNormal = Vector3.Normalize(Vector3.Cross(xTangent, yTangent));

        float sinSDF = CanyonCarve(new Vector2(x, y), 20f, 10f, 1f, m_textureWidth/2, 100f, 10f, 0f);
        sinSDF += CanyonCarve(new Vector2(x, y), 20f, 10f, 0.5f, m_textureWidth/2, 120f, 8f, 30f);


        sample.x -= sinSDF;

        return new Vector4(sample.x, pointNormal.x, pointNormal.y, pointNormal.z);
    }

    private float CanyonCarve(Vector2 point, float canyonWidth, float canyonBaseWidth, float canyonDepth, float axisOffset, float period, float amplitude, float periodOffset) {
        
        // Clamp canyons base width
        canyonBaseWidth = Mathf.Min(canyonWidth, canyonBaseWidth);

        // Calc x-axis distance from sine wave (not currently sdf)
        float mod = Mathf.Sin(point.y*2*Mathf.PI/period + periodOffset)*amplitude;
        float sinSDF = Mathf.Abs(point.x -axisOffset - mod);

        sinSDF = (Mathf.Pow(Mathf.Min(canyonWidth, Mathf.Max(sinSDF -canyonBaseWidth, 0f) ), 2) / Mathf.Pow(canyonWidth, 2));
        sinSDF = 1 - sinSDF;

        return sinSDF * canyonDepth;
    }


    float Sigmoid_SoftMin( float a, float b, float k )
    {
        k *= Mathf.Log(2f);
        float x = b-a;
        return a + x/(1f-Mathf.Pow(2f, x/k));
    }

    // cubic
    float SoftMin( float a, float b, float k )
    {
        k *= 6.0f;
        float h = Mathf.Max( k-Mathf.Abs(a-b), 0.0f)/k;
        return Mathf.Min(a,b) - h*h*h*k*(1.0f/6.0f);
    }
    
    Vector2 m1 = new Vector2(0.8f,-0.6f);  
    Vector2 m2 = new Vector2(0.6f,0.8f);  

    // FBM with erosion
    Vector3 FBMErosion(Vector2 p , int octaves)
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
    Vector3 Noised( Vector2 p )
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
    Vector2 Hash( Vector2 x ) {
        Vector2 k = new( 0.3183099f, 0.3678794f);
        x = x*k + new Vector2(k.y, k.x); // note y,x instead of x,y
        Vector2 l = 16.0f * Fract( x.x*x.y*(x.x+x.y)) * k;
        return 2.0f * new Vector2(Fract( l.x), Fract( l.y)) - Vector2.one;
    }

    // Get fractional component of float p
    private float Fract(float p) {
        return p - Mathf.Floor(p);
    }

}