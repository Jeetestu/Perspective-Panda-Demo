using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundType : MonoBehaviour
{
    public enum Type { normal, flat, impassable, flatImpassable, rotator, finishZone, ice, flatIce}

    public Type typeOfGround;
}
