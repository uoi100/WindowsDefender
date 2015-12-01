using UnityEngine;
using System.Collections;
using System.Reflection;

public class ButtonScript : MonoBehaviour {

    public float speed;
    public Vector3 MoveLocation { get; set; }

    public GameObject SourceObject { get; set; }
    public GameObject GameController { get; set; }
    public Texture2D backgroundTexture;
    public string UpgradeTower { get; set; }
    public int UpgradeTowerLevel { get; set; }
    public object UpgradeTowerValue { get; set; }
    public int towerDamage;
    public float towerSpeed;
    public float towerRange;


    public bool Hide { get; set; }

    public AudioClip upgradeSound;

    private string toolTipText = null;
    private GUIStyle guiStyleFore;
    private GUIStyle guiStyleBack;    

    // Use this for initialization
    void Start () {
        guiStyleFore = new GUIStyle();
        guiStyleFore.normal.textColor = Color.white;
        guiStyleFore.wordWrap = true;
        guiStyleFore.alignment = TextAnchor.MiddleCenter;
        guiStyleBack = new GUIStyle();
        guiStyleBack.normal.textColor = Color.black;
        guiStyleBack.alignment = TextAnchor.MiddleCenter;
        guiStyleBack.normal.background = backgroundTexture;
        guiStyleBack.wordWrap = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (Vector2.Distance(transform.position, MoveLocation) > 0.01f)
            transform.position = Vector2.MoveTowards(transform.position, MoveLocation, speed * Time.deltaTime);
        else if (Hide)
        {
            Debug.Log("Should be destroyed");
            Destroy(gameObject);
        }

        if (!string.IsNullOrEmpty(UpgradeTower))
        {
            if(string.Compare(UpgradeTower, "Information") == 0)
            {
                TowerAI tower = SourceObject.GetComponent<TowerAI>();
                string text = "Damage Level: " + tower.Levels[0]
                    + "\nSpeed Level: " + tower.Levels[1]
                    + "\nRange Level: " + tower.Levels[2];

                GetComponentInChildren<UnityEngine.UI.Text>().text = text;
                GetComponentInChildren<UnityEngine.UI.Text>().fontSize = 9;
            }
        }
	}

    /// <summary>
    /// When the button is clicked, trigger the Game Controller's Build Mode Function
    /// </summary>
    public void OnClick()
    {
        if (SourceObject.GetComponent<Building>())
        {
            if (string.IsNullOrEmpty(UpgradeTower))
                GameController.GetComponent<BuildMode>().BuildTower(SourceObject.GetComponent<Building>());
            else
                upgradeTower();
        }
        if (SourceObject.GetComponent<EnemyAI>())
            GameController.GetComponent<EnemyMode>().SendEnemy(SourceObject.GetComponent<EnemyAI>());

    }
    /// <summary>
    /// When mouse enters the button, get the tooltip text
    /// </summary>
    public void OnMouseEnter()
    {
        if (SourceObject.GetComponent<TowerAI>())
            toolTipText = SourceObject.GetComponent<TowerAI>().ToolTip();
        else if (SourceObject.GetComponent<EnemyAI>())
            toolTipText = SourceObject.GetComponent<EnemyAI>().ToolTip();
    }

    /// <summary>
    /// When mouse leaves the button, remove the tooltip text
    /// </summary>
    public void OnMouseExit()
    {
        toolTipText = null;
    }

    /// <summary>
    /// Draws the tooltip onto the screen
    /// </summary>
    public void OnGUI()
    {
        if(toolTipText != null && string.IsNullOrEmpty(UpgradeTower))
        {
            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;
            int width = 135 ;
            int height = 135;

            GUI.Label(new Rect(x - 1 - width/2, y - height, width, height), toolTipText, guiStyleBack);
            GUI.Label(new Rect(x - width/2, y - height, width, height), toolTipText, guiStyleFore);
        }
    }

    private void upgradeTower()
    {
        TowerAI tower = SourceObject.GetComponent<TowerAI>();

        object info = tower.getTowerInfo(UpgradeTower);

        if (info != null)
        {
            if (UpgradeTowerLevel < 3) {

                if (info.GetType() == typeof(int))
                    UpgradeTowerValue = (int)info + (UpgradeTowerLevel * towerDamage);
                else if (info.GetType() == typeof(float))
                {
                    switch (UpgradeTower)
                    {
                        case "attackSpd": UpgradeTowerValue = (float)info - (UpgradeTowerLevel * towerSpeed); break;
                        case "attackRange": UpgradeTowerValue = (float)info + (UpgradeTowerLevel * towerRange); break;
                    }
                }

                UpgradeTowerLevel++;
                tower.upgradeTower(UpgradeTower, UpgradeTowerValue);

                GetComponent<AudioSource>().PlayOneShot(upgradeSound, 0.6f);
            }
        }
    }
}
