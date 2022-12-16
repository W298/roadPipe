using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemType
{
    STOP,
    SLOW
}

[ExecuteInEditMode]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    
    public static GameManager instance
    {
        get
        {
            _instance ??= FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    public ThemePalette themePalette;
    public InventoryManager inventoryManager = new InventoryManager(5, 5);

    public Theme GetTheme(ThemeName name)
    {
        return themePalette.palette.FirstOrDefault(theme => theme.name == name);
    }
}
