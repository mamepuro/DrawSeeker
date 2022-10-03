using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.IO;

namespace Destiny
{
    internal class Seeker_MainSystem
    {
        /// <summary>
        /// ユニットの3Dモデルの右端の頂点インデックス
        /// </summary>
        private static int rightEndVertexIndex = 0;
        /// <summary>
        /// ユニットの3Dモデルの左端の頂点インデックス
        /// </summary>
        private static int leftEndVertexIndex = 0;
        /// <summary>
        /// ユニットの3Dモデルの一番上の頂点インデックス
        /// </summary>
        private static int topVertexIndex = 0;
        /// <summary>
        /// ユニットの外周上(エッジ上)に存在する頂点のインデックス
        /// </summary>
        private static HashSet<int> VertexIndexOnUnitEdges = new HashSet<int>();
        public static void LoadObjFlie(string filename, List<Vertex> vertices, List<int[]> faces, float angle, float scale)
        {
            byte iD = 0;
            //頂点配列を使用可能にする
            //GL.EnableClientState(ArrayCap.VertexArray);
            //GL.VertexPointer(3, VertexPointerType.Float, 0, vertex);
            using (StreamReader streamReader = new StreamReader(filename))
            {
                while (streamReader.EndOfStream == false)
                {
                    string line = streamReader.ReadLine();
                    //頂点情報が記載されている場合
                    if (line.StartsWith("v "))
                    {
                        string[] points = line.Split(' ');
                        //頂点座標がxyzの3点分ない場合
                        if (points.Length != 4)
                        {
                            Console.WriteLine("ERROR! OBJファイルの頂点フォーマットが読み込めません");
                        }
                        else
                        {
                            Vertex newVertex = new Vertex(iD, double.Parse(points[1]), double.Parse(points[2]), double.Parse(points[3]));
                            vertices.Add(newVertex);
                            iD++;
                        }
                    }
                    //面情報
                    else if (line.StartsWith("f"))
                    {

                        string[] param = line.Split(' ');
                        //面が三角形で張られていない場合
                        if (param.Length != 4)
                        {
                            //Console.WriteLine(line);
                            Console.WriteLine("ERROR! OBJファイルの面のフォーマットが三角形ではありません");
                        }
                        else
                        {
                            int[] indexes = new int[3];
                            //インデックスが1ずれるので-1をする。(0スタートと1スタートの違い)
                            indexes[0] = int.Parse((param[1].Split('/'))[0]) - 1;
                            indexes[1] = int.Parse((param[2].Split('/'))[0]) - 1;
                            indexes[2] = int.Parse((param[3].Split('/'))[0]) - 1;
                            int[] edgeInfo = new int[] { indexes[0], indexes[1], indexes[2] };
                            faces.Add(edgeInfo);
                            //頂点の描画a

                            //面の描画



                        }

                    }
                }
            }
            AddVertexConnectionInfomation(faces, vertices);
            SetEndVertexInformation(vertices);
            SetVertexIndexOnUnitEdges(vertices);

        }
        public static void GetTriangleUnitObjFile(int split, string fileName)
        {
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = -0.5f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            float endYPos = Seeker_Sys.Seeker_ShapeData.OCTO_radius;
            //行あたりの増加分y座標
            float _updateHeight = endYPos / (split + 1);
            //ここから頂点計算
            for (int column = 0; column <= (split + 1); column++)
            {
                //行を何等分するか
                int _columnsplit = (split + 1) - column;
                //行内に点が一点のみの場合(最後の行の場合)
                if (_columnsplit == 0)
                {
                    vertexPosX[index] = startXPos;
                    vertexPosY[index] = startYPos + (column * _updateHeight);
                    index++;
                }
                else
                {
                    startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    for (int row = 0; row <= _columnsplit; row++)
                    {
                        vertexPosX[index] = startXPos + (row * _updateWidth);
                        vertexPosY[index] = startYPos + (column * _updateHeight);
                        index++;
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = (vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = (vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            //ここまで
            //ここからObjファイル生成
            using (StreamWriter streamWriter = new StreamWriter(@"testData.obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    streamWriter.WriteLine("v" +
                        " " + vertexPosX[vertexPoint] + " "
                        + vertexPosY[vertexPoint] + " "
                        + "0.0");
                }
                streamWriter.WriteLine("vn 0 0 1");
                for (int column = 0; column < split + 1; column++)
                {
                    //下の行がいくつ頂点を持っているか
                    int _LowercolumnVertexPoints = (split + 1) - column + 1;
                    int _UppercolumnVertexPoints = (split + 1) - column;
                    int _LowercolumnPointsIndex = ENDOfLowerVertexPoint;
                    int _UppercolumnPointsIndex = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    ENDOfLowerVertexPoint = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    int _columnsplit = (split + 1) - column;
                    //行内の三角形の個数
                    int TrianglesInColumn = _columnsplit * 2 - 1;
                    for (int triangleIndex = 0; triangleIndex < TrianglesInColumn; triangleIndex++)
                    {
                        if (triangleIndex % 2 == 0)
                        {
                            streamWriter.WriteLine("f" +
                                " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                + (_UppercolumnPointsIndex + 1) + "//1");
                            _LowercolumnPointsIndex++;
                        }
                        else
                        {
                            streamWriter.WriteLine("f" +
    " " + (_UppercolumnPointsIndex + 1) + "//1" + " "
    + (_LowercolumnPointsIndex + 1) + "//1" + " "
    + (_UppercolumnPointsIndex + 2) + "//1");
                            _UppercolumnPointsIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 頂点に接続している他の頂点の情報を追加する
        /// </summary>
        public static void AddVertexConnectionInfomation(List<int[]> faces, List<Vertex> vertices)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                int[] vertexIndex = new int[3];
                for (int vindex = 0; vindex < 3; vindex++)
                {
                    vertexIndex[vindex] = faces[i][vindex];
                }
                for (int vindex = 0; vindex < 3; vindex++)
                {
                    int[] connectVertexPair = new int[]
                    {
                        vertexIndex[(vindex + 1) % 3],
                        vertexIndex[(vindex + 2) % 3]
                    };
                    //接続情報のペアが存在しない場合新規登録
                    if (!vertices[vertexIndex[vindex]].connectVertexId.Contains(connectVertexPair))
                    {
                        vertices[vertexIndex[vindex]].connectVertexId.Add(connectVertexPair);
                    }
                }
            }
            //頂点[5]周りの接続情報表示
            Console.WriteLine("頂点[5]周りの接続情報を開示します");
            foreach (var c in vertices[5].connectVertexId)
            {
                foreach (var nc in c)
                {
                    Console.WriteLine(nc);
                }
                //Console.WriteLine(c[0] + " : "+ c[1]);
            }
        }

        /// <summary>
        /// ユニットの上端、左端、右端の頂点の情報を格納する
        /// </summary>
        /// <param name="vertices">頂点情報のリスト</param>
        public static void SetEndVertexInformation(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの端点の情報を探索を開始します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            double maxX = 0;
            double minX = 0;
            double maxY = 0;
            foreach (var v in vertices)
            {
                if (maxX < v.VertexX)
                {
                    rightEndVertexIndex = v.ID;
                    maxX = v.VertexX;
                    //continue;
                }
                if (v.VertexX < minX)
                {
                    leftEndVertexIndex = v.ID;
                    minX = v.VertexX;
                    //continue;
                }
                if (maxY < v.VertexY)
                {
                    topVertexIndex = v.ID;
                    maxY = v.VertexY;
                }
            }
            Console.WriteLine("左端は頂点 " + leftEndVertexIndex + "です。(座標 " + vertices[leftEndVertexIndex].VertexPosition.ToString() + ")");
            Console.WriteLine("右端は頂点 " + rightEndVertexIndex + "です。(座標 " + vertices[rightEndVertexIndex].VertexPosition.ToString() + ")");
            Console.WriteLine("上端は頂点 " + topVertexIndex + "です。(座標 " + vertices[topVertexIndex].VertexPosition.ToString()+ ")");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの端点の情報を探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// ユニットの外周上に存在する頂点情報を格納する
        /// </summary>
        /// <param name="vertices">頂点情報のリスト</param>
        private static void SetVertexIndexOnUnitEdges(List<Vertex> vertices)
        {
            int c = 0;
            Vector3d leftoToTop = vertices[topVertexIndex].VertexPosition - vertices[leftEndVertexIndex].VertexPosition;
            Console.WriteLine("L -> T : " + leftoToTop.ToString());
            Vector3d rightToTop = vertices[topVertexIndex].VertexPosition - vertices[rightEndVertexIndex].VertexPosition;
            Vector3d leftToRight = vertices[rightEndVertexIndex].VertexPosition - vertices[leftEndVertexIndex].VertexPosition;
            Console.WriteLine("L -> R : " + leftToRight.ToString());
            Vector3d leftVertex = vertices[leftEndVertexIndex].VertexPosition;
            Vector3d rightVertex = vertices[rightEndVertexIndex].VertexPosition;
            Vector3d topVertex = vertices[topVertexIndex].VertexPosition;
            Console.WriteLine("~~~~~~~~~~~ユニットの外周上の点の探索を開始します。~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("左端は頂点 " + leftEndVertexIndex + "です。(座標 " + leftVertex.ToString() + ")");
            Console.WriteLine("右端は頂点 " + rightEndVertexIndex + "です。(座標 " + rightVertex.ToString() + ")");
            Console.WriteLine("上端は頂点 " + topVertexIndex + "です。(座標 " + topVertex.ToString() + ")");
            foreach (var v in vertices)
            {
                
                Vector3d lv = v.VertexPosition - leftVertex;
                Vector3d rv = v.VertexPosition - rightVertex;
                Vector3d tv = v.VertexPosition - topVertex;
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = "+ Vector3d.Dot(lv, leftoToTop).ToString()) ;
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = " + Vector3d.Dot(lv, leftToRight).ToString());
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = " + Vector3d.Dot(lv, rightToTop).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, leftoToTop).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, leftToRight).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, rightToTop).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, leftoToTop).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, leftToRight).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, rightToTop).ToString());
                if (Vector3d.Dot(lv, leftToRight) == 1 || Vector3d.Dot(lv, leftoToTop) == 1 || Vector3d.Dot(lv, rightToTop) == 1
                    || Vector3d.Dot(lv, leftToRight) == -1 || Vector3d.Dot(lv, leftoToTop) == -1 || Vector3d.Dot(lv, rightToTop) == -1
                     || Vector3d.Dot(rv, leftToRight) == 1 || Vector3d.Dot(rv, leftoToTop) == 1 || Vector3d.Dot(rv, rightToTop) == 1
                      || Vector3d.Dot(rv, leftToRight) == -1 || Vector3d.Dot(rv, leftoToTop) == -1 || Vector3d.Dot(rv, rightToTop) == -1
                       || Vector3d.Dot(tv, leftToRight) == 1 || Vector3d.Dot(tv, leftoToTop) == 1 || Vector3d.Dot(tv, rightToTop) == 1
                        || Vector3d.Dot(tv, leftToRight) == -1 || Vector3d.Dot(tv, leftoToTop) == -1 || Vector3d.Dot(tv, rightToTop) == -1)
                {
                    if(!VertexIndexOnUnitEdges.Contains(v.ID))
                    {
                        VertexIndexOnUnitEdges.Add(v.ID);
                        Console.WriteLine("ユニットの外周上の点として、頂点 "+v.ID.ToString() +"を登録しました。");
                    }
                }
                c++;
            }
            Console.WriteLine("~~~~~~~~~~~ユニットの外周上の点の探索を終了します。~~~~~~~~~~~~~~~~~~");

        }

        private void ShowPositions(double[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (i % 3 == 0)
                {
                    Console.WriteLine("v" + ((int)(i / 3)).ToString() + "X  :" + list[i].ToString());
                }
                if (i % 3 == 1)
                {
                    Console.WriteLine("v" + ((int)(i / 3)).ToString() + "Y  :" + list[i].ToString());
                }
                else
                {
                    Console.WriteLine("v" + ((int)(i / 3)).ToString() + "Z  :" + list[i].ToString());
                }
            }
        }

        /// <summary>
        /// 2つのベクトルのなす角のArcCosを返す。ただし、渡す変数はベクトルであるためあらかじめベクトルを計算しておかなければならない
        /// </summary>
        /// <param name="Vx1">ベクトル1のx成分</param>
        /// <param name="Vy1">ベクトル1のy成分</param>
        /// <param name="Vz1">ベクトル1のz成分</param>
        /// <param name="Vx2">ベクトル2のx成分</param>
        /// <param name="Vy2">ベクトル2のy成分</param>
        /// <param name="Vz2">ベクトル2のz成分</param>
        /// <returns>2つのベクトルのなす角(単位はラジアン)</returns>
        public static double GetArcCos(double Vx1, double Vy1, double Vz1,
            double Vx2, double Vy2, double Vz2)
        {
            double ret = Math.Acos(
                (Vx1 * Vx2 + Vy1 * Vy2 + Vz1 * Vz2)
                /
                (
                    (
                    Math.Sqrt(Vx1 * Vx1 + Vy1 * Vy1 + Vz1 * Vz1)
                    )
                    *
                    (
                    Math.Sqrt(Vx2 * Vx2 + Vy2 * Vy2 + Vz2 * Vz2)
                    )

                )
                );

            return ret;
        }
        /// <summary>
        /// 再急降下法により可展性を維持した形状に変形する
        /// </summary>
        /// <param name="verteices">ユニットのすべての頂点情報を持つリスト</param>
        /// <param name="manipulatedVertexPointIndex">内部頂点のリスト(予定)</param>
        public static void SetAdjustedUnitVertexes(List<Vertex> verteices, int manipulatedVertexPointIndex)
        {
            HashSet<int> connectVertxPoint = new HashSet<int>();
            List<Vertex> connectVertex = new List<Vertex>();
            foreach (var pair in verteices[manipulatedVertexPointIndex].connectVertexId)
            {
                foreach (var point in pair)
                {
                    if (!connectVertxPoint.Contains(point))
                    {
                        connectVertxPoint.Add(point);
                    }
                }
            }
            foreach (var point in connectVertxPoint)
            {
                connectVertex.Add(verteices[point]);
            }

            Vector3d v1 = verteices[4].VertexPosition - verteices[5].VertexPosition;//0~2
            Vector3d v2 = verteices[1].VertexPosition - verteices[5].VertexPosition;//3~5
            Vector3d v3 = verteices[2].VertexPosition - verteices[5].VertexPosition;//6~8
            Vector3d v4 = verteices[6].VertexPosition - verteices[5].VertexPosition;//9~11
            Vector3d v5 = verteices[7].VertexPosition - verteices[5].VertexPosition;//12~14
            Vector3d v6 = verteices[8].VertexPosition - verteices[5].VertexPosition;//15~17
            double ans =
    Math.Acos((v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z)
    / (Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z)
    * Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z)))

    + Math.Acos((v3.X * v2.X + v3.Y * v2.Y + v3.Z * v2.Z)
    / (Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z)
    * Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z)))

    + Math.Acos((v3.X * v4.X + v3.Y * v4.Y + v3.Z * v4.Z)
    / (Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z)
    * Math.Sqrt(v4.X * v4.X + v4.Y * v4.Y + v4.Z * v4.Z)))

    + Math.Acos((v1.X * v5.X + v1.Y * v5.Y + v1.Z * v5.Z)
    / (Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z)
    * Math.Sqrt(v5.X * v5.X + v5.Y * v5.Y + v5.Z * v5.Z)))

    + Math.Acos((v6.X * v5.X + v6.Y * v5.Y + v6.Z * v5.Z)
    / (Math.Sqrt(v6.X * v6.X + v6.Y * v6.Y + v6.Z * v6.Z)
    * Math.Sqrt(v5.X * v5.X + v5.Y * v5.Y + v5.Z * v5.Z)))

    + Math.Acos((v6.X * v4.X + v6.Y * v4.Y + v6.Z * v4.Z)
    / (Math.Sqrt(v6.X * v6.X + v6.Y * v6.Y + v6.Z * v6.Z)
    * Math.Sqrt(v4.X * v4.X + v4.Y * v4.Y + v4.Z * v4.Z)));

            //再急降下法の損失関数の式
            Func<double[], double> f = (double[] x) =>
            {
                double arcCosSum = 0;
                foreach (var pair in verteices[manipulatedVertexPointIndex].connectVertexId)
                {
                    int pos1XIndex = pair[0] * 3;
                    int pos1YIndex = pair[0] * 3 + 1;
                    int pos1ZIndex = pair[0] * 3 + 2;
                    int pos2XIndex = pair[1] * 3;
                    int pos2YIndex = pair[1] * 3 + 1;
                    int pos2ZIndex = pair[1] * 3 + 2;
                    int centerXIndex = manipulatedVertexPointIndex * 3;
                    int centerYIndex = manipulatedVertexPointIndex * 3 + 1;
                    int centerZIndex = manipulatedVertexPointIndex * 3 + 2;
                    if (VertexIndexOnUnitEdges.Contains(pair[0]))
                    {
                        arcCosSum += GetArcCos(
                        verteices[pair[0]].VertexX - x[centerXIndex],
                        verteices[pair[0]].VertexY - x[centerYIndex],
                        x[pos1ZIndex] - x[centerZIndex],
                        x[pos2XIndex] - x[centerXIndex],
                        x[pos2YIndex] - x[centerYIndex],
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                    {
                        arcCosSum += GetArcCos(
                        x[pos1XIndex] - x[centerXIndex],
                        x[pos1YIndex] - x[centerYIndex],
                        x[pos1ZIndex] - x[centerZIndex],
                        verteices[pair[1]].VertexX - x[centerXIndex],
                        verteices[pair[1]].VertexY - x[centerYIndex],
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    else
                    {
                        arcCosSum += GetArcCos(
                        x[pos1XIndex] - x[centerXIndex],
                        x[pos1YIndex] - x[centerYIndex],
                        x[pos1ZIndex] - x[centerZIndex],
                        x[pos2XIndex] - x[centerXIndex],
                        x[pos2YIndex] - x[centerYIndex],
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }

                }
                return (2 * Math.PI - arcCosSum) * (2 * Math.PI - arcCosSum);
            };

            //とりあえず100頂点分用意する
            var initialX = new double[300];
            //頂点数*3個分変数を再急降下法用に格納する
            for (int initialXIndex = 0; initialXIndex < 3 * verteices.Count; initialXIndex++)
            {
                if (initialXIndex % 3 == 0)
                {
                    initialX[initialXIndex] = verteices[(int)(initialXIndex / 3)].VertexX;
                }
                else if (initialXIndex % 3 == 1)
                {
                    initialX[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexY;
                }
                else
                {
                    initialX[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexZ;
                }
            }
            int iteration = 100;
            double learningRate = 0.01;
            double[] answer = Seeker_Sys.SteepestDescentMethodMV.Compute(f, initialX, iteration, learningRate);
            //最適化処理の結果を格納する
            for (int i = 0; i < verteices.Count * 3; i++)
            {
                {
                    if (i % 3 == 0)
                    {
                        verteices[(int)(i / 3)].VertexX = answer[i];
                    }
                    else if (i % 3 == 1)
                    {
                        verteices[(int)(i / 3)].VertexY = answer[i];
                    }
                    else
                    {
                        verteices[(int)(i / 3)].VertexZ = answer[i];
                    }
                }
            }
            for (int i = 0; i < verteices.Count; i++)
            {
                verteices[i].VertexPosition = new Vector3d(verteices[i].VertexX, verteices[i].VertexY, verteices[i].VertexZ);
            }
            v1 = verteices[4].VertexPosition - verteices[5].VertexPosition;//0~2
            v2 = verteices[1].VertexPosition - verteices[5].VertexPosition;//3~5
            v3 = verteices[2].VertexPosition - verteices[5].VertexPosition;//6~8
            v4 = verteices[6].VertexPosition - verteices[5].VertexPosition;//9~11
            v5 = verteices[7].VertexPosition - verteices[5].VertexPosition;//12~14
            v6 = verteices[8].VertexPosition - verteices[5].VertexPosition;//15~17
            ans =
    Math.Acos((v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z)
    / (Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z)
    * Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z)))

    + Math.Acos((v3.X * v2.X + v3.Y * v2.Y + v3.Z * v2.Z)
    / (Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z)
    * Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z)))

    + Math.Acos((v3.X * v4.X + v3.Y * v4.Y + v3.Z * v4.Z)
    / (Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z)
    * Math.Sqrt(v4.X * v4.X + v4.Y * v4.Y + v4.Z * v4.Z)))

    + Math.Acos((v1.X * v5.X + v1.Y * v5.Y + v1.Z * v5.Z)
    / (Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z)
    * Math.Sqrt(v5.X * v5.X + v5.Y * v5.Y + v5.Z * v5.Z)))

    + Math.Acos((v6.X * v5.X + v6.Y * v5.Y + v6.Z * v5.Z)
    / (Math.Sqrt(v6.X * v6.X + v6.Y * v6.Y + v6.Z * v6.Z)
    * Math.Sqrt(v5.X * v5.X + v5.Y * v5.Y + v5.Z * v5.Z)))

    + Math.Acos((v6.X * v4.X + v6.Y * v4.Y + v6.Z * v4.Z)
    / (Math.Sqrt(v6.X * v6.X + v6.Y * v6.Y + v6.Z * v6.Z)
    * Math.Sqrt(v4.X * v4.X + v4.Y * v4.Y + v4.Z * v4.Z)));
            Console.WriteLine((ans * (180 / Math.PI)).ToString() + ":");
        }
    }
}
