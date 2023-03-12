using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ENUMS USED IN RELATION TO SYNTHSETTINGS
public enum ActivationModeEnum
{
    QUEUE_REVERSE = 0,
    ACCORD = 1,
    QUEUE = 2,
    RANDOM = 3,
    SWEEP = 4,

};

public enum MushroomEffects
{
    FREQUENCY_FILTER,
    PITCH,
    DELAY,
    REPEATER
}


[Serializable]
public enum FrequencyFilterTypeEnum
{
    Lowpass,
    Highpass,
    Bandpass
};

[Serializable]
public enum AvailableWaveforms
{
    Sine = 0,
    Sawtooth = 1,
    Square = 2,
    Triangle = 3,
    Pulse = 4,
    WhiteNoise = 5
};

[Serializable]
public enum ModifiableVariables
{
    // Plant variables
    ACTIVATIONMODE = 0,

    // Synth settings
    WAVEFORM = 1,

    OCTAVE = 2,
    TONEOFFSET = 3,

    VELOCITY = 4,
    DURATION = 5,
    REVERB = 6,

    AMP_ATTACK = 7,
    AMP_DECAY = 8,
    AMP_SUSTAIN = 9,
    AMP_RELEASE = 10,

    PITCH_ATTACK = 11,
    PITCH_DECAY = 12,
    PITCH_SUSTAIN = 13,
    PITCH_RELEASE = 14,

    FILTER_ATTACK = 15,
    FILTER_DECAY = 16,
    FILTER_SUSTAIN = 17,
    FILTER_RELEASE = 18,
    FILTER_FREQUENCY = 19,
    FILTER_RESONANCE = 20,
    FILTER_ENVELOPE_AMOUNT = 21,
    FILTER_FREQUENCY_TYPE = 22,

    DELAY_FEEDBACK = 23,
    DELAY_TIME = 24,
    NOTE = 25,
    SUBDIVISION_COUNT = 26,
}

public enum SynthVariableVisualType
{
    Linear,
    Exponential,
    TimeSteps, // used for delay times and uses the list in synthsettings
    Steps,
}





[Serializable]
public class SynthSettings
{
     internal static readonly List<float> delayTimes = new List<float> {
        1.0f/2.0f,
        1.0f/3.0f,
        1.0f/4.0f,
        1.0f/6.0f,
        1.0f/8.0f,
        1.0f/12.0f,
        1.0f/16.0f,
    };

    [JsonProperty]
    bool initialized = false;

    internal void InitSynthVariables()
    {
        if (initialized) return;

        Duration = new SynthVariable(duration, durationMin, durationMax);
        Note = new SynthVariableInt(note, noteMin, noteMax);
        Velocity = new SynthVariable(velocity, velocityMin, velocityMax);
        Waveform = new SynthVariableInt((int) waveform, 0, Enum.GetNames(typeof(AvailableWaveforms)).Length - 1, SynthVariableVisualType.Steps);
        PitchStartNoteOffset = new SynthVariableInt(pitchStartNoteOffset, noteOffsetMin, noteOffsetMax);
        PitchEndNoteOffset = new SynthVariableInt(pitchEndNoteOffset, noteOffsetMin, noteOffsetMax);
        FilterFrequency = new SynthVariable(filterFrequency, filterFrequencyMin, filterFrequencyMax); //maybe should be exponential
        FilterEnvelopeAmount = new SynthVariable(filterEnvelopeAmount, filterEnvelopeAmountMin, filterEnvelopeAmountMax);
        FilterResonanceAmount = new SynthVariable(filterResonanceAmount, filterResonanceMin, filterResonanceMax);
        FrequencyFilterType = new SynthVariableInt((int) frequencyFilterType, 0, Enum.GetNames(typeof(FrequencyFilterTypeEnum)).Length - 1, SynthVariableVisualType.Steps);
        DelayFeedback = new SynthVariable(delayFeedback, delayFeedbackMin, delayFeedbackMax);
        DelayTime = new SynthVariable(delayTime, delayTimeMin, delayTimeMax, SynthVariableVisualType.TimeSteps);
        ReverbAmount = new SynthVariable(reverbAmount, reverbAmountMin, reverbAmountMax);
        ReverbRoomSize = new SynthVariable(reverbRoomSize, reverbRoomsizeMin, reverbRoomsizeMax);
        ReverbHiFreqDamping = new SynthVariable(reverbHiFreqDamping, reverbHiFreqDampingMin, reverbHiFreqDampingMax);
        Octave = new SynthVariableInt(octave, octaveMin, octaveMax, SynthVariableVisualType.Steps);
        ToneOffset = new SynthVariableInt(toneOffset, 0, 11, SynthVariableVisualType.Steps);
        SubdivisionCount = new SynthVariableInt(4, subdivisionCountMin, subdivisionCountMax, SynthVariableVisualType.Steps);
        ActivationMode = new SynthVariableInt((int) activationMode, 0, Enum.GetNames(typeof(ActivationModeEnum)).Length - 1, SynthVariableVisualType.Steps);


        initialized = true;

        AmplitudeADSR.Init();
        PitchADSR.Init();
        FilterADSR.Init();

    }
    
