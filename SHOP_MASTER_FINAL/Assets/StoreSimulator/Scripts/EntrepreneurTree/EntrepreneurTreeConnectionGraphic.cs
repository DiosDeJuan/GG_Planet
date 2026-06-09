//Adaptado por POMPIC 20100333
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    public class EntrepreneurTreeConnectionGraphic : Graphic
    {
        private readonly List<ConnectionLine> lines = new List<ConnectionLine>();

        public struct ConnectionLine
        {
            public readonly Vector2 Start;
            public readonly Vector2 End;
            public readonly Color Color;
            public readonly float Width;

            public ConnectionLine(Vector2 start, Vector2 end, Color color, float width)
            {
                Start = start;
                End = end;
                Color = color;
                Width = width;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        public void SetConnections(IReadOnlyList<ConnectionLine> connections)
        {
            lines.Clear();
            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                    lines.Add(connections[i]);
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            for (int i = 0; i < lines.Count; i++)
                AddLine(vh, lines[i]);
        }

        private static void AddLine(VertexHelper vh, ConnectionLine line)
        {
            Vector2 delta = line.End - line.Start;
            if (delta.sqrMagnitude < 0.1f)
                return;

            Vector2 normal = new Vector2(-delta.y, delta.x).normalized * (line.Width * 0.5f);
            int index = vh.currentVertCount;

            vh.AddVert(line.Start - normal, line.Color, Vector2.zero);
            vh.AddVert(line.Start + normal, line.Color, Vector2.zero);
            vh.AddVert(line.End + normal, line.Color, Vector2.zero);
            vh.AddVert(line.End - normal, line.Color, Vector2.zero);

            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index + 2, index + 3, index);
        }
    }
}
