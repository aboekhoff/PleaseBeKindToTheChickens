using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{ 
    public enum Type { 
        None, 
        Floor, 
        Switch, 
        Wall, 
        MechanicalDoor, 
        Player, 
        Chicken,
        Block, 
        Food,
        LevelEntrance,
        CompletedLevelEntrance,
    }

    public class Snapshot
    {
        public Entity self;
        public Entity tile;
        public Entity content;
        public Type type;
        public int state;
    }

    public Type type;
    public int id;
    public int x;
    public int y;
    public Entity tile;
    public Entity content;
    public Entity target;
    public int state;
    public string entrance;
    public Type goalType;
    public bool pushed;
    public Snapshot snapshot;

    public void ResetSnapshot()
    {
        if (snapshot == null)
        {
            snapshot = new Snapshot();
        }
        snapshot.self = this;
        snapshot.content = content;
        snapshot.tile = tile;
        snapshot.type = type;
        snapshot.state = state;
    }
}

/*
 * 
 * State changes we need to respond to:
 * tile/content changed (facing could change, animation could change, screen position will change)
 * state changed (animation could change, transition could trigger, sound could trigger)
 * type changed (transition animation could trigger, animation could trigger, sound could trigger)
 */