    internal const float durationMin = 0.001f;
    internal const float durationMax = 4f;
    [SerializeField, Range(durationMin, durationMax)]
    private float duration = 0.5f;
    [JsonProperty]
    internal SynthVariable Duration;


    internal const int noteMin = 1;
    internal const int noteMax = 127;
    /// <summary>
    /// this should not be used by plants, it wont work, but use it by things who dont have notes
    /// </summary>
    [SerializeField, Range(noteMin, noteMax)]
    private int note = 64;
    [JsonProperty]
    internal SynthVariableInt Note;

    internal const float velocityMin = 0.05f;
    internal const float velocityMax = 0.5f;
    [SerializeField, Range(velocityMin, velocityMax)]
    private float velocity;
    [JsonProperty]
    internal SynthVariable Velocity;


    public AvailableWaveforms waveform;
    [JsonProperty]
    internal SynthVariableInt Waveform;

    [SerializeField]
    private ActivationModeEnum activationMode = ActivationModeEnum.RANDOM;
    [JsonProperty]
    internal SynthVariableInt ActivationMode;

    public float LowpassFilterFrequency { get; set; }

    [Header("== Amplitude ==")]
    public ADSR AmplitudeADSR = new ADSR(0.004f, 0f, 1f, 0.004f);


    [Header("== Pitch ==")]
    public bool EnablePitchModulation = false;
    public ADSR PitchADSR = new ADSR(0.004f, 0f, 1f, 0.004f);


    internal const int noteOffsetMin = -48;
    internal const int noteOffsetMax = 48;
    [SerializeField, Range(noteOffsetMin, noteOffsetMax)]
    private int pitchStartNoteOffset = -12;
    [JsonProperty]
    internal SynthVariableInt PitchStartNoteOffset;


    [SerializeField, Range(noteOffsetMin, noteOffsetMax)]
    private int pitchEndNoteOffset = 12;
    [JsonProperty]
    internal SynthVariableInt PitchEndNoteOffset;

    [Header("== Filter ==")]
    public bool EnableFrequencyModulation = true;
    public ADSR FilterADSR = new ADSR(0.004f, 0f, 1f, 0.004f);


    internal const float filterFrequencyMin = 20;
    internal const float filterFrequencyMax = 8000;
    [SerializeField, Range(filterFrequencyMin, filterFrequencyMax)]
    private float filterFrequency = 2000f;
    [JsonProperty]
    internal SynthVariable FilterFrequency;

    public FrequencyFilterTypeEnum frequencyFilterType = FrequencyFilterTypeEnum.Lowpass;
    [JsonProperty]
    internal SynthVariableInt FrequencyFilterType;


    internal const float filterEnvelopeAmountMin = 0f;
    internal const float filterEnvelopeAmountMax = 1f;
    [Range(filterEnvelopeAmountMin, filterEnvelopeAmountMax)]
    public float filterEnvelopeAmount = 0f;
    [JsonProperty]
    internal SynthVariable FilterEnvelopeAmount;

    internal const float filterResonanceMin = 0.5f;
    internal const float filterResonanceMax = 30f;
    [Range(filterResonanceMin, filterResonanceMax)]
    public float filterResonanceAmount = 1f;
    [JsonProperty]
    internal SynthVariable FilterResonanceAmount;

    [Header("== Delay Controls ==")]
    public bool EnableDelay = false;

    internal const float delayFeedbackMin = 0;
    internal const float delayFeedbackMax = 0.8f;
    [SerializeField, Range(delayFeedbackMin, delayFeedbackMax)]
    public float delayFeedback = .5f;
    [JsonProperty]
    internal SynthVariable DelayFeedback;


    internal const float delayTimeMin = 0.01f;
    internal const float delayTimeMax = 0.5f;
    [SerializeField, Range(delayTimeMin, delayTimeMax)]
    private float delayTime = 0.3f;
    [JsonProperty]
    internal SynthVariable DelayTime;

    [Header("== Reverb Controls ==")]
    public bool EnableReverb = false;

    internal const float reverbAmountMin = 0;
    internal const float reverbAmountMax = 1f;
    [SerializeField, Range(reverbAmountMin, reverbAmountMax)]
    private float reverbAmount = 0f;
    [JsonProperty]
    internal SynthVariable ReverbAmount;

    internal const float reverbRoomsizeMin = 0;
    internal const float reverbRoomsizeMax = 1f;
    [SerializeField, Range(reverbRoomsizeMin, reverbRoomsizeMax)]
    private float reverbRoomSize = 0.85f;
    [JsonProperty]
    internal SynthVariable ReverbRoomSize;

    internal const float reverbHiFreqDampingMin = 0;
    internal const float reverbHiFreqDampingMax = 1f;
    [SerializeField, Range(reverbHiFreqDampingMin, reverbHiFreqDampingMax)]
    private float reverbHiFreqDamping = 0.5f;
    [JsonProperty]
    internal SynthVariable ReverbHiFreqDamping;

