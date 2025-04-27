using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct PositionContainer
{
   public Vector2 spawnPosition;
   public Vector2 endPosition;
   public List<Vector2> intermedatePos;
}

public class BaseEnemy : MonoBehaviour
{
    [SerializeField] PositionContainer posData;
    [SerializeField] float health;
    [SerializeField] float range;
    [SerializeField] DropTypes dropType;
    [SerializeField] float speed = 3f;
    [SerializeField] bool isBoss = false;
    [SerializeField] GameObject m_DeathAni;
    [SerializeField] private float mDeathAniScale = 0;
    [SerializeField] private Sprite mDeathAniSprite;

    //this needs a sequencer
    Sequencer sqe;
    private Vector2 currentSelectedPositon;
    int currentIndex = 0;

    public bool isAtEnd = false;
    public bool isDead = false;
    private float despawnTimer = 5.0f;

    [Header("Sounds")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    //Bool to not play multiple bullet sfx sounds at once (this would make audio sound bad)
    private bool mPlayDamageSFX = true;

    void Start()
    {
        sqe = GetComponent<Sequencer>();
        //we are in the range of our array 
        if(currentIndex < posData.intermedatePos.Count)
        {
            currentSelectedPositon = posData.intermedatePos[currentIndex];
        }
        else
        {
            currentSelectedPositon = posData.endPosition;
        }
    }
    private void FixedUpdate()
    {
        DoMovement();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Bullet") && !isBoss)
        {
            //the player will have a damage value associated with it
            TakeDamage(PlayerAttacks.bulletDamage);

            Destroy(collision.gameObject);
        }
    }

    protected void TakeDamage(float damadge)
    {
        if (isDead) { return; }

        health -= damadge;
        if (health > 0 && mPlayDamageSFX) 
        {
            SoundManager.Instance.PlaySFXClip(damageSound, transform, 1f);
            mPlayDamageSFX = false;
            StartCoroutine(Co_ResetPlayDamageSFX());
        } 
        if (health <= 0)
        {
            SoundManager.Instance.PlaySFXClip(deathSound, transform, 1f);
            Destroy(gameObject.GetComponent<BoxCollider2D>());

            //trigger drop event
            DropEvent evt = new DropEvent(dropType);
            dropType.SetLocation(this.transform.position);
            EventSystem.fireEvent(evt);

            //turn the sprite render off before chaning the game state
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            isDead = true;

            GameObject deathAni = Instantiate(m_DeathAni, transform.position, Quaternion.identity);

            if (mDeathAniScale == 0)
            {
                deathAni.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.x);
            }
            else
            {
                deathAni.transform.localScale = new Vector3(mDeathAniScale, mDeathAniScale, mDeathAniScale);
            }

            if (mDeathAniSprite != null)
            {
                deathAni.GetComponent<SpriteRenderer>().sprite = mDeathAniSprite;
            }

            deathAni.transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0, 180));

            Debug.Log("Killing Enemy from Base enemey");
            Destroy(gameObject);
        }
    }

    private IEnumerator Co_ResetPlayDamageSFX()
    {
        yield return new WaitForSeconds(0.05f);
        mPlayDamageSFX = true;
    }

    public bool ShouldDestroy()
    {
        if (isAtEnd && sqe.GetIfAtEnd())
        {
            return true;
        }
        //this is so that if the player has been of screen for a long time then just remove it
        else if (isAtEnd && !sqe.GetIfAtEnd())
        {
            if(despawnTimer <= 0)
            {
                return true;
            }
            despawnTimer -= Time.deltaTime;
        }
        return false;
    }
    public void DoMovement() 
    {
        Vector2 currentPos = transform.position;

        //start the lerp
        Vector2 interpolatedPosition = Vector2.MoveTowards(currentPos, FindClosestPosition(), Time.deltaTime * speed);
        Vector2 diff = interpolatedPosition - new Vector2(transform.position.x, transform.position.y);
        transform.position += (Vector3)diff;

        if((Vector2)transform.position == posData.endPosition && !isBoss)
        {
            isAtEnd = true;
            isDead = false;
        }
    }

    private Vector2 FindClosestPosition()
    {
        //if the distance is to close then change to the next
        if (Vector2.Distance(transform.position, currentSelectedPositon) < range)
        {
            currentIndex++;

            if (currentIndex >= posData.intermedatePos.Count)
            {
                currentSelectedPositon = posData.endPosition;
                return currentSelectedPositon;
            }

            currentSelectedPositon = posData.intermedatePos[currentIndex];
            return currentSelectedPositon;
        }

        return currentSelectedPositon;
    }
    
    public void SetPositionContainer(PositionContainer cont)
    {
        posData = cont;
    }
    public PositionContainer GetPosData()
    {
        return posData;
    }

    private void OnDestroy()
    {
        //sequener should be clearing its own attacks
        //if(sqe != null)
        //{
        //    sqe.ClearAttackList();
        //    sqe.CleanSequencer();
        //}
    }
}
