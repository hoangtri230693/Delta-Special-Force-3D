using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int Gold;
    public List<int> UnlockedWeaponIDs = new List<int>();
}
