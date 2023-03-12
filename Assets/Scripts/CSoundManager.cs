using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class CSoundManager : MonoBehaviour
{
    public static CSoundManager instance;
    CsoundUnity csound;
    NumberFormatInfo nfi;

    public bool debugNotesPlayed = false;
    public Dictionary<int, int> played_notes = new Dictionary<int, int>();

    public static readonly int MAX_INSTANCE_OF_GLOBAL_EFFECTS = 4;

    public readonly int MAX_GLOBAL_DELAY_ID = MAX_INSTANCE_OF_GLOBAL_EFFECTS;
    public List<DelayDTO> delays = new List<DelayDTO>();
    int globalDelayId = 0;
    private readonly int DELAY_INSTRUMENT_ID = 98;


    public readonly int MAX_GLOBAL_REVERB_ID = MAX_INSTANCE_OF_GLOBAL_EFFECTS;
    public List<ReverbDTO> reverbs = new List<ReverbDTO>();
    int globalReverbId = 1; // id 0 is reserved for distance plants
    private readonly int REVERB_INSTRUMENT_ID = 99;

    [Range(0, 1)]
    public float Volume = 0.5f;

    AudioSource audioSrc;

    // Start is called before the first frame update
    private Transform playerTransform;

    [SerializeField, Range(1, 100f)]
    private float distanceOfSound = 16f;

    public float DistanceOfSound
    {
        get => distanceOfSound;
        set => distanceOfSound = Math.Max(1, Mathf.Min(value, 100));
    }

    public bool Mute = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var UUID = 0;
        for (int i = 0; i < 127; i++)
        {
            played_notes[i] = (UUID++);
        }
        csound = GetComponent<CsoundUnity>();
        audioSrc = GetComponent<AudioSource>();

        nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        audioSrc.volume = Volume;
    }


    [Range(0.0f, 0.35f)]
    public float maxDirectionalFactor = 0.2f;

    public void PlayNote(int note, SynthSettings synthSettings, Vector3 soundPosition, float lowPassFreq = 20000f, float playOffset = 0, bool spatialSound = true, bool playIndefinitely = false, float? overwriteDuration = null)
    {
        synthSettings.InitSynthVariables();
        if (Mute) return;

        var volumeFactor = spatialSound ?
            SoundMath.LinearScaleVolumeFromPosition(soundPosition, playerTransform.position, DistanceOfSound)
            : 1;

        (var leftSideVolume, var rightSideVolume) = spatialSound ?
            SoundMath.CalculateLeftAndRightVolumeFactorFromPositions(soundPosition, playerTransform.position, maxDirectionalFactor) :
            (1, 1);

        if (volumeFactor <= 0) return;

        var delayDTO = GetDelay(synthSettings);
        var reverbDTO = GetReverb(synthSettings);

        var reverbAmount = 1 - volumeFactor;
        var reverbId = 0;

        var id = played_notes[note];

        var pitchInitialNote = note + synthSettings.PitchStartNoteOffset.GetValue();
        var pitchEndNote = note + synthSettings.PitchEndNoteOffset.GetValue();

        var duration = (playIndefinitely ? -1f : (overwriteDuration.HasValue ? overwriteDuration.Value : synthSettings.Duration.GetValue()));

        var str =
            $"i1.{id} " +
            $"{playOffset.ToString(nfi)} " + //p2 playOffset
            $"{duration.ToString(nfi)} " + //p3
            $"{(((int) Mathf.Floor(note / 12) + 2) + (note % 12) / 100f).ToString(nfi)} " + //p4
            $"{(synthSettings.Velocity.GetValue() * volumeFactor).ToString(nfi)} " + //p5
            $"{reverbAmount.ToString(nfi)} " + //p6
            $"{synthSettings.AmplitudeADSR.Attack.GetValue().ToString(nfi)} " + //p7
            $"{synthSettings.AmplitudeADSR.Decay.GetValue().ToString(nfi)} " + //p8
            $"{synthSettings.AmplitudeADSR.Sustain.GetValue().ToString(nfi)} " + //p9
            $"{synthSettings.AmplitudeADSR.Release.GetValue().ToString(nfi)} " + //p10
            $"{(synthSettings.Waveform.GetValue()) + 1} " + //p11
            $"{lowPassFreq.ToString(nfi)} " + //p12

            $"{synthSettings.PitchADSR.Attack.GetValue().ToString(nfi)} " + //p13
            $"{synthSettings.PitchADSR.Decay.GetValue().ToString(nfi)} " + //p14
            $"{synthSettings.PitchADSR.Sustain.GetValue().ToString(nfi)} " + //p15
            $"{synthSettings.PitchADSR.Release.GetValue().ToString(nfi)} " + //p16

            $"{synthSettings.FilterADSR.Attack.GetValue().ToString(nfi)} " + //p17
            $"{synthSettings.FilterADSR.Decay.GetValue().ToString(nfi)} " + //p18
            $"{synthSettings.FilterADSR.Sustain.GetValue().ToString(nfi)} " + //p19
            $"{synthSettings.AmplitudeADSR.Release.GetValue().ToString(nfi)} " + //p20 // hacking the system, because filteradsr sucks
            $"{synthSettings.FilterFrequency.GetValue().ToString(nfi)} " + //p21
            $"{synthSettings.FilterResonanceAmount.GetValue().ToString(nfi)} " +  //p22

            $"{leftSideVolume.ToString(nfi)} " + // p23
            $"{rightSideVolume.ToString(nfi)} " + // p24
            $"{(((int) Mathf.Floor(pitchInitialNote / 12) + 2) + (pitchInitialNote % 12) / 100f).ToString(nfi)} " + // p25
            $"{(((int) Mathf.Floor(pitchEndNote / 12) + 2) + (pitchEndNote % 12) / 100f).ToString(nfi)} " + // p26s
            $"{synthSettings.FilterEnvelopeAmount.GetValue().ToString(nfi)} " +  // p27

            $"{(!synthSettings.EnableDelay ? -1 : delayDTO.DelayId)} " +  // p28

            $"{(synthSettings.EnablePitchModulation ? 1 : 0)} " + // p29
            $"{(synthSettings.EnableFrequencyModulation ? 1 : 0)} " + // p30
            $"{synthSettings.GetFilterFrequencyType()} " +  // p31

            $"{(reverbId)}";  // p32

        if (debugNotesPlayed) Debug.Log(str);
        csound.SendScoreEvent(str);
    }


    private DelayDTO GetDelay(SynthSettings synthSettings)
    {
        if (!synthSettings.EnableDelay) return null;

        var existingDelayDTO = delays.Where(d => d.Feedback == synthSettings.DelayFeedback.GetValue() && d.DelayTime == synthSettings.DelayTime.GetValue()).FirstOrDefault();

        if (existingDelayDTO != null)
        {
            return existingDelayDTO;
        }

        // maximum of globalDelayId reached, start overwriting existing delays
        if (globalDelayId == MAX_GLOBAL_DELAY_ID)
        {
            globalDelayId = 0;
        }
        // check if a delayDTO exists with the same global id
        var delayDTOWithId = delays.Where(d => d.DelayId == globalDelayId).FirstOrDefault();
        // if ye then remove it from list
        if (delayDTOWithId != null) delays.Remove(delayDTOWithId);


        var delayDTO = new DelayDTO()
        {
            Feedback = synthSettings.DelayFeedback.GetValue(),
            DelayTime = synthSettings.DelayTime.GetValue(),
            DelayId = globalDelayId
        };
        // create new delay instrument
        var scoreEvent = $"i{DELAY_INSTRUMENT_ID}.{globalDelayId} " +
            $"0 " +
            $"-1 " +
            $"{globalDelayId} " +
            $"{synthSettings.DelayFeedback.GetValue().ToString(nfi)} " +
            $"{synthSettings.DelayTime.GetValue().ToString(nfi)}";
        csound.SendScoreEvent(scoreEvent);

        // IMPORTANT: increment globalDelayId
        globalDelayId += 1;

        delays.Add(delayDTO);
        return delayDTO;
    }

    private ReverbDTO GetReverb(SynthSettings synthSettings)
    {
        if (!synthSettings.EnableReverb) return null;

        var existingDelayDTO = reverbs.Where(d => d.Roomsize == synthSettings.ReverbRoomSize.GetValue() && d.HiFreqDamping == synthSettings.ReverbHiFreqDamping.GetValue()).FirstOrDefault();

        if (existingDelayDTO != null)
        {
            return existingDelayDTO;
        }

        // maximum of globalDelayId reached, start overwriting existing delays
        if (globalReverbId == MAX_GLOBAL_DELAY_ID)
        {
            globalReverbId = 0;
        }
        // check if a delayDTO exists with the same global id
        var reverbDTOWithId = reverbs.Where(d => d.ReverbId == globalReverbId).FirstOrDefault();
        // if ye then remove it from list
        if (reverbDTOWithId != null) reverbs.Remove(reverbDTOWithId);


        var reverbDTO = new ReverbDTO()
        {
            Roomsize = synthSettings.ReverbRoomSize.GetValue(),
            HiFreqDamping = synthSettings.ReverbHiFreqDamping.GetValue(),
            ReverbId = globalReverbId
        };
        // create new delay instrument
        var scoreEvent = $"i{REVERB_INSTRUMENT_ID}.{globalReverbId} " +
            $"0 " +
            $"-1 " +
            $"{globalReverbId} " +
            $"{synthSettings.ReverbRoomSize.GetValue().ToString(nfi)} " +
            $"{synthSettings.ReverbHiFreqDamping.GetValue().ToString(nfi)}";
        csound.SendScoreEvent(scoreEvent);

        // IMPORTANT: increment globalDelayId
        globalReverbId += 1;

        reverbs.Add(reverbDTO);
        return reverbDTO;
    }

    public void StopNote(float note)
    {
        var octave = Math.Floor(note / 12) + 2;
        var id = played_notes[(int) note];

        var str = $"i-1.{id} 0 1 {(8.0f + (note % 12) / 100).ToString(nfi)} 0.1 0";
        if (debugNotesPlayed) Debug.Log(str);
        csound.SendScoreEvent(str);
    }

    public void PlayNoteRealtime(float note, float velocity, float duration)
    {
        float secDuration = -1f;

        var octave = Math.Floor(note / 12) + 2;


        var str = $"i1.{180} 0 {(secDuration).ToString(nfi)} {(octave + (note % 12) / 100).ToString(nfi)} {velocity.ToString(nfi)}";
        if (debugNotesPlayed) Debug.Log(str);
        csound.SendScoreEvent(str);
    }
}
[Serializable]
public abstract class EffectDTO
{
}
[Serializable]
public class DelayDTO : EffectDTO
{
    public float Feedback { get; set; } = 0.5f;

    public float DelayTime { get; set; } = 0.2f;

    public int DelayId { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is DelayDTO otherDelayDTO)
        {
            return Feedback == otherDelayDTO.Feedback && DelayTime == otherDelayDTO.DelayTime;
        }
        return false;
    }
}

[Serializable]
public class ReverbDTO : EffectDTO
{
    // between 0 and 1
    public float Roomsize { get; set; } = 0.5f;

    // between 0 and 1
    public float HiFreqDamping { get; set; } = 0.2f;

    public int ReverbId { get; set; }
}