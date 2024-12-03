using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutrophilTowerScript : BaseCellTower
{
    protected override void Awake()
    {
        base.Awake();
        damage = 25f;
        attackInterval = 1f;
    }
}
