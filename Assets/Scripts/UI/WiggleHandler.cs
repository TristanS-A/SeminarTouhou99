using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiggleHandler : MonoBehaviour
{
    [SerializeField] private float mWiggleStrength = 1;
    [SerializeField] private float mWiggleSpeed = 1;
    private const float BASE_SPEED = 1f;
    private float mStartingAngle = 0;

    // Start is called before the first frame update
    void Start()
    {
        mStartingAngle = transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        float newAngle = mStartingAngle + Mathf.Sin(Time.time * BASE_SPEED * mWiggleSpeed) * mWiggleStrength;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, newAngle);
    }
}
