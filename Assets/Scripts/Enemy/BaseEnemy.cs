using System;
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
        health -= damadge;
        if (health > 0) 
        {
            SoundManager.Instance.PlaySFXClip(damageSound, transform, 1f);
        } 
        if (health <= 0)
        {
            SoundManager.Instance.PlaySFXClip(deathSound, transform, 1f);

            //trigger drop event
            DropEvent evt = new DropEvent(dropType);
            dropType.SetLocation(this.transform.position);
            EventSystem.fireEvent(evt);

            //turn the sprite render off before chaning the game state
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject, 1f);
        }
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


    //when we destroy the object make sure to clear the sequencer so to that the attacks clear
    private void OnDestroy()
    {
        if(sqe != null)
        {
            sqe.ClearAttackList();
            sqe.CleanSequencer();
        }
    }
}
