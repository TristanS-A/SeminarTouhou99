using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern : MonoBehaviour
{
    [SerializeField] public GameObject bullet;
    protected List<BaseBullet> bullets = new List<BaseBullet>();

    public void ClearBullets()
    {
        foreach(var bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        bullets.Clear();
        
    }
    private void OnDestroy()
    {
        ClearBullets();
    }
}
