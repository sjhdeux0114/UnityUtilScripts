using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OniMusha.Utils
{
    [ExecuteAlways]
    [AddComponentMenu("UI/Effects/UI Rainbow Mesh Effect")]
    public class UIRainbowMeshEffect : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            List<UIVertex> vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);

            Rect rect = GetComponent<RectTransform>().rect;
            
            // 0~1 로컬 UV를 계산하여 UV1(TEXCOORD1) 채널에 주입합니다.
            // rect.xMin, rect.yMin은 피벗에 상관없이 RectTransform의 실제 좌하단 좌표를 나타냅니다.
            for (int i = 0; i < vertices.Count; i++)
            {
                UIVertex v = vertices[i];
                
                float localX = (v.position.x - rect.xMin) / rect.width;
                float localY = (v.position.y - rect.yMin) / rect.height;
                
                v.uv1 = new Vector2(localX, localY);
                vertices[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}
