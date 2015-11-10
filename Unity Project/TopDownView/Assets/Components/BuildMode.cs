using UnityEngine;
using System.Collections;

public class BuildMode : MonoBehaviour {

    private bool buildMode;

    public UnityEngine.UI.Text resourceText;
    public int flashCount;
    
    Building building;

	// Use this for initialization
	void Start () {
        buildMode = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Escape))
        {
            buildMode = false;
            Destroy(building.GetComponent<SpriteRenderer>());
            //building = null;
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
                        building = null;
                    }
                    else if (Input.GetMouseButtonDown(0) && !((money - temp.cost) >= 0))
                    {
                        flashCount = 6;
                        buildMode = false;
                        Destroy(building.GetComponent<SpriteRenderer>());
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
    /// <param name="tower"></param>
    public void BuildTower(Building tower)
    {
        if (!buildMode)
        {
            buildMode = true;
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            building = (Building) Instantiate(tower, mousePos, transform.rotation);
           
            building.operating = false;
        }
    }
}
