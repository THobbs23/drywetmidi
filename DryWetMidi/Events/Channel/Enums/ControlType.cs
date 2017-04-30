﻿namespace Melanchall.DryWetMidi
{
    public enum ControlType : byte 
    {
        BankSelect                      = 0x00,
        Modulation                      = 0x01,
        BreathController                = 0x02,
        FootController                  = 0x04,
        PortamentoTime                  = 0x05,
        DataEntryMsb                    = 0x06,
        MainVolume                      = 0x07,
        Balance                         = 0x08,
        Pan                             = 0x0A,
        ExpressionController            = 0x0B,
        EffectControl1                  = 0x0C,
        EffectControl2                  = 0x0D,
        GeneralPurposeController1       = 0x10,
        GeneralPurposeController2       = 0x11,
        GeneralPurposeController3       = 0x12,
        GeneralPurposeController4       = 0x13,
        LsbForController0               = 0x20,
        LsbForController1               = 0x21,
        LsbForController2               = 0x22,
        LsbForController3               = 0x23,
        LsbForController4               = 0x24,
        LsbForController5               = 0x25,
        LsbForController6               = 0x26,
        LsbForController7               = 0x27,
        LsbForController8               = 0x28,
        LsbForController9               = 0x29,
        LsbForController10              = 0x2A,
        LsbForController11              = 0x2B,
        LsbForController12              = 0x2C,
        LsbForController13              = 0x2D,
        LsbForController14              = 0x2E,
        LsbForController15              = 0x2F,
        LsbForController16              = 0x30,
        LsbForController17              = 0x31,
        LsbForController18              = 0x32,
        LsbForController19              = 0x33,
        LsbForController20              = 0x34,
        LsbForController21              = 0x35,
        LsbForController22              = 0x36,
        LsbForController23              = 0x37,
        LsbForController24              = 0x38,
        LsbForController25              = 0x39,
        LsbForController26              = 0x3A,
        LsbForController27              = 0x3B,
        LsbForController28              = 0x3C,
        LsbForController29              = 0x3D,
        LsbForController30              = 0x3E,
        LsbForController31              = 0x3F,
        DamperPedal                     = 0x40,
        Portamento                      = 0x41,
        Sostenuto                       = 0x42,
        SoftPedal                       = 0x43,
        LegatoFootswitch                = 0x44,
        Hold2                           = 0x45,
        SoundController1                = 0x46,
        SoundController2                = 0x47,
        SoundController3                = 0x48,
        SoundController4                = 0x49,
        SoundController5                = 0x4A,
        SoundController6                = 0x4B,
        SoundController7                = 0x4C,
        SoundController8                = 0x4D,
        SoundController9                = 0x4E,
        SoundController10               = 0x4F,
        GeneralPurposeController5       = 0x50,
        GeneralPurposeController6       = 0x51,
        GeneralPurposeController7       = 0x52,
        GeneralPurposeController8       = 0x53,
        PortamentoControl               = 0x54,
        Effects1Depth                   = 0x5B,
        Effects2Depth                   = 0x5C,
        Effects3Depth                   = 0x5D,
        Effects4Depth                   = 0x5E,
        Effects5Depth                   = 0x5F,
        DataIncrement                   = 0x60,
        DataDecrement                   = 0x61,
        NonRegisteredParameterNumberLsb = 0x62,
        NonRegisteredParameterNumberMsb = 0x63,
        RegisteredParameterNumberLsb    = 0x64,
        RegisteredParameterNumberMsb    = 0x65,

        AllSoundOff                     = 120,
        ResetAllControllers             = 121,
        LocalControl                    = 122,
        AllNotesOff                     = 123,
        OmniModeOff                     = 124,
        OmniModeOn                      = 125,
        PolyModeOff                     = 126,
        PolyModeOn                      = 127
    }
}