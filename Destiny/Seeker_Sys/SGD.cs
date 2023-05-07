using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Destiny.Seeker_Sys
{
    public class SGD
    {
        private readonly Func<double[], double> _f;
        private readonly double[] _xn;
        private readonly double _learningRate;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="f">多変数関数</param>
        /// <param name="initialX">初期値</param>
        /// <param name="learningRate">学習係数</param>
        public SGD(Func<double[], double> f, double[] initialX, double learningRate)
        {
            ValidateArguments(initialX, learningRate);

            _f = f;
            _xn = new double[initialX.Length];
            Array.Copy(initialX, _xn, _xn.Length);
            _learningRate = learningRate;
        }

        private static void ValidateArguments(double[] initialX, double learningRate)
        {
            if (initialX == null)
            {
                throw new ArgumentNullException("initialX");
            }
            if (initialX.Length <= 1)
            {
                throw new ArgumentOutOfRangeException("initialX", "initialX length must be more than 1");
            }
            if (learningRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException("learningRate", "learningRate must be positive");
            }
        }

        /// <summary>
        /// 現在の解を取得するためのプロパティ
        /// </summary>
        public double[] Xn
        {
            get { return _xn; }
        }

        /// <summary>
        /// 最急降下法を利用して多変数関数の最小値を探索するメソッド
        /// </summary>
        /// <param name="f">多変数関数</param>
        /// <param name="initialX">初期値</param>
        /// <param name="iteration">計算回数</param>
        /// <param name="learningRate">学習係数</param>
        /// <returns>関数の最小値を与える変数</returns>
        public static double[] Compute(List<Func<double[], double>> fs, double[] initialX, int iteration, double learningRate)
        {
            ValidateArguments(initialX, iteration, learningRate);

            var xn = new double[initialX.Length];
            Array.Copy(initialX, xn, xn.Length);
            Random r  = new Random();
            for (int i = 0; i < iteration; ++i)
            {
                var randNum = r.Next(0, fs.Count);
                double[] gradient = Gradient.Compute(fs[randNum], xn);

                for (int j = 0; j < xn.Length; ++j)
                {
                    xn[j] -= learningRate * gradient[j];
                }
            }

            return xn;
        }

        private static void ValidateArguments(double[] initialX, int iteration, double learningRate)
        {
            ValidateArguments(initialX, learningRate);

            if (iteration <= 0)
            {
                throw new ArgumentOutOfRangeException("iteration", "iteration must be positive");
            }
        }

        /// <summary>
        /// 現在の解を更新するメソッド
        /// </summary>]

        public void Update()
        {
            double[] gradient = Gradient.Compute(_f, _xn);

            for (int i = 0; i < _xn.Length; ++i)
            {
                _xn[i] -= _learningRate * gradient[i];
            }
        }
    }
}
