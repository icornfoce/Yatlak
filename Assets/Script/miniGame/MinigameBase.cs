using UnityEngine;
using DG.Tweening;

public class MinigameBase : MonoBehaviour
{
   public void Result()
   {
      OverwritedResult();
      if (MinigameManager.Instance != null)
      {
         DOVirtual.DelayedCall(1.5f, () =>
         {
            if (MinigameManager.Instance != null)
            {
               MinigameManager.Instance.UpdateGameState();
            }
         });
      }
   }

   public virtual void OverwritedResult()
   {
      
   }
}

