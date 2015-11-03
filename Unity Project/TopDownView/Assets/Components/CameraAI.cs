using UnityEngine;
using System.Collections;

public class CameraAI : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 direction = transform.position;

        if (Input.GetKey(KeyCode.W))
            direction.y += 2;
        if (Input.GetKey(KeyCode.A))
            direction.x -= 2;
        if (Input.GetKey(KeyCode.S))
            direction.y -= 2;
        if (Input.GetKey(KeyCode.D))
            direction.x += 2;

        transform.position = Vector3.MoveTowards(transform.position, direction, 1.0f * Time.deltaTime);
    }
}
