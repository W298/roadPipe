using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager instance
    {
        get
        {
            _instance = _instance ?? FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    public ThemePalette themePalette;

    public Theme GetTheme(ThemeName name)
    {
        return themePalette.palette.FirstOrDefault(theme => theme.name == name);
    }
}
