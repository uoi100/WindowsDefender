using UnityEngine;
using System.Collections;

public class projectileAI : MonoBehaviour {
    public GameObject target;
    public Vector2 aoeTarget = Vector2.zero;

    public int damage;
    public int drainDamage;
    public float drainSpd;
    public float drainDuration;
	// Use this for initialization
	protected void Start () {
	}

    // Update is called once per frame
    protected virtual void Update()
    {
        if (target != null && aoeTarget == Vector2.zero)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, 2 * Time.deltaTime);
        }
        else if (target != null && aoeTarget != Vector2.zero)
        {
            if (Vector2.Distance(transform.position, aoeTarget) == 0)
               Destroy(this.gameObject);
            transform.position = Vector2.MoveTowards(transform.position, aoeTarget, 2 * Time.deltaTime);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
