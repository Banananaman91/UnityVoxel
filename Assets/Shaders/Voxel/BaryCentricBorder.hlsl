//UNITY_SHADER_NO_UPGRADE
#ifndef BCFUNCTION_INCLUDED
#define BCFUNCTION_INCLUDED

void BCFunction_float(float4 A, float B, out float4 Out)
{
    if (B <= 0)
    {
        Out = A;
        return;
    }

    if (B > 1) B = 1;
    float x = A.x - B;
    float y = A.y - B;
    float z = A.z - B;
    if (x < 0) x = 0;
    if (y < 0) y = 0;
    if (z < 0) z = 0;
    float sum = x + y + z;
    Out = float4(x/sum, y/sum, z/sum, A.w);
}
#endif