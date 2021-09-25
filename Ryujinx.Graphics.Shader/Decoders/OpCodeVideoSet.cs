using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeVideoSet : OpCodeVideo, IOpCodePredicate39
    {
        public Register Predicate0 { get; }
        public Register Predicate3 { get; }
        public Register Predicate39 { get; }

        public bool InvertP { get; }

        public LogicalOperation LogicalOp { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeVideoSet(emitter, address, opCode);

        public OpCodeVideoSet(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Predicate0  = new Register(opCode.Extract(0, 3),  RegisterType.Predicate);
            Predicate3  = new Register(opCode.Extract(3, 3),  RegisterType.Predicate);
            Predicate39 = new Register(opCode.Extract(39, 3), RegisterType.Predicate);

            InvertP = opCode.Extract(42);

            LogicalOp = (LogicalOperation)opCode.Extract(45, 2);
        }
    }
}