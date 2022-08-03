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

        public Vertex(byte iD, Vector3d pos)
        {
            ID = iD;
            VertexPosition = pos;
            VertexX = pos.X;
            VertexY = pos.Y;
            VertexZ = pos.Z;
        }
    }
}