    [Header("== Tone controls ==")]
    internal const int octaveMin = 3;
    internal const int octaveMax = 8;
    [SerializeField, Range(octaveMin, octaveMax)]
    private int octave = 4;
    [JsonProperty]
    internal SynthVariableInt Octave;

    internal const int toneOffsetMin = 0;
    internal const int toneOffsetMax = 11;
    [SerializeField, Range(toneOffsetMin, toneOffsetMax)]
    private int toneOffset = 0;
    [JsonProperty]
    internal SynthVariableInt ToneOffset;


    internal const int subdivisionCountMin = 3;
    internal const int subdivisionCountMax = 8;
    [JsonProperty]
    internal SynthVariableInt SubdivisionCount;


    internal Dictionary<ModifiableVariables, float> minLimits = new Dictionary<ModifiableVariables, float>();
    internal Dictionary<ModifiableVariables, float> maxLimits = new Dictionary<ModifiableVariables, float>();
    internal void UpdateLimit(ModifiableVariables Variable, float minValue, float maxValue)
    {
        minLimits[Variable] = minValue;
        maxLimits[Variable] = maxValue;

        SetValue(Variable, GetValue(Variable));
    }

    internal static bool IsSynthSetting(ModifiableVariables variable)
    {
        return true; //variable != ModifiableVariables.SUBDIVISION_COUNT;
    }

    /// <summary>
    ///  Gets the min value of a synth variable
    /// </summary>
    /// <param name="xVar"></param>
    internal float GetMinValue(ModifiableVariables variableType)
    {
        float tmpMin = variableType switch
        {
            ModifiableVariables.OCTAVE => octaveMin,
            ModifiableVariables.VELOCITY => velocityMin,
            ModifiableVariables.DURATION => durationMin,
            ModifiableVariables.REVERB => reverbAmountMin,
            ModifiableVariables.AMP_ATTACK => ADSR.attackMin,
            ModifiableVariables.FILTER_ATTACK => ADSR.attackMin,
            ModifiableVariables.PITCH_ATTACK => ADSR.attackMin,
            ModifiableVariables.AMP_DECAY => ADSR.decayMin,
            ModifiableVariables.PITCH_DECAY => ADSR.decayMin,
            ModifiableVariables.FILTER_DECAY => ADSR.decayMin,
            ModifiableVariables.AMP_SUSTAIN => ADSR.sustainMin,
            ModifiableVariables.PITCH_SUSTAIN => ADSR.sustainMin,
            ModifiableVariables.FILTER_SUSTAIN => ADSR.sustainMin,
            ModifiableVariables.AMP_RELEASE => ADSR.releaseMin,
            ModifiableVariables.PITCH_RELEASE => ADSR.releaseMin,
            ModifiableVariables.FILTER_RELEASE => ADSR.releaseMin,
            ModifiableVariables.FILTER_FREQUENCY => filterFrequencyMin,
            ModifiableVariables.FILTER_RESONANCE => filterResonanceMin,
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => filterEnvelopeAmountMin,
            ModifiableVariables.FILTER_FREQUENCY_TYPE => 0,
            ModifiableVariables.DELAY_FEEDBACK => delayFeedbackMin,
            ModifiableVariables.DELAY_TIME => delayTimeMin,
            ModifiableVariables.TONEOFFSET => toneOffsetMin,
            ModifiableVariables.SUBDIVISION_COUNT => subdivisionCountMin,
            

            // special cases
            ModifiableVariables.WAVEFORM => 0,
            ModifiableVariables.ACTIVATIONMODE => 0,
            _ => throw new Exception("SHOULD NOT HAPPEN! was called with -> " + variableType.ToString()),
        };

        if (minLimits.ContainsKey(variableType))
        {
            tmpMin = Mathf.Max(tmpMin, minLimits[variableType]);
        }
        return tmpMin;
    }

