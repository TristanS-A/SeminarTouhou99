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
    //this needs a sequencer
    Sequencer sqe;
    private Vector2 currentSelectedPositon;
    int currentIndex = 0;
    // Start is called before the first frame update
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
        
        //trigger drop event
        DropEvent evt = new DropEvent(dropType);
        dropType.SetLocation(this.transform.position);
        eventSystem.fireEvent(evt);
    }
    private void FixedUpdate()
    {
        DoMovement();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Bullet"))
        {
            //this will need to be changed at some point
            //the player will have a damadge value associated with it
            TakeDamadge(1);
            Destroy(collision.gameObject);
        }
    }

    protected void TakeDamadge(float damadge)
    {
        health -= damadge;

        if(health <= 0)
        {
            //player killed enemy

            //trigger drop event
            DropEvent evt = new DropEvent(dropType);

            //make sure to save the position of the object to that it does not spawn in some weird place
            
            dropType.SetLocation(this.transform.position);
            eventSystem.fireEvent(evt);

            Destroy(this.gameObject);
        }
    }
    public void DoMovement() 
    {
        Vector2 currentPos = transform.position;

        //Vector2 direction = currentPos - FindClosestPosition();
        //start the lerp
        Vector2 interpolatedPosition = Vector2.MoveTowards(currentPos, FindClosestPosition(), Time.deltaTime * 3f);
        Vector2 diff = interpolatedPosition - new Vector2(transform.position.x, transform.position.y);
        transform.position += (Vector3)diff;

        //transform.Translate(interpolatedPosition);
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


    //when we distroy the object make sure to clear the squ so to that the attacks clear
    private void OnDestroy()
    {
        sqe.ClearAttackList();
        sqe.CleanSequencer();
    }

}
