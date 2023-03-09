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
        Door, 
        Player, 
        Chicken, 
        Block, 
        Food, 
        LevelEntrance, 
        LevelComplete,
        Fire,
        Ice,
        Water,
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
}