    /// <summary>
    ///  Gets the max value of a synth variable
    /// </summary>
    /// <param name="var"></param>
    internal float GetMaxValue(ModifiableVariables variableType)
    {
        float tmpMax = variableType switch
        {
            ModifiableVariables.OCTAVE => octaveMax,
            ModifiableVariables.VELOCITY => velocityMax,
            ModifiableVariables.DURATION => durationMax,
            ModifiableVariables.REVERB => reverbAmountMax,
            ModifiableVariables.AMP_ATTACK => ADSR.attackMax,
            ModifiableVariables.FILTER_ATTACK => ADSR.attackMax,
            ModifiableVariables.PITCH_ATTACK => ADSR.attackMax,
            ModifiableVariables.AMP_DECAY => ADSR.decayMax,
            ModifiableVariables.PITCH_DECAY => ADSR.decayMax,
            ModifiableVariables.FILTER_DECAY => ADSR.decayMax,
            ModifiableVariables.AMP_SUSTAIN => ADSR.sustainMax,
            ModifiableVariables.PITCH_SUSTAIN => ADSR.sustainMax,
            ModifiableVariables.FILTER_SUSTAIN => ADSR.sustainMax,
            ModifiableVariables.AMP_RELEASE => ADSR.releaseMax,
            ModifiableVariables.PITCH_RELEASE => ADSR.releaseMax,
            ModifiableVariables.FILTER_RELEASE => ADSR.releaseMax,
            ModifiableVariables.FILTER_FREQUENCY => filterFrequencyMax,
            ModifiableVariables.FILTER_RESONANCE => filterResonanceMax,
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => filterEnvelopeAmountMax,
            ModifiableVariables.FILTER_FREQUENCY_TYPE => Enum.GetNames(typeof(FrequencyFilterTypeEnum)).Length - 1,
            ModifiableVariables.DELAY_FEEDBACK => delayFeedbackMax,
            ModifiableVariables.DELAY_TIME => delayTimeMax,
            ModifiableVariables.TONEOFFSET => toneOffsetMax,
            ModifiableVariables.SUBDIVISION_COUNT => subdivisionCountMax,

            // special cases
            ModifiableVariables.WAVEFORM => Enum.GetNames(typeof(AvailableWaveforms)).Length - 1,
            ModifiableVariables.ACTIVATIONMODE => Enum.GetNames(typeof(ActivationModeEnum)).Length - 1,
            _ => throw new Exception("SHOULD NOT HAPPEN"),
        };

        if (maxLimits.ContainsKey(variableType))
        {
            tmpMax = Mathf.Min(tmpMax, maxLimits[variableType]);
        }
        return tmpMax;
    }

    /// <summary>
    ///  Rnadomizes a modifiable variable
    /// </summary>
    /// <param name="variable"></param>
    internal void RandomizeVisualVariable(ModifiableVariables variable, float minAmount = 0f, float maxAmount = 1f)
    {
        SetVisualValue(variable, UnityEngine.Random.Range(minAmount, maxAmount));
    }


    private void UpdateInspectorValue(ModifiableVariables variable, float v)
    {
        switch (variable)
        {
            // == special cases:  ==
            case ModifiableVariables.OCTAVE:
                // round to nearest int
                octave = (int)v;
                break;
            case ModifiableVariables.TONEOFFSET:
                // round to nearest int
                toneOffset = (int)v;
                break;
            case ModifiableVariables.WAVEFORM:
                waveform = (AvailableWaveforms)v;
                break;
            // == The rest, should all be continous values for now.==
            case ModifiableVariables.DURATION:
                duration = v;
                break;
            case ModifiableVariables.DELAY_TIME:
                delayTime = v;
                break;
            case ModifiableVariables.FILTER_FREQUENCY:
                filterFrequency = v;
                break;
            case ModifiableVariables.FILTER_RESONANCE:
                filterResonanceAmount = v;
                break;
            case ModifiableVariables.REVERB:
                reverbAmount = v;
                break;
            case ModifiableVariables.AMP_ATTACK:
                AmplitudeADSR.attack = v;
                break;
            case ModifiableVariables.AMP_DECAY:
                AmplitudeADSR.decay = v;
                break;
            case ModifiableVariables.AMP_SUSTAIN:
                AmplitudeADSR.sustain = v;
                break;
            case ModifiableVariables.AMP_RELEASE:
                AmplitudeADSR.release = v;
                break;
            case ModifiableVariables.PITCH_ATTACK:
                PitchADSR.attack = v;
                break;
            case ModifiableVariables.PITCH_DECAY:
                PitchADSR.decay = v;
                break;
            case ModifiableVariables.PITCH_SUSTAIN:
                PitchADSR.sustain = v;
                break;
            case ModifiableVariables.PITCH_RELEASE:
                PitchADSR.release = v;
                break;
            case ModifiableVariables.FILTER_ATTACK:
                FilterADSR.attack = v;
                break;
            case ModifiableVariables.FILTER_DECAY:
                FilterADSR.decay = v;
                break;
            case ModifiableVariables.FILTER_SUSTAIN:
                FilterADSR.sustain = v;
                break;
            case ModifiableVariables.FILTER_RELEASE:
                FilterADSR.release = v;
                break;
            case ModifiableVariables.FILTER_ENVELOPE_AMOUNT:
                filterEnvelopeAmount = v;
                break;
            case ModifiableVariables.FILTER_FREQUENCY_TYPE:
                frequencyFilterType = (FrequencyFilterTypeEnum) v;
                break;
            case ModifiableVariables.DELAY_FEEDBACK:
                delayFeedback = v;
                break;
            case ModifiableVariables.VELOCITY:
                velocity = v;
                break;
            case ModifiableVariables.SUBDIVISION_COUNT:
                // do nothing
                break;
            case ModifiableVariables.ACTIVATIONMODE:
                // do nothing
                activationMode = (ActivationModeEnum) v;
                break;
            default:
                throw new Exception("did not match a case, should not happen?");
        }
    }

