using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int level;
    public int time;
    public List<string> booster;
    public List<ObstacleData> obstacle;
    public List<string> foodTypes;
    public int perfect;
    public int step;
    public LayoutRow[] layout;
    public TrayCollection trays;
}

[Serializable]
public class ObstacleData
{
    public string type;
    public int layout;
    public string FoodLockImg;
}

[Serializable]
public class LayoutRow
{
    public int[] row;
}

[Serializable]
public class TrayCollection
{
    public List<TrayItem> items;
}

[Serializable]
public class TrayItem
{
    public int key;
    public TrayData value;
}

[Serializable]
public class TrayData
{
    public VisibleFood visible;
    public List<VisibleFood> hidden;
}

[Serializable]
public class VisibleFood
{
    public string _1;
    public string _2;
    public string _3;
}