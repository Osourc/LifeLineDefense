using UnityEngine;

public class TowerBase : MonoBehaviour
{
    public bool IsOccupied = false;

    public void PlaceTower()
    {
        IsOccupied = true;
    }

    public void RemoveTower()
    {
        IsOccupied = false;
    }
}
