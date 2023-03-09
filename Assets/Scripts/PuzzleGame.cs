using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGame : MonoBehaviour
{
    [SerializeField]
    Transform entityPrefab;

    [SerializeField]
    Transform cameraRef;

    [SerializeField]
    Material wallMaterial;

    [SerializeField]
    Material floorMaterial;

    [SerializeField]
    Material chickenMaterial;

    [SerializeField]
    Material playerMaterial;

    [SerializeField]
    Material foodMaterial;

    [SerializeField]
    Material blockMaterial;

    [SerializeField]
    Material goalBlockMaterial;

    [SerializeField]
    Material goalChickenMaterial;

    [SerializeField]
    AudioSource audioSource;

    Transform player;

    List<Transform> actors;

    JsonMapLoader jsonMapLoader;

    Map map;

    Dictionary<string, AudioClip> soundEffects;

    bool levelComplete = false;

    string testString = @"
************
*----------*
*----*--*--*
*----*--*--*
*----*--*--*
*--B-*--*--*
*-------*--*
************".Trim();

    void LoadStringMap()
    {
        map = new Map();
        string[] arr = testString.Split('\n');

        for (int z = 0; z < arr.Length; z++)
        {
            string s = arr[z];

            for (int x = 0; x < s.Length; x++)
            {
                Entity.Type goal = Entity.Type.None;
                Entity.Type type;
                Material mat;
                float y;
                char c = s[x];
                if (c == '*')
                {
                    y = 0.5f;
                    mat = wallMaterial;
                    type = Entity.Type.Wall;
                }
                else if (c == '-')
                {
                    y = 0f;
                    mat = floorMaterial;
                    type = Entity.Type.Floor;
                }
                else if (c == 'B')
                {
                    y = 0f;
                    mat = goalBlockMaterial;
                    type = Entity.Type.Floor;
                    goal = Entity.Type.Block;
                }
                else
                {
                    continue;
                }
                Entity e = map.CreateTile(type, x, z, goal);
                Transform t = Instantiate(entityPrefab);
                DisplayEntity de = t.GetComponent<DisplayEntity>();
                de.entityId = e.id;
                t.localPosition = new Vector3(x, y, z);
                t.GetComponent<Renderer>().material = mat;
                t.SetParent(GetComponent<Transform>());
            }
        }

        CreatePlayer(4, 4);
        CreateBlock(3, 3);
        CreateChicken(2, 2);
        CreateFood(10, 6);
    }

    void LoadMapByUUUID(string uuid)
    {
        map = jsonMapLoader.LoadMap(uuid);
        SetupEntities();
    }

    void LoadMapByName(string name)
    {
        map = jsonMapLoader.LoadMapByName(name);
        SetupEntities();
    }

    void SetupEntities()
    {
        Dictionary<Entity.Type, Material> mmap = new Dictionary<Entity.Type, Material>();
        mmap.Add(Entity.Type.Player, playerMaterial);
        mmap.Add(Entity.Type.Block, blockMaterial);
        mmap.Add(Entity.Type.Chicken, chickenMaterial);
        mmap.Add(Entity.Type.Food, foodMaterial);

        foreach (Entity e in map.tiles.Values)
        {
            Material mat;
            float y;

            if (e.type == Entity.Type.Wall)
            {
                mat = wallMaterial;
                y = 0.5f;
            }
            else
            {
                mat = floorMaterial;
                y = 0f;
            }

            if (e.goalType == Entity.Type.Block)
            {
                mat = goalBlockMaterial;
            }
            else if (e.goalType == Entity.Type.Chicken)
            {
                mat = goalChickenMaterial;
            }
           
            Transform t = Instantiate(entityPrefab);
            DisplayEntity de = t.GetComponent<DisplayEntity>();
            de.entityId = e.id;
            t.localPosition = new Vector3(e.x, y, e.y);
            t.GetComponent<Renderer>().material = mat;
            t.SetParent(GetComponent<Transform>());
        }

        // actors

        foreach (Entity e in map.actors)
        {
            Transform se = Instantiate(entityPrefab);
            se.GetComponent<DisplayEntity>().entityId = e.id;
            se.GetComponent<Renderer>().material = mmap[e.type];
            se.localPosition = new Vector3(e.x, 1, e.y);
            actors.Add(se);

            if (e.type == Entity.Type.Player)
            {
                player = se;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        jsonMapLoader = new JsonMapLoader();
        audioSource = GetComponent<AudioSource>();
        InitSoundEffects();
        actors = new List<Transform>();

        LoadMapByName("No Sudden Moves");

        cameraRef.position = new Vector3(
            player.position.x,
            player.position.y + 10,
            player.position.z - 1
        );
        cameraRef.LookAt(player);
    }

    void CreatePlayer(int x, int y)
    {
        player = Instantiate(entityPrefab);
        Entity e = map.CreateActor(Entity.Type.Player, x, y);
        player.GetComponent<DisplayEntity>().entityId = e.id;
        player.GetComponent<Renderer>().material = playerMaterial;
        player.localPosition = new Vector3(x, 1, y);
        actors.Add(player);
    }

    void CreateBlock(int x, int y)
    {
        Transform block = Instantiate(entityPrefab);
        Entity e = map.CreateActor(Entity.Type.Block, x, y);
        block.GetComponent<DisplayEntity>().entityId = e.id;
        block.GetComponent<Renderer>().material = blockMaterial;
        block.localPosition = new Vector3(x, 1, y);
        actors.Add(block);
    }

    void CreateFood(int x, int y)
    {
        Transform block = Instantiate(entityPrefab);
        Entity e = map.CreateActor(Entity.Type.Food, x, y);
        block.GetComponent<DisplayEntity>().entityId = e.id;
        block.GetComponent<Renderer>().material = foodMaterial;
        block.localPosition = new Vector3(x, 1, y);
        actors.Add(block);
    }

    void CreateChicken(int x, int y)
    {
        Transform block = Instantiate(entityPrefab);
        Entity e = map.CreateActor(Entity.Type.Chicken, x, y);
        block.GetComponent<DisplayEntity>().entityId = e.id;
        block.GetComponent<Renderer>().material = chickenMaterial;
        block.localPosition = new Vector3(x, 1, y);
        actors.Add(block);
    }

    void InitSoundEffects()
    {
        foreach (AudioClip ac in Resources.FindObjectsOfTypeAll(typeof(AudioClip)) as AudioClip[])
        {
            Debug.Log(ac);
        }
        soundEffects = new Dictionary<string, AudioClip>();
        string[] names = { 
            "chicken1",
            "chicken2",
            "click1",
            "click2",
            "cluck1",
            "cluck2",
            "cluck3",
            "cluck4",
            "cluck5",
            "mechanical-door",
            "slide1",
            "slide2",
            "slide3",
            "slide4",
            "squawk1",
            "squawk2",
            "success"
        };
        foreach (string name in names)
        {
            AudioClip ac = Resources.Load<AudioClip>(name);
            Debug.Log(name);
            Debug.Log(ac);
            soundEffects[name] = ac;
        }
    }

    void MovePlayer(int dx, int dy)
    {
        map.MaybeMovePlayer(dx, dy);
        map.MaybeMoveChickens();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown("up"))
        {
            MovePlayer(0, 1);
        }
        if (Input.GetKeyDown("down"))
        {
            MovePlayer(0, -1);
        }
        if (Input.GetKeyDown("right"))
        {
            MovePlayer(1, 0);
        }
        if (Input.GetKeyDown("left"))
        {
            MovePlayer(-1, 0);
        }
        if (Input.GetKeyDown("backspace"))
        {
            map.Undo();
        }
    }

    void Animate()
    {
        foreach (Transform actor in actors)
        {
            int id = actor.GetComponent<DisplayEntity>().entityId;
            Entity e = map.entities[id];
            if (e.x != actor.position.x || e.y != actor.position.z)
            {
                actor.position = actor.position + new Vector3(
                    (e.x - actor.position.x) * 0.01f,
                    0,
                    (e.y - actor.position.z) * 0.01f
                );
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        Animate();
        if (!levelComplete && map.IsLevelComplete())
        {
            Debug.Log("Level Complete!");
            levelComplete = true;
            audioSource.PlayOneShot(soundEffects["success"]);
        }
    }

}
