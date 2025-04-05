using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    public void DestroyAnimationObject()
    { 
        Destroy(gameObject);
    }

    public void DestroyAnimationComponent()
    {
        Destroy(GetComponent<Animator>());
    }
}
