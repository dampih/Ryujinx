namespace Ryujinx.Graphics.Shader.Decoders
{
    enum SystemRegister
    {
        LaneId = 0,
        Clock = 1,
        VirtCfg = 2,
        VirtId = 3,
        Pm0 = 4,
        Pm1 = 5,
        Pm2 = 6,
        Pm3 = 7,
        Pm4 = 8,
        Pm5 = 9,
        Pm6 = 10,
        Pm7 = 11,
        OrderingTicket = 15,
        PrimType = 16,
        InvocationId = 17,
        YDirection = 18,
        ThreadKill = 19,
        ShaderType = 20,
        DirectCbeWriteAddressLow = 21,
        DirectCbeWriteAddressHigh = 22,
        DirectCbeWriteEnabled = 23,
        MachineId0 = 24,
        MachineId1 = 25,
        MachineId2 = 26,
        MachineId3 = 27,
        Affinity = 28,
        InvocationInfo = 29,
        WScaleFactorXY = 30,
        WScaleFactorZ = 31,
        TId = 32,
        TIdX = 33,
        TIdY = 34,
        TIdZ = 35,
        Cta_param = 36,
        CtaIdX = 37,
        CtaIdY = 38,
        CtaIdZ = 39,
        Ntid = 40,
        CirQueueIncrMinusOne = 41,
        Nlatc = 42,
        Swinlo = 48,
        Swinsz = 49,
        Smemsz = 50,
        Smembanks = 51,
        LWinLo = 52,
        LWinSz = 53,
        LMemLoSz = 54,
        LMemHiOff = 55,
        EqMask = 56,
        LtMask = 57,
        LeMask = 58,
        GtMask = 59,
        GeMask = 60,
        RegAlloc = 61,
        CtxAddr = 62,
        GlobalErrorStatus = 64,
        WarpErrorStatus = 66,
        WarpErrorStatusClear = 67,
        PmHi0 = 72,
        PmHi1 = 73,
        PmHi2 = 74,
        PmHi3 = 75,
        PmHi4 = 76,
        PmHi5 = 77,
        PmHi6 = 78,
        PmHi7 = 79,
        ClockLo = 80,
        ClockHi = 81,
        GlobalTimerLo = 82,
        GlobalTimerHi = 83,
        HwTaskId = 96,
        CircularQueueEntryIndex = 97,
        CircularQueueEntryAddressLow = 98,
        CircularQueueEntryAddressHigh = 99
    }
}