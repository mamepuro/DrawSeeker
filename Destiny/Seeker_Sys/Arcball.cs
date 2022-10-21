using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

            }
            return MathF.Sqrt((this.Radius) * (this.Radius) - screenX * screenX - screenY * screenY);
        }

        public float GetRotateAngle(float screenX, float screenY, float screenCenterX, float screenCenterY)
        {
            float x = (screenX - screenCenterX) / this.Radius;
            float y = (screenY - screenCenterY) / this.Radius;
            
        }
    }
}