    internal SynthSettings CopySynthSettings(SynthSettings synthSettings, bool updateOctave = true)
    {
        synthSettings.InitSynthVariables();

        var newSynth = new SynthSettings(synthSettings.Velocity.GetValue(), synthSettings.Duration.GetValue());
        newSynth.InitSynthVariables();
        
        newSynth.AmplitudeADSR.Attack.SetValue(synthSettings.AmplitudeADSR.Attack.GetValue());
        newSynth.AmplitudeADSR.Decay.SetValue(synthSettings.AmplitudeADSR.Decay.GetValue());
        newSynth.AmplitudeADSR.Release.SetValue(synthSettings.AmplitudeADSR.Release.GetValue());
        newSynth.AmplitudeADSR.Sustain.SetValue(synthSettings.AmplitudeADSR.Sustain.GetValue());

        newSynth.PitchADSR.Attack.SetValue(synthSettings.PitchADSR.Attack.GetValue());
        newSynth.PitchADSR.Decay.SetValue(synthSettings.PitchADSR.Decay.GetValue());
        newSynth.PitchADSR.Release.SetValue(synthSettings.PitchADSR.Release.GetValue());
        newSynth.PitchADSR.Sustain.SetValue(synthSettings.PitchADSR.Sustain.GetValue());

        newSynth.FilterADSR.Attack.SetValue(synthSettings.FilterADSR.Attack.GetValue());
        newSynth.FilterADSR.Decay.SetValue(synthSettings.FilterADSR.Decay.GetValue());
        newSynth.FilterADSR.Release.SetValue(synthSettings.FilterADSR.Release.GetValue());
        newSynth.FilterADSR.Sustain.SetValue(synthSettings.FilterADSR.Sustain.GetValue());

        newSynth.ReverbAmount.SetValue(synthSettings.ReverbAmount.GetValue());
        newSynth.PitchStartNoteOffset.SetValue(synthSettings.PitchStartNoteOffset.GetValue());
        newSynth.PitchEndNoteOffset.SetValue(synthSettings.PitchEndNoteOffset.GetValue());
        newSynth.FilterFrequency.SetValue(synthSettings.FilterFrequency.GetValue());
        newSynth.FilterEnvelopeAmount.SetValue(synthSettings.FilterEnvelopeAmount.GetValue());
        newSynth.FilterResonanceAmount.SetValue(synthSettings.FilterResonanceAmount.GetValue());
        newSynth.FrequencyFilterType.SetValue(synthSettings.FrequencyFilterType.GetValue());

        newSynth.Waveform.SetValue(synthSettings.Waveform.GetValue());

        newSynth.DelayFeedback.SetValue(synthSettings.DelayFeedback.GetValue());
        newSynth.DelayTime.SetValue(synthSettings.DelayTime.GetValue());

        newSynth.ReverbRoomSize.SetValue(synthSettings.ReverbRoomSize.GetValue());
        newSynth.ReverbHiFreqDamping.SetValue(synthSettings.ReverbHiFreqDamping.GetValue());


        newSynth.EnableDelay = synthSettings.EnableDelay;
        newSynth.EnableReverb = synthSettings.EnableReverb;
        newSynth.EnableFrequencyModulation = synthSettings.EnableFrequencyModulation;
        newSynth.EnablePitchModulation = synthSettings.EnablePitchModulation;
        newSynth.frequencyFilterType = synthSettings.frequencyFilterType;

        newSynth.ToneOffset.SetValue(synthSettings.ToneOffset.GetValue());
        newSynth.SubdivisionCount.SetValue(synthSettings.SubdivisionCount.GetValue());
        newSynth.ActivationMode.SetValue(synthSettings.ActivationMode.GetValue());


        if (updateOctave) newSynth.Octave.SetValue(synthSettings.Octave.GetValue());
        else newSynth.Octave.SetValue(this.Octave.GetValue());

        newSynth.minLimits = synthSettings.minLimits;
        newSynth.maxLimits = synthSettings.maxLimits;

        // copy inspector values
#if UNITY_EDITOR
        newSynth.AmplitudeADSR.attack = synthSettings.AmplitudeADSR.Attack.GetValue();
        newSynth.AmplitudeADSR.decay = synthSettings.AmplitudeADSR.Decay.GetValue();
        newSynth.AmplitudeADSR.release = synthSettings.AmplitudeADSR.Release.GetValue();
        newSynth.AmplitudeADSR.sustain = synthSettings.AmplitudeADSR.Sustain.GetValue();

        newSynth.PitchADSR.attack = synthSettings.PitchADSR.Attack.GetValue();
        newSynth.PitchADSR.decay = synthSettings.PitchADSR.Decay.GetValue();
        newSynth.PitchADSR.release = synthSettings.PitchADSR.Release.GetValue();
        newSynth.PitchADSR.sustain = synthSettings.PitchADSR.Sustain.GetValue();

        newSynth.FilterADSR.attack = synthSettings.FilterADSR.Attack.GetValue();
        newSynth.FilterADSR.decay = synthSettings.FilterADSR.Decay.GetValue();
        newSynth.FilterADSR.release = synthSettings.FilterADSR.Release.GetValue();
        newSynth.FilterADSR.sustain = synthSettings.FilterADSR.Sustain.GetValue();

        newSynth.reverbAmount = synthSettings.ReverbAmount.GetValue();
        newSynth.pitchStartNoteOffset = synthSettings.PitchStartNoteOffset.GetValue();
        newSynth.pitchEndNoteOffset = synthSettings.PitchEndNoteOffset.GetValue();
        newSynth.filterFrequency = synthSettings.FilterFrequency.GetValue();
        newSynth.filterEnvelopeAmount = synthSettings.FilterEnvelopeAmount.GetValue();
        newSynth.filterResonanceAmount = synthSettings.FilterResonanceAmount.GetValue();
        newSynth.frequencyFilterType = (FrequencyFilterTypeEnum) synthSettings.FrequencyFilterType.GetValue();

        newSynth.waveform = (AvailableWaveforms) synthSettings.Waveform.GetValue();

        newSynth.delayFeedback = synthSettings.DelayFeedback.GetValue();
        newSynth.delayTime = synthSettings.DelayTime.GetValue();

        newSynth.reverbRoomSize = synthSettings.ReverbRoomSize.GetValue();
        newSynth.reverbHiFreqDamping = synthSettings.ReverbHiFreqDamping.GetValue();


        newSynth.EnableDelay = synthSettings.EnableDelay;
        newSynth.EnableReverb = synthSettings.EnableReverb;
        newSynth.EnableFrequencyModulation = synthSettings.EnableFrequencyModulation;
        newSynth.EnablePitchModulation = synthSettings.EnablePitchModulation;
        newSynth.frequencyFilterType = synthSettings.frequencyFilterType;

        newSynth.toneOffset = synthSettings.ToneOffset.GetValue();
        //newSynth.SubdivisionCount.SetValue(synthSettings.SubdivisionCount.GetValue());
        newSynth.activationMode = (ActivationModeEnum) synthSettings.ActivationMode.GetValue();

        if (updateOctave) newSynth.octave = synthSettings.Octave.GetValue();
        else newSynth.octave = this.Octave.GetValue();
#endif

        return newSynth;
    }

