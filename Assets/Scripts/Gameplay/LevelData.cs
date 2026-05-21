using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int level;
    public int time;
    public List<string> booster;
    public List<string> obstacle;
    public List<string> foodTypes;
    public int perfect;
    public int step;
    public LayoutRow[] layout;          // Mỗi dòng chứa mảng int ID của khay
    public TrayCollection trays;
}

[Serializable]
public class LayoutRow
{
    public int[] row;   // đã đổi từ string[] sang int[]
}

[Serializable]
public class TrayCollection
{
    public List<TrayItem> items;
}

[Serializable]
public class TrayItem
{
    public int key;          // ID của khay (số nguyên)
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