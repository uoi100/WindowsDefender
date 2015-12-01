using UnityEngine;
using System.Collections.Generic;

using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public LinkedList<Vector2> movementPoints;
    

    public AudioClip hitSound;

    public int enemyId;
    
    public int cost = 10;
    public int maxHealth;
    public int health;
    public int armour;
    public int reward;
    public float movementSpeed;
    public double resistance;
    public Waypoint targetWaypoint;
	public Vector2 diff;

    public int drainDamage;
    public float drainSpd;
    public float drainDuration;

    public bool isVisible;
    public bool isGround;

    public int numDuplicates;
    public bool duplicates;
    public bool hasDuplicated;

    private float timer;
   
    private TowerAI revealingTower;
    private float revealDist;

	private AudioSource aSource;
	private SpriteRenderer renderer;
    private bool arrived = false;
	public bool Arrived { get { return arrived; } }
	private Animator animator;

    public EnemyAI()
    {
        health = 50;
        armour = 0;
        movementSpeed = 1.5f;
        resistance = 0;
        reward = 50;
        timer = 0;

        drainDamage = 0;
        drainSpd = 0;
        drainDuration = 0;

        isVisible = true;
        isGround = true;

        numDuplicates = 0;
        duplicates = false;
		hasDuplicated = false;

    }

    // Use this for initialization
	void Start () {
		revealingTower = null;
		revealDist = 0;
		aSource = GetComponent<AudioSource>();
		renderer = this.GetComponent<SpriteRenderer>();
		animator = this.GetComponent<Animator> ();
    }

    // Update is called once per frame
    void Update()
    {
        ExecuteEnemyAI();
    }

    /// <summary>
    /// The enemies do the following steps:
    /// 1. Check if there is a place to move
    /// 2. If there is then move to that location
    /// 3. Apply any drain damage
    /// </summary>
    protected void ExecuteEnemyAI()
    {
        if (movementPoints != null)
        {
            if (movementPoints.Count > 0)
            {
                Vector2 first = movementPoints.First.Value;

				diff = new Vector2(first.x - transform.position.x, first.y - transform.position.y);

				if(animator == null)
					animator = this.GetComponent<Animator> ();
				else
					if(diff.y < 0 && Mathf.Abs (diff.y) > Mathf.Abs(diff.x))
						animator.Play("down");//animator.SetInteger("dir", 0);	// DOWN
					else if(diff.x > 0 && Mathf.Abs (diff.y) < Mathf.Abs(diff.x))
						animator.Play("right");//animator.SetInteger("dir", 1);	// RIGHT
					else if(diff.y > 0 && Mathf.Abs (diff.y) > Mathf.Abs(diff.x))
						animator.Play("up");//animator.SetInteger("dir", 2);	// UP
					else if(diff.x < 0 && Mathf.Abs (diff.y) < Mathf.Abs(diff.x))
						animator.Play("left");//animator.SetInteger("dir", 3);	// LEFT	

                float dist = Vector2.Distance(transform.position, first);

                if (dist <= 0.1)
                    movementPoints.RemoveFirst();
                else
                    transform.position = Vector2.MoveTowards(transform.position, first, movementSpeed * Time.deltaTime);
            } else
            {
                arrived = true;
            }
        }

        timer += Time.deltaTime;

        while (timer > drainSpd && drainDuration > 0)
            DrainHealth();

        checkIfRevealed();

		if(renderer == null)
			renderer = this.GetComponent<SpriteRenderer>();
        if (!isVisible)
			renderer.enabled = false;
        else
			renderer.enabled = true;
    }

    /// <summary>
    /// Check if this enemy is revealed, if this enemy is invisible
    /// </summary>
    private void checkIfRevealed()
    {
        if (revealingTower == null)
        {
            GameObject[] towers = GameObject.FindGameObjectsWithTag("TOWER");

            if (towers.Length > 0)
            {
                foreach (GameObject obj in towers)
                {
                    revealDist = Vector2.Distance(transform.position, obj.transform.position);
                    revealingTower = obj.GetComponent<TowerAI>();

                    if (revealingTower != null)
                    {
                        if (revealDist < revealingTower.attackRange && revealingTower.revealsInvisible)
                        {
                            isVisible = true;
                            break;
                        }
                    }
                    revealingTower = null;
                }
            }
        }
        else
        {
            revealDist = Vector2.Distance(transform.position, revealingTower.gameObject.transform.position);
            if (revealDist > revealingTower.attackRange)
            {
                revealingTower = null;
                isVisible = false;
            }
        }
    }

    /// <summary>
    /// Method to drain enemy's health
    /// </summary>
    void DrainHealth()
    {
		health -= drainDamage;
		drainDuration -= drainSpd;
		timer -= drainSpd;

        if (drainDuration < 0)
        {
            drainDuration = 0;
            drainDamage = 0;
        }
    }
	
    void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.gameObject.tag == "PROJECTILE")
        {
            projectileAI projectile = obj.gameObject.GetComponent<projectileAI>();

            if (projectile.target == this.gameObject)
            {
                OnCollide(projectile);
            }

        }
    }

    /// <summary>
    /// When a projectile collides with this enemy object
    /// </summary>
    protected virtual void OnCollide(projectileAI projectile)
    {
        int damage;

        if ((damage = projectile.damage - armour) > 0)
            health -= damage;

        GetComponent<AudioSource>().PlayOneShot(hitSound, 0.9f);

        if (drainDuration <= 0)
        {
            drainDamage = projectile.drainDamage;
            drainSpd = projectile.drainSpd;
            drainDuration += projectile.drainDuration;
        }

        Destroy(projectile.gameObject);
    }

    /// <summary>
    /// When an enemy dies, if applicable duplicate the enemies on the spot that the enemy had died.
    /// </summary>
    /// <returns></returns>
    public virtual EnemyAI[] OnDeath()
    {
        EnemyAI[] duplicateEnemies = null;

        if (duplicates && !hasDuplicated)
        {
            health = maxHealth;

            duplicateEnemies = new EnemyAI[numDuplicates];

            for (int i = 0; i < numDuplicates; i++)
            {
                Vector3 newPosition = transform.position - new Vector3(0.1f, 0.1f, 0);

                EnemyAI duplicate = (EnemyAI)GameObject.Instantiate(this, newPosition, transform.rotation);
                duplicate.transform.Rotate(Vector3.zero, 15 * i, Space.Self);
                duplicate.hasDuplicated = true;
                duplicate.health = maxHealth;
                duplicate.movementPoints = movementPoints;
                duplicate.transform.parent = transform.parent;
                duplicate.hitSound = hitSound;
                duplicate.aSource = aSource;

                duplicateEnemies[i] = duplicate;
            }
        }

        GameObject.Destroy(gameObject);

        return duplicateEnemies;
    }

    /// <summary>
    /// Returns description of the Enemy
    /// </summary>
    /// <returns></returns>
    public virtual string ToolTip()
    {
        string toolTip = "";
        toolTip += name;
        toolTip += "\nCost: " + cost;
        toolTip += "\nHealth: " + maxHealth;
        toolTip += "\nArmour: " + armour;
        toolTip += "\nResistance: " + resistance;
        toolTip += "\nSpeed: " + movementSpeed;
        toolTip += "\nBounty: " + reward;
        if (!isVisible)
        {
            toolTip += "\nInvisible";

        }
        if (!isGround)
        {
            toolTip += "\nFlying";
        }
        if (duplicates)
        {
            toolTip += "\nDuplicating";
        }

        return toolTip;
    }
}
