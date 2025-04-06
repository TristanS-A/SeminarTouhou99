using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, GetComponent<VisualEffect>().GetFloat("Lifetime"));
    }
}
