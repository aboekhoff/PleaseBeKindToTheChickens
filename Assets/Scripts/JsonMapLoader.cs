using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class JsonMapLoader
{
    private Dictionary<string, Entity.Type> typemap;
    private Dictionary<string, Entity.Type> goalmap;

    private static JsonMapLoader instance;

    public static JsonMapLoader GetInstance()
    {
        if (instance == null) { instance = new JsonMapLoader(); }
        return instance;
    }

    public Dictionary<string, string> manifest;

    public JsonMapLoader()
    {
        typemap = new Dictionary<string, Entity.Type>();
        typemap.Add("floor", Entity.Type.Floor);
        typemap.Add("wall", Entity.Type.Wall);
        typemap.Add("goal-block", Entity.Type.Floor);
        typemap.Add("goal-chicken", Entity.Type.Floor);
        typemap.Add("door", Entity.Type.Door);
        typemap.Add("switch", Entity.Type.Switch);
        typemap.Add("player", Entity.Type.Player);
        typemap.Add("chicken", Entity.Type.Chicken);
        typemap.Add("food", Entity.Type.Food);
        typemap.Add("block", Entity.Type.Block);

        goalmap = new Dictionary<string, Entity.Type>();
        goalmap.Add("goal-chicken", Entity.Type.Chicken);
        goalmap.Add("goal-block", Entity.Type.Block);

        manifest = new Dictionary<string, string>();
        TextAsset json = Resources.Load<TextAsset>("manifest");
        Dictionary<string, Dictionary<string, string>> tmp = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json.ToString());
        foreach (string uuid in tmp.Keys)
        {
            string name = tmp[uuid]["value"];
            manifest[name] = uuid;
            Debug.Log(name);
        }

        LoadMapByName("Distraction");
    }

    public Map LoadMap(string uuid)
    {
        TextAsset text = Resources.Load<TextAsset>(uuid);
        JObject js = JObject.Parse(text.ToString());

        Map m = new Map();

        string name = (string)js["name"];
        string music = (string)js["music"];
        JArray tiles = (JArray)js["tiles"];
        JArray actors = (JArray)js["actors"];

        foreach (JObject tile in tiles)
        {
            int x = (int)tile["x"];
            int y = (int)tile["y"];
            string type = (string)tile["type"];
            string entrance = (string)tile["entrance"];
            Entity.Type goaltype = Entity.Type.None;

            if (goalmap.ContainsKey(type))
            {
                goaltype = goalmap[type];
            }

            if (tile["target"].Type != JTokenType.Null)
            {
                JArray target = (JArray)tile["target"];
                m.CreateTile(typemap[type], x, y, goaltype, new int[] { (int)target[0], (int)target[1] });
            }
            else
            {
                m.CreateTile(typemap[type], x, y, goaltype);
            }
        }

        foreach (JObject actor in actors)
        {
            int x = (int)actor["x"];
            int y = (int)actor["y"];
            string type = (string)actor["type"];
            m.CreateActor(typemap[type], x, y);
        }

        m.name = name;
        m.music = music;
        m.uuid = uuid;
        m.Init();
        return m;
    }

    public Map LoadMapByName(string name)
    {
        return LoadMap(manifest[name]);
    }
}
