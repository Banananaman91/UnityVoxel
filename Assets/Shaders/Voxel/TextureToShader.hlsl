//UNITY_SHADER_NO_UPGRADE
#ifndef TEXTURECONVERT_INCLUDED
#define TEXTURECONVERT_INCLUDED

void TextureConvert_float(float1 index, float4 input,
    out float4 grass, out float4 dirt, out float4 stone,
    out float4 sand, out float4 snow, out float4 water,
    out float4 forest, out float4 beach, out float4 plains,
    out float4 sandstone, out float4 savannahGrass, out float4 savannahForest,
    out float4 jungleForest, out float4 pineForest, out float4 swampForest,
    out float4 mud, out float4 ice)
{
    switch (index)
    {
        case 1:
            grass = input;
        break;
        case 2:
            dirt = input;
        break;
        case 3:
            stone = input;
        break;
        case 4:
            sand = input;
        break;
        case 5:
            snow = input;
        break;
        case 6:
            water = input;
        break;
        case 7:
            forest = input;
        break;
        case 8:
            beach = input;
        break;
        case 9:
            plains = input;
        break;
        case 10:
            sandstone = input;
        break;
        case 11:
            savannahGrass = input;
        break;
        case 12:
            savannahForest = input;
        break;
        case 13:
            jungleForest = input;
        break;
        case 14:
            pineForest = input;
        break;
        case 15:
            swampForest = input;
        break;
        case 16:
            mud = input;
        break;
        case 17:
            ice = input;
        break;
        default:
            grass = input;
        break;
    }
}
#endif