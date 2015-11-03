using UnityEngine;
using System.Collections;

public class Game_Timer : MonoBehaviour {
    public UnityEngine.UI.Text txt_Timer;

    float _Time;

    public void setTime(float time)
    {
        _Time = time;
    }

    public void tick()
    {
        _Time -= 1 * Time.deltaTime;
        txt_Timer.text = ((int)_Time).ToString();
    }

    public bool timeUp()
    {
        if (_Time <= 0)
            txt_Timer.text = "";

        return _Time <= 0;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