    internal void SetEffect(MushroomEffects synthEffect, bool enabled)
    {
        switch (synthEffect)
        {
            case MushroomEffects.FREQUENCY_FILTER:
                {
                    EnableFrequencyModulation = enabled;
                    break;
                }
            case MushroomEffects.PITCH:
                {
                    EnablePitchModulation = enabled;
                    break;
                }
            case MushroomEffects.DELAY:
                {
                    EnableDelay = enabled;
                    break;
                }
            case MushroomEffects.REPEATER:
                {
                    // do nothing because the changed activation is handled in plantbase
                    break;
                }
            default:
                {
                    Debug.Log("THIS SHOULD NOT HAPPEN");
                    break;
                }
        }
    }

    public float GetValue(ModifiableVariables variable)
    {
        if (!initialized) InitSynthVariables();

        return variable switch
        {
            ModifiableVariables.VELOCITY => Velocity.GetValue(),
            ModifiableVariables.DURATION => Duration.GetValue(),
            ModifiableVariables.REVERB => ReverbAmount.GetValue(),
            ModifiableVariables.AMP_ATTACK => AmplitudeADSR.Attack.GetValue(),
            ModifiableVariables.AMP_DECAY => AmplitudeADSR.Decay.GetValue(),
            ModifiableVariables.AMP_SUSTAIN => AmplitudeADSR.Sustain.GetValue(),
            ModifiableVariables.AMP_RELEASE => AmplitudeADSR.Release.GetValue(),
            ModifiableVariables.PITCH_ATTACK => PitchADSR.Attack.GetValue(),
            ModifiableVariables.PITCH_DECAY => PitchADSR.Decay.GetValue(),
            ModifiableVariables.PITCH_SUSTAIN => PitchADSR.Sustain.GetValue(),
            ModifiableVariables.PITCH_RELEASE => PitchADSR.Release.GetValue(),
            ModifiableVariables.FILTER_ATTACK => FilterADSR.Attack.GetValue(),
            ModifiableVariables.FILTER_DECAY => FilterADSR.Decay.GetValue(),
            ModifiableVariables.FILTER_SUSTAIN => FilterADSR.Sustain.GetValue(),
            ModifiableVariables.FILTER_RELEASE => FilterADSR.Release.GetValue(),
            ModifiableVariables.FILTER_FREQUENCY => FilterFrequency.GetValue(),
            ModifiableVariables.FILTER_RESONANCE => FilterResonanceAmount.GetValue(),
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => FilterEnvelopeAmount.GetValue(),
            ModifiableVariables.FILTER_FREQUENCY_TYPE => FrequencyFilterType.GetValue(),
            ModifiableVariables.DELAY_FEEDBACK => DelayFeedback.GetValue(),
            ModifiableVariables.DELAY_TIME => DelayTime.GetValue(),
            ModifiableVariables.OCTAVE => Octave.GetValue(),
            ModifiableVariables.NOTE => Note.GetValue(),
            ModifiableVariables.TONEOFFSET => ToneOffset.GetValue(),
            ModifiableVariables.SUBDIVISION_COUNT => SubdivisionCount.GetValue(),
            ModifiableVariables.ACTIVATIONMODE => ActivationMode.GetValue(),

            //special case
            ModifiableVariables.WAVEFORM => Waveform.GetValue(),
            
            _ => throw new Exception("Should not happen"),
        };
    }

