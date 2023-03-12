using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SynthVariableInt
{
    internal float minValue;
    internal float maxValue;

    [JsonProperty]
    private int val;

    private int Value
    {
        get => val;
        set => val = (int)Mathf.Clamp(value, minValue, maxValue);
    }
    private float visualValue;
    private float VisualValue
    {
        get => visualValue;
        set => visualValue = Mathf.Clamp(value, 0, 1);
    }
    internal SynthVariableVisualType VisualType;

    public int SetValue(int val)
    {
        Value = val;
        VisualValue = (Value - minValue) / (maxValue - minValue);
        return Value;
    }

    public int GetValue()
    {
        return Value;
    }

    public float SetVisualValue(float _visualValue)
    {
        switch (VisualType)
        {
            case SynthVariableVisualType.Linear:
                {
                    VisualValue = _visualValue;
                    Value = (int)(minValue + (maxValue - minValue) * VisualValue);
                    return VisualValue;
                }
            case SynthVariableVisualType.Exponential:
                {
                    VisualValue = Mathf.Clamp(_visualValue, 0, 1);
                    Value = (int)Mathf.Pow(VisualValue, GlobalVariables.ExpFactor);
                    return VisualValue;
                }
            case SynthVariableVisualType.Steps:
                {
                    VisualValue = Mathf.Round(_visualValue * (maxValue - minValue)) / (maxValue - minValue);
                    Value = Mathf.RoundToInt(minValue + (maxValue - minValue) * VisualValue);
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

    public SynthVariableInt(int startVal, int _minVal, int _maxVal, SynthVariableVisualType visualType = SynthVariableVisualType.Linear)
    {

        minValue = _minVal;
        maxValue = _maxVal;
        VisualType = visualType;

        SetValue(startVal);
    }
}
