#if defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL)
// AMD recommand this value for GCN http://amd-dev.wpengine.netdna-cdn.com/wordpress/media/2013/05/GCNPerformanceTweets.pdf
#define MAX_TESSELLATION_FACTORS 15.0
#else
#define MAX_TESSELLATION_FACTORS 64.0
#endif

struct TessellationFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

float3 CalDistFactor(float3 posOS0, float3 posOS1, float3 posRWS0)
{
    float distanceFromCamera = length(posRWS0);
    float fadeDist = _TessellationFactorMaxDistance - _TessellationFactorMinDistance;
    float distanceFade = saturate((distanceFromCamera - _TessellationFactorMinDistance) / fadeDist);
    distanceFade = clamp(distanceFade, _TessellationMin, _TessellationMax);

    return distance(posOS0, posOS1) / distanceFade;
}

float4 tessellationEdgeFactor(VaryingsToDS vert0, VaryingsToDS vert1)
{
    float3 v0 = vert0.vmesh.positionOS.xyz;
    float3 v1 = vert1.vmesh.positionOS.xyz;
    float3 p0 = vert0.vmesh.positionRWS.xyz;

    float3 distFactor = CalDistFactor(v0, v1, p0);
    float3 edgeFactor = distFactor * distFactor * _TessellationFactor;
    edgeFactor = max(edgeFactor, float3(1.0, 1.0, 1.0));

    return CalcTriTessFactorsFromEdgeTessFactors(edgeFactor);
}

TessellationFactors HullConstant(InputPatch<PackedVaryingsToDS, 3> input)
{
    VaryingsToDS varying0 = UnpackVaryingsToDS(input[0]);
    VaryingsToDS varying1 = UnpackVaryingsToDS(input[1]);

    float4 tf = tessellationEdgeFactor(varying0, varying1);

    TessellationFactors output;
    output.edge[0] = min(tf.x, MAX_TESSELLATION_FACTORS);
    output.edge[1] = min(tf.y, MAX_TESSELLATION_FACTORS);
    output.edge[2] = min(tf.z, MAX_TESSELLATION_FACTORS);
    output.inside  = min(tf.w, MAX_TESSELLATION_FACTORS);

    return output;
}

[maxtessfactor(MAX_TESSELLATION_FACTORS)]
[domain("tri")]
[outputcontrolpoints(3)]
[outputtopology("triangle_cw")]
[partitioning("integer")]
[patchconstantfunc("HullConstant")]
PackedVaryingsToDS Hull(InputPatch<PackedVaryingsToDS, 3> input, uint id : SV_OutputControlPointID)
{
    return input[id];
}

[domain("tri")]
PackedVaryingsToDS Domain(TessellationFactors tessFactors, const OutputPatch<PackedVaryingsToDS, 3> input, float3 baryCoords : SV_DomainLocation)
{
    VaryingsToDS varying0 = UnpackVaryingsToDS(input[0]);
    VaryingsToDS varying1 = UnpackVaryingsToDS(input[1]);
    VaryingsToDS varying2 = UnpackVaryingsToDS(input[2]);

    VaryingsToDS varying = InterpolateWithBaryCoordsToDS(varying0, varying1, varying2, baryCoords);

    return PackVaryingsToDS(varying);
}