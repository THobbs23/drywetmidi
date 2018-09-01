using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiOutWinApi
    {
        #region Types

        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIOUTCAPS
        {
            public ushort wMid;
            public ushort wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public ushort wTechnology;
            public ushort wVoices;
            public ushort wNotes;
            public ushort wChannelMask;
            public uint dwSupport;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIHDR
        {
            public IntPtr lpData;
            public int dwBufferLength;
            public int dwBytesRecorded;
            public IntPtr dwUser;
            public int dwFlags;
            public IntPtr lpNext;
            public IntPtr reserved;
            public int dwOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public IntPtr[] dwReserved;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MMTIME
        {
            [FieldOffset(0)] public uint wType;
            [FieldOffset(4)] public uint ms;
            [FieldOffset(4)] public uint sample;
            [FieldOffset(4)] public uint cb;
            [FieldOffset(4)] public uint ticks;
            [FieldOffset(4)] public byte smpteHour;
            [FieldOffset(5)] public byte smpteMin;
            [FieldOffset(6)] public byte smpteSec;
            [FieldOffset(7)] public byte smpteFrame;
            [FieldOffset(8)] public byte smpteFps;
            [FieldOffset(9)] public byte smpteDummy;
            [FieldOffset(10)] public byte smptePad0;
            [FieldOffset(11)] public byte smptePad1;
            [FieldOffset(4)] public uint midiSongPtrPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIDIEVENT
        {
            public uint dwDeltaTime;
            public uint dwStreamID;
            public uint dwEvent;
            //[MarshalAs(UnmanagedType.ByValArray)]
            //public uint[] dwParms;
        }

        [Flags]
        public enum MIDICAPS : uint
        {
            MIDICAPS_VOLUME = 1,
            MIDICAPS_LRVOLUME = 2,
            MIDICAPS_CACHE = 4,
            MIDICAPS_STREAM = 8
        }

        #endregion

        #region Constants

        public const uint MIDIPROP_GET = 1073741824;
        public const uint MIDIPROP_SET = 2147483648;
        public const uint MIDIPROP_TEMPO = 2;
        public const uint MIDIPROP_TIMEDIV = 1;

        public const uint MEVT_F_CALLBACK = 1073741824;
        public const uint MEVT_F_LONG = 2147483648;
        public const uint MEVT_F_SHORT = 0;

        public const uint MEVT_SHORTMSG = 0;
        public const uint MEVT_TEMPO = 1;
        public const uint MEVT_NOP = 2;
        public const uint MEVT_LONGMSG = 128;
        public const uint MEVT_COMMENT = 130;
        public const uint MEVT_VERSION = 132;

        #endregion

        #region Methods

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutGetDevCaps(UIntPtr uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutGetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutOpen(out IntPtr lphmo, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutClose(IntPtr hmo);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutShortMsg(IntPtr hMidiOut, uint dwMsg);

        [DllImport("winmm.dll")]
        public extern static MMRESULT midiOutPrepareHeader(IntPtr hmo, ref MIDIHDR lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll")]
        public extern static MMRESULT midiOutUnprepareHeader(IntPtr hmo, ref MIDIHDR lpMidiOutHdr, int cbMidiOutHdr);

        [DllImport("winmm.dll")]
        public extern static MMRESULT midiStreamOpen(ref IntPtr hMidiStream, ref uint puDeviceID, int cMidi, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint fdwOpen);

        [DllImport("winmm.dll")]
        public extern static MMRESULT midiStreamClose(IntPtr hMidiStream);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamOut(IntPtr hMidiStream, ref MIDIHDR lpMidiHdr, int cbMidiHdr);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamPause(IntPtr hMidiStream);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamPosition(IntPtr hms, ref MMTIME pmmt, uint cbmmt);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamProperty(IntPtr hm, byte[] lppropdata, uint dwProperty);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamRestart(IntPtr hMidiStream);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiStreamStop(IntPtr hMidiStream);

        #endregion
    }
}
