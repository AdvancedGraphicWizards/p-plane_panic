float sdfUnion( float distance1, float distance2 )
{
    return min(distance1, distance2);
}

float sdfUnionSmooth(float dist1, float dist2, float k)
{
    float h = clamp(0.5 + 0.5 * (dist2 - dist1) / k, 0.0, 1.0);
    return lerp(dist2, dist1, h) - k * h * (1.0 - h);
}

float sdfBox(float3 p, float3 origin, float3 bounds)
{
    float3 q = abs(p - origin) - bounds;
    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
} 

float sdfSphere(float3 p, float3 origin, float radius)
{
    return length(p - origin) - radius;
}

float sdfEllipsoid(float3 p, float3 origin, float3 radius)
{
    float k1 = length((p - origin) / radius);
    float k2 = length((p - origin) / (radius * radius));
    return k1 * (k1 - 1.0) / k2;
}
