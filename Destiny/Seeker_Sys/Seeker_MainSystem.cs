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
    internal  class Seeker_MainSystem
    {
        public static void LoadObjFlie(string filename, List<Vertex> vertices, List<int[]> faces, float angle, float scale)
        {
            byte iD = 0;
            //頂点配列を使用可能にする
            GL.EnableClientState(ArrayCap.VertexArray);
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
                            //頂点の描画

                            //面の描画
                            

                            
                        }

                    }
                }
            }
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

        private void ShowPositions(double[] list)
        {
            for(int i =0;i < list.Length;i++)
            {
                if(i % 3 == 0)
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
        public static void SetAdjustedUnitVertexes(List<Vertex> verteices)
        {
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

            Console.WriteLine((ans * (180 / Math.PI)).ToString() + ":");
            /*double ans =360 -
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
            ans = (180 / Math.PI) * ans;**/
            //index % 3 = 0 => x座標
            //index % 3 = 1 => y座標
            //index % 3 = 2 => z座標

            /*Func<double[], double> f = x =>
            (360 - ((Math.Acos(((x[0] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[0] - verteices[5].VertexPosition.X) * (x[0] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[1] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[2] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[3] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[4] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[5] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[6] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[6] - verteices[5].VertexPosition.X) * (x[6] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[7] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[8] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[3] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[4] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[5] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[6] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[6] - verteices[5].VertexPosition.X) * (x[6] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[7] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[8] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[9] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[10] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[11] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[0] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[0] - verteices[5].VertexPosition.X) * (x[0] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[1] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[2] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[12] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[13] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[14] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[15] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[15] - verteices[5].VertexPosition.X) * (x[15] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[16] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[17] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[12] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[13] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[14] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[15] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[15] - verteices[5].VertexPosition.X) * (x[15] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[16] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[17] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[9] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[10] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[11] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))))) * (180 / Math.PI))) *
            (360 - ((Math.Acos(((x[0] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[0] - verteices[5].VertexPosition.X) * (x[0] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[1] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[2] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[3] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[4] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[5] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[6] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[6] - verteices[5].VertexPosition.X) * (x[6] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[7] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[8] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[3] - verteices[5].VertexPosition.X) * (x[3] - verteices[5].VertexPosition.X) + (x[4] - verteices[5].VertexPosition.Y) * (x[4] - verteices[5].VertexPosition.Y) + (x[5] - verteices[5].VertexPosition.Z) * (x[5] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[6] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[6] - verteices[5].VertexPosition.X) * (x[6] - verteices[5].VertexPosition.X) + (x[7] - verteices[5].VertexPosition.Y) * (x[7] - verteices[5].VertexPosition.Y) + (x[8] - verteices[5].VertexPosition.Z) * (x[8] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[9] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[10] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[11] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[0] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[0] - verteices[5].VertexPosition.X) * (x[0] - verteices[5].VertexPosition.X) + (x[1] - verteices[5].VertexPosition.Y) * (x[1] - verteices[5].VertexPosition.Y) + (x[2] - verteices[5].VertexPosition.Z) * (x[2] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[12] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[13] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[14] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[15] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[15] - verteices[5].VertexPosition.X) * (x[15] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[16] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[17] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[12] - verteices[5].VertexPosition.X) * (x[12] - verteices[5].VertexPosition.X) + (x[13] - verteices[5].VertexPosition.Y) * (x[13] - verteices[5].VertexPosition.Y) + (x[14] - verteices[5].VertexPosition.Z) * (x[14] - verteices[5].VertexPosition.Z))))

            + Math.Acos(((x[15] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))
            / (Math.Sqrt((x[15] - verteices[5].VertexPosition.X) * (x[15] - verteices[5].VertexPosition.X) + (x[16] - verteices[5].VertexPosition.Y) * (x[16] - verteices[5].VertexPosition.Y) + (x[17] - verteices[5].VertexPosition.Z) * (x[17] - verteices[5].VertexPosition.Z))
            * Math.Sqrt((x[9] - verteices[5].VertexPosition.X) * (x[9] - verteices[5].VertexPosition.X) + (x[10] - verteices[5].VertexPosition.Y) * (x[10] - verteices[5].VertexPosition.Y) + (x[11] - verteices[5].VertexPosition.Z) * (x[11] - verteices[5].VertexPosition.Z))))) * (180 / Math.PI)));*/
            Func<double[], double> f = x =>
            (2 * Math.PI - ((Math.Acos(((x[0] - x[18]) * (x[3] - x[18]) + (x[1] - x[19]) * (x[4] - x[19]) + (x[2] - x[20]) * (x[5] - x[20]))
            / (Math.Sqrt((x[0] - x[18]) * (x[0] - x[18]) + (x[1] - x[19]) * (x[1] - x[19]) + (x[2] - x[20]) * (x[2] - x[20]))
            * Math.Sqrt((x[3] - x[18]) * (x[3] - x[18]) + (x[4] - x[19]) * (x[4] - x[19]) + (x[5] - x[20]) * (x[5] - x[20]))))

            + Math.Acos(((x[6] - x[18]) * (x[3] - x[18]) + (x[7] - x[19]) * (x[4] - x[19]) + (x[8] - x[20]) * (x[5] - x[20]))
            / (Math.Sqrt((x[6] - x[18]) * (x[6] - x[18]) + (x[7] - x[19]) * (x[7] - x[19]) + (x[8] - x[20]) * (x[8] - x[20]))
            * Math.Sqrt((x[3] - x[18]) * (x[3] - x[18]) + (x[4] - x[19]) * (x[4] - x[19]) + (x[5] - x[20]) * (x[5] - x[20]))))

            + Math.Acos(((x[6] - x[18]) * (x[9] - x[18]) + (x[7] - x[19]) * (x[10] - x[19]) + (x[8] - x[20]) * (x[11] - x[20]))
            / (Math.Sqrt((x[6] - x[18]) * (x[6] - x[18]) + (x[7] - x[19]) * (x[7] - x[19]) + (x[8] - x[20]) * (x[8] - x[20]))
            * Math.Sqrt((x[9] - x[18]) * (x[9] - x[18]) + (x[10] - x[19]) * (x[10] - x[19]) + (x[11] - x[20]) * (x[11] - x[20]))))

            + Math.Acos(((x[0] - x[18]) * (x[12] - x[18]) + (x[1] - x[19]) * (x[13] - x[19]) + (x[2] - x[20]) * (x[14] - x[20]))
            / (Math.Sqrt((x[0] - x[18]) * (x[0] - x[18]) + (x[1] - x[19]) * (x[1] - x[19]) + (x[2] - x[20]) * (x[2] - x[20]))
            * Math.Sqrt((x[12] - x[18]) * (x[12] - x[18]) + (x[13] - x[19]) * (x[13] - x[19]) + (x[14] - x[20]) * (x[14] - x[20]))))

            + Math.Acos(((x[15] - x[18]) * (x[12] - x[18]) + (x[16] - x[19]) * (x[13] - x[19]) + (x[17] - x[20]) * (x[14] - x[20]))
            / (Math.Sqrt((x[15] - x[18]) * (x[15] - x[18]) + (x[16] - x[19]) * (x[16] - x[19]) + (x[17] - x[20]) * (x[17] - x[20]))
            * Math.Sqrt((x[12] - x[18]) * (x[12] - x[18]) + (x[13] - x[19]) * (x[13] - x[19]) + (x[14] - x[20]) * (x[14] - x[20]))))

            + Math.Acos(((x[15] - x[18]) * (x[9] - x[18]) + (x[16] - x[19]) * (x[10] - x[19]) + (x[17] - x[20]) * (x[11] - x[20]))
            / (Math.Sqrt((x[15] - x[18]) * (x[15] - x[18]) + (x[16] - x[19]) * (x[16] - x[19]) + (x[17] - x[20]) * (x[17] - x[20]))
            * Math.Sqrt((x[9] - x[18]) * (x[9] - x[18]) + (x[10] - x[19]) * (x[10] - x[19]) + (x[11] - x[20]) * (x[11] - x[20]))))))) *
            (2 * Math.PI - ((Math.Acos(((x[0] - x[18]) * (x[3] - x[18]) + (x[1] - x[19]) * (x[4] - x[19]) + (x[2] - x[20]) * (x[5] - x[20]))
            / (Math.Sqrt((x[0] - x[18]) * (x[0] - x[18]) + (x[1] - x[19]) * (x[1] - x[19]) + (x[2] - x[20]) * (x[2] - x[20]))
            * Math.Sqrt((x[3] - x[18]) * (x[3] - x[18]) + (x[4] - x[19]) * (x[4] - x[19]) + (x[5] - x[20]) * (x[5] - x[20]))))

            + Math.Acos(((x[6] - x[18]) * (x[3] - x[18]) + (x[7] - x[19]) * (x[4] - x[19]) + (x[8] - x[20]) * (x[5] - x[20]))
            / (Math.Sqrt((x[6] - x[18]) * (x[6] - x[18]) + (x[7] - x[19]) * (x[7] - x[19]) + (x[8] - x[20]) * (x[8] - x[20]))
            * Math.Sqrt((x[3] - x[18]) * (x[3] - x[18]) + (x[4] - x[19]) * (x[4] - x[19]) + (x[5] - x[20]) * (x[5] - x[20]))))

            + Math.Acos(((x[6] - x[18]) * (x[9] - x[18]) + (x[7] - x[19]) * (x[10] - x[19]) + (x[8] - x[20]) * (x[11] - x[20]))
            / (Math.Sqrt((x[6] - x[18]) * (x[6] - x[18]) + (x[7] - x[19]) * (x[7] - x[19]) + (x[8] - x[20]) * (x[8] - x[20]))
            * Math.Sqrt((x[9] - x[18]) * (x[9] - x[18]) + (x[10] - x[19]) * (x[10] - x[19]) + (x[11] - x[20]) * (x[11] - x[20]))))

            + Math.Acos(((x[0] - x[18]) * (x[12] - x[18]) + (x[1] - x[19]) * (x[13] - x[19]) + (x[2] - x[20]) * (x[14] - x[20]))
            / (Math.Sqrt((x[0] - x[18]) * (x[0] - x[18]) + (x[1] - x[19]) * (x[1] - x[19]) + (x[2] - x[20]) * (x[2] - x[20]))
            * Math.Sqrt((x[12] - x[18]) * (x[12] - x[18]) + (x[13] - x[19]) * (x[13] - x[19]) + (x[14] - x[20]) * (x[14] - x[20]))))

            + Math.Acos(((x[15] - x[18]) * (x[12] - x[18]) + (x[16] - x[19]) * (x[13] - x[19]) + (x[17] - x[20]) * (x[14] - x[20]))
            / (Math.Sqrt((x[15] - x[18]) * (x[15] - x[18]) + (x[16] - x[19]) * (x[16] - x[19]) + (x[17] - x[20]) * (x[17] - x[20]))
            * Math.Sqrt((x[12] - x[18]) * (x[12] - x[18]) + (x[13] - x[19]) * (x[13] - x[19]) + (x[14] - x[20]) * (x[14] - x[20]))))

            + Math.Acos(((x[15] - x[18]) * (x[9] - x[18]) + (x[16] - x[19]) * (x[10] - x[19]) + (x[17] - x[20]) * (x[11] - x[20]))
            / (Math.Sqrt((x[15] - x[18]) * (x[15] - x[18]) + (x[16] - x[19]) * (x[16] - x[19]) + (x[17] - x[20]) * (x[17] - x[20]))
            * Math.Sqrt((x[9] - x[18]) * (x[9] - x[18]) + (x[10] - x[19]) * (x[10] - x[19]) + (x[11] - x[20]) * (x[11] - x[20])))))));
            var initialX = new double[]
            {
                verteices[4].VertexX,
                verteices[4].VertexY,
                verteices[4].VertexZ,

                verteices[1].VertexX,
                verteices[1].VertexY,
                verteices[1].VertexZ,

                verteices[2].VertexX,
                verteices[2].VertexY,
                verteices[2].VertexZ,

                verteices[6].VertexX,
                verteices[6].VertexY,
                verteices[6].VertexZ,

                verteices[7].VertexX,
                verteices[7].VertexY,
                verteices[7].VertexZ,

                verteices[8].VertexX,
                verteices[8].VertexY,
                verteices[8].VertexZ,

                verteices[5].VertexX,
                verteices[5].VertexY,
                verteices[5].VertexZ,
            };
            int iteration = 10000;
            double learningRate = 0.01;
            double[] answer = Seeker_Sys.SteepestDescentMethodMV.Compute(f, initialX, iteration, learningRate);
            verteices[4].VertexX = answer[0];
            verteices[4].VertexY = answer[1];
            verteices[4].VertexZ = answer[2];
            verteices[1].VertexX = answer[3];
            verteices[1].VertexY = answer[4];
            verteices[1].VertexZ = answer[5];
            verteices[2].VertexX = answer[6];
            verteices[2].VertexY = answer[7];
            verteices[2].VertexZ = answer[8];
            verteices[6].VertexX = answer[9];
            verteices[6].VertexY = answer[10];
            verteices[6].VertexZ = answer[11];
            verteices[7].VertexX = answer[12];
            verteices[7].VertexY = answer[13];
            verteices[7].VertexZ = answer[14];
            verteices[8].VertexX = answer[15];
            verteices[8].VertexY = answer[16];
            verteices[8].VertexZ = answer[17];
            verteices[5].VertexX = answer[18];
            verteices[5].VertexY = answer[19];
            verteices[5].VertexZ = answer[20];
            verteices[4].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[1].VertexPosition = new Vector3d(verteices[1].VertexX, verteices[1].VertexY, verteices[1].VertexZ);
            verteices[2].VertexPosition = new Vector3d(verteices[2].VertexX, verteices[2].VertexY, verteices[2].VertexZ);
            verteices[6].VertexPosition = new Vector3d(verteices[6].VertexX, verteices[6].VertexY, verteices[6].VertexZ);
            verteices[7].VertexPosition = new Vector3d(verteices[7].VertexX, verteices[7].VertexY, verteices[7].VertexZ);
            verteices[8].VertexPosition = new Vector3d(verteices[8].VertexX, verteices[8].VertexY, verteices[8].VertexZ);
            verteices[5].VertexPosition = new Vector3d(verteices[5].VertexX, verteices[5].VertexY, verteices[5].VertexZ);
            Console.WriteLine("1=" + (Math.Acos((answer[0] * answer[3] + answer[1] * answer[4] + answer[2] * answer[5])
            / (Math.Sqrt(answer[0] * answer[0] + answer[1] * answer[1] + answer[2] * answer[2])
            * Math.Sqrt(answer[3] * answer[3] + answer[4] * answer[4] + answer[5] * answer[5])))).ToString());
            Console.WriteLine("2=" + (1).ToString());
            Console.WriteLine("3=" + (1).ToString());
            Console.WriteLine("4=" + (1).ToString());
            Console.WriteLine("5=" + (1).ToString());
            Console.WriteLine("2=" + (1).ToString());
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
           
            /*
            verteices[4].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[1].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[2].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[6].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[7].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);
            verteices[8].VertexPosition = new Vector3d(verteices[4].VertexX, verteices[4].VertexY, verteices[4].VertexZ);*/
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
            /*ans =
                 (2 * Math.PI - ((Math.Acos((answer[0] * answer[3] + answer[1] * answer[4] + answer[2] * answer[5])
             / (Math.Sqrt(answer[0] * answer[0] + answer[1] * answer[1] + answer[2] * answer[2])
             * Math.Sqrt(answer[3] * answer[3] + answer[4] * answer[4] + answer[5] * answer[5])))

             + Math.Acos((answer[6] * answer[3] + answer[7] * answer[4] + answer[8] * answer[5])
             / (Math.Sqrt(answer[6] * answer[6] + answer[7] * answer[7] + answer[8] * answer[8])
             * Math.Sqrt(answer[3] * answer[3] + answer[4] * answer[4] + answer[5] * answer[5])))

             + Math.Acos((answer[6] * answer[9] + answer[7] * answer[10] + answer[8] * answer[11])
             / (Math.Sqrt(answer[6] * answer[6] + answer[7] * answer[7] + answer[8] * answer[8])
             * Math.Sqrt(answer[9] * answer[9] + answer[10] * answer[10] + answer[11] * answer[11])))

             + Math.Acos((answer[0] * answer[12] + answer[1] * answer[13] + answer[2] * answer[14])
             / (Math.Sqrt(answer[0] * answer[0] + answer[1] * answer[1] + answer[2] * answer[2])
             * Math.Sqrt(answer[12] * answer[12] + answer[13] * answer[13] + answer[14] * answer[14])))

             + Math.Acos((answer[15] * answer[12] + answer[16] * answer[13] + answer[17] * answer[14])
             / (Math.Sqrt(answer[15] * answer[15] + answer[16] * answer[16] + answer[17] * answer[17])
             * Math.Sqrt(answer[12] * answer[12] + answer[13] * answer[13] + answer[14] * answer[14])))

             + Math.Acos((answer[15] * answer[9] + answer[16] * answer[10] + answer[17] * answer[11])
             / (Math.Sqrt(answer[15] * answer[15] + answer[16] * answer[16] + answer[17] * answer[17])
             * Math.Sqrt(answer[9] * answer[9] + answer[10] * answer[10] + answer[11] * answer[11])))))) *
 (2 * Math.PI - ((Math.Acos((answer[0] * answer[3] + answer[1] * answer[4] + answer[2] * answer[5])
             / (Math.Sqrt(answer[0] * answer[0] + answer[1] * answer[1] + answer[2] * answer[2])
             * Math.Sqrt(answer[3] * answer[3] + answer[4] * answer[4] + answer[5] * answer[5])))

             + Math.Acos((answer[6] * answer[3] + answer[7] * answer[4] + answer[8] * answer[5])
             / (Math.Sqrt(answer[6] * answer[6] + answer[7] * answer[7] + answer[8] * answer[8])
             * Math.Sqrt(answer[3] * answer[3] + answer[4] * answer[4] + answer[5] * answer[5])))

             + Math.Acos((answer[6] * answer[9] + answer[7] * answer[10] + answer[8] * answer[11])
             / (Math.Sqrt(answer[6] * answer[6] + answer[7] * answer[7] + answer[8] * answer[8])
             * Math.Sqrt(answer[9] * answer[9] + answer[10] * answer[10] + answer[11] * answer[11])))

             + Math.Acos((answer[0] * answer[12] + answer[1] * answer[13] + answer[2] * answer[14])
             / (Math.Sqrt(answer[0] * answer[0] + answer[1] * answer[1] + answer[2] * answer[2])
             * Math.Sqrt(answer[12] * answer[12] + answer[13] * answer[13] + answer[14] * answer[14])))

             + Math.Acos((answer[15] * answer[12] + answer[16] * answer[13] + answer[17] * answer[14])
             / (Math.Sqrt(answer[15] * answer[15] + answer[16] * answer[16] + answer[17] * answer[17])
             * Math.Sqrt(answer[12] * answer[12] + answer[13] * answer[13] + answer[14] * answer[14])))

             + Math.Acos((answer[15] * answer[9] + answer[16] * answer[10] + answer[17] * answer[11])
             / (Math.Sqrt(answer[15] * answer[15] + answer[16] * answer[16] + answer[17] * answer[17])
             * Math.Sqrt(answer[9] * answer[9] + answer[10] * answer[10] + answer[11] * answer[11]))))));*/
            Console.WriteLine((ans*(180/Math.PI)).ToString() + ":");
        }
    }
}
