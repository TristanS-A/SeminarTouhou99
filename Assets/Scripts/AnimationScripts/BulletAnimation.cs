using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAnimation : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float changeTime = 0.5f;
    [SerializeField] SpriteRenderer spriteRenderer;
    float currentTime;
    bool flipSprite = false;
    // Start is called before the first frame update
    void Start()
    { 
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if(currentTime <= 0)
        {
            currentTime = changeTime;

            int index = flipSprite ? 1 : 0;
            //change the sprite
            spriteRenderer.sprite = sprites[index];

            flipSprite = !flipSprite;
        }
        currentTime -= Time.deltaTime;
    }
}
