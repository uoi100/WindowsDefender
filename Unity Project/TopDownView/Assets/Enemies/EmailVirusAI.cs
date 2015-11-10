using UnityEngine;
using System.Collections;

public class EmailVirusAI : EnemyAI {

    public int hitCount;
	// Use this for initialization
	void Start () {
        hitCount = 0;
        health = 9999;
	}

    protected override void OnCollide(projectileAI projectile)
    {
        hitCount++;

        if (hitCount == 2)
            health = 0;

        Destroy(projectile.gameObject);
    }
}
