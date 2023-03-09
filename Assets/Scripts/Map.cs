using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoricalRecord
{
    public Entity actor;
    public Entity tile;
    public int state;

    public HistoricalRecord(Entity actor, Entity tile, int state)
    {
        this.actor = actor;
        this.tile = tile;
        this.state = state;
    }
}

public class Map
{
    public string uuid;
    public string name;
    public string music;
    private int keywidth = 9999;
    private int nextId;
    private Dictionary<Entity, int[]> targets;
    public Dictionary<int, Entity> entities;
    public Dictionary<int, Entity> tiles;
    public Dictionary<Entity, int> foodScores;
    public HashSet<Entity> goals;
    public HashSet<Entity> switches;
    public HashSet<Entity> food;
    public HashSet<Entity> chickens;
    public HashSet<Entity> actors;
    public Entity player;
    public Stack<List<HistoricalRecord>> history;



    public Map()
    {
        targets = new Dictionary<Entity, int[]>();
        foodScores = new Dictionary<Entity, int>();
        food = new HashSet<Entity>();
        actors = new HashSet<Entity>();
        chickens = new HashSet<Entity>();
        goals = new HashSet<Entity>();
        switches = new HashSet<Entity>();
        entities = new Dictionary<int, Entity>();
        tiles = new Dictionary<int, Entity>();
        history = new Stack<List<HistoricalRecord>>();
    }

    public void Init()
    {
        AssignTargets();
    }

    int GetKey(int x, int y)
    {
        return y * keywidth + x;
    }

    Entity GetTile(int x, int y)
    {
        int key = GetKey(x, y);
        if (!tiles.ContainsKey(key)) return null;
        return tiles[GetKey(x, y)];
    }

    public Entity CreateEntity()
    {
        Entity e = new Entity();
        e.id = nextId++;
        entities[e.id] = e;
        return e;
    }

    public Entity CreateActor(Entity.Type type, int x, int y)
    {
        Entity tile = GetTile(x, y);
        Entity e = CreateEntity();
        e.type = type;
        e.tile = tile;
        tile.content = e;
        e.x = tile.x;
        e.y = tile.y;

        if (e.type == Entity.Type.Player)
        {
            player = e;
        }

        if (e.type == Entity.Type.Food)
        {
            food.Add(e);
        }

        if (e.type == Entity.Type.Chicken)
        {
            chickens.Add(e);
        }

        actors.Add(e);

        return e;
    }

    public Entity CreateTile(Entity.Type type, int x, int y, Entity.Type goalType)
    {
        Entity e = CreateEntity();
        tiles[GetKey(x, y)] = e;
        e.type = type;
        e.x = x;
        e.y = y;
        e.goalType = goalType;

        if (e.goalType != Entity.Type.None)
        {
            goals.Add(e);
        }

        if (e.type == Entity.Type.Switch)
        {
            switches.Add(e);
        }

        return e;
    }

    public Entity CreateTile(Entity.Type type, int x, int y, Entity.Type goalType, int[] target)
    {
        Entity e = CreateTile(type, x, y, goalType);
        targets.Add(e, target);
        return e;
    }

    void AssignTargets()
    {
        foreach (Entity e in targets.Keys) 
        {
            int[] target = targets[e];
            e.target = GetTile(target[0], target[1]);
        }
    }

    void PushHistory()
    {
        List<HistoricalRecord> currentState = new List<HistoricalRecord>();
        foreach(Entity actor in actors)
        {
            HistoricalRecord rec = new HistoricalRecord(actor, actor.tile, actor.state);
            currentState.Add(rec);
        }
        history.Push(currentState);
    }

    public void Undo()
    {
        if (history.Count > 0)

        {
            foreach(Entity tile in tiles.Values)
            {
                tile.content = null;
            }
            List<HistoricalRecord> recs = history.Pop();
            foreach(HistoricalRecord rec in recs)
            {
                rec.actor.x = rec.tile.x;
                rec.actor.y = rec.tile.y;
                rec.actor.tile = rec.tile;
                rec.actor.state = rec.state;
                rec.tile.content = rec.actor;
            }
        }
    }

    bool CanPushObject(Entity actor, Entity obj)
    {
        Entity.Type t1 = actor.type;
        Entity.Type t2 = obj.type;

        if (t1 == Entity.Type.Player)
        {
            return t2 == Entity.Type.Block ||
                   t2 == Entity.Type.Chicken ||
                   t2 == Entity.Type.Food;
        } else
        {
            return false;
        }
    }

    bool IsPassableForActor(Entity tile)
    {
        Entity.Type t = tile.type;
        return t == Entity.Type.Floor ||
               t == Entity.Type.Door && tile.state == 1;     
    }

    bool CanMoveActor(Entity actor, int dx, int dy, bool isBeingPushed)
    {
        Entity target = GetTile(actor.x + dx, actor.y + dy);
        // tile not in map
        if (target == null)
        {
            return false;
        }
        // actor can not enter tile
        if (!IsPassableForActor(target))
        {
            return false;
        }
        // tile is legal and empty
        if (target.content == null)
        {
            return true;
        }
        // tile is legal but not empty and actor cannot push contents
        if (!isBeingPushed && !CanPushObject(actor, target.content))
        {
            return false;
        }
        // content can be legally pushed to resulting tile
        return CanMoveActor(target.content, dx, dy, true);
    } 

