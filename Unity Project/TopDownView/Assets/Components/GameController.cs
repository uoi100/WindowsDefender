using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public Waypoint wayPoint;
    public EnemyAI enemy;
    public Tile tile;
    public SpawnerAI spawner;
    public Game_Timer gameTime;
    public Rounds rounds;
    public UnityEngine.UI.Text gameInfoDebugText;
    public UnityEngine.UI.Text resourceText;

    private ArrayList enemies;
    private ArrayList spawners;
    private ArrayList[] teamSpawners = new ArrayList[2];
    private float timer;

    private Waypoint _point;
    private Waypoint _opponentPoint;

    private Tile[,] map;
    public int money;
    private int _mapWidth, _mapHeight;
    private int teamId = 1;

    struct Square
    {
        public int g;
        public int h;
        public int f;
        public int x;
        public int y;
    };

    // Use this for initialization
    void Start()
    {
        GetGameInitInfo();
        enemies = new ArrayList();
        spawners = new ArrayList();
        teamSpawners[0] = new ArrayList();
        teamSpawners[1] = new ArrayList();

        money = 50;
        gameTime.setTime(10.0f);
        timer = 0;
        rounds.notParsed = true;
        resourceText.text = money.ToString();

        TMXLoader tmxl = new TMXLoader(Resources.Load<TextAsset>("coolmap2"), this, teamId);
        tmxl.loadMeta();
        _mapWidth = tmxl.realMapWidth;
        _mapHeight = tmxl.realMapHeight;    
        map = tmxl.tiles;
        tmxl.load();

        
    }


    // Update is called once per frame
    void Update()
    {
        money = int.Parse(resourceText.text);
        if (rounds.notParsed)
            rounds.parseWave();
        
        // Check health of monsters
        CheckEnemy();

        gameTime.tick();

        if (gameTime.timeUp())
        {
            // Create Monsters
            timer += 1*Time.deltaTime;
            if (timer > 0.5f)
            {
                EnemyAI selectedAI = rounds.getEnemySpawn();
                foreach (SpawnerAI spawn in spawners)
                {
                    EnemyAI temp;
                    timer -= 0.5f;
                    temp = (EnemyAI)GameObject.Instantiate(selectedAI, spawn.transform.position, transform.rotation);
                    temp.transform.parent = transform.Find("Enemies").transform;
                    // Some Path-finding Algorithm here
                    temp.movementPoints = copyWaypoints(spawn, temp.isGround);
                    

                    enemies.Add(temp);
                }
            }
        }
    }



    void GetGameInitInfo()
    {
        string schemaName = "towerdefender:";

        string serverIpAddr;
        string username;
        string[] cmdLineArgs = System.Environment.GetCommandLineArgs();
        if (cmdLineArgs.Length > 1 && cmdLineArgs[1].StartsWith(schemaName))
        {
            string[] uriList = cmdLineArgs[1].Substring(schemaName.Length).Split('|');
            serverIpAddr = uriList[0];
            username = uriList[1];
            gameInfoDebugText.text = "IP: " + serverIpAddr + "\nUser: " + username;
        }
    }

    /// <summary>
    /// Check the health of the enemies, if they are 0 then execute their OnDeath functions
    /// If the enemy spawned more enemies then add those enemies to the list
    /// </summary>
    void CheckEnemy()
    {
        ArrayList tempNewEnemies = new ArrayList();

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAI temp = (EnemyAI)enemies[i];

            if (temp.health <= 0)
            {
                EnemyAI[] newEnemies = null;
                money += temp.reward;
                
                resourceText.text = money.ToString();

                if((newEnemies = temp.OnDeath()) != null)
                    for (int j = 0; j < newEnemies.Length; j++)
                        tempNewEnemies.Add(newEnemies[j]);
                
                enemies.Remove(temp);
                
            }
        }

        for (int i = 0; i < tempNewEnemies.Count; i++)
            enemies.Add(tempNewEnemies[i]);
    }

    /// <summary>
    /// Get Enemies within range of a position
    /// </summary>
    /// <param name="atransform"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public LinkedList<EnemyAI> getEnemyWithinRange(Transform atransform, float range)
    {
        LinkedList<EnemyAI> enemyList = new LinkedList<EnemyAI>();
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyAI temp = (EnemyAI)enemies[i];
            if ((temp.transform.position - atransform.position).magnitude <= range && temp.transform != atransform)
            {
                enemyList.AddLast(temp);
            }
        }
        return enemyList;
    }

    /// <summary>
    /// Adds a spawner to a spawner list
    /// </summary>
    /// <param name="spai"></param>
    public void addSpawnerToSpawnerList(SpawnerAI spai, int teamId)
    {

        spawners.Add(spai);
        spai.wayPoints = pathFinding(spai, false, teamId);
        spai.flyPoints = pathFinding(spai, true, teamId);
        teamSpawners[teamId - 1].Add(spai);
    }


    public void setWayPoint(Waypoint way, int teamId)
    {
        if (teamId != this.teamId)
            _opponentPoint = way;
        else
            _point = way;
    }

    /// <summary>
    /// A* Pathfinding Algorithm
    /// </summary>
    /// <param name="spawner"></param>
    /// <returns>List of Vectors that the Monsters will follow till they reach their destination</returns>
    LinkedList<Vector2> pathFinding(SpawnerAI spawner, bool flying, int teamId)
    {
        // 2D Array List of possible paths monsters can take to reach their destination
        LinkedList<Vector2> paths = new LinkedList<Vector2>();

        // List of tile indexes used for calculating waypoints
        LinkedList<Square> usedSquares = new LinkedList<Square>();
        LinkedList<Square> openSquares = new LinkedList<Square>();

        LinkedList<Square> leadingSquares = new LinkedList<Square>();

        Vector2 destSquare = FindTile((teamId != this.teamId ? _opponentPoint.gameObject : _point.gameObject));
        int fMin = 0, g = 0;
        Square spawn = createSquare(g, FindTile(spawner.gameObject), destSquare);
        leadingSquares.AddLast(spawn);

        int counter = 0;
        int counterMax = (_mapWidth * _mapHeight);
        counterMax = counterMax * counterMax;
        do
        {
            if (counter > counterMax)
            {
                Debug.Log("Algorithm ran for too long, counts: " + counter);
                return paths;
            }
            else
                counter++;

			foreach(Square square in leadingSquares){
				openSquares.Remove (square);
				usedSquares.AddLast (square);

                //Debug.Log ("Square: g: " + g + " h: " + square.h + " f: " + square.f + " x: " + square.x + " y: " + square.y);
                g = square.g;
	            if (square.h != 0)
	            {
	                // Check Adjacent Squares
	                LinkedList<Vector2> adjacentTiles = AdjacentTiles(square.x, square.y, square.g + 1, usedSquares, openSquares, flying);

	                foreach (Vector2 adjacentSquare in adjacentTiles)
	                {
						Square temp = createSquare(g +1, adjacentSquare, destSquare);

						foreach(Square openSquare in openSquares)
							if(distSquare(temp, openSquare) == 0)
								continue;
						foreach(Square usedSquare in usedSquares)
							if(distSquare(temp, usedSquare) == 0)
								continue;
				
                        openSquares.AddLast(temp);
	                }

	            }
	            else // Found the goal
	            {
	                openSquares.Clear();

					Square tempSquare = square;
					Square cmpSquare = square;
	                do
	                {
						paths.AddFirst(map[tempSquare.y, tempSquare.x].transform.position);
						usedSquares.Remove(tempSquare);
	                    g--;
	                    int smallestDist = int.MaxValue;

	                    foreach(Square usedSquare in usedSquares)
	                    {
                            if (usedSquare.g == g)
                            {
                                if (distSquare(usedSquare, tempSquare) < smallestDist)
                                {
                                    smallestDist = distSquare(usedSquare, tempSquare);
                                    cmpSquare = usedSquare;
                                }
                            }
	                    }

						tempSquare = cmpSquare;
	                } while (g > 0);

					paths.AddFirst(map[spawn.y, spawn.x].transform.position);
					usedSquares.Remove(tempSquare);
	                //Debug.Log("Counter Finished: " + counter);
	                return paths;
	            }
			}

			leadingSquares.Clear();
			fMin = int.MaxValue;

			foreach (Square openSquare in openSquares)
			{
				if (openSquare.f < fMin)
				{
					leadingSquares.Clear ();
					leadingSquares.AddLast (openSquare);
					fMin = openSquare.f;
				} else if (openSquare.f == fMin){
					leadingSquares.AddLast (openSquare);
				}
			}
			g++;
        } while (leadingSquares.Count > 0);

        Debug.Log("No possible path exists!");
        return paths;
    }

    /// <summary>
    /// Calculates possible adjacent tiles that monsters can walk on
    /// Diagonal tiles are only included if the adjacent tiles are available
    /// to walk on too
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    LinkedList<Vector2> AdjacentTiles(int x, int y, int g, LinkedList<Square> usedSquares, LinkedList<Square> openSquares, bool flying)
    {
        LinkedList<Vector2> adjacentTiles = new LinkedList<Vector2>();

        for (int i = y - 1; i < y + 2; i++)
        {
            if (i < 0 || i >= _mapHeight)
                continue;

            // Check Left-side
            if ((x - 1 >= 0))
            {
                // top-left & bottom-left corners
                if (i == (y - 1) || i == (y + 1))
                {
                    if (map[i, (x - 1)].Walkable && map[y, (x - 1)].Walkable && map[i, x].Walkable || flying)
                    {
                        Vector2 position = new Vector2((x - 1), i);
                        adjacentTiles.AddLast(position);
                    }
                }
                else if (map[y, (x - 1)].Walkable || flying)
                {
                    Vector2 position = new Vector2((x - 1), y);
                    adjacentTiles.AddLast(position);
                }
            }

            // Check Middle, we skip (x,y) since we have that data already
            if ((i != y) && map[i, x].Walkable || flying)
            {
                Vector2 position = new Vector2(x, i);
                adjacentTiles.AddLast(position);
            }

            // Check Right-side
            if ((x + 1 < _mapWidth))
            {
                // top-right & bottom-right corners
                if (i == (y - 1) || i == (y + 1))
                {
                    if (map[i, (x + 1)].Walkable && map[y, (x + 1)].Walkable && map[i, x].Walkable || flying)
                    {
                        Vector2 position = new Vector2((x + 1), i);
                        adjacentTiles.AddLast(position);
                    }
                }
                else if (map[y, (x + 1)].Walkable || flying)
                {
                    Vector2 position = new Vector2((x + 1), y);
                    adjacentTiles.AddLast(position);
                }
            }
        }

        LinkedList<Vector2> removeTiles = new LinkedList<Vector2>();
        LinkedList<Square> removeUsedSquares = new LinkedList<Square>();
        LinkedList<Square> removeOpenSquares = new LinkedList<Square>();

        foreach (Vector2 adjacentTile in adjacentTiles)
        {
            foreach (Square usedSquare in usedSquares)
                if ((int)adjacentTile.x == usedSquare.x && (int)adjacentTile.y == usedSquare.y)
                {
                    if (g > usedSquare.g)
                        removeTiles.AddLast(adjacentTile);
                    else
                        removeUsedSquares.AddLast(usedSquare);
                }

            foreach (Square openSquare in openSquares)
                if ((int)adjacentTile.x == openSquare.x && (int)adjacentTile.y == openSquare.y)
                {
                    if (g > openSquare.g)
                        removeTiles.AddLast(adjacentTile);
                    else
                        removeOpenSquares.AddLast(openSquare);
                }
        }

        foreach (Vector2 removeTile in removeTiles)
            adjacentTiles.Remove(removeTile);
        foreach (Square removeSquare in removeUsedSquares)
            usedSquares.Remove(removeSquare);
        foreach (Square removeSquare in removeOpenSquares)
            openSquares.Remove(removeSquare);

        return adjacentTiles;
    }

    /// <summary>
    /// Find the specific tile that the object is on
    /// </summary>
    /// <returns>Vector2 where x is the column index and y is the row index</returns>
    Vector2 FindTile(GameObject gameObject)
    {
        Vector2 index = new Vector2(-1, -1);

        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                float dist = Vector2.Distance(gameObject.transform.position, map[i, j].transform.position);

                if (dist == 0)
                {
                    index.x = j;
                    index.y = i;
                    return index;
                }
            }
        }

        return index;
    }

    /// <summary>
    /// Calculates the distance from point A to B
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>Returns the distnce</returns>
    int getHeuristics(Vector2 a, Vector2 b)
    {
        int dx, dy;

        dx = (int)(b.x - a.x);
        dy = (int)(b.y - a.y);

        if (dx < 0)
            dx *= -1;
        if (dy < 0)
            dy *= -1;

        return dx + dy;
    }
    
    /// <summary>
    /// Creates a new square
    /// </summary>
    /// <param name="g">Movement Cost</param>
    /// <param name="v">First Point</param>
    /// <param name="dv">Second Point</param>
    /// <returns></returns>
    Square createSquare(int g, Vector2 v, Vector2 dv)
    {
        Square newSquare = new Square();

        newSquare.x = (int)v.x;
        newSquare.y = (int)v.y;
        newSquare.h = getHeuristics(v, dv);
        newSquare.f = g + newSquare.h;
        newSquare.g = g;

        return newSquare;
    }

	/// <summary>
	/// Compares two squares
	/// </summary>
	/// <returns>The delta distance between the two squares</returns>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
    int distSquare(Square a, Square b)
    {
        int dx, dy;

        dx = (int)(b.x - a.x);
        dy = (int)(b.y - a.y);

        if (dx < 0)
            dx *= -1;
        if (dy < 0)
            dy *= -1;

        return dx + dy;
    }

    /// <summary>
    /// creates a new copy of a LinkedList
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="ground"></param>
    /// <returns></returns>
    LinkedList<Vector2> copyWaypoints(SpawnerAI spawner, bool ground)
    {
        LinkedList<Vector2> copyWaypoints = new LinkedList<Vector2>();

        if (ground)
            foreach (Vector2 v in spawner.wayPoints)
                copyWaypoints.AddLast(v);
        else
            foreach(Vector2 v in spawner.flyPoints)
                copyWaypoints.AddLast(v);

        return copyWaypoints;
    }
}