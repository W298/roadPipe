using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemePalette", menuName = "Scriptable Object/ThemePalette", order = int.MaxValue)]
public class ThemePalette : ScriptableObject
{
    [SerializeField] public List<Theme> palette;
}

[Serializable]
public class Theme
{
    public ThemeName name;
    public Color color;
}

public enum ThemeName
{
    YELLOW,
    BLUE
}