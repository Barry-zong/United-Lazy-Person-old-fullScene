struct AttributesMesh
{
    float3 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT; // Store sign in w
    float2 uv0          : TEXCOORD0;
    half4 color        : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsMeshToPS
{
    float3 positionOS;
    float4 positionCS;
    float3 positionRWS;
    float3 normalWS;
    float4 tangentWS;  // w contain mirror sign
    float2 texCoord0;
    half4 color;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct PackedVaryingsMeshToPS
{
    float4 positionCS : SV_Position;

    float3 interpolators0 : TEXCOORD0; // PositionWS
    float3 interpolators1 : TEXCOORD1; // NormalWS
    float4 interpolators2 : TEXCOORD2; // TangentWS
    float2 interpolators3 : TEXCOORD3; // UV0
    float3 interpolators4 : TEXCOORD4; //PositionOS
    half4 interpolators5 : TEXCOORD5; //color

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Functions to pack data to use as few interpolator as possible, the ShaderGraph should generate these functions
PackedVaryingsMeshToPS PackVaryingsMeshToPS(VaryingsMeshToPS input)
{
    PackedVaryingsMeshToPS output;

    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.positionCS = input.positionCS;
    output.interpolators0 = input.positionRWS;
    output.interpolators1 = input.normalWS;
    output.interpolators2 = input.tangentWS;
    output.interpolators3 = input.texCoord0;
    output.interpolators4 = input.positionOS;
    output.interpolators5 = input.color;

    return output;
}

FragInputs UnpackVaryingsMeshToFragInputs(PackedVaryingsMeshToPS input)
{
    FragInputs output;
    ZERO_INITIALIZE(FragInputs, output);

    UNITY_SETUP_INSTANCE_ID(input);

    // Init to some default value to make the computer quiet (else it output "divide by zero" warning even if value is not used).
    // TODO: this is a really poor workaround, but the variable is used in a bunch of places
    // to compute normals which are then passed on elsewhere to compute other values...
    output.tangentToWorld = k_identity3x3;
    output.positionSS = input.positionCS; // input.positionCS is SV_Position
    output.positionRWS.xyz = input.interpolators0.xyz;
    output.texCoord0.xy = input.interpolators3;
    output.color = input.interpolators5;

    return output;
}

// Varying for domain shader
// Position and normal are always present (for tessellation) and in world space
struct VaryingsMeshToDS
{
    float3 positionOS;
    float3 positionRWS;
    float3 normalOS;
    float4 tangentOS;
    float2 texCoord0;
    half4 color;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct PackedVaryingsMeshToDS
{
    float3 interpolators0 : INTERNALTESSPOS; // positionRWS.xyz
    float3 interpolators1 : NORMAL; // NormalOS
    float4 interpolators2 : TANGENT; // TangentOS
    float2 interpolators3 : TEXCOORD0; // UV0 distanceFade
    float3 interpolators4 : TEXCOORD4; //PositionOS
    half4 interpolators5 : TEXCOORD5; //color

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Functions to pack data to use as few interpolator as possible, the ShaderGraph should generate these functions
PackedVaryingsMeshToDS PackVaryingsMeshToDS(VaryingsMeshToDS input)
{
    PackedVaryingsMeshToDS output;
    ZERO_INITIALIZE(PackedVaryingsMeshToDS, output);

    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.interpolators0 = input.positionRWS;
    output.interpolators1 = input.normalOS;
    output.interpolators2 = input.tangentOS;
    output.interpolators3 = input.texCoord0;
    output.interpolators4 = input.positionOS;
    output.interpolators5 = input.color;

    return output;
}

VaryingsMeshToDS UnpackVaryingsMeshToDS(PackedVaryingsMeshToDS input)
{
    VaryingsMeshToDS output;

    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.positionRWS = input.interpolators0.xyz;
    output.normalOS = input.interpolators1;
    output.tangentOS = input.interpolators2;
    output.texCoord0 = input.interpolators3;
    output.positionOS = input.interpolators4;
    output.color = input.interpolators5;

    return output;
}

VaryingsMeshToDS InterpolateWithBaryCoordsMeshToDS(VaryingsMeshToDS input0, VaryingsMeshToDS input1, VaryingsMeshToDS input2, float3 baryCoords)
{
    VaryingsMeshToDS output;

    UNITY_TRANSFER_INSTANCE_ID(input0, output);

    TESSELLATION_INTERPOLATE_BARY(positionOS, baryCoords);
    TESSELLATION_INTERPOLATE_BARY(positionRWS, baryCoords);
    TESSELLATION_INTERPOLATE_BARY(normalOS, baryCoords);
    TESSELLATION_INTERPOLATE_BARY(tangentOS, baryCoords);
    TESSELLATION_INTERPOLATE_BARY(texCoord0, baryCoords);
    TESSELLATION_INTERPOLATE_BARY(color, baryCoords);

    return output;
}
