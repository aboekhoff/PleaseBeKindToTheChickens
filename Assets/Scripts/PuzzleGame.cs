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
    Material levelEntranceMaterial;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    Vector3 cameraOffset;

    Transform player;

    List<Transform> actors;

    Map map;

    Timeout squawkTimeout = new Timeout(2f);

    bool levelComplete = false;

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
            else if (e.type == Entity.Type.LevelEntrance)
            {
                mat = levelEntranceMaterial;
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
        cameraOffset = new Vector3(0, 10, -1);
        audioSource = GetComponent<AudioSource>();
        actors = new List<Transform>();
        map = GameManager.GetCurrentPuzzle();
        SetupEntities();
        cameraRef.position = player.localPosition + cameraOffset;
        cameraRef.LookAt(player);
    }

    void FollowPlayer()
    {
        Vector3 target = player.localPosition + cameraOffset;
        cameraRef.localPosition = Vector3.Lerp(cameraRef.localPosition, target, 0.1f);
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

    void MovePlayer(int dx, int dy)
    {
        map.MaybeMovePlayer(dx, dy);
        map.MaybeMoveChickens();
        HandleChanges();
        
    }

    void HandleChanges()
    {
        AudioManager am = AudioManager.GetInstance();
        foreach (Entity e in map.GetChanges())
        {
            if (e.type == Entity.Type.Chicken)
            {
                if (e.pushed)
                {
                    if (!squawkTimeout.IsTimedOut())
                    {
                        squawkTimeout.Reset();
                        am.PlayClip(AudioManager.SoundType.Squawk, audioSource);
                    }
                } else
                {
                    am.PlayClip(AudioManager.SoundType.Cluck, audioSource);
                }
            }
            else if (e.type == Entity.Type.Block)
            {
                am.PlayClip(AudioManager.SoundType.Slide, audioSource);
            }
            else if (e.type == Entity.Type.Player)
            {
                if (e.tile.entrance != null)
                {
                    GameManager.PushPuzzle(e.tile.entrance);
                    map.Undo();
                }
            }
        }
        map.ClearChanges();
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
        squawkTimeout.Update(Time.deltaTime);
        HandleInput();
        Animate();
        FollowPlayer();
        if (!levelComplete && map.IsLevelComplete())
        {
            levelComplete = true;
            AudioManager.GetInstance().PlayClip(AudioManager.SoundType.Success, audioSource);
            // probably want some transition here
            GameManager.PopPuzzle();
        }
    }

}
