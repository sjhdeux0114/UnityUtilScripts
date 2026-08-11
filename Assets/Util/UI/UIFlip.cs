using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIFlip : BaseMeshEffect
{
    public bool flipX = false;
    public bool flipY = false;

    [Header("플립 시 위치 보정값 (로컬 좌표 기준)")]
    public Vector2 offsetWhenFlipX;   // flipX=true일 때 추가로 이동
    public Vector2 offsetWhenFlipY;   // flipY=true일 때 추가로 이동

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        int count = vh.currentVertCount;
        if (count == 0)
            return;

        var verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        // 🔥 RectTransform 전체 박스 기준 중심점
        var rt = graphic.rectTransform;
        Rect rect = rt.rect;

        float centerX = (rect.xMin + rect.xMax) * 0.5f;
        float centerY = (rect.yMin + rect.yMax) * 0.5f;

        // 이 프레임에서 적용할 총 오프셋
        Vector2 totalOffset = Vector2.zero;
        if (flipX)
            totalOffset += offsetWhenFlipX;
        if (flipY)
            totalOffset += offsetWhenFlipY;

        for (int i = 0; i < verts.Count; i++)
        {
            var v = verts[i];
            Vector3 pos = v.position;

            // 중심 기준 플립
            if (flipX)
            {
                pos.x = 2f * centerX - pos.x;
            }

            if (flipY)
            {
                pos.y = 2f * centerY - pos.y;
            }

            // 🔧 플립 이후 위치 보정
            pos.x += totalOffset.x;
            pos.y += totalOffset.y;

            v.position = pos;
            verts[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}
