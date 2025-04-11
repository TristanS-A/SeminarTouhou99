using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerSprite : MonoBehaviour
{
    [SerializeField] PlayerMovement PlayerMovement;
    // Start is called before the first frame update
    [SerializeField] List<Sprite> sprites = new();
    SpriteRenderer render;
    bool isSecondSprite = false;
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    private void FixedUpdate()
    {
        RotateAsset();
        SwitchSprite();
    }

    private void RotateAsset()
    {
        this.transform.Rotate(Vector3.forward, 100 *  Time.deltaTime);

    }

    private void SwitchSprite()
    {   
        int index = PlayerMovement.IsInFocusTime() ? 1 : 0;
        render.sprite = sprites[index];
    }

}
