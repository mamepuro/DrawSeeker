﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Destiny
{
    public static class OpenTKExSys
    {
        public static void GluPickMatrix(double x, double y, double deltax, double deltay, int[] viewport)
        {
            if (deltax <= 0 || deltay <= 0)
            {
                return;
            }

            GL.Translate((viewport[2] - 2 * (x - viewport[0])) / deltax, (viewport[3] - 2 * (y - viewport[1])) / deltay, 0);
            GL.Scale(viewport[2] / deltax, viewport[3] / deltay, 1.0);
        }

        public static int GluProject(float objx, float objy, float objz,
            float[] modelview, float[] projection,
            int[] viewport, float[] windowCoordinate)
        {
            float[] fTempo = new float[8];
            fTempo[0] = modelview[0] * objx + modelview[4] * objy + modelview[8] * objz + modelview[12]; // w is always 1
            fTempo[1] = modelview[1] * objx + modelview[5] * objy + modelview[9] * objz + modelview[13];
            fTempo[2] = modelview[2] * objx + modelview[6] * objy + modelview[10] * objz + modelview[14];
            fTempo[3] = modelview[3] * objx + modelview[7] * objy + modelview[11] * objz + modelview[15];
            fTempo[4] = projection[0] * fTempo[0] + projection[4] * fTempo[1] + projection[8] * fTempo[2] +
                projection[12] * fTempo[3];
            fTempo[5] = projection[1] * fTempo[0] + projection[5] * fTempo[1] + projection[9] * fTempo[2] +
                projection[13] * fTempo[3];
            fTempo[6] = projection[2] * fTempo[0] + projection[6] * fTempo[1] + projection[10] * fTempo[2] +
                projection[14] * fTempo[3];
            fTempo[7] = -fTempo[2];
            if (fTempo[7] == 0.0)
            {
                // w
                return 0;
            }
            fTempo[7] = 1.0f / fTempo[7];
            // Perspective division
            fTempo[4] *= fTempo[7];
            fTempo[5] *= fTempo[7];
            fTempo[6] *= fTempo[7];

            windowCoordinate[0] = (fTempo[4] * 0.5f + 0.5f) * viewport[2] + viewport[0];
            windowCoordinate[1] = (fTempo[5] * 0.5f + 0.5f) * viewport[3] + viewport[1];
            windowCoordinate[2] = (1.0f + fTempo[6]) * 0.5f;
            return 1;
        }
        /// <summary>
        /// マウスでクリックした場所の3次元位置を求める
        /// </summary>
        /// <param name="winx">マウスポインタのx座標</param>
        /// <param name="winy">マウスポインタのy座標</param>
        /// <param name="winz">マウスポインタのz座標</param>
        /// <param name="modelview">モデルビュー行列</param>
        /// <param name="projection">透視投影行列</param>
        /// <param name="viewport">ビューポート</param>
        /// <param name="objectX">クリックした３次元位置のx座標</param>
        /// <param name="objectY">クリックした３次元位置のy座標</param>
        /// <param name="objectZ">クリックした３次元位置のz座標</param>
        /// <returns></returns>

        public static int GluUnProject(double winx, double winy, double winz,
            double[] modelview, double[] projection, int[] viewport,
            out double objectX, out double objectY, out double objectZ)
        {
            objectX = 0;
            objectY = 0;
            objectZ = 0;

            OpenTK.Matrix4d projectionM = new OpenTK.Matrix4d(
                projection[0], projection[4], projection[8], projection[12],
                projection[1], projection[5], projection[9], projection[13],
                projection[2], projection[6], projection[10], projection[14],
                projection[3], projection[7], projection[11], projection[15]);
            OpenTK.Matrix4d modelviewM = new OpenTK.Matrix4d(
                modelview[0], modelview[4], modelview[8], modelview[12],
                modelview[1], modelview[5], modelview[9], modelview[13],
                modelview[2], modelview[6], modelview[10], modelview[14],
                modelview[3], modelview[7], modelview[11], modelview[15]);
            OpenTK.Matrix4d AM = projectionM * modelviewM;
            OpenTK.Matrix4d mM = OpenTK.Matrix4d.Invert(AM);

            OpenTK.Vector4d inV = new OpenTK.Vector4d();
            inV.X = ((winx - viewport[0]) / viewport[2] * 2.0 - 1.0);
            inV.Y = ((winy - viewport[1]) / viewport[3] * 2.0 - 1.0);
            inV.Z = (2.0 * winz - 1.0);
            inV.W = 1.0;
            OpenTK.Vector4d outV;
            MultiplyMatrix4x4ByVector4(out outV, mM, inV);
            if (outV.Z == 0.0)
            {
                return 0;
            }
            outV.W = (1.0 / outV.W);
            objectX = outV.X * outV.W;
            objectY = outV.Y * outV.W;
            objectZ = outV.Z * outV.W;

            /*
            System.Diagnostics.Debug.WriteLine("GluUnProject");
            System.Diagnostics.Debug.WriteLine("objectX = " + objectX);
            System.Diagnostics.Debug.WriteLine("objectY = " + objectY);
            System.Diagnostics.Debug.WriteLine("objectZ = " + objectZ);
            */
            return 1;
        }

        private static void MultiplyMatrix4x4ByVector4(out OpenTK.Vector4d resultvector,
            OpenTK.Matrix4d matrix, OpenTK.Vector4d pvector)
        {
            resultvector.X = matrix.M11 * pvector.X + matrix.M12 * pvector.Y +
                matrix.M13 * pvector.Z + matrix.M14 * pvector.W;
            resultvector.Y = matrix.M21 * pvector.X + matrix.M22 * pvector.Y +
                matrix.M23 * pvector.Z + matrix.M24 * pvector.W;
            resultvector.Z = matrix.M31 * pvector.X + matrix.M32 * pvector.Y +
                matrix.M33 * pvector.Z + matrix.M34 * pvector.W;
            resultvector.W = matrix.M41 * pvector.X + matrix.M42 * pvector.Y +
                matrix.M43 * pvector.Z + matrix.M44 * pvector.W;
        }

        public static Vector3d GetNormalVector(Vector3d v1, Vector3d v2 , Vector3d v3)
        {
            Vector3d normal = new Vector3d(0,0,0);
            normal.X = (v2.Y - v1.Y) * (v3.Z - v2.Z) - (v2.Z - v1.Z) * (v3.Y - v2.Y);
            normal.Y = (v2.Z - v1.Z) * (v3.X - v2.X) - (v2.X - v1.X) * (v3.Z - v2.Z);
            normal.Z = (v2.X - v1.X) * (v3.Y - v2.Y) - (v2.Y - v1.Y) * (v3.X - v2.X);
            normal.Normalize();
            return normal;
        }

        /// <summary>
        /// 2つのベクトルの外積を求め、法線ベクトルを返す
        /// </summary>
        /// <param name="v1">ベクトル1</param>
        /// <param name="v2">ベクトル2</param>
        /// <returns></returns>
        public static Vector3d GetNormalVector(Vector3d v1, Vector3d v2)
        {
            Vector3d normal = new Vector3d(0, 0, 0);
            normal.X = v1.Y * v2.Z - v1.Z * v2.Y;
            normal.Y = v1.Z * v2.X - v1.X * v2.Z;
            normal.Z = v1.X * v2.Y - v1.Y * v2.X;
            normal.Normalize();
            return normal;
        }
    }
}
