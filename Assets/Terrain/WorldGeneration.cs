using Unity.Mathematics;
using static Unity.Mathematics.math;

public class WorldGeneration {
    
    // Get fractional component of float p
    static float Fract(float p) {
        return p - floor(p);
    }

    // Hash function Vector2 => Vector2
    // (Potentially Replace with a better hash)
    static float2 Hash(float2 x) {
        float2 k = float2(0.3183099f, 0.3678794f);
        x = x * k + float2(k.y, k.x); // note y,x instead of x,y
        float2 l = 16 * Fract(x.x * x.y * (x.x + x.y)) * k;
        return 2 * float2(Fract(l.x), Fract(l.y)) - float2(1, 1);
    }

    // Exponential smooth minimum function
    static float Smin( float a, float b, float k )
    {
        k *= 1;
        float r = pow(2, -a / k) + pow(2, -b / k);
        return -k * log2(r);
    }

    // Calculate quintic interpolation
    static float QuinticInterpolation(float x) {
        return x * x * x * (x * (x * 6 - 15) + 10);
    }

    // Calculate quintic derivatives
    static float QuinticInterpolationDerivative(float x) {
        return 30 * x * x * (x * (x - 2) + 1);
    }

    static float3 Noised(float2 p) {
        float2 i = float2(floor(p.x), floor(p.y));
        float2 f = float2(p.x - i.x, p.y - i.y);               // get fractional part of x

        float2 u = float2(QuinticInterpolation(f.x), QuinticInterpolation(f.y));  
        float2 du = float2(QuinticInterpolationDerivative(f.x), QuinticInterpolationDerivative(f.y));  

        float2 ga = Hash(i + float2(0, 0));
        float2 gb = Hash(i + float2(1, 0));
        float2 gc = Hash(i + float2(0, 1));
        float2 gd = Hash(i + float2(1, 1));

        float va = dot(ga, f - float2(0, 0));
        float vb = dot(gb, f - float2(1, 0));
        float vc = dot(gc, f - float2(0, 1));
        float vd = dot(gd, f - float2(1, 1));

        float c = (va - vb - vc + vd);
        float2 FBMDerivative = ga + u.x * (gb - ga) + u.y * (gc - ga) + u.x * u.y * (ga - gb - gc + gd) + du 
                                * (float2(u.y, u.x) * float2(c, c) + float2(vb, vc) - float2(va, va)); // note y,x instead of x,y on "u"
                        
        return float3(va + u.x * (vb - va) + u.y * (vc - va) + u.x * u.y * (va - vb - vc + vd),   // value
                        FBMDerivative.x, FBMDerivative.y); // derivatives
    }

    // FBM with erosion
    static float3 FBMErosion(float2 p, int octaves) {
        float a = 0;
        float b = 1;
        float2 d = (float2)0;

        float2 m1 = float2(0.8f, -0.6f);  
        float2 m2 = float2(0.6f, 0.8f);  

        for (int i= 0; i < octaves; i++)
        {
            float3 n = Noised(p);

            // Separate noise 2D-gradients
            d += float2(n.y, n.z);

            // Accumulate noise values, 
            // dampens the contribution of noise based on the accumulated gradients (magnitude)^2
            a += b * n.x / (1 + dot(d, d));

            // Half the amplitude of noise each octave
            b *= 0.5f;

            // Apply magic vectors that rotate and scale the noise each octave to add variation each octave
            p.x = (m1.x * p.x + m1.y * p.y) * 2;
            p.y = (m2.x * p.x + m2.y * p.y) * 2;
        }

        return float3(a, d.x, d.y);
    }

    // Canyon carving function
    static float CanyonCarve(float2 p, float canyonWidth, float canyonBaseWidth, float canyonDepth, float axisOffset, float period, float amplitude, float periodOffset) {
        
        // Clamp canyons base width
        canyonBaseWidth = min(canyonWidth, canyonBaseWidth);

        // Calc x-axis distance from sine wave (not currently sdf)
        float mod = sin(p.y * 2 * PI / period + periodOffset);
        mod = min(amplitude, abs(p.y / 50)) * mod; // Damping factor

        float sinSDF = abs(p.x -axisOffset - mod);

        sinSDF = (pow(Smin(canyonWidth, max(0, sinSDF -canyonBaseWidth), 2), 2) / pow(canyonWidth, 2));
        sinSDF = 1 - sinSDF;

        return sinSDF * canyonDepth;
    }

    // World heightmap generation function
    public static float SampleHeight(float x, float z) {
        
        // Get the height from the heightmap
        float height = FBMErosion(float2(x / 300, z / 300), 10).x * 150;

        // Carve a canyon
        
        // Get canyon depth
        float sinSDF = CanyonCarve(float2(x, z), 80, 30, 50, 0, 500, 100, 0);
        sinSDF += CanyonCarve(float2(x, z), 50, 80, 35, 0, 1500, 100, 30);
        sinSDF += CanyonCarve(float2(x, z), 30, 130, 10, 0, 2500, 100, 65);

        // Decrease the height of the terrain in the canyon
        height *= min(1, 60 / sinSDF);

        height -= sinSDF;
        
        // Return the height
        return height + 70;
    }
}
