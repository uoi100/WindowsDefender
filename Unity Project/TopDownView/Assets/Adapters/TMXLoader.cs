using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class TMXLoader {
    private TextAsset tAsset;
    private int mapWidth, mapHeight;
    public int realMapWidth, realMapHeight;
    public Sprite[] sprites;
    public Tile[,] tiles;
    private GameController context;
    private XmlDocument doc;
    private float tilesize;

    private enum ReflectMode { Horizontal, Vertical };
    private ReflectMode rMode = ReflectMode.Horizontal;

    private XmlNodeList mapList;
    private XmlNode mapNode;

    private Vector2 waypoint;
    private LinkedList<Vector2> spawners = new LinkedList<Vector2>();
    private int teamId;

    public TMXLoader(TextAsset tAsset, GameController context, int teamId)
    {
        this.tAsset = tAsset;
        this.context = context;
        this.teamId = teamId;
    }
    public void loadMeta()
    {
        doc = new XmlDocument();
        doc.LoadXml(tAsset.text);

        mapList = doc["map"].ChildNodes;
        mapNode = doc["map"];

        XmlNode propNode = mapNode["properties"];
        foreach (XmlNode prop in propNode.ChildNodes)
        {
            if (prop.Attributes["name"].InnerText == "TeamReflectMode")
            {
                if (prop.Attributes["value"].InnerText == "vertical")
                {
                    rMode = ReflectMode.Vertical;
                }
            }
        }

        int tilewidth = Convert.ToInt32(((XmlElement)mapNode).Attributes["tilewidth"].InnerText);
        int tileheight = Convert.ToInt32(((XmlElement)mapNode).Attributes["tileheight"].InnerText);
        mapWidth = Convert.ToInt32(((XmlElement)mapNode).Attributes["width"].InnerText);
        mapHeight = Convert.ToInt32(((XmlElement)mapNode).Attributes["height"].InnerText);
        tilesize = 0.32f;

        realMapHeight = mapHeight * (rMode == ReflectMode.Vertical ? 2 : 1);
        realMapWidth = mapWidth * (rMode == ReflectMode.Horizontal ? 2 : 1);

        tiles = new Tile[realMapHeight, realMapWidth];

        for (int i = 0; i < mapHeight * (rMode == ReflectMode.Vertical ? 2 : 1); i++)
        {
            for (int j = 0; j < mapWidth * (rMode == ReflectMode.Horizontal ? 2 : 1); j++)
            {
                tiles[i, j] = (Tile)UnityEngine.Object.Instantiate(context.tile, new Vector2((tilesize * j), (tilesize * i)), context.transform.rotation);
                tiles[i, j].Buildable = false;
                tiles[i, j].Walkable = true;
                tiles[i, j].transform.parent = context.transform.Find("Tilemap").transform;
            }
        }
    }
    public void load()
    {
        
        
        sprites = new Sprite[1];
        int spriteArraySize = 1;

        for (int i = 0; i < mapList.Count; i++)
        {
            XmlNode i2 = mapList[i];
            if (i2.Name == "tileset")
            {
                Texture2D tileset = Resources.Load<Texture2D>(i2["image"].Attributes["source"].InnerText.Substring(0, i2["image"].Attributes["source"].InnerText.LastIndexOf('.')));
                int imgWidth = Convert.ToInt32(i2["image"].Attributes["width"].InnerText);
                int firstgid = Convert.ToInt32(i2.Attributes["firstgid"].InnerText);

                int tilecount = Convert.ToInt32(i2.Attributes["tilecount"].InnerText);
                int afterlastgid = firstgid + tilecount;
                spriteArraySize += tilecount;

                Vector2 pivot = new Vector2(0.5f, 0.5f);

                Array.Resize<Sprite>(ref sprites, spriteArraySize);
                for (int y = 0; firstgid < afterlastgid; y++)
                {
                    for (int x = 0; (firstgid < afterlastgid) && (x * 32 < imgWidth); x++)
                    {
                        sprites[firstgid] = Sprite.Create(tileset, new Rect(x * 32, y * 32, 32, 32), pivot);
                        firstgid++;
                    }
                }
            }
            if (i2.Name == "layer")
            {
                bool walkable = false;
                bool buildable = false;
                if (i2.Attributes["name"].InnerText.ToLower() == "walls")
                {
                    buildable = true;
                }
                if (i2.Attributes["name"].InnerText.ToLower() == "walkables")
                {
                    walkable = true;
                }
                string[] linesplit = i2["data"].InnerText.Split(',');
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        if (Convert.ToInt32(linesplit[(y * mapWidth) + x]) > 0)
                        {
                            tiles[mapHeight - y - 1, x].Buildable = buildable;
                            tiles[mapHeight - y - 1, x].Walkable = walkable;
                            tiles[mapHeight - y - 1, x].mapSprite = sprites[Convert.ToInt32(linesplit[(y * mapWidth) + x])];
                        }
                    }
                }


            }
            if (i2.Name == "objectgroup")
            {
                if (i2.Attributes["name"].InnerText.ToLower() == "entities")
                {
                    foreach (XmlNode obj in i2.ChildNodes)
                    {
                        int x = Convert.ToInt32(obj.Attributes["x"].InnerText) / 32;
                        int y = mapHeight - (Convert.ToInt32(obj.Attributes["y"].InnerText) / 32) - 1;
                        
                        if (obj.Attributes["type"].InnerText.ToLower() == "waypoint")
                        {
                            Waypoint point = (Waypoint)UnityEngine.Object.Instantiate(context.wayPoint, tiles[y, x].transform.position, context.transform.rotation);
                            point.transform.parent = context.transform;
                            context.setWayPoint(point, 1);
                            waypoint = new Vector2(x, y);
                        }
                    }
                    foreach (XmlNode obj in i2.ChildNodes)
                    {
                        int x = Convert.ToInt32(obj.Attributes["x"].InnerText) / 32;
                        int y = mapHeight - (Convert.ToInt32(obj.Attributes["y"].InnerText) / 32) - 1;
                        if (obj.Attributes["type"].InnerText.ToLower() == "spawner")
                        {
                            SpawnerAI enemySpawner = (SpawnerAI)UnityEngine.Object.Instantiate(context.spawner, tiles[y, x].transform.position, context.transform.rotation);
                            enemySpawner.transform.parent = context.transform.Find("Spawners").transform;
                            context.addSpawnerToSpawnerList(enemySpawner, 1);
                            spawners.AddLast(new Vector2(x, y));
                        }
                    }
                }

            }
        }
        if (rMode == ReflectMode.Horizontal)
            reflectMapHorizontal();
        else
            reflectMapVertical();
    }
    private void reflectMapHorizontal()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tiles[mapHeight - y - 1, x + mapWidth].Buildable = false;
                tiles[mapHeight - y - 1, x + mapWidth].Walkable = tiles[mapHeight - y - 1, mapWidth - x - 1].Walkable;
                tiles[mapHeight - y - 1, x + mapWidth].mapSprite = tiles[mapHeight - y - 1, mapWidth - x - 1].mapSprite;
                tiles[mapHeight - y - 1, x + (teamId == 1 ? mapWidth : 0)].Buildable = false;
            }
        }
        Waypoint point = (Waypoint)UnityEngine.Object.Instantiate(context.wayPoint, tiles[(int)waypoint.y, (mapWidth * 2 - (int)waypoint.x - 1)].transform.position, context.transform.rotation);
        point.transform.parent = context.transform;
        context.setWayPoint(point, 2);
        foreach (Vector2 pos in spawners)
        {
            SpawnerAI enemySpawner = (SpawnerAI)UnityEngine.Object.Instantiate(context.spawner, tiles[(int)pos.y, (mapWidth * 2 - (int)pos.x - 1)].transform.position, context.transform.rotation);
            enemySpawner.transform.parent = context.transform.Find("Spawners").transform;
            context.addSpawnerToSpawnerList(enemySpawner, 2);
        }
    }
    private void reflectMapVertical()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tiles[mapHeight + y, x].Buildable = tiles[mapHeight - y - 1, x].Buildable;
                tiles[mapHeight + y, x].Walkable = tiles[mapHeight - y - 1, x].Walkable;
                tiles[mapHeight + y, x].mapSprite = tiles[mapHeight - y - 1, x].mapSprite;
                tiles[(teamId == 1 ? mapHeight : 0) + y, x].Buildable = false;
            }
        }
        Waypoint point = (Waypoint)UnityEngine.Object.Instantiate(context.wayPoint, tiles[(2 * mapHeight - (int)waypoint.y - 1), (int)waypoint.x].transform.position, context.transform.rotation);
        point.transform.parent = context.transform;
        context.setWayPoint(point, 2);
        foreach (Vector2 pos in spawners)
        {
            SpawnerAI enemySpawner = (SpawnerAI)UnityEngine.Object.Instantiate(context.spawner, tiles[(2 * mapHeight - (int)pos.y - 1), (int)pos.x].transform.position, context.transform.rotation);
            enemySpawner.transform.parent = context.transform.Find("Spawners").transform;
            context.addSpawnerToSpawnerList(enemySpawner, 2);
        }
    }
}
