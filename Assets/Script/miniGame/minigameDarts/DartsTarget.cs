using UnityEngine;

namespace MiniGame.Darts
{
    /// <summary>
    /// เป้าปาลูกดอก (Dartboard):
    /// ตรวจสอบระยะห่างจากจุดกึ่งกลางเพื่อคิดคะแนนตามวงกลม:
    /// - Bullseye (กลางเป้า) = 100 แต้ม
    /// - Inner Ring (วงใน) = 60 แต้ม
    /// - Middle Ring (วงกลาง) = 40 แต้ม
    /// - Outer Ring (วงนอก) = 20 แต้ม
    /// - หลุดขอบ = 0 แต้ม (Miss)
    /// </summary>
    public class DartsTarget : MonoBehaviour
    {
        [Header("Target Center")]
        [Tooltip("จุดกึ่งกลางของเป้า (ถ้าไม่ใส่จะใช้ตำแหน่งของ GameObject ตัวเอง)")]
        public Transform centerPoint;

        [Header("Ring Radii (รัศมีของแต่ละวง)")]
        [Tooltip("รัศมีวงกลางเป๊ะ (Bullseye = 100 แต้ม)")]
        public float bullseyeRadius = 0.35f;

        [Tooltip("รัศมีวงใน (60 แต้ม)")]
        public float innerRadius = 0.9f;

        [Tooltip("รัศมีวงกลาง (40 แต้ม)")]
        public float middleRadius = 1.6f;

        [Tooltip("รัศมีวงนอกสุด (20 แต้ม)")]
        public float outerRadius = 2.4f;

        [Header("Points")]
        public int bullseyePoints = 100;
        public int innerPoints = 60;
        public int middlePoints = 40;
        public int outerPoints = 20;

        public Vector2 CenterPosition => (centerPoint != null) ? (Vector2)centerPoint.position : (Vector2)transform.position;

        /// <summary>
        /// คำนวณคะแนนและชื่อวงจากพิกัดตกกระทบ
        /// </summary>
        public (int points, string ringName) CalculateScore(Vector2 hitPosition)
        {
            float distance = Vector2.Distance(hitPosition, CenterPosition);

            if (distance <= bullseyeRadius)
            {
                return (bullseyePoints, "BULLSEYE!");
            }
            else if (distance <= innerRadius)
            {
                return (innerPoints, "INNER RING");
            }
            else if (distance <= middleRadius)
            {
                return (middlePoints, "MIDDLE RING");
            }
            else if (distance <= outerRadius)
            {
                return (outerPoints, "OUTER RING");
            }

            return (0, "MISS");
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = (centerPoint != null) ? centerPoint.position : transform.position;

            // วาดวงกลมใน Scene View ให้ปรับขนาดรัศมีได้ง่ายๆ
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, bullseyeRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, innerRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, middleRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, outerRadius);
        }
    }
}
