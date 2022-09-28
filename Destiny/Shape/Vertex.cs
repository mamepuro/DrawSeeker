using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Destiny
{
    public class Vertex
    {
        /// <summary>
        /// 頂点ID
        /// </summary>
        public byte ID { get; set; }
        /// <summary>
        /// 頂点X座標
        /// </summary>
        public double VertexX { get; set; }
        /// <summary>
        /// 頂点Y座標
        /// </summary>
        public double VertexY { get; set; }
        /// <summary>
        /// 頂点Z座標
        /// </summary>
        public double VertexZ { get; set; }
        /// <summary>
        /// 頂点座標
        /// </summary>
        public Vector3d VertexPosition { get; set; }
        /// <summary>
        /// 頂点について角を成す2つの頂点のセット(例えば、頂点A,B,Cで三角形が成されるとき頂点AはBとCによって角を成すため頂点Aは[B, C]という値を持つ)
        /// </summary>
        public HashSet<int[]> connectVertexId { get; set; }

        public Vertex(byte iD, Vector3d pos)
        {
            ID = iD;
            VertexPosition = pos;
            VertexX = pos.X;
            VertexY = pos.Y;
            VertexZ = pos.Z;
            connectVertexId = new HashSet<int[]>();
        }
        public Vertex(byte iD, double x, double y, double z)
        {
            ID = iD;
            VertexX = x;
            VertexY = y;
            VertexZ = z;
            VertexPosition = new Vector3d(x, y, z);
            connectVertexId = new HashSet<int[]>();
        }
    }
}
