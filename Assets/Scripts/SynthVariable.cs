using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Newtonsoft.Json;

[Serializable]
public class SynthVariable
{
    internal SynthVariableVisualType VisualType;

    internal float minValue;
    internal float maxValue;

    [JsonProperty]
    private float val;
    private float Value
    {
        get => val;
        set => val = Mathf.Clamp(value, minValue, maxValue);
    }
    private float visualValue;
    private float VisualValue
    {
        get => visualValue;
        set => visualValue = Mathf.Clamp(value, 0, 1);
    }

    public float SetValue(float _value)
    {
        _value = Mathf.Clamp(_value, minValue, maxValue);
        Value = _value;
        switch (VisualType)
        {
            case SynthVariableVisualType.Linear:
                {
                    VisualValue = (Value - minValue) / (maxValue - minValue);
                    break;
                }
            case SynthVariableVisualType.Exponential:
                {
                    VisualValue = Value == minValue ? 0f : Mathf.Pow(_value, 1 / GlobalVariables.ExpFactor);
                    break;
                }
            case SynthVariableVisualType.TimeSteps:
                {
                    float index = (float)SynthSettings.delayTimes.IndexOf(Value);
                    VisualValue = index / (SynthSettings.delayTimes.Count() - 1);
                    break;
                }
            default:
                {
                    throw new Exception("IMPLEMENT THIS BRO");
                }
        }
        return Value;

    }
    public float GetValue()
    {
        return Value;
    }

    public float SetVisualValue(float _visualValue)
    {
        VisualValue = _visualValue;
        switch (VisualType)
        {
            case SynthVariableVisualType.Linear:
                {
                    Value = minValue + (maxValue - minValue) * VisualValue;

                    return VisualValue;
                }
            case SynthVariableVisualType.Exponential:
                {
                    Value = Mathf.Pow(VisualValue, GlobalVariables.ExpFactor);
                    return VisualValue;
                }
            case SynthVariableVisualType.TimeSteps:
                {
                    var delayTimes = SynthSettings.delayTimes;
                    var min = 0;
                    var max = delayTimes.Count() - 1;

                    //VisualValue = Mathf.Round(_visualValue * delayTimes.Count()) / delayTimes.Count();

                    //Value = delayTimes[Mathf.Clamp(Mathf.RoundToInt(_visualValue * (delayTimes.Count() - 1)), 0, delayTimes.Count() - 1)]; // clamp or else index out of bounds

                    VisualValue = Mathf.Round(_visualValue * (max - min)) / (max - min);
                    Value = delayTimes[Mathf.RoundToInt(min + (max - min) * VisualValue)];

                    return VisualValue;
                }
            default:
                {
                    throw new Exception("Should never happen");
                }
        };
    }

    public float GetVisualValue()
    {
        return VisualValue;
    }

    public SynthVariable()
    {
    }

    public SynthVariable(float startVal, float _minVal, float _maxVal, SynthVariableVisualType visualType = SynthVariableVisualType.Linear)
    {
        minValue = _minVal;
        maxValue = _maxVal;
        VisualType = visualType;

        SetValue(startVal);
    }
}
