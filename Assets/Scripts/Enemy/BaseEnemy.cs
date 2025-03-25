using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
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
    //this needs a sequencer
    Sequencer sqe;
    private Vector2 currentSelectedPositon;
    int currentIndex = 0;

    public bool isAtEnd = false;
    void Start()
    {
        //if (sqe = gameObject.GetComponent<Sequencer>())
        //{
        //    //found the sequencer
        //    Debug.Log("found sequencer");

        //}
        //else
        //{
        //    //we did not have a sequencer --> might have issues becasue it is full of nothing!!
        //    sqe = gameObject.AddComponent<Sequencer>();
        //}
        
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
        if(collision.CompareTag("Bullet"))
        {
            Debug.LogWarning("Got Hit by somthing I was not supposed to");
            //this will need to be changed at some point
            //the player will have a damadge value associated with it
            TakeDamadge(1);
            //trigger drop event
            DropEvent evt = new DropEvent(dropType);
            dropType.SetLocation(this.transform.position);
            EventSystem.fireEvent(evt);

            Destroy(collision.gameObject);
        }
    }

    protected void TakeDamadge(float damadge)
    {

        health -= damadge;

        if(health <= 0)
        {
            //player killed enemy

            Debug.Log("CALLED A DROP EVENT");
            //trigger drop event
            DropEvent evt = new DropEvent(dropType);

            //make sure to save the position of the object to that it does not spawn in some weird place
            
            dropType.SetLocation(this.transform.position);
            EventSystem.fireEvent(evt);

            Destroy(this.gameObject);
        }
    }
    public void DoMovement() 
    {
        Vector2 currentPos = transform.position;

        //Vector2 direction = currentPos - FindClosestPosition();
        //start the lerp
        Vector2 interpolatedPosition = Vector2.MoveTowards(currentPos, FindClosestPosition(), Time.deltaTime * speed);
        Vector2 diff = interpolatedPosition - new Vector2(transform.position.x, transform.position.y);
        transform.position += (Vector3)diff;

        //transform.Translate(interpolatedPosition);

        if((Vector2)transform.position == posData.endPosition)
        {
            Debug.Log("ReachedEnd");
            isAtEnd = true;
        }
        Debug.Log(gameObject.name + " : object position " + transform.position + " endpos: " + posData.endPosition);
    }

    //all these returns statments suck change them to not that
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


    //when we distroy the object make sure to clear the squ so to that the attacks clear
    private void OnDestroy()
    {
        if(sqe != null)
        {
            sqe.ClearAttackList();
            sqe.CleanSequencer();
        }
       
    }

    
}
