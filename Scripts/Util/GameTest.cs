using UnityEngine;

public class GameTest : MonoBehaviour
{
    public void Unlock(Define.UnlockType unlockType)
    {
        Managers.Game.UnlockSystem(unlockType);   
    }
}
