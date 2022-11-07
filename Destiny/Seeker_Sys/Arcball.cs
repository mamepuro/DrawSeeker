using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Destiny.Seeker_Sys
{
    internal class Arcball
    {
        public float Radius { get; set; }

        public Arcball(float radius)
        {
            Radius = radius;
        }

        private float GetZCoordinateOnSphere(float screenX, float screenY, float screenCenterX, float screenCenterY)
        {
            float x = (screenX - screenCenterX) / this.Radius;
            float y = (screenY - screenCenterY) / this.Radius;
            float r = x * x + y * y;
            if(r > 1.0)
            {
                float s = 1.0f / MathF.Sqrt(r);
                x *= s;
                y *= s;
                return 0.0f;
            }
            else
            {
                return MathF.Sqrt((this.Radius) * (this.Radius) - screenX * screenX - screenY * screenY);
            }
        }

        public float GetRotateAngle(float prev_screenX, float prev_screenY, float cur_screenX, float cur_screenY, float screenCenterX, float screenCenterY)
        {
            float x = (prev_screenX - screenCenterX) / this.Radius;
            float y = (prev_screenY - screenCenterY) / this.Radius;
            float z = 0.0f;
            float r = x * x + y * y;
            if (r > 1.0)
            {
                float s = 1.0f / MathF.Sqrt(r);
                x *= s;
                y *= s;
                z = 0.0f;
            }
            else
            {
                z = MathF.Sqrt(1.0f - r);
            }
            float cx = (cur_screenX - screenCenterX) / this.Radius;
            float cy = (cur_screenY - screenCenterY) / this.Radius;
            float cz = 0.0f;
            float cr = cx * cx + cy * cy;
            if (cr > 1.0)
            {
                float cs = 1.0f / MathF.Sqrt(r);
                cx *= cs;
                cy *= cs;
                cz = 0.0f;
            }
            else
            {
                cz = MathF.Sqrt(1.0f - cr);
            }
            Vector3d p1 = new Vector3d(x, y, z);
            Vector3d p2 = new Vector3d(cx, cy, cz);
            float theta = MathF.Acos((float)Vector3d.Dot(p1, p2) / (float)(p1.Length * p2.Length));
            return theta;
        }
        /*public float GetRotateAngle(float screenX, float screenY, float screenCenterX, float screenCenterY)
        {
            float x = (screenX - screenCenterX) / this.Radius;
            float y = (screenY - screenCenterY) / this.Radius;
            
        }*/
    }
}
