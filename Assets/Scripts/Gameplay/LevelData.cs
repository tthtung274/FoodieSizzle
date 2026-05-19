using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class LevelData
{
    public int level;
    public int time;
    public int perfect;
    public int totalFoods;

    public List<string> booster;
    public List<string> obstacle;
    public List<string> foodTypes;

    public string[][] layout;

    public Dictionary<string, TrayData>
        trays;
}

[Serializable]
public class TrayData
{
    public SlotData visible;

    public List<SlotData>
        hidden;
}

[Serializable]
public class SlotData
{
    [JsonProperty("1")]
    public string slot1;

    [JsonProperty("2")]
    public string slot2;

    [JsonProperty("3")]
    public string slot3;
}