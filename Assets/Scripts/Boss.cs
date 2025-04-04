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

    protected override void Init()
    {
        base.Init();
        mTimeTracker = Time.time;

        //Orders list by shortest completion time
        completionTimeBonusList.OrderBy(timeBonus => (-1 * timeBonus.timeToCompleteIn));
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
}
