using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInputBase : MonoBehaviour
{
    public abstract float GetRotationInput();
    public abstract bool GetThrustInput();
    public abstract bool GetFireInput();
    public abstract bool GetHyperspaceInput();
    public abstract bool AnyInputThisFrame { get; }
}
