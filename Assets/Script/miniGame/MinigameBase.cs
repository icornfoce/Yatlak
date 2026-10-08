using UnityEngine;

public class MinigameBase : MonoBehaviour
{
   public void Result()
   {

   OverwritedResult();
       MinigameManager.Instance.UpdateGameState();
}

public virtual void OverwritedResult()
{
}
}
