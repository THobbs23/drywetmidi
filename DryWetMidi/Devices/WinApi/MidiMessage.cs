namespace Melanchall.DryWetMidi.Devices
{
    internal enum MidiMessage : int
    {
        MM_MIM_CLOSE = 962,
        MM_MIM_DATA = 963,
        MM_MIM_ERROR = 965,
        MM_MIM_LONGDATA = 964,
        MM_MIM_LONGERROR = 966,
        MM_MIM_MOREDATA = 972,
        MM_MIM_OPEN = 961,
        MIM_CLOSE = MM_MIM_CLOSE,
        MIM_DATA = MM_MIM_DATA,
        MIM_ERROR = MM_MIM_ERROR,
        MIM_LONGDATA = MM_MIM_LONGDATA,
        MIM_LONGERROR = MM_MIM_LONGERROR,
        MIM_MOREDATA = MM_MIM_MOREDATA,
        MIM_OPEN = MM_MIM_OPEN
    }
}
