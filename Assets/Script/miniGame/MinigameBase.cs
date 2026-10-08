using UnityEngine;

public class MinigameBase : MonoBehaviour
{
   public void Result()
   {
      OverwritedResult();
      if (MinigameManager.Instance != null) MinigameManager.Instance.UpdateGameState();
   }

   public virtual void OverwritedResult()
   {
      
   }
}
