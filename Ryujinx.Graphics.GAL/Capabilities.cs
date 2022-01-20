namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public readonly bool HasFrontFacingBug;
        public readonly bool HasVectorIndexingBug;

        public readonly bool SupportsAstcCompression;
        public readonly bool Supports3DTextureCompression;
        public readonly bool SupportsR4G4Format;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsFragmentShaderOrderingIntel;
        public readonly bool SupportsImageLoadFormatted;
        public readonly bool SupportsMismatchingViewFormat;
        public readonly bool SupportsNonConstantTextureOffset;
        public readonly bool SupportsShaderBallot;
        public readonly bool SupportsTextureShadowLod;
        public readonly bool SupportsViewportSwizzle;
        public readonly bool SupportsIndirectParameters;

        public int MaximumComputeSharedMemorySize { get; }
        public float MaximumSupportedAnisotropy { get; }
        public int MaximumSupportedComputeUniforms { get; }
        public int MaximumSupportedVertexUniforms { get; }
        public int MaximumSupportedTessControlUniforms { get; }
        public int MaximumSupportedTessEvaluationUniforms { get; }
        public int MaximumSupportedGeometryUniforms { get; }
        public int MaximumSupportedFragmentUniforms { get; }
        public int StorageBufferOffsetAlignment { get; }

        public Capabilities(
            bool hasFrontFacingBug,
            bool hasVectorIndexingBug,
            bool supportsAstcCompression,
            bool supports3DTextureCompression,
            bool supportsR4G4Format,
            bool supportsFragmentShaderInterlock,
            bool supportsFragmentShaderOrderingIntel,
            bool supportsImageLoadFormatted,
            bool supportsMismatchingViewFormat,
            bool supportsNonConstantTextureOffset,
            bool supportsShaderBallot,
            bool supportsTextureShadowLod,
            bool supportsViewportSwizzle,
            bool supportsIndirectParameters,
            int maximumComputeSharedMemorySize,
            float maximumSupportedAnisotropy,
            int maximumSupportedComputeUniforms,
            int maximumSupportedVertexUniforms,
            int maximumSupportedTessControlUniforms,
            int maximumSupportedTessEvaluationUniforms,
            int maximumSupportedGeometryUniforms,
            int maximumSupportedFragmentUniforms,
            int storageBufferOffsetAlignment)
        {
            HasFrontFacingBug = hasFrontFacingBug;
            HasVectorIndexingBug = hasVectorIndexingBug;
            SupportsAstcCompression = supportsAstcCompression;
            Supports3DTextureCompression = supports3DTextureCompression;
            SupportsR4G4Format = supportsR4G4Format;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsFragmentShaderOrderingIntel = supportsFragmentShaderOrderingIntel;
            SupportsImageLoadFormatted = supportsImageLoadFormatted;
            SupportsMismatchingViewFormat = supportsMismatchingViewFormat;
            SupportsNonConstantTextureOffset = supportsNonConstantTextureOffset;
            SupportsShaderBallot = supportsShaderBallot;
            SupportsTextureShadowLod = supportsTextureShadowLod;
            SupportsViewportSwizzle = supportsViewportSwizzle;
            SupportsIndirectParameters = supportsIndirectParameters;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            MaximumSupportedAnisotropy = maximumSupportedAnisotropy;
            MaximumSupportedComputeUniforms = maximumSupportedComputeUniforms;
            MaximumSupportedVertexUniforms = maximumSupportedVertexUniforms;
            MaximumSupportedTessControlUniforms = maximumSupportedTessControlUniforms;
            MaximumSupportedTessEvaluationUniforms = maximumSupportedTessEvaluationUniforms;
            MaximumSupportedGeometryUniforms = maximumSupportedGeometryUniforms;
            MaximumSupportedFragmentUniforms = maximumSupportedFragmentUniforms;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}