using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss : TempEnemy
{
    [Serializable]
    private struct CompletionTimeBonus
    {
        public int bonusPoints;
        public float timeToCompleteIn;
    }

    [SerializeField] List<CompletionTimeBonus> completionTimeBonusList = new();

    private float mTimeTracker = 0;
    [SerializeField] private PositionContainer positions = new();
    Vector3 targetPosition;
    int currentPositionIndex = 0;
    GameObject player;
    bool isMoving = false;
    protected override void Init()
    {
        base.Init();
        mTimeTracker = Time.time;

        //Orders list by shortest completion time
        completionTimeBonusList.OrderBy(timeBonus => (-1 * timeBonus.timeToCompleteIn));

        StageComplete += SpecialMoveHandeler;

        targetPosition = positions.intermedatePos[currentPositionIndex];
        currentPositionIndex = -1;

        player = GameObject.FindWithTag("Player");
    }
    private void FixedUpdate()
    {
       
        if (isMoving)
        {
            //LookAtPlayer();
            DoMovement();
        }
    }

    void LookAtPlayer()
    {
        if(player != null)
        { 

            transform.LookAt(Vector3.up, player.transform.position);

        }
        
    }

    protected override void Kill()
    {
        HandleBonuses(Time.time - mTimeTracker);
        base.Kill();
    }

    private void HandleBonuses(float completionTime)
    {
        for (int i = 0; i < completionTimeBonusList.Count; i++)
        {
            if (completionTime <= completionTimeBonusList[i].timeToCompleteIn)
            {
                //Debug.Log("Completion TIME: " + completionTime);
                PlayerInfo.PlayerTimeBonus += completionTimeBonusList[i].bonusPoints;
                return;
            }
        }
    }

    private void SpecialMoveHandeler()
    {
        switch(currentStage)
        {
            case 2:
                FinalStageMove();
                break;
        }
    }

    void DoMovement()
    {
        float dist = Vector3.Distance(this.transform.position, targetPosition);

        if (dist < 0.5)
        {
            //change this to a current index or somthing
            currentPositionIndex++;

            if (currentPositionIndex >= positions.intermedatePos.Count)
            {
                currentPositionIndex = 0;
            }

            targetPosition = positions.intermedatePos[currentPositionIndex];
            StartCoroutine(Co_CheckDistance());

            Debug.Log("We are changing position");

        }

        Vector2 interpolatedPosition = Vector2.MoveTowards(transform.position, targetPosition, Time.deltaTime * 1.5f);
        Vector2 diff = interpolatedPosition - new Vector2(transform.position.x, transform.position.y);
        transform.position += (Vector3)diff;


    }
    IEnumerator Co_CheckDistance()
    {
        isMoving = false;
        yield return new WaitForSeconds(1.5f);
        isMoving = true;
    }
    private void FinalStageMove()
    {
        isMoving = true;

    }
}
