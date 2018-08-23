using System;

namespace Melanchall.DryWetMidi.Devices
{
    [Flags]
    internal enum CALLBACK : uint
    {
        CALLBACK_NULL = 0,
        CALLBACK_FUNCTION = 196608,
        CALLBACK_TASK = 131072,
        CALLBACK_TYPEMASK = 458752,
        CALLBACK_WINDOW = 65536
    }
}
