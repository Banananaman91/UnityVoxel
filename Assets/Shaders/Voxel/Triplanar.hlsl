//UNITY_SHADER_NO_UPGRADE
#ifndef TRIPLANARVOXEL_INCLUDED
#define TRIPLANARVOXEL_INCLUDED

void TriplanarVoxel_float(sampler2D bottomTex, sampler2D topTex, sampler2D rightTex, float3 worldPos, float3 uv, out float4 Out)
{
    float4 x = tex2D(bottomTex, worldPos.zy);
    float4 y = tex2D(bottomTex, worldPos.xz);
    float4 z = tex2D(bottomTex, worldPos.xy);

    float4 bottom = x * uv.x + y * uv.y + z * uv.z;

    float4 xTop = tex2D(topTex, worldPos.zy);
    float4 yTop = tex2D(topTex, worldPos.xz);
    float4 zTop = tex2D(topTex, worldPos.xy);

    float4 top = xTop * uv.x + yTop * uv.y + zTop * uv.z;

    float4 xRight = tex2D(rightTex, worldPos.zy);
    float4 yRight = tex2D(rightTex, worldPos.xz);
    float4 zRight = tex2D(rightTex, worldPos.xy);

    float4 right = xRight * uv.x + yRight * uv.y + zRight * uv.z;

    Out = bottom + top + right;
}
#endif