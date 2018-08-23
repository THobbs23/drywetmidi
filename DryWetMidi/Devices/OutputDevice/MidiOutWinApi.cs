using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiOutWinApi
    {
        #region Constants

        public const uint MAXERRORLENGTH = 256;

        #endregion

        #region Types

        [StructLayout(LayoutKind.Sequential)]
        internal struct MIDIOUTCAPS
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

        #endregion

        #region Methods

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutGetDevCaps(UIntPtr uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);

        [DllImport("winmm.dll")]
        public static extern MMRESULT midiOutGetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern int midiOutGetNumDevs();

        #endregion
    }
}
