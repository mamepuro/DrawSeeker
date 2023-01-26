





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Destiny.Seeker_Sys
{
    public class PartialDerivative
    {
        private const double _h = 1e-8;

        /// <summary>
        /// 偏導関数を計算するメソッド
        /// </summary>
        /// <param name="f">関数</param>
        /// <param name="index">偏微分を行うインデックス</param>
        /// <returns>fの偏導関数</returns>
        public static Func<double[], double> Compute(Func<double[], double> f, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return x =>
            {
                if (index > x.Length - 1)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                var xCopied = new double[x.Length];
                Array.Copy(x, xCopied, x.Length);
                xCopied[index] += _h;
                double f1 = f(xCopied);
                xCopied[index] -= _h;

                xCopied[index] -= _h;
                double f2 = f(xCopied);
                return (f1 - f2) / (2.0 * _h);
            };
        }

        /// <summary>
        /// 偏微分係数を計算するメソッド
        /// </summary>
        /// <param name="f">関数</param>
        /// <param name="x">偏微分係数を計算する座標値</param>
        /// <param name="index">偏微分を行うインデックス</param>
        /// <returns>偏微分係数</returns>
        public static double Compute(Func<double[], double> f, double[] x, int index)
        {
            return Compute(f, index)(x);
        }
    }
}
