namespace Ryujinx.Graphics.Shader.Decoders
{
    interface IOpCodeAttribute : IOpCode
    {
        int AttributeOffset { get; }
        bool Patch { get; }
        bool Output { get; }
        int Count { get; }
        bool Indexed { get; }
    }
}