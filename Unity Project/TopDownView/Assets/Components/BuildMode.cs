using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
public class BuildMode : MonoBehaviour {

    /// <summary>
    /// The build mode
    /// </summary>
    private bool buildMode;

    /// <summary>
    /// The resource text
    /// </summary>
    public UnityEngine.UI.Text resourceText;
    /// <summary>
    /// The flash count
    /// </summary>
    public int flashCount;

    /// <summary>
    /// The pre building
    /// </summary>
    Building preBuilding;
    /// <summary>
    /// The building
    /// </summary>
    Building building;

    /// <summary>
    /// The message adapter
    /// </summary>
    MultiplayerMessagingAdapter messageAdapter;

    // Use this for initialization
    /// <summary>
    /// Starts this instance.
    /// </summary>
    void Start () {
        buildMode = false;
	}

    /// <summary>
    /// Initializes the specified MSG adapter.
    /// </summary>
    /// <param name="msgAdapter">The MSG adapter.</param>
    public void Initialize(MultiplayerMessagingAdapter msgAdapter)
    {
        messageAdapter = msgAdapter;
    }

    // Update is called once per frame
    /// <summary>
    /// Updates this instance.
    /// </summary>
    void Update () {
        if (Input.GetKey(KeyCode.Escape))
        {
            buildMode = false;
            Destroy(building.gameObject);

        }
        int money = int.Parse(resourceText.text);
        if (buildMode && building != null)
        {
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            Tile[] tiles = FindObjectsOfType<Tile>();
            Tile closestTile = null;
            float closestDist = 1.0f;

            foreach(Tile tile in tiles)
            {
                float dist = Vector2.Distance(tile.transform.position, mousePos);

                if( dist < closestDist)
                {
                    closestDist = dist;
                    closestTile = tile;
                }
            }

            if(closestTile != null)
            {
                TowerAI temp = building.GetComponent<TowerAI>();
                

                if (closestTile.Buildable)
                {
                    building.GetComponent<SpriteRenderer>().color = UnityEngine.Color.white;
                    building.transform.position = closestTile.transform.position;
                    
                    if (Input.GetMouseButtonDown(0) && ((money - temp.cost) >= 0))
                    {
                        building.operating = true;
                        buildMode = false;
                        money -= temp.cost;
                        resourceText.text = money.ToString();
                        building.transform.position = closestTile.transform.position;
                        closestTile.Buildable = false;
                        closestTile.Walkable = false;
                        building.transform.parent = transform.Find("Towers").transform;
                        building.GetComponent<SpriteRenderer>().color = preBuilding.GetComponent<SpriteRenderer>().color;
                        messageAdapter.SendTowerBuilt(preBuilding.name, building.transform.position.x, building.transform.position.y);
                        building = null;
                    }
                    else if (Input.GetMouseButtonDown(0) && !((money - temp.cost) >= 0))
                    {
                        flashCount = 6;
                        buildMode = false;
                        Destroy(building.gameObject);
                        InvokeRepeating("flash", 0, 0.15f);
                    }
                }
                else
                {
                    building.GetComponent<SpriteRenderer>().color = UnityEngine.Color.red;
                }
            }
        }
	}

    /// <summary>
    /// Flashes this instance.
    /// </summary>
    public void flash()
    {
        flashCount--;
        if (resourceText.color == UnityEngine.Color.white)
        {
            resourceText.color = UnityEngine.Color.red;
        }
        else
        {
            resourceText.color = UnityEngine.Color.white;
        }
        if (flashCount < 1) CancelInvoke("flash");
    }

    /// <summary>
    /// Build Tower Function which allows the player to build the specified tower
    /// </summary>
    /// <param name="tower">The tower.</param>
    public void BuildTower(Building tower)
    {
        preBuilding = tower;
        if (!buildMode)
        {
            buildMode = true;
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            building = (Building) Instantiate(tower, mousePos, transform.rotation);
            building.name = building.name.Replace("(Clone)", "");

            building.operating = false;
        }
    }
}
