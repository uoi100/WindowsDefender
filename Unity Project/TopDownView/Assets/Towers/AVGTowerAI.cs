using UnityEngine;
using System.Collections;

public class AVGTowerAI : TowerAI {

    AVGTowerAI() : base()
    {
        towerDamage = 50;
        attackSpd = 0.5f;
        attacksGround = true;
        attacksAir = true;
        drainDamage = 20;
        drainSpd = 0.8f;
        drainDuration = 2.0f;
    }

    // End of Function
}
