using UnityEngine;
using System.Collections;

public class BuildMode : MonoBehaviour {

    private bool buildMode;

    public UnityEngine.UI.Text resourceText;
    
    Building building;

	// Use this for initialization
	void Start () {
        buildMode = false;
	}
	
	// Update is called once per frame
	void Update () {
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
                }
                else
                {
                    building.GetComponent<SpriteRenderer>().color = UnityEngine.Color.red;
                }
            }
        }
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
