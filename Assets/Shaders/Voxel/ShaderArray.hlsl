//UNITY_SHADER_NO_UPGRADE
#ifndef SHADERSWITCH_INCLUDED
#define SHADERSWITCH_INCLUDED

void ShaderSwitch_float(float1 index,
    float4 grass, float4 dirt, float4 stone,
    float4 sand, float4 snow, float4 water,
    float4 forest, float4 beach, float4 plains,
    float4 sandstone, float4 savannahGrass, float4 savannahForest,
    float4 jungleForest, float4 pineForest, float4 swampForest,
    float4 mud, float4 ice, out float4 Out)
{
    switch (index)
    {
        case 1:
            Out = grass;
        break;
        case 2:
            Out = dirt;
        break;
        case 3:
            Out = stone;
        break;
        case 4:
            Out = sand;
        break;
        case 5:
            Out = snow;
        break;
        case 6:
            Out = water;
        break;
        case 7:
            Out = forest;
        break;
        case 8:
            Out = beach;
        break;
        case 9:
            Out = plains;
        break;
        case 10:
            Out = sandstone;
        break;
        case 11:
            Out = savannahGrass;
        break;
        case 12:
            Out = savannahForest;
        break;
        case 13:
            Out = jungleForest;
        break;
        case 14:
            Out = pineForest;
        break;
        case 15:
            Out = swampForest;
        break;
        case 16:
            Out = mud;
        break;
        case 17:
            Out = ice;
        break;
        default:
            Out = grass;
        break;
    }
}
#endif