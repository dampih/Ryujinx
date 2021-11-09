namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public bool HasFrontFacingBug { get; }
        public bool HasVectorIndexingBug { get; }

        public bool SupportsAstcCompression { get; }
        public bool SupportsBlendEquationAdvanced { get; }
        public bool SupportsFragmentShaderInterlock { get; }
        public bool SupportsFragmentShaderOrderingIntel { get; }
        public bool SupportsImageLoadFormatted { get; }
        public bool SupportsMismatchingViewFormat { get; }
        public bool SupportsNonConstantTextureOffset { get; }
        public bool SupportsShaderBallot { get; }
        public bool SupportsTextureShadowLod { get; }
        public bool SupportsViewportSwizzle { get; }
        public bool SupportsIndirectParameters { get; }

        public int MaximumComputeSharedMemorySize { get; }
        public float MaximumSupportedAnisotropy { get; }
        public int StorageBufferOffsetAlignment { get; }

        public Capabilities(
            bool hasFrontFacingBug,
            bool hasVectorIndexingBug,
            bool supportsAstcCompression,
            bool supportsBlendEquationAdvanced,
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
            int storageBufferOffsetAlignment)
        {
            HasFrontFacingBug = hasFrontFacingBug;
            HasVectorIndexingBug = hasVectorIndexingBug;
            SupportsAstcCompression = supportsAstcCompression;
            SupportsBlendEquationAdvanced = supportsBlendEquationAdvanced;
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
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}