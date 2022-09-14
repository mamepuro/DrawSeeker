using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Destiny.Seeker_Sys
{
    public class Gradient
    {
        private const double _h = 1e-5;

        public static double[] Compute(Func<double[], double> f, double[] x)
        {
            var gradient = new double[x.Length];
            for (int i = 0; i < gradient.Length; ++i)
            {
                gradient[i] = PartialDerivative.Compute(f, x, i);
            }

            return gradient;
        }
    }

}
