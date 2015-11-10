using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HijackerEnemyAI : EnemyAI {
    public float hijackRange = 3;

    public override EnemyAI[] OnDeath()
    {
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
        LinkedList<EnemyAI> aiguys = gc.getEnemyWithinRange(transform, hijackRange);
        foreach (EnemyAI aiguy in aiguys)
        {
            aiguy.movementSpeed *= 2;
        }

        return base.OnDeath();
    }
}