    internal float GetVisualValue(ModifiableVariables variable)
    {
        if (!initialized) InitSynthVariables();

        return variable switch
        {
            ModifiableVariables.VELOCITY => Velocity.GetVisualValue(),
            ModifiableVariables.DURATION => Duration.GetVisualValue(),
            ModifiableVariables.REVERB => ReverbAmount.GetVisualValue(),
            ModifiableVariables.AMP_ATTACK => AmplitudeADSR.Attack.GetVisualValue(),
            ModifiableVariables.AMP_DECAY => AmplitudeADSR.Decay.GetVisualValue(),
            ModifiableVariables.AMP_SUSTAIN => AmplitudeADSR.Sustain.GetVisualValue(),
            ModifiableVariables.AMP_RELEASE => AmplitudeADSR.Release.GetVisualValue(),
            ModifiableVariables.PITCH_ATTACK => PitchADSR.Attack.GetVisualValue(),
            ModifiableVariables.PITCH_DECAY => PitchADSR.Decay.GetVisualValue(),
            ModifiableVariables.PITCH_SUSTAIN => PitchADSR.Sustain.GetVisualValue(),
            ModifiableVariables.PITCH_RELEASE => PitchADSR.Release.GetVisualValue(),
            ModifiableVariables.FILTER_ATTACK => FilterADSR.Attack.GetVisualValue(),
            ModifiableVariables.FILTER_DECAY => FilterADSR.Decay.GetVisualValue(),
            ModifiableVariables.FILTER_SUSTAIN => FilterADSR.Sustain.GetVisualValue(),
            ModifiableVariables.FILTER_RELEASE => FilterADSR.Release.GetVisualValue(),
            ModifiableVariables.FILTER_FREQUENCY => FilterFrequency.GetVisualValue(),
            ModifiableVariables.FILTER_RESONANCE => FilterResonanceAmount.GetVisualValue(),
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => FilterEnvelopeAmount.GetVisualValue(),
            ModifiableVariables.FILTER_FREQUENCY_TYPE => FrequencyFilterType.GetVisualValue(),
            ModifiableVariables.DELAY_FEEDBACK => DelayFeedback.GetVisualValue(),
            ModifiableVariables.DELAY_TIME => DelayTime.GetVisualValue(),
            ModifiableVariables.OCTAVE => Octave.GetVisualValue(),
            ModifiableVariables.TONEOFFSET => ToneOffset.GetVisualValue(),
            ModifiableVariables.SUBDIVISION_COUNT => SubdivisionCount.GetVisualValue(),
            ModifiableVariables.ACTIVATIONMODE => ActivationMode.GetVisualValue(),

            //special case
            ModifiableVariables.WAVEFORM => Waveform.GetVisualValue(),
            _ => throw new Exception("Should not happen"),
        };
    }

    public float SetValue(ModifiableVariables variable, float value)
    {
        if (!initialized) InitSynthVariables();

        value = Mathf.Clamp(value, GetMinValue(variable), GetMaxValue(variable));
        var newVal = variable switch
        {
            ModifiableVariables.VELOCITY => Velocity.SetValue(value),
            ModifiableVariables.DURATION => Duration.SetValue(value),
            ModifiableVariables.REVERB => ReverbAmount.SetValue(value),
            ModifiableVariables.AMP_ATTACK => AmplitudeADSR.Attack.SetValue(value),
            ModifiableVariables.AMP_DECAY => AmplitudeADSR.Decay.SetValue(value),
            ModifiableVariables.AMP_SUSTAIN => AmplitudeADSR.Sustain.SetValue(value),
            ModifiableVariables.AMP_RELEASE => AmplitudeADSR.Release.SetValue(value),
            ModifiableVariables.PITCH_ATTACK => PitchADSR.Attack.SetValue(value),
            ModifiableVariables.PITCH_DECAY => PitchADSR.Decay.SetValue(value),
            ModifiableVariables.PITCH_SUSTAIN => PitchADSR.Sustain.SetValue(value),
            ModifiableVariables.PITCH_RELEASE => PitchADSR.Release.SetValue(value),
            ModifiableVariables.FILTER_ATTACK => FilterADSR.Attack.SetValue(value),
            ModifiableVariables.FILTER_DECAY => FilterADSR.Decay.SetValue(value),
            ModifiableVariables.FILTER_SUSTAIN => FilterADSR.Sustain.SetValue(value),
            ModifiableVariables.FILTER_RELEASE => FilterADSR.Release.SetValue(value),
            ModifiableVariables.FILTER_FREQUENCY => FilterFrequency.SetValue(value),
            ModifiableVariables.FILTER_RESONANCE => FilterResonanceAmount.SetValue(value),
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => FilterEnvelopeAmount.SetValue(value),
            ModifiableVariables.FILTER_FREQUENCY_TYPE => FrequencyFilterType.SetValue((int) value),
            ModifiableVariables.DELAY_FEEDBACK => DelayFeedback.SetValue(value),
            ModifiableVariables.DELAY_TIME => DelayTime.SetValue(value),
            ModifiableVariables.OCTAVE => Octave.SetValue((int)value),
            ModifiableVariables.TONEOFFSET => ToneOffset.SetValue((int)value),
            ModifiableVariables.WAVEFORM => Waveform.SetValue((int)value),
            ModifiableVariables.SUBDIVISION_COUNT => SubdivisionCount.SetValue((int) value),
            ModifiableVariables.ACTIVATIONMODE => ActivationMode.SetValue((int) value),


            _ => throw new Exception("Should not happen"),
        };

        UpdateInspectorValue(variable, newVal);

        return newVal;
    }

