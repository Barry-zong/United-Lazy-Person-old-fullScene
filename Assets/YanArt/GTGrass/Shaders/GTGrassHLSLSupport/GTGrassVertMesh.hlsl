struct VaryingsToPS
{
    VaryingsMeshToPS vmesh;
};

struct PackedVaryingsToPS
{
    PackedVaryingsMeshToPS vmesh;
    UNITY_VERTEX_OUTPUT_STEREO

    #if defined(PLATFORM_SUPPORTS_PRIMITIVE_ID_IN_PIXEL_SHADER) && SHADER_STAGE_FRAGMENT
    #if (defined(VARYINGS_NEED_PRIMITIVEID) || (SHADERPASS == SHADERPASS_FULL_SCREEN_DEBUG))
        uint primitiveID : SV_PrimitiveID;
    #endif
    #endif
};

PackedVaryingsToPS PackVaryingsToPS(VaryingsToPS input)
{
    PackedVaryingsToPS output;
    output.vmesh = PackVaryingsMeshToPS(input.vmesh);

    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    return output;
}

FragInputs UnpackVaryingsToFragInputs(PackedVaryingsToPS packedInput)
{
    FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);

#if defined(PLATFORM_SUPPORTS_PRIMITIVE_ID_IN_PIXEL_SHADER) && SHADER_STAGE_FRAGMENT
#if (defined(VARYINGS_NEED_PRIMITIVEID) || (SHADERPASS == SHADERPASS_FULL_SCREEN_DEBUG))
    input.primitiveID = packedInput.primitiveID;
#endif
#endif

#if defined(VARYINGS_NEED_CULLFACE) && SHADER_STAGE_FRAGMENT
    input.isFrontFace = IS_FRONT_VFACE(packedInput.cullFace, true, false);
#endif

    return input;
}

struct VaryingsToDS
{
    VaryingsMeshToDS vmesh;
};

struct PackedVaryingsToDS
{
    PackedVaryingsMeshToDS vmesh;
};

PackedVaryingsToDS PackVaryingsToDS(VaryingsToDS input)
{
    PackedVaryingsToDS output;
    // ZERO_INITIALIZE(PackedVaryingsToDS, output);
    output.vmesh = PackVaryingsMeshToDS(input.vmesh);

    return output;
}

VaryingsToDS UnpackVaryingsToDS(PackedVaryingsToDS input)
{
    VaryingsToDS output;
    output.vmesh = UnpackVaryingsMeshToDS(input.vmesh);

    return output;
}

VaryingsToDS InterpolateWithBaryCoordsToDS(VaryingsToDS input0, VaryingsToDS input1, VaryingsToDS input2, float3 baryCoords)
{
    VaryingsToDS output;

    output.vmesh = InterpolateWithBaryCoordsMeshToDS(input0.vmesh, input1.vmesh, input2.vmesh, baryCoords);

    return output;
}

// TODO: Here we will also have all the vertex deformation (GPU skinning, vertex animation, morph target...) or we will need to generate a compute shaders instead (better! but require work to deal with unpacking like fp16)
VaryingsMeshToDS VertMesh(AttributesMesh input, float3 worldSpaceOffset)
{
    VaryingsMeshToDS output;
    ZERO_INITIALIZE(VaryingsMeshToDS, output); // Only required with custom interpolator to quiet the shader compiler about not fully initialized struct


    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    // This return the camera relative position (if enable)
    float3 positionRWS = TransformObjectToWorld(input.positionOS) + worldSpaceOffset;
    // float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    // float4 tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

    output.positionRWS = positionRWS;
    output.normalOS = input.normalOS;
    output.tangentOS = input.tangentOS;

    output.positionOS = input.positionOS;
    output.color = input.color;
    output.texCoord0 = input.uv0;

    return output;
}

VaryingsMeshToDS VertMesh(AttributesMesh input)
{
    return VertMesh(input, 0.0f);
}
