using UnityEngine;

namespace CakeDev
{
    public static class DebugUtilities
    {
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float duration = 10f)
        {
            Debug.DrawRay(pos, direction, Color.white, duration);
       
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, Color.white, duration);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, Color.white, duration);
        }
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float duration = 10f)
        {
            Debug.DrawRay(pos, direction, color);
       
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color, duration);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color, duration);
        }
    }
}