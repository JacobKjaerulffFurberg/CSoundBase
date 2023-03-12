using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ADSR
{
    internal const float attackMin = 0.001f;
    internal const float attackMax = 1f;
    [SerializeField, Range(attackMin, attackMax)]
    internal float attack = .001f;

    [JsonProperty]
    internal SynthVariable Attack;

    internal const float decayMin = 0.001f;
    internal const float decayMax = 1f;
    [SerializeField, Range(decayMin, decayMax)]
    internal float decay = 0.001f;

    [JsonProperty]
    internal SynthVariable Decay;

    internal const float sustainMin = 0f;
    internal const float sustainMax = 1f;
    [SerializeField, Range(sustainMin, sustainMax)]
    internal float sustain = 1f;

    [JsonProperty]
    internal SynthVariable Sustain;

    internal const float releaseMin = 0.0001f;
    internal const float releaseMax = 1f;
    [SerializeField, Range(releaseMin, releaseMax)]
    internal float release = 0.0001f;

    [JsonProperty]
    internal SynthVariable Release;

    public ADSR()
    {
    }

    public ADSR(float attack, float decay, float sustain, float release)
    {
    }

    public void Init()
    {
        Attack = new SynthVariable(attack, attackMin, attackMax, SynthVariableVisualType.Exponential);
        Decay = new SynthVariable(decay, decayMin, decayMax, SynthVariableVisualType.Exponential);
        Sustain = new SynthVariable(sustain, sustainMin, sustainMax);
        Release = new SynthVariable(release, releaseMin, releaseMax, SynthVariableVisualType.Exponential);
    }
}
