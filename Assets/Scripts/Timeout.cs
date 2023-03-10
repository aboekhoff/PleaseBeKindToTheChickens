using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timeout { 
    public float duration;
    public float value;

    public Timeout(float duration)
    {
        this.duration = duration;
        this.value = 0f;
    }

    public void Reset()
    {
        value = duration;
    }

    public void Update(float time)
    {
        value -= time;
    }

    public bool IsTimedOut()
    {
        return value > 0f;
    }
}
