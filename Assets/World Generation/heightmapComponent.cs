using UnityEngine;

public static class HeightmapComponent
{
    /// <summary>
    /// Get Terrain function Height at given (x,y) position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns> terrain height</returns>
    public static float HeightAtPosition(float x, float y) {
        Vector2 p = new Vector2(x, y);

        // Get base Terrain, FBM with erosion
        Vector3 baseTerrain = FBMErosion(p/300f, 10) *150f;
        float baseTerrainHeight = baseTerrain.x;

        // Get canyon depth
        float sinSDF = CanyonCarve(p, 40f, 10f, 40f, 0f, 2500f, 100f, 0f);
        sinSDF += CanyonCarve(p, 20f, 80f, 40f, 0f, 2500f, 100f, 0f);
        sinSDF += CanyonCarve(p, 10f, 130f, 40f, 0f, 2500f, 100f, 0f);

        // Dimminish baseTerrain Impact within canyon
        baseTerrainHeight *= Mathf.Min(1f, 60f/sinSDF);

        // Subtract Canyon from terrain height
        baseTerrainHeight -= sinSDF;

        // Y-offset
        float baseOffset = 70f;

        return baseTerrainHeight + baseOffset;
    }



    /// Get normal at given x,y,z position with a certain consistent vertex distance
    public static Vector3 GetNormalAtPosition(float x, float y, float z, float vertexDistance) {
        Vector3 v1 = new Vector3(x,y,z);
        Vector3 v2 = new Vector3(x,HeightAtPosition(x , z + vertexDistance),z + vertexDistance);
        Vector3 v3 = new Vector3(x + vertexDistance,HeightAtPosition(x + vertexDistance , z),z);
        v2 -= v1;
        v3 -= v1;
        return Vector3.Normalize(Vector3.Cross(v2, v3));
    }



    // Carve canyon shape, stepped method
    private static float CanyonCarve(Vector2 point, float canyonWidth, float canyonBaseWidth, float canyonDepth, float axisOffset, float period, float amplitude, float periodOffset) {
        
        float k = 2f;

        // Calc x-axis distance from sine wave (not currently sdf)
        float mod = Mathf.Sin(point.y*2*Mathf.PI/period + periodOffset);

        // Apply Amplitude and Damping factor
        mod *= Mathf.Min(amplitude, Mathf.Abs(point.y/50f));

        // Get distance from sine wave
        float xDistFromSine = Mathf.Abs(point.x -axisOffset - mod);

        float canyonFunc = Mathf.Pow(Smin(canyonWidth, Mathf.Max(xDistFromSine -canyonBaseWidth, 0f), k), 2) / Mathf.Pow(canyonWidth, 2);
        canyonFunc = 1 - canyonFunc;

        return canyonFunc * canyonDepth;
    }

    // Carve canyon shape, stepped method
    private static float CanyonCarveStepped(Vector2 point, float canyonWidth, float canyonBaseWidth, float canyonDepth, float axisOffset, float period, float amplitude, float periodOffset) {
        
        float k = 2f;


        // Calc x-axis distance from sine wave (not currently sdf)
        float mod = Mathf.Sin(point.y*2*Mathf.PI/period + periodOffset);

        // Apply Amplitude and Damping factor
        mod *= Mathf.Min(amplitude, Mathf.Abs(point.y/50f));

        // Get distance from sine wave
        float xDistFromSine = Mathf.Abs(point.x -axisOffset - mod);

        float canyonFunc = Mathf.Min(Mathf.Max((xDistFromSine -canyonBaseWidth/2f), 0f),canyonWidth/2f) / canyonWidth;

        // Steps
        int steps = 5;
        float stepWidth = 0.2f;
        float stepHeight = (1-stepWidth)/steps;

        for (int i = 1; i <= steps; i++)
        {
            float currStepHeight = i*stepHeight;
            if (currStepHeight < canyonFunc &&  canyonFunc < currStepHeight + stepWidth){
                canyonFunc = currStepHeight; // replace with smin/smax/smoothstep
            }
        }


        canyonFunc = 1 - canyonFunc;

        return canyonFunc * canyonDepth;
    }


    // Exponential Soft-min function
    // Returns Minimum of a, b with a smoothing factor of k.
    static private float Smin( float a, float b, float k )
    {
        k *= 1.0f;
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

        // Normalize the noise (not currently functional)
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

}