    internal float SetVisualValue(ModifiableVariables variable, float value)
    {
        if (!initialized) InitSynthVariables();

        value = Mathf.Clamp(value, 0f, 1f);
        var newVal = variable switch
        {
            ModifiableVariables.VELOCITY => Velocity.SetVisualValue(value),
            ModifiableVariables.DURATION => Duration.SetVisualValue(value),
            ModifiableVariables.REVERB => ReverbAmount.SetVisualValue(value),
            ModifiableVariables.AMP_ATTACK => AmplitudeADSR.Attack.SetVisualValue(value),
            ModifiableVariables.AMP_DECAY => AmplitudeADSR.Decay.SetVisualValue(value),
            ModifiableVariables.AMP_SUSTAIN => AmplitudeADSR.Sustain.SetVisualValue(value),
            ModifiableVariables.AMP_RELEASE => AmplitudeADSR.Release.SetVisualValue(value),
            ModifiableVariables.PITCH_ATTACK => PitchADSR.Attack.SetVisualValue(value),
            ModifiableVariables.PITCH_DECAY => PitchADSR.Decay.SetVisualValue(value),
            ModifiableVariables.PITCH_SUSTAIN => PitchADSR.Sustain.SetVisualValue(value),
            ModifiableVariables.PITCH_RELEASE => PitchADSR.Release.SetVisualValue(value),
            ModifiableVariables.FILTER_ATTACK => FilterADSR.Attack.SetVisualValue(value),
            ModifiableVariables.FILTER_DECAY => FilterADSR.Decay.SetVisualValue(value),
            ModifiableVariables.FILTER_SUSTAIN => FilterADSR.Sustain.SetVisualValue(value),
            ModifiableVariables.FILTER_RELEASE => FilterADSR.Release.SetVisualValue(value),
            ModifiableVariables.FILTER_FREQUENCY => FilterFrequency.SetVisualValue(value),
            ModifiableVariables.FILTER_RESONANCE => FilterResonanceAmount.SetVisualValue(value),
            ModifiableVariables.FILTER_ENVELOPE_AMOUNT => FilterEnvelopeAmount.SetVisualValue(value),
            ModifiableVariables.FILTER_FREQUENCY_TYPE => FrequencyFilterType.SetVisualValue(value),
            ModifiableVariables.DELAY_FEEDBACK => DelayFeedback.SetVisualValue(value),
            ModifiableVariables.DELAY_TIME => DelayTime.SetVisualValue(value),
            ModifiableVariables.OCTAVE => Octave.SetVisualValue(value),
            ModifiableVariables.TONEOFFSET => ToneOffset.SetVisualValue(value),
            ModifiableVariables.WAVEFORM => Waveform.SetVisualValue(value),
            ModifiableVariables.SUBDIVISION_COUNT => SubdivisionCount.SetVisualValue(value),
            ModifiableVariables.ACTIVATIONMODE => ActivationMode.SetVisualValue(value),


            _ => throw new Exception("Should not happen"),
        };

        UpdateInspectorValue(variable, GetValue(variable));

        return newVal;
    }

    internal float GetMeanValue(ModifiableVariables value)
    {
        return GetMinValue(value) + ((GetMaxValue(value) - GetMinValue(value)) / 2);
    }

    internal int GetFilterFrequencyType()
    {
        switch (frequencyFilterType)
        {
            case FrequencyFilterTypeEnum.Lowpass:
                return 0;
            case FrequencyFilterTypeEnum.Highpass:
                return 1;
            case FrequencyFilterTypeEnum.Bandpass:
                return 2;
            default:
                return -1;
        }
    }
    public SynthSettings() // empty constructor for JSON
    {
    }

    public SynthSettings(float _velocity, float _duration = -1f, double _startTime = 0f, float _reverbAmount = 0f)
    {
        velocity = _velocity;
        duration = _duration;
        //Velocity.SetValue(_velocity);
        //Duration.SetValue(_duration);
    }
}
