using UnityEngine;
using System.Collections.Generic;

using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public LinkedList<Vector2> movementPoints;
    

    public AudioClip hitSound;

    public int maxHealth;
    public int health;
    public int armour;
    public int reward;
    public float movementSpeed;
    public double resistance;

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
    void Start()
    {
        aSource = GetComponent<AudioSource>();
        revealingTower = null;
        revealDist = 0;
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
        if (movementPoints != null && movementPoints.Count > 0)
        {
            Vector2 first = movementPoints.First.Value;

            float dist = Vector2.Distance(transform.position, first);

            if (dist <= 0.1)
                movementPoints.RemoveFirst();
            else
                transform.position = Vector2.MoveTowards(transform.position, first, movementSpeed * Time.deltaTime);
        }

        timer += Time.deltaTime;

        while (timer > drainSpd && drainDuration > 0)
            DrainHealth();

        checkIfRevealed();

        if (!isVisible)
            GetComponent<SpriteRenderer>().enabled = false;
        else
            GetComponent<SpriteRenderer>().enabled = true;
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

        aSource.PlayOneShot(hitSound, 0.9f);

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

                duplicateEnemies[i] = duplicate;
            }
        }

        GameObject.Destroy(gameObject);

        return duplicateEnemies;
    }
}
