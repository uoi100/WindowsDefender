using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>
    /// The way point
    /// </summary>
    public Waypoint wayPoint;
    /// <summary>
    /// The enemy
    /// </summary>
    public EnemyAI enemy;
    /// <summary>
    /// The tile
    /// </summary>
    public Tile tile;
    /// <summary>
    /// The spawner
    /// </summary>
    public SpawnerAI spawner;
    /// <summary>
    /// The game time
    /// </summary>
    public Game_Timer gameTime;
    /// <summary>
    /// The rounds
    /// </summary>
    public Rounds rounds;
    /// <summary>
    /// The game information debug text
    /// </summary>
    public UnityEngine.UI.Text gameInfoDebugText;
    /// <summary>
    /// The resource text
    /// </summary>
    public UnityEngine.UI.Text resourceText;
    /// <summary>
    /// The health text
    /// </summary>
    public UnityEngine.UI.Text healthText;
    /// <summary>
    /// The opponent health text
    /// </summary>
    public UnityEngine.UI.Text opponentHealthText;
    /// <summary>
    /// The network cli
    /// </summary>
    public NetworkClient NetworkCli;
    /// <summary>
    /// The game state text
    /// </summary>
    public UnityEngine.UI.Text gameStateText;
    /// <summary>
    /// The build mode
    /// </summary>
    public BuildMode buildMode;
    /// <summary>
    /// The enemy mode
    /// </summary>
    public EnemyMode enemyMode;
    /// <summary>
    /// The chat scroll view
    /// </summary>
    public GameObject chatScrollView;

    /// <summary>
    /// The enemies
    /// </summary>
    private ArrayList enemies;
    /// <summary>
    /// The spawners
    /// </summary>
    private ArrayList spawners;
    /// <summary>
    /// The team spawners
    /// </summary>
    private ArrayList[] teamSpawners = new ArrayList[2];
    /// <summary>
    /// The timer
    /// </summary>
    private float timer;

    /// <summary>
    /// The _point
    /// </summary>
    private Waypoint _point;
    /// <summary>
    /// The _opponent point
    /// </summary>
    private Waypoint _opponentPoint;

    /// <summary>
    /// The map
    /// </summary>
    private Tile[,] map;
    /// <summary>
    /// The money
    /// </summary>
    public int money;
    /// <summary>
    /// The health
    /// </summary>
    public int health;
    /// <summary>
    /// The opponent health
    /// </summary>
    private int opponentHealth;
    /// <summary>
    /// The health damage amount
    /// </summary>
    public int healthDamageAmount;
    /// <summary>
    /// The _map width
    /// </summary>
    private int _mapWidth, _mapHeight;


    /// <summary>
    /// The username
    /// </summary>
    private string username = "test12";
    /// <summary>
    /// The ip
    /// </summary>
    private string ip = "127.0.0.1";
    /// <summary>
    /// The match identifier
    /// </summary>
    private string matchId = "4fg7-38g3-d922-f75g-48g6";
    /// <summary>
    /// The team identifier
    /// </summary>
    private int teamId = 2;


    /// <summary>
    /// The net adapter
    /// </summary>
    MessagingNetworkAdapter netAdapter;
    /// <summary>
    /// The multi message adapter
    /// </summary>
    MultiplayerMessagingAdapter multiMessageAdapter;

    /// <summary>
    /// 
    /// </summary>
    struct Square
    {
        /// <summary>
        /// The g
        /// </summary>
        public int g;
        /// <summary>
        /// The h
        /// </summary>
        public int h;
        /// <summary>
        /// The f
        /// </summary>
        public int f;
        /// <summary>
        /// The x
        /// </summary>
        public int x;
        /// <summary>
        /// The y
        /// </summary>
        public int y;
    };

    // Use this for initialization
    /// <summary>
    /// Starts this instance.
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;

        GetGameInitInfo();
        enemies = new ArrayList();
        spawners = new ArrayList();
        teamSpawners[0] = new ArrayList();
        teamSpawners[1] = new ArrayList();

        
        gameTime.setTime(0.0f);
        timer = 0;
        rounds.notParsed = true;
        resourceText.text = money.ToString();
        //healthText.text = health.ToString();
        opponentHealth = health;
        opponentHealthText.text = opponentHealth.ToString();


        TMXLoader tmxl = new TMXLoader(Resources.Load<TextAsset>("map2"), this, teamId);
        tmxl.loadMeta();
        _mapWidth = tmxl.realMapWidth;
        _mapHeight = tmxl.realMapHeight;    
        map = tmxl.tiles;
        tmxl.load();

        GameObject.FindWithTag("MainCamera").transform.position += tmxl.TransformVector;
        GameObject.FindWithTag("MainCamera").GetComponent<CameraAI>().SetMapSize(_mapWidth * 0.32f, _mapHeight * 0.32f);

        StartNetworking();
    }

    /// <summary>
    /// Starts the networking.
    /// </summary>
    void StartNetworking()
    {
        netAdapter = new MessagingNetworkAdapter(NetworkCli);
        NetworkCli.Initialize(ip, matchId, username, netAdapter);
        multiMessageAdapter = new MultiplayerMessagingAdapter(netAdapter, this, username, teamId, teamSpawners, enemies, chatScrollView.GetComponent<Chat>());
        buildMode.Initialize(multiMessageAdapter);
        enemyMode.Initialize(multiMessageAdapter, (teamId == 1 ? 2 : 1), teamSpawners, enemies);

        chatScrollView.GetComponent<Chat>().Initialize(multiMessageAdapter);
    }

    /// <summary>
    /// Updates the state text.
    /// </summary>
    /// <param name="gs">The gs.</param>
    void UpdateStateText(MultiplayerMessagingAdapter.GameState gs)
    {
        switch (gs)
        {
            case MultiplayerMessagingAdapter.GameState.WaitingForPlayers:
                gameStateText.text = "Waiting For Players...";
                break;
            case MultiplayerMessagingAdapter.GameState.GameInProgress:
                gameStateText.text = "Game In Progress!";
                break;
            case MultiplayerMessagingAdapter.GameState.PlayerDisconnect:
                gameStateText.text = "Peer left game, game over.";
                break;
            case MultiplayerMessagingAdapter.GameState.TeamLoss:
                gameStateText.text = "Game Loss";
                break;
            case MultiplayerMessagingAdapter.GameState.TeamWin:
                gameStateText.text = "Game Win";
                break;
            default:
                break;
        }
    }
    // Update is called once per frame
    /// <summary>
    /// Updates this instance.
    /// </summary>
    void Update()
    {
        money = int.Parse(resourceText.text);
        if (rounds.notParsed)
            rounds.parseWave();
        
        // Check health of monsters
        CheckEnemy();

        multiMessageAdapter.ReceiveAndUpdate();
        UpdateStateText(multiMessageAdapter.CurrentGameState);

        gameTime.tick();

        if (gameTime.timeUp())
        {
            // Create Monsters
            // Below for single-player debug enemy spawning
            /*
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
                    temp.targetWaypoint = spawn.targetWaypoint;

                    enemies.Add(temp);
                }
            }
            */
        }
    }



    /// <summary>
    /// Gets the game initialize information.
    /// </summary>
    void GetGameInitInfo()
    {
        string schemaName = "towerdefender:";
        string[] cmdLineArgs = System.Environment.GetCommandLineArgs();
        if (cmdLineArgs.Length > 1 && cmdLineArgs[1].StartsWith(schemaName))
        {
            string[] uriList = cmdLineArgs[1].Substring(schemaName.Length).Split('|');
            ip = uriList[0];
            username = uriList[1];
            matchId = uriList[2];
            teamId = Convert.ToInt32(uriList[3]);
            //gameInfoDebugText.text = "IP: " + serverIpAddr + "\nUser: " + username;
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

            if (temp.Arrived)
            {
                if (temp.targetWaypoint == _point)
                {
                    health -= healthDamageAmount;
                    healthText.text = health.ToString();
                    multiMessageAdapter.SendHealthUpdate(health);
                } else
                {
                    opponentHealth -= healthDamageAmount;
                    opponentHealthText.text = opponentHealth.ToString();
                }
                GameObject.Destroy(temp.gameObject);
                enemies.Remove(temp);
            }
            if (temp.health <= 0)
            {
                EnemyAI[] newEnemies = null;

                if (temp.targetWaypoint == _point)
                {
                    money += temp.reward;
                    resourceText.text = money.ToString();
                }

                if((newEnemies = temp.OnDeath()) != null)
                    for (int j = 0; j < newEnemies.Length; j++)
                        tempNewEnemies.Add(newEnemies[j]);
                
                enemies.Remove(temp);
                multiMessageAdapter.SendEnemyDeath(temp.enemyId);
            }
        }

        for (int i = 0; i < tempNewEnemies.Count; i++)
            enemies.Add(tempNewEnemies[i]);
    }

    /// <summary>
    /// Get Enemies within range of a position
    /// </summary>
    /// <param name="atransform">The atransform.</param>
    /// <param name="range">The range.</param>
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
    /// <param name="spai">The spai.</param>
    /// <param name="teamId">The team identifier.</param>
    public void addSpawnerToSpawnerList(SpawnerAI spai, int teamId)
    {
        spai.targetWaypoint = (teamId == this.teamId ? _point : _opponentPoint);
        spawners.Add(spai);
        spai.wayPoints = pathFinding(spai, false, teamId);
        spai.flyPoints = pathFinding(spai, true, teamId);
        teamSpawners[teamId - 1].Add(spai);
    }


    /// <summary>
    /// Sets the way point.
    /// </summary>
    /// <param name="way">The way.</param>
    /// <param name="teamId">The team identifier.</param>
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
    /// <param name="spawner">The spawner.</param>
    /// <param name="flying">if set to <c>true</c> [flying].</param>
    /// <param name="teamId">The team identifier.</param>
    /// <returns>
    /// List of Vectors that the Monsters will follow till they reach their destination
    /// </returns>
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
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="g">The g.</param>
    /// <param name="usedSquares">The used squares.</param>
    /// <param name="openSquares">The open squares.</param>
    /// <param name="flying">if set to <c>true</c> [flying].</param>
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
    /// <param name="gameObject">The game object.</param>
    /// <returns>
    /// Vector2 where x is the column index and y is the row index
    /// </returns>
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
    /// <param name="a">a.</param>
    /// <param name="b">The b.</param>
    /// <returns>
    /// Returns the distnce
    /// </returns>
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
    /// <param name="a">The alpha component.</param>
    /// <param name="b">The blue component.</param>
    /// <returns>
    /// The delta distance between the two squares
    /// </returns>
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
    /// <param name="spawner">The spawner.</param>
    /// <param name="ground">if set to <c>true</c> [ground].</param>
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