using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public int health;
   

	// Use this for initialization
	void Start () {
        health = 1000;
       
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
            gameObject.transform.Translate(Vector2.up * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.S))
            gameObject.transform.Translate(Vector2.down * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.A))
            gameObject.transform.Translate(Vector2.left * Time.deltaTime, Space.World);
        if (Input.GetKey(KeyCode.D))
            gameObject.transform.Translate(Vector2.right * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.gameObject.tag == "PROJECTILE")
        {
            projectileAI projectile = obj.gameObject.GetComponent<projectileAI>();

            health -= projectile.damage;
            Destroy(obj.gameObject);

            if (health < 0)
                Destroy(this);
        }
    }
}
