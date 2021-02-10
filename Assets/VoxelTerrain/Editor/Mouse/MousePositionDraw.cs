using System;
using UnityEngine;

namespace VoxelTerrain.Editor.Mouse
{
    public class MousePositionDraw : MonoBehaviour
    {
        private float offset = 0.5f;
        private Camera CamMain => Camera.main;
        
        

        private void Update()
        {
            var ray = CamMain.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out var hit)) return;

            var hitPos = hit.point;

            hitPos.x = (float) Math.Round(hitPos.x) - offset;
            hitPos.y = (float) Math.Round(hitPos.y) + offset;
            hitPos.z = (float) Math.Round(hitPos.z) - offset;
            
            

            transform.position = hitPos;
        }
    }
}
