using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacrophageTowerScript : BaseCellTower
{
    protected override void Awake()
    {
        base.Awake();
        damage = 10f;
        attackInterval = 1f;
    }


}
