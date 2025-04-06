using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXHandler : MonoBehaviour
{
    public enum VFXType
    {
        OFFENSIVE_BOMB,
        DEFFENSIVE_BOMB
    }

    [SerializeField] private VFXType mType;

    // Start is called before the first frame update
    void Start()
    {
        switch (mType)
        {
            case VFXType.OFFENSIVE_BOMB:
                Destroy(gameObject, GetComponent<VisualEffect>().GetFloat("Lifetime"));
                break;
            case VFXType.DEFFENSIVE_BOMB:
                StartCoroutine(Co_StupidHelperFunctionToSetAValueYouCantInVFXGraph(GetComponent<VisualEffect>().GetFloat("Lifetime")));
                break;
        }
    }

    private IEnumerator Co_StupidHelperFunctionToSetAValueYouCantInVFXGraph(float lifetime)
    {
        yield return new WaitForSeconds(lifetime * 0.5f);
        GetComponent<VisualEffect>().SetFloat("InnerSpawnCount", 0);
        Destroy(gameObject, lifetime);
    }
}