    public bool MaybeMovePlayer(int dx, int dy)
    {
        if (CanMoveActor(player, dx, dy, false)) 
        {
            PushHistory();
            MoveActor(player, dx, dy);
            return true;
        }
        return false;
    }

    public bool MaybeMoveActor(Entity actor, int dx, int dy)
    {
        if (CanMoveActor(actor, dx, dy, false))
        {
            MoveActor(actor, dx, dy);
            UpdateSwitches();
            return true;
        }
        return false;
    }

    void MoveActor(Entity actor, int dx, int dy)
    {
        Entity prev = GetTile(actor.x, actor.y);
        Entity next = GetTile(actor.x + dx, actor.y + dy);
        if (next.content != null) 
        {
            MoveActor(next.content, dx, dy);
        }
        actor.x = actor.x + dx;
        actor.y = actor.y + dy;
        actor.tile = next;
        if (prev.content == actor)
        {
            prev.content = null;
        }
        next.content = actor;
    }

    public void UpdateSwitches()
    {
        foreach (Entity s in switches)
        {
            s.state = s.content != null ? 1 : 0;
            if (s.target != null) 
            {
                s.target.state = s.state;
            }
        }
    }

    public bool IsLevelComplete()
    {
        foreach (Entity goal in goals)
        {
            if (goal.content == null || goal.goalType != goal.content.type)
            {
                return false;
            }
        }
        return true;
    }

    static Map FromJSON()
    {
        return new Map();
    }

    Queue<Entity> GetNeighbors(Entity e)
    {
        int[,] deltas = new int [4, 2] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        Queue<Entity> q = new Queue<Entity>();
        for (int i = 0; i < 4; i++)
        {
            int x = deltas[i, 0];
            int y = deltas[i, 1];
            Entity neighbor = GetTile(e.x + x, e.y + y);
            if (neighbor != null)
            {
                q.Enqueue(neighbor);
            }
        }
        return q;
    }

    void ComputeFoodScores()
    {
        foodScores.Clear();
        foreach (Entity f in food)
        {
            ComputeFoodScore1(f);
        }
    }

    bool IsPassableForChicken(Entity tile)
    {
        if (tile.content != null)
        {
            return false;
        }
        return (tile.content == null) &&
               (tile.type == Entity.Type.Floor) ||
               (tile.type == Entity.Type.Door && tile.state == 1);
    }

    void ComputeFoodScore1(Entity f)
    {
        HashSet<Entity> visited = new HashSet<Entity>();
        Queue<(Entity, int)> candidates = new Queue<(Entity, int)>();
        foreach (Entity t2 in GetNeighbors(f))
        {
            candidates.Enqueue((t2, 1));
        }

        while (candidates.Count > 0)
        {
            (Entity, int) tup = candidates.Dequeue();
            Entity t = tup.Item1;
            int cost = tup.Item2;

            if (IsPassableForChicken(t) && (!foodScores.ContainsKey(t) || foodScores[t] > cost))
            {
                Debug.Log("setting best cost: " + cost + " x: " + t.x + " y: " + t.y);
                foodScores[t] = cost;
                foreach (Entity t2 in GetNeighbors(t))
                {
                    candidates.Enqueue((t2, cost + 1));
                }
            }
        }
    }

    int dist(Entity a, Entity b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public void MaybeMoveChickens()
    {
        foreach (Entity chicken in chickens)
        {
            UpdateChickenIntention(chicken);
            if (chicken.target != null)
            {
                MaybeMoveActor(chicken, chicken.target.x - chicken.x, chicken.target.y - chicken.y);
            }
        }
    }

    void UpdateChickenIntention(Entity chicken)
    {
        ComputeFoodScores();
        chicken.target = null;
        int bestCost = 99999;
        if (foodScores.ContainsKey(chicken.tile))
        {
            bestCost = foodScores[chicken.tile];
        }

        foreach(Entity tile in GetNeighbors(chicken.tile))
        {
            if (tile.content != null && tile.content.type == Entity.Type.Food)
            {
                chicken.target = null;
                return;
            }
            if (foodScores.ContainsKey(tile))
            {
                Debug.Log("cost: " + foodScores[tile] + " at x: " + tile.x + "y: " + tile.y);
                if (foodScores[tile] < bestCost)
                {
                    chicken.target = tile;
                    bestCost = foodScores[tile];
                }
            }
        }
    }

    /*

    List<Entity> FindPath(Entity actor, Entity goal)
    {
        List<int> out = new List<int>();
        Entity actor = entities[actorId];
        Entity start = entities[actor.tileId];

        PriorityQueue<Entity, int> openSet = new PriorityQueue(Entity, int);
        Dictionary<Entity, Entity> cameFrom = new Dictionary<Entity, Entity>();
        Dictionary<Entity, int> gScore = new Map();
        Dictionary<Entity, int> fScore = new Map();

        gScore[start] = 0;
        fScore[start] = dist(start, goal);
        openSet.Enqueue(start, fScore[start]);

        while (openSet.Count > 0) 
        {
            Entity current = openSet.Dequeue();

            if (current == goal) {
                return reconstructPath(cameFrom, current);
            }


        }
    }
    */
}
