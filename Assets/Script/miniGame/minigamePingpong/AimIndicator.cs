using UnityEngine;

namespace Yatlak.MiniGame.PingPong
{
    public class AimIndicator : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float length = 3.5f;
        [SerializeField] private float startWidth = 0.08f;
        [SerializeField] private float endWidth = 0.02f;
        [SerializeField] private Color indicatorColor = new Color(1f, 0.9f, 0.1f, 0.9f);

        [Header("Optional Arrow Tip")]
        [SerializeField] private Transform arrowTip;

        private void Awake()
        {
            EnsureLineRenderer();
        }

        private void EnsureLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = startWidth;
                lineRenderer.endWidth = endWidth;
                lineRenderer.useWorldSpace = true;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;

                // Create a basic unlit material if none exists
                if (lineRenderer.sharedMaterial == null)
                {
                    Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
                    if (unlitShader == null) unlitShader = Shader.Find("Mobile/Particles/Additive");
                    
                    if (unlitShader != null)
                    {
                        Material mat = new Material(unlitShader);
                        mat.color = indicatorColor;
                        lineRenderer.material = mat;
                    }
                }

                lineRenderer.startColor = indicatorColor;
                lineRenderer.endColor = indicatorColor;
            }
        }

        public void SetAim(Vector3 pivot, float angleDegrees, Vector3 forwardDir)
        {
            EnsureLineRenderer();

            // Calculate aim direction by rotating forward vector around up axis by angleDegrees
            Vector3 aimDir = Quaternion.AngleAxis(angleDegrees, Vector3.up) * forwardDir;
            aimDir.Normalize();

            Vector3 endPoint = pivot + aimDir * length;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, pivot);
                lineRenderer.SetPosition(1, endPoint);
            }

            if (arrowTip != null)
            {
                arrowTip.gameObject.SetActive(true);
                arrowTip.position = endPoint;
                arrowTip.rotation = Quaternion.LookRotation(aimDir, Vector3.up);
            }
        }

        public void Show(bool show)
        {
            EnsureLineRenderer();
            if (lineRenderer != null) lineRenderer.enabled = show;
            if (arrowTip != null) arrowTip.gameObject.SetActive(show);
        }
    }
}
