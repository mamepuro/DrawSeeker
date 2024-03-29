﻿using System;
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
using Destiny.Seeker_Sys;
using System.Collections;

namespace Destiny
{
    internal class Seeker_MainSystem
    {
        static int _split = 0; 
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
        public static HashSet<int> VertexIndexOnUnitEdges = new HashSet<int>();
        /// <summary>
        /// ユニットの底辺部分に存在する頂点のインデックス
        /// </summary>
        public static HashSet<int> VertexIndexOnUnitButtomEdge = new HashSet<int>();
        /// <summary>
        /// ユニットの内部頂点のインデックス
        /// </summary>
        public static HashSet<int> InnnerVertexIndex = new HashSet<int>();
        /// <summary>
        /// ユニットの底辺上でかつ左右端点でない頂点のインデックス
        /// </summary>
        public static HashSet<int> InnerVertexIndexOnButtomEdge = new HashSet<int>();

        /// <summary>
        /// ユニットの左辺上に存在する頂点のインデックス
        /// </summary>
        public static HashSet<int> VertexIndexOnUnitLeftEdge = new HashSet<int>();

        /// <summary>
        /// ユニットの左辺上でかつ端点でない頂点のインデックス
        /// </summary>
        public static HashSet<int> InnerVertexIndexOnUnitLeftEdge = new HashSet<int>();

        /// <summary>
        /// 固定された頂点座標のリスト
        /// </summary>
        public static HashSet<int> FixedVertexIndexes = new HashSet<int>();

        /// <summary>
        /// 固定された頂点座標のリスト
        /// </summary>
        public static HashSet<int> OKVertexIndexes = new HashSet<int>();

        /// <summary>
        /// 切り込みを許す頂点
        /// </summary>
        public static int okPoint = 13;
        /// <summary>
        /// デバッグモードが有効かどうか．　デバッグモードでは安定していないテスト機能を実行できる
        /// </summary>
        public readonly static bool isDebugging =false;

        public static double InnerBottomErrorZ = 0.0;
        public static void LoadObjFlie(string filename, List<Vertex> vertices, List<int[]> faces, float angle, float scale)
        {
            if(isDebugging)
            {
                Console.WriteLine("Debug mode ON");
            }
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
                            Console.WriteLine("OBJファイルの面のフォーマットが三角形ではありません");
                            if(isDebugging)
                            {
                                int[] indexes = new int[param.Length-1];
                                int[] edgeInfo = new int[param.Length-1];
                                //インデックスが1ずれるので-1をする。(0スタートと1スタートの違い)
                                for (int index = 1;index<param.Length;index++)
                                {
                                    indexes[index-1] = int.Parse((param[index].Split('/'))[0]) - 1;
                                    edgeInfo[index-1] = indexes[index - 1];
                                }
                                faces.Add(edgeInfo);
                            }
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
                            Console.WriteLine("頂点情報を開示します");
            foreach (var f in faces)
            {
                Console.WriteLine(string.Join(", ", f)); // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
            }
            AddVertexConnectionInfomation(faces, vertices);
            SetEndVertexInformation(vertices);
            
            SetVertexIndexOnUnitEdges(vertices);
            SetVertexIndexOnUnitButtomEdges(vertices);
            SetInnerVertex(vertices);
            SetInnerVertexOnButtomEdge(vertices);
            SetInnerVertexOnRightEdge();
            SetVertexOnLeftEdge(vertices);
            SetInnerVertexOnRightEdge();
           
            //SetVertexIndexCheatMode(vertices);
            test(vertices);
            GetAllAngle(vertices);
            if(isDebugging)
            {
                SetInnerVertexOnRightEdge();
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

        public static void GetHalfTriangleUnitObjFile(int split, string fileName)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = 0.0f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            //将来的にこれは変更する(OCTO以外も選択可能に)
            float endYPos = Seeker_Sys.Seeker_ShapeData.OCTO_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.HEXA_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.PENTA_radius;
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
                    //startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    for (int row = 0; row <= _columnsplit; row++)
                    {
                        vertexPosX[index] = startXPos + (row * _updateWidth);
                        vertexPosY[index] = startYPos + (column * _updateHeight);
                        index++;
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = 0;//(vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = vertexPosX[endIndex - 1];//(vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成が完了しました。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //ここまで
            //ここからObjファイル生成
            using (StreamWriter streamWriter = new StreamWriter(fileName+".obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    if(vertexPoint != 0)
                    {
                        if(vertexPoint % 2 == 0)
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "-0.00");
                        }
                        else
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "0.00");
                        }
                    }
                    else
                    {
                        streamWriter.WriteLine("v" +
                        " " + vertexPosX[vertexPoint] + " "
                        + vertexPosY[vertexPoint] + " "
                        + "0.0");
                    }

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

        public static void GetHalfTriangleUnitObjFiler2(int split, string fileName)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = 0.0f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            //将来的にこれは変更する(OCTO以外も選択可能に)
            float endYPos = Seeker_Sys.Seeker_ShapeData.OCTO_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.HEXA_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.PENTA_radius;
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
                    //startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    for (int row = 0; row <= _columnsplit; row++)
                    {
                        vertexPosX[index] = startXPos + (row * _updateWidth);
                        vertexPosY[index] = startYPos + (column * _updateHeight);
                        index++;
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = 0;//(vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = vertexPosX[endIndex - 1];//(vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成が完了しました。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //ここまで
            //ここからObjファイル生成
            Random rnd = new Random();

            using (StreamWriter streamWriter = new StreamWriter(fileName + ".obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    
                        if(vertexPoint < 6)
                            {
                                streamWriter.WriteLine("v" +
      " " + vertexPosX[vertexPoint] + " "
      + vertexPosY[vertexPoint] + " "
      + "0.0");
                            }
                        else
                        {
                            
                                                streamWriter.WriteLine("v" +
                    " " + vertexPosX[vertexPoint] + " "
                    + vertexPosY[vertexPoint] + " "
                    + (rnd.NextDouble() * 0.03).ToString());
                        }
  
                    

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
        public static void GetHalfTriangleUnitObjFile2(int split, string fileName)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = 0.0f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            //将来的にこれは変更する(OCTO以外も選択可能に)
            float endYPos = Seeker_Sys.Seeker_ShapeData.OCTO_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.HEXA_radius;
            //float endYPos = Seeker_Sys.Seeker_ShapeData.PENTA_radius;
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
                    //startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    for (int row = 0; row <= _columnsplit; row++)
                    {
                        vertexPosX[index] = startXPos + (row * _updateWidth);
                        vertexPosY[index] = startYPos + (column * _updateHeight);
                        index++;
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = 0;//(vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = vertexPosX[endIndex - 1];//(vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成が完了しました。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //ここまで
            //ここからObjファイル生成
            using (StreamWriter streamWriter = new StreamWriter(fileName + ".obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    if (vertexPoint != 0)
                    {
                        if (vertexPoint % 2 == 0)
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "-0.01");
                        }
                        else
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "0.01");
                        }
                    }
                    else
                    {
                        streamWriter.WriteLine("v" +
                        " " + vertexPosX[vertexPoint] + " "
                        + vertexPosY[vertexPoint] + " "
                        + "0.0");
                    }

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

        public static void GetPleatHalfTriangleUnitObjFile(int split, string fileName)
        {
            _split = split;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~プリーツユニットの三角形メッシュを作成します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = 0.0f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            //将来的にこれは変更する(OCTO以外も選択可能に)
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
                    //startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    if(column == 0)
                    {
                        for (int row = 0; row <= _columnsplit; row++)
                        {
                            vertexPosX[index] = startXPos + (row * _updateWidth);
                            vertexPosY[index] = startYPos + (column * _updateHeight);
                            index++;
                        }
                    }
                    else
                    {
                            for (int row = 0; row <= (split + 1); row++)
                            {
                                vertexPosY[index] = startYPos + (column * _updateHeight);
                                vertexPosX[index] = (vertexPosX[row] - startXPos) * (vertexPosY[index] - endYPos) / (vertexPosY[row] - endYPos);
                                index++;
                            }
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = 0;//(vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = vertexPosX[endIndex - 1];//(vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成が完了しました。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //ここまで
            //ここからObjファイル生成
            int u_row = 1;
            int count = 0;
            using (StreamWriter streamWriter = new StreamWriter(fileName + ".obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    if (vertexPoint != 0 || vertexPoint != 1 || vertexPoint < split + 1+1 || vertexPoint == index-1)
                    {
                        if ((u_row) % 2 == 0)
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "-0.1");
                        }
                        else
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "0.00");
                        }
                    }
                    else
                    {
                        streamWriter.WriteLine("v" +
                        " " + vertexPosX[vertexPoint] + " "
                        + vertexPosY[vertexPoint] + " "
                        + "0.0");
                    }
                    count++;
                    if(count >= split + 2)
                    {
                        count = 0;
                        u_row++;
                    }

                }
                streamWriter.WriteLine("vn 0 0 1");
                for (int column = 0; column < split + 1; column++)
                {
                    //下の行がいくつ頂点を持っているか
                    int _LowercolumnVertexPoints = (split + 1) + 1;
                    int _UppercolumnVertexPoints = (split + 1) - column;
                    if(_UppercolumnVertexPoints != 1)
                    {
                        _UppercolumnVertexPoints = split + 1;
                    }
                    int _LowercolumnPointsIndex = ENDOfLowerVertexPoint;
                    int _UppercolumnPointsIndex = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    ENDOfLowerVertexPoint = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    int _columnsplit = (split + 1);
                    //行内の三角形の個数
                    int TrianglesInColumn = _columnsplit * 2;
                    if(_UppercolumnVertexPoints == 1)
                    {
                        TrianglesInColumn = split + 1;
                    }
                    if(_UppercolumnVertexPoints == 1)
                    {
                        for (int triangleIndex = 0; triangleIndex < TrianglesInColumn; triangleIndex++)
                        {
                            
                                streamWriter.WriteLine("f" +
                                    " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                    + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                    + (_UppercolumnPointsIndex + 1) + "//1");
                                Console.WriteLine("f" +
                                    " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                    + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                    + (_UppercolumnPointsIndex + 1) + "//1");
                                _LowercolumnPointsIndex++;
                            
                        }
                    }
                    else
                    {
                        for (int triangleIndex = 0; triangleIndex < TrianglesInColumn; triangleIndex++)
                        {
                            if (triangleIndex % 2 == 0)
                            {
                                streamWriter.WriteLine("f" +
                                    " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                    + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                    + (_UppercolumnPointsIndex + 1) + "//1");
                                Console.WriteLine("f" +
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
                                Console.WriteLine("f" +
        " " + (_UppercolumnPointsIndex + 1) + "//1" + " "
        + (_LowercolumnPointsIndex + 1) + "//1" + " "
        + (_UppercolumnPointsIndex + 2) + "//1");
                                _UppercolumnPointsIndex++;
                            }
                        }
                    }
                }
            }

        }

        public static void GetPleatHalfTriangleUnitObjFile2(int split, string fileName)
        {
            _split = split;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~プリーツユニットの三角形メッシュを作成します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            int index = 0;
            int ENDOfLowerVertexPoint = 0;
            int startIndex;
            int endIndex;
            int VertexCount = 0;
            float[] vertexPosX = new float[10000];
            float[] vertexPosY = new float[10000];
            //始まりのx座標
            float startXPos = 0.0f;
            //終わりのx座標
            float endXPos = 0.5f;
            float startYPos = 0;
            //将来的にこれは変更する(OCTO以外も選択可能に)
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
                    //startIndex = index;
                    float _updateWidth = (endXPos - startXPos) / _columnsplit;
                    if (column == 0)
                    {
                        for (int row = 0; row <= _columnsplit; row++)
                        {
                            vertexPosX[index] = startXPos + (row * _updateWidth);
                            vertexPosY[index] = startYPos + (column * _updateHeight);
                            index++;
                        }
                    }
                    else
                    {
                        for (int row = 0; row <= (split + 1); row++)
                        {
                            vertexPosY[index] = startYPos + (column * _updateHeight);
                            vertexPosX[index] = (vertexPosX[row] - startXPos) * (vertexPosY[index] - endYPos) / (vertexPosY[row] - endYPos);
                            index++;
                        }
                    }
                    //最後に+1されてしまうので終点のindexは-1したものになる
                    endIndex = index - 1;
                    startXPos = 0;//(vertexPosX[startIndex] + vertexPosX[startIndex + 1]) / 2;
                    endXPos = vertexPosX[endIndex - 1];//(vertexPosX[endIndex - 1] + vertexPosX[endIndex]) / 2;
                }
            }
            VertexCount = index - 1;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ユニットの三角形メッシュを作成が完了しました。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            //ここまで
            //ここからObjファイル生成
            int u_row = 1;
            int count = 0;
            using (StreamWriter streamWriter = new StreamWriter(fileName + ".obj", false, Encoding.UTF8))
            {
                for (int vertexPoint = 0; vertexPoint < index; vertexPoint++)
                {
                    if (vertexPoint != 0 || vertexPoint != 1 || vertexPoint < split + 1 + 1 || vertexPoint == index - 1)
                    {
                        if ((vertexPoint) % 2 == 0)
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "-0.01");
                        }
                        else
                        {
                            streamWriter.WriteLine("v" +
                            " " + vertexPosX[vertexPoint] + " "
                            + vertexPosY[vertexPoint] + " "
                            + "0.01");
                        }
                    }
                    else
                    {
                        streamWriter.WriteLine("v" +
                        " " + vertexPosX[vertexPoint] + " "
                        + vertexPosY[vertexPoint] + " "
                        + "0.0");
                    }
                    count++;
                    if (count >= split + 2)
                    {
                        count = 0;
                        u_row++;
                    }

                }
                streamWriter.WriteLine("vn 0 0 1");
                for (int column = 0; column < split + 1; column++)
                {
                    //下の行がいくつ頂点を持っているか
                    int _LowercolumnVertexPoints = (split + 1) + 1;
                    int _UppercolumnVertexPoints = (split + 1) - column;
                    if (_UppercolumnVertexPoints != 1)
                    {
                        _UppercolumnVertexPoints = split + 1;
                    }
                    int _LowercolumnPointsIndex = ENDOfLowerVertexPoint;
                    int _UppercolumnPointsIndex = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    ENDOfLowerVertexPoint = _LowercolumnVertexPoints + ENDOfLowerVertexPoint;
                    int _columnsplit = (split + 1);
                    //行内の三角形の個数
                    int TrianglesInColumn = _columnsplit * 2;
                    if (_UppercolumnVertexPoints == 1)
                    {
                        TrianglesInColumn = split + 1;
                    }
                    if (_UppercolumnVertexPoints == 1)
                    {
                        for (int triangleIndex = 0; triangleIndex < TrianglesInColumn; triangleIndex++)
                        {

                            streamWriter.WriteLine("f" +
                                " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                + (_UppercolumnPointsIndex + 1) + "//1");
                            Console.WriteLine("f" +
                                " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                + (_UppercolumnPointsIndex + 1) + "//1");
                            _LowercolumnPointsIndex++;

                        }
                    }
                    else
                    {
                        for (int triangleIndex = 0; triangleIndex < TrianglesInColumn; triangleIndex++)
                        {
                            if (triangleIndex % 2 == 0)
                            {
                                streamWriter.WriteLine("f" +
                                    " " + (_LowercolumnPointsIndex + 1) + "//1" + " "
                                    + (_LowercolumnPointsIndex + 2) + "//1" + " "
                                    + (_UppercolumnPointsIndex + 1) + "//1");
                                Console.WriteLine("f" +
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
                                Console.WriteLine("f" +
        " " + (_UppercolumnPointsIndex + 1) + "//1" + " "
        + (_LowercolumnPointsIndex + 1) + "//1" + " "
        + (_UppercolumnPointsIndex + 2) + "//1");
                                _UppercolumnPointsIndex++;
                            }
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
                int[] vertexIndex = new int[faces[i].Length];
                for (int vindex = 0; vindex < faces[i].Length; vindex++)
                {
                    vertexIndex[vindex] = faces[i][vindex];
                }
                for (int vindex = 0; vindex < faces[i].Length; vindex++)
                {
                    int[] connectVertexPair = new int[]
                    {
                        vertexIndex[(vindex + 1) % faces[i].Length],
                        vertexIndex[(vindex + faces[i].Length - 1) % faces[i].Length]
                    };
                    //接続情報のペアが存在しない場合新規登録
                    if (!vertices[vertexIndex[vindex]].connectVertexId.Contains(connectVertexPair))
                    {
                        vertices[vertexIndex[vindex]].connectVertexId.Add(connectVertexPair);
                    }
                }
            }
            //デバッグ用
            Console.WriteLine("-------------------------接続情報の開示を行います-------------------------");
            foreach (Vertex vertex in vertices)
            {
                Console.WriteLine("_________頂点 " + vertex.ID + "___________________");
                foreach (var c in vertex.connectVertexId)
                {
                    Console.WriteLine(string.Join(", ", c));
                }
            }
        }

        /// <summary>
        /// ユニットの上端、左端、右端の頂点の情報を格納する
        /// </summary>
        /// <param name="vertices">頂点情報のリスト</param>
        public static void SetEndVertexInformation(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの端点の情報を探索を開始します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            double maxX = -100;
            double minX = 100;
            double maxY = -100;
            double minY = 100;
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
                    minY = v.VertexY;
                    //continue;
                }
                if (maxY < v.VertexY)
                {
                    topVertexIndex = v.ID;
                    maxY = v.VertexY;
                }
            }
            Console.WriteLine("左端は頂点 " + leftEndVertexIndex + "です。(座標 " + vertices[leftEndVertexIndex].VertexPosition.ToString() + ")");
            //FixedVertexIndexes.Add(leftEndVertexIndex);
            Console.WriteLine("左端を固定点として追加しました");
            Console.WriteLine("右端は頂点 " + rightEndVertexIndex + "です。(座標 " + vertices[rightEndVertexIndex].VertexPosition.ToString() + ")");
            Console.WriteLine("上端は頂点 " + topVertexIndex + "です。(座標 " + vertices[topVertexIndex].VertexPosition.ToString()+ ")");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの端点の情報を探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// 予め用意したリストに頂点情報を格納する(チートモードにつき実行非推奨)
        /// </summary>
        public static void SetVertexIndexCheatMode(List<Vertex> vertices)
        {
            List<int> inner = new List<int>()
            {
                2,5,6,9,10,17,18,21,25,26
            };
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの内部頂点の探索を開始します．(チートモード)~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (var index in inner)
            {
                Console.WriteLine("頂点 " + index.ToString() + "を内部頂点として登録いたしました．");
                InnnerVertexIndex.Add(index);
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの内部頂点の探索を終了します．(チートモード)~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
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
            //z座標が異なることによる外周上の頂点探索のミスをなくすため，全てz=0で計算する
            Console.WriteLine("全てのベクトル計算についてz=0に変更します．");
            leftoToTop.Z = 0;
            rightToTop.Z = 0;
            leftToRight.Z = 0;
            leftVertex.Z = 0;
            rightVertex.Z = 0;
            topVertex.Z = 0;
            foreach (var v in vertices)
            {
                //ここはテスト用のコード
                Vector3d vv = new Vector3d(v.VertexX, v.VertexY, 0);
                Vector3d lv = v.VertexPosition - leftVertex;
                Vector3d rv = v.VertexPosition - rightVertex;
                Vector3d tv = v.VertexPosition - topVertex;
                 lv = vv - leftVertex;
                 rv = vv - rightVertex;
                 tv = vv - topVertex;
                /*
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = "+ Vector3d.Dot(lv, leftoToTop).ToString() + " Prod = " + (lv.Length * leftoToTop.Length).ToString()) ;
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = " + Vector3d.Dot(lv, leftToRight).ToString() + " Prod = " + (lv.Length * leftToRight.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  lv = " + lv.ToString() + "  Dot = " + Vector3d.Dot(lv, rightToTop).ToString() + " Prod = " + (lv.Length * rightToTop.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, leftoToTop).ToString() + " Prod = " + (rv.Length * leftoToTop.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, leftToRight).ToString() + " Prod = " + (rv.Length * leftToRight.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  rv = " + rv.ToString() + "  Dot = " + Vector3d.Dot(rv, rightToTop).ToString() + " Prod = " + (rv.Length * rightToTop.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, leftoToTop).ToString() + " Prod = " + (tv.Length * leftoToTop.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, leftToRight).ToString() + " Prod = " + (tv.Length * leftToRight.Length).ToString());
                Console.WriteLine("c = " + c.ToString() + "  tv = " + tv.ToString() + "  Dot = " + Vector3d.Dot(tv, rightToTop).ToString() + " Prod = " + (tv.Length * rightToTop.Length).ToString());
                */
                //少数の計算結果の誤差による誤判定を防ぐため差で比較する
                //誤差許容範囲はerror_toleranceに格納する
                const double errorTolerance = 0.00000001;
                double Diff_LV_LR = Math.Abs(Math.Abs(Vector3d.Dot(lv, leftToRight)) - lv.Length * leftToRight.Length);
                double Diff_LV_LT = Math.Abs(Math.Abs(Vector3d.Dot(lv, leftoToTop)) - lv.Length * leftoToTop.Length);
                double Diff_LV_RT = Math.Abs(Math.Abs(Vector3d.Dot(lv, rightToTop)) - lv.Length * rightToTop.Length);
                double Diff_RV_LR = Math.Abs(Math.Abs(Vector3d.Dot(rv, leftToRight)) - rv.Length * leftToRight.Length);
                double Diff_RV_LT = Math.Abs(Math.Abs(Vector3d.Dot(rv, leftoToTop)) - rv.Length * leftoToTop.Length);
                double Diff_RV_RT = Math.Abs(Math.Abs(Vector3d.Dot(rv, rightToTop)) - rv.Length * rightToTop.Length);
                double Diff_TV_LR = Math.Abs(Math.Abs(Vector3d.Dot(tv, leftToRight)) - tv.Length * leftToRight.Length);
                double Diff_TV_LT = Math.Abs(Math.Abs(Vector3d.Dot(tv, leftoToTop)) - tv.Length * leftoToTop.Length);
                double Diff_TV_RT = Math.Abs(Math.Abs(Vector3d.Dot(tv, rightToTop)) - tv.Length * rightToTop.Length);
                if (Diff_LV_LR < errorTolerance || Diff_LV_LT < errorTolerance || Diff_LV_RT < errorTolerance
                     || Diff_RV_LR < errorTolerance ||  Diff_RV_LT < errorTolerance || Diff_RV_RT < errorTolerance
                       ||  Diff_TV_LR < errorTolerance || Diff_TV_LT < errorTolerance || Diff_TV_RT < errorTolerance)
                {
                    //左辺上の点の場合
                    if(isDebugging)
                    {
                        if (Diff_LV_LT < errorTolerance || Diff_TV_LT < errorTolerance)
                        {
                            if (!VertexIndexOnUnitLeftEdge.Contains(v.ID))
                            {
                                VertexIndexOnUnitLeftEdge.Add(v.ID);
                                Console.WriteLine("ユニットの左辺上の点として、頂点 " + v.ID.ToString() + "を登録しました。");
                            }
                        }
                    }
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

        /// <summary>
        /// ユニットの底辺上に存在する頂点情報を格納する
        /// </summary>
        /// <param name="vertices">頂点情報のリスト</param>
        private static void SetVertexIndexOnUnitButtomEdges(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの底辺上に存在する点の探索を開始します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (Vertex v in vertices)
            {
                if(v.VertexY == vertices[leftEndVertexIndex].VertexY)
                {
                    if(!VertexIndexOnUnitButtomEdge.Contains(v.ID))
                    {
                        Console.WriteLine("頂点 " + v.ID.ToString() + "を底辺上の頂点として登録しました。");
                        VertexIndexOnUnitButtomEdge.Add(v.ID);
                    }
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの底辺上に存在する点の探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// ユニットの内部頂点を格納する
        /// </summary>
        /// <param name="vertices">頂点情報リスト</param>
        private static void SetInnerVertex(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの内部頂点の探索を開始します．~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (Vertex v in vertices)
            {
                if(!VertexIndexOnUnitEdges.Contains(v.ID)
                    && !VertexIndexOnUnitButtomEdge.Contains(v.ID))
                {
                    Console.WriteLine("頂点 " + v.ID.ToString() + "を内部頂点として登録いたしました．");
                    InnnerVertexIndex.Add(v.ID);
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの内部頂点の探索を終了します．~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// ユニットの底辺のうち左右端点でない頂点のインデックスを格納する
        /// </summary>
        /// <param name="vertices"></param>
        private static void SetInnerVertexOnButtomEdge(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~底辺上に存在する内部頂点を探索します~~~~~~~~~~~~~~~~~~~~~");
            Vector3d rightToTop = vertices[topVertexIndex].VertexPosition - vertices[rightEndVertexIndex].VertexPosition;
            Vector3d rightVertex = vertices[rightEndVertexIndex].VertexPosition;
            
            rightToTop.Z = 0;
           
            rightVertex.Z = 0;
            
            foreach (var v in VertexIndexOnUnitEdges)
            {
                Vector3d vv = new Vector3d(vertices[v].VertexX, vertices[v].VertexY, 0);
                Vector3d rv = vertices[v].VertexPosition - rightVertex;
                rv = vv - rightVertex;
                const double errorTolerance = 0.00000001;
                double Diff_RV_RT = Math.Abs(Math.Abs(Vector3d.Dot(rv, rightToTop)) - rv.Length * rightToTop.Length);
                if (v != leftEndVertexIndex 
                    && v != rightEndVertexIndex 
                    && v != topVertexIndex 
                    && Diff_RV_RT >= errorTolerance)
                {
                    InnerVertexIndexOnButtomEdge.Add(v);
                    Console.WriteLine("底辺上の内部頂点として頂点 " + v + " を登録しました。");
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~底辺上に存在する内部頂点の探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// ユニットの左辺上存在する頂点インデックスを格納する
        /// </summary>
        /// <param name="vertices"></param>
        private static void SetVertexOnLeftEdge(List<Vertex> vertices)
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの左辺上に存在する点の探索を開始します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            foreach (Vertex v in vertices)
            {
                if (v.VertexX == vertices[leftEndVertexIndex].VertexX)
                {
                    if (!VertexIndexOnUnitLeftEdge.Contains(v.ID))
                    {
                        Console.WriteLine("頂点 " + v.ID.ToString() + "を左辺上の頂点として登録しました。");
                        VertexIndexOnUnitLeftEdge.Add(v.ID);
                    }
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~ユニットの左辺上に存在する点の探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        /// <summary>
        /// ユニットの左辺上に存在しかつ端点でない頂点のインデックスを格納する
        /// </summary>
        private static void SetInnerVertexOnRightEdge()
        {
            Console.WriteLine("~~~~~~~~~~~~~~~~~左辺上に存在する内部頂点を探索します~~~~~~~~~~~~~~~~~~~~~");
            foreach (var v in VertexIndexOnUnitLeftEdge)
            {
                if (v != leftEndVertexIndex
                    && v != topVertexIndex)
                {
                    InnerVertexIndexOnUnitLeftEdge.Add(v);
                    Console.WriteLine("左辺上の内部頂点として頂点 " + v + " を登録しました。");
                }
            }
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~左辺上に存在する内部頂点の探索を終了します。~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
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

        public static void GetInnerAngleSum(int vertexIndex, List<Vertex> verteices)
        {
            double arcCosSum = 0;
            foreach (var pair in verteices[vertexIndex].connectVertexId)
                    {
                        //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                        int pos1XIndex = pair[0];
                        int pos1YIndex = pair[0];
                        int pos1ZIndex = pair[0];
                        int pos2XIndex = pair[1];
                        int pos2YIndex = pair[1];
                        int pos2ZIndex = pair[1];
                        int centerXIndex = vertexIndex;
                        int centerYIndex = vertexIndex;
                        int centerZIndex = vertexIndex;
                            //Console.WriteLine("4番目が呼ばれています");
                            arcCosSum += GetArcCos(
                            verteices[pos1XIndex].VertexX - verteices[centerXIndex].VertexX,
                            verteices[pos1YIndex].VertexY - verteices[centerYIndex].VertexY,
                            verteices[pos1ZIndex].VertexZ - verteices[centerZIndex].VertexZ,
                            verteices[pos2XIndex].VertexX - verteices[centerXIndex].VertexX,
                            verteices[pos2YIndex].VertexY - verteices[centerYIndex].VertexY,
                            verteices[pos2ZIndex].VertexZ - verteices[centerZIndex].VertexZ
                            );

                    }
            arcCosSum = arcCosSum / (Math.PI) * 180 ;
            Console.WriteLine("頂点 " + vertexIndex.ToString() + " 周りの角度 " + arcCosSum.ToString());
        }

        /// <summary>
        /// 再急降下法により可展性を維持した形状に変形する
        /// </summary>
        /// <param name="verteices">ユニットのすべての頂点情報を持つリスト</param>
        /// <param name="manipulatedVertexPointIndex">内部頂点のリスト(予定)</param>
        public static void SetAdjustedUnitVertexes(List<Vertex> verteices, 
            int manipulatedVertexPointIndex, 
            HashSet<int> innerVertexIndexes, 
            HashSet<int> innerVertexIndexesOnBottomEdge)
        {
            //内部頂点と接続している頂点のインデックス集合
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
            //リストに再格納
            foreach (var point in connectVertxPoint)
            {
                connectVertex.Add(verteices[point]);
            }

            var initialX2 = new double[300];
            for (int initialXIndex = 0; initialXIndex < 3 * verteices.Count; initialXIndex++)
            {
                if (initialXIndex % 3 == 0)
                {
                    initialX2[initialXIndex] = verteices[(int)(initialXIndex / 3)].VertexX;
                }
                else if (initialXIndex % 3 == 1)
                {
                    initialX2[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexY;
                }
                else
                {
                    initialX2[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexZ;
                }
            }
            //再急降下法の損失関数の式
            Func<double[], double> f = (double[] x) =>
            {
                //底辺の内部頂点周りの角度の和
                double sumOfInnerVertexOnButtomEdgeAngle = 0;
                var e = 0.0;
                double arcCosSum = 0;
                double error = 0;
                double error_length = 0;
                double weight_1 = 1000.0;
                double weight_2 = 10.0;
                //内部頂点が2piになるように最適化処理をする
                foreach (var innervertex in innerVertexIndexes)
                {
                    if(innervertex != okPoint)
                    {
                        foreach (var pair in verteices[innervertex].connectVertexId)
                        {
                            //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                            int pos1XIndex = pair[0] * 3;
                            int pos1YIndex = pair[0] * 3 + 1;
                            int pos1ZIndex = pair[0] * 3 + 2;
                            int pos2XIndex = pair[1] * 3;
                            int pos2YIndex = pair[1] * 3 + 1;
                            int pos2ZIndex = pair[1] * 3 + 2;
                            //TODO 以下の部分がおかしいので修正する)(
                            int centerXIndex = innervertex * 3;
                            int centerYIndex = innervertex * 3 + 1;
                            int centerZIndex = innervertex * 3 + 2;
                            if (FixedVertexIndexes.Contains(pair[0])
                            && FixedVertexIndexes.Contains(pair[1]))
                            {

                                arcCosSum += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                            else if (FixedVertexIndexes.Contains(pair[0]))
                            {
                                if (VertexIndexOnUnitEdges.Contains(pair[1]))
                                {
                                    if (VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                    {
                                        arcCosSum += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    verteices[pair[0]].VertexZ - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]
    );
                                        //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    }
                                    else
                                    {
                                        //
                                        //Console.WriteLine("f 0 v 1です");
                                        arcCosSum += GetArcCos(
                                        verteices[pair[0]].VertexX - x[centerXIndex],
                                        verteices[pair[0]].VertexY - x[centerYIndex],
                                        verteices[pair[0]].VertexZ - x[centerZIndex],
                                        verteices[pair[1]].VertexX - x[centerXIndex],
                                        verteices[pair[1]].VertexY - x[centerYIndex],
                                        x[pos2ZIndex] - x[centerZIndex]
                                        );
                                    }
                                }
                                else
                                {
                                    arcCosSum += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    verteices[pair[0]].VertexZ - x[centerZIndex],
                                    x[pos2XIndex] - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }
                            }
                            else if (FixedVertexIndexes.Contains(pair[1]))
                            {
                                if (VertexIndexOnUnitEdges.Contains(pair[0]))
                                {
                                    if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                    {
                                        arcCosSum += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    x[pos1YIndex] - x[centerYIndex],
                                    x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    verteices[pair[1]].VertexZ - x[centerZIndex]
    ); //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    }
                                    else
                                    {
                                        //Console.WriteLine("f 1 v 0です");
                                        arcCosSum += GetArcCos(
                                        verteices[pair[0]].VertexX - x[centerXIndex],
                                        verteices[pair[0]].VertexY - x[centerYIndex],
                                        x[pos1ZIndex] - x[centerZIndex],
                                        verteices[pair[1]].VertexX - x[centerXIndex],
                                        verteices[pair[1]].VertexY - x[centerYIndex],
                                        verteices[pair[1]].VertexZ - x[centerZIndex]
                                        );
                                    }
                                }
                                else
                                {
                                    arcCosSum += GetArcCos(
                                    x[pos1XIndex] - x[centerXIndex],
                                    x[pos1YIndex] - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    verteices[pair[1]].VertexZ - x[centerZIndex]
                                    );
                                }
                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                if (!VertexIndexOnUnitButtomEdge.Contains(pair[0]) && !VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    //Console.WriteLine("一番目が呼ばれています");
                                    arcCosSum += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }

                                else if (VertexIndexOnUnitButtomEdge.Contains(pair[0]) && !VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    arcCosSum += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
    verteices[pair[1]].VertexY - x[centerYIndex],
    x[pos2ZIndex] - x[centerZIndex]
    );
                                    //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else if (!VertexIndexOnUnitButtomEdge.Contains(pair[0]) && VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    arcCosSum += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    verteices[pair[0]].VertexY - x[centerYIndex],
    x[pos1ZIndex] - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]
    );
                                    //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else
                                {
                                    arcCosSum += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]);
                                }
                                //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));

                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                            {
                                if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                {
                                    arcCosSum += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    x[pos2XIndex] - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2ZIndex] - x[centerZIndex]
    );
                                    //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else
                                {
                                    //Console.WriteLine("2番目が呼ばれています");
                                    arcCosSum += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    x[pos2XIndex] - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }
                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                {
                                    arcCosSum += GetArcCos(
        x[pos1XIndex] - x[centerXIndex],
        x[pos1YIndex] - x[centerYIndex],
        x[pos1ZIndex] - x[centerZIndex],
        verteices[pair[1]].VertexX - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]);
                                    //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else
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

                            }
                            else
                            {
                                //Console.WriteLine("4番目が呼ばれています");
                                arcCosSum += GetArcCos(
                                x[pos1XIndex] - x[centerXIndex],
                                x[pos1YIndex] - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                x[pos2XIndex] - x[centerXIndex],
                                x[pos2YIndex] - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }

                            Vector3d init = new Vector3d(initialX2[centerXIndex], initialX2[centerYIndex], initialX2[centerZIndex]);
                            Vector3d v = new Vector3d(x[centerXIndex], x[centerYIndex], x[centerZIndex]);
                            e += (v - init).Length;
                        }

                        error += weight_1 * (2 * Math.PI - arcCosSum) * (2 * Math.PI - arcCosSum) + e * weight_2;
                        arcCosSum = 0;
                        e = 0;
                    }
                }
                {
                    //底辺上に存在する内部頂点の最適化
                    /*
                    foreach (var buttomvertex in innerVertexIndexesOnBottomEdge)
                    {
                        foreach (var pair in verteices[buttomvertex].connectVertexId)
                        {
                            //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                            int pos1XIndex = pair[0] * 3;
                            int pos1YIndex = pair[0] * 3 + 1;
                            int pos1ZIndex = pair[0] * 3 + 2;
                            int pos2XIndex = pair[1] * 3;
                            int pos2YIndex = pair[1] * 3 + 1;
                            int pos2ZIndex = pair[1] * 3 + 2;
                            int centerXIndex = buttomvertex * 3;
                            int centerYIndex = buttomvertex * 3 + 1;
                            int centerZIndex = buttomvertex * 3 + 2;
                            if (FixedVertexIndexes.Contains(pair[0])
                            && FixedVertexIndexes.Contains(pair[1]))
                            {

                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                            else if (FixedVertexIndexes.Contains(pair[0]))
                            {
                                if (VertexIndexOnUnitEdges.Contains(pair[1]))
                                {
                                    /*if (VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                    {
                                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    verteices[pair[0]].VertexZ - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]
    );
                                        //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    }
                                    else
                                    {
                                        //
                                        //Console.WriteLine("f 0 v 1です");
                                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                        verteices[pair[0]].VertexX - x[centerXIndex],
                                        verteices[pair[0]].VertexY - x[centerYIndex],
                                        verteices[pair[0]].VertexZ - x[centerZIndex],
                                        verteices[pair[1]].VertexX - x[centerXIndex],
                                        verteices[pair[1]].VertexY - x[centerYIndex],
                                        x[pos2ZIndex] - x[centerZIndex]
                                        );
                                    }
                                }
                                else
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    verteices[pair[0]].VertexZ - x[centerZIndex],
                                    x[pos2XIndex] - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }
                            }
                            else if (FixedVertexIndexes.Contains(pair[1]))
                            {
                                if (VertexIndexOnUnitEdges.Contains(pair[0]))
                                {
                                    /*if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                    {
                                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    x[pos1YIndex] - x[centerYIndex],
                                    x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    verteices[pair[1]].VertexZ - x[centerZIndex]
    );
                                        //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    }
                                    else
                                    {
                                        //Console.WriteLine("f 1 v 0です");
                                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                        verteices[pair[0]].VertexX - x[centerXIndex],
                                        verteices[pair[0]].VertexY - x[centerYIndex],
                                        x[pos1ZIndex] - x[centerZIndex],
                                        verteices[pair[1]].VertexX - x[centerXIndex],
                                        verteices[pair[1]].VertexY - x[centerYIndex],
                                        verteices[pair[1]].VertexZ - x[centerZIndex]
                                        );
                                    }
                                }
                                else
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    x[pos1XIndex] - x[centerXIndex],
                                    x[pos1YIndex] - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    verteices[pair[1]].VertexZ - x[centerZIndex]
                                    );
                                }
                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                //if (!VertexIndexOnUnitButtomEdge.Contains(pair[0]) && !VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    //Console.WriteLine("一番目が呼ばれています");
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    verteices[pair[1]].VertexX - x[centerXIndex],
                                    verteices[pair[1]].VertexY - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }
                                /*else if (VertexIndexOnUnitButtomEdge.Contains(pair[0]) && !VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
    verteices[pair[1]].VertexY - x[centerYIndex],
    x[pos2ZIndex] - x[centerZIndex]
    );
                                    ////x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else if (!VertexIndexOnUnitButtomEdge.Contains(pair[0]) && VertexIndexOnUnitButtomEdge.Contains(pair[1]))
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    verteices[pair[0]].VertexY - x[centerYIndex],
    x[pos1ZIndex] - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]
    );
                                    //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]);
                                    //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                    //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }

                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                            {
                                /*if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
    verteices[pair[0]].VertexX - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
     x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex],
    x[pos2XIndex] - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2ZIndex] - x[centerZIndex]
    );
                                    //x[pos1ZIndex] = x[pos1YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else// 
                                {
                                    //Console.WriteLine("2番目が呼ばれています");
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                    verteices[pair[0]].VertexX - x[centerXIndex],
                                    verteices[pair[0]].VertexY - x[centerYIndex],
                                    x[pos1ZIndex] - x[centerZIndex],
                                    x[pos2XIndex] - x[centerXIndex],
                                    x[pos2YIndex] - x[centerYIndex],
                                    x[pos2ZIndex] - x[centerZIndex]
                                    );
                                }
                            }
                            else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                /*if (VertexIndexOnUnitButtomEdge.Contains(pair[0]))
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
        x[pos1XIndex] - x[centerXIndex],
        x[pos1YIndex] - x[centerYIndex],
        x[pos1ZIndex] - x[centerZIndex],
        verteices[pair[1]].VertexX - x[centerXIndex],
    x[pos2YIndex] - x[centerYIndex],
    x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180)) - x[centerZIndex]);
                                    //x[pos2ZIndex] = x[pos2YIndex] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                                }
                                else
                                {
                                    sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
    x[pos1XIndex] - x[centerXIndex],
    x[pos1YIndex] - x[centerYIndex],
    x[pos1ZIndex] - x[centerZIndex],
    verteices[pair[1]].VertexX - x[centerXIndex],
    verteices[pair[1]].VertexY - x[centerYIndex],
    x[pos2ZIndex] - x[centerZIndex]
    );
                                }

                            }
                            else
                            {
                                //Console.WriteLine("4番目が呼ばれています");
                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                x[pos1XIndex] - x[centerXIndex],
                                x[pos1YIndex] - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                x[pos2XIndex] - x[centerXIndex],
                                x[pos2YIndex] - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }
                            Vector3d init = new Vector3d(initialX2[centerXIndex], initialX2[centerYIndex], initialX2[centerZIndex]);
                            Vector3d v = new Vector3d(x[centerXIndex], x[centerYIndex], x[centerZIndex]);
                            e += (v - init).Length;
                        }
                        //Console.WriteLine("aa = " + sumOfInnerVertexOnButtomEdgeAngle.ToString());


                        error += weight_2 * (Math.PI - sumOfInnerVertexOnButtomEdgeAngle) * (Math.PI - sumOfInnerVertexOnButtomEdgeAngle);
                        sumOfInnerVertexOnButtomEdgeAngle = 0;
                        e = 0;
                    }
        */
                    //return (2 * Math.PI - arcCosSum) * (2 * Math.PI - arcCosSum);
                    // Console.WriteLine(error.ToString());
                    /*  {

                          foreach (var pair in verteices[leftEndVertexIndex].connectVertexId)
                          {
                              //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                              int pos1XIndex = pair[0] * 3;
                              int pos1YIndex = pair[0] * 3 + 1;
                              int pos1ZIndex = pair[0] * 3 + 2;
                              int pos2XIndex = pair[1] * 3;
                              int pos2YIndex = pair[1] * 3 + 1;
                              int pos2ZIndex = pair[1] * 3 + 2;
                              int centerXIndex = leftEndVertexIndex * 3;
                              int centerYIndex = leftEndVertexIndex * 3 + 1;
                              int centerZIndex = leftEndVertexIndex * 3 + 2;
                              if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                              {
                                  //Console.WriteLine("一番目が呼ばれています");
                                  sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                  verteices[pair[0]].VertexX - verteices[leftEndVertexIndex].VertexX,
                                  verteices[pair[0]].VertexY - verteices[leftEndVertexIndex].VertexY,
                                  x[pos1ZIndex] - x[centerZIndex],
                                  verteices[pair[1]].VertexX - verteices[leftEndVertexIndex].VertexX,
                                  verteices[pair[1]].VertexY - verteices[leftEndVertexIndex].VertexY,
                                  x[pos2ZIndex] - x[centerZIndex]
                                  );
                              }
                              else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                              {
                                  //Console.WriteLine("2番目が呼ばれています");
                                  sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                  verteices[pair[0]].VertexX - verteices[leftEndVertexIndex].VertexX,
                                  verteices[pair[0]].VertexY - verteices[leftEndVertexIndex].VertexY,
                                  x[pos1ZIndex] - x[centerZIndex],
                                  x[pos2XIndex] - verteices[leftEndVertexIndex].VertexX,
                                  x[pos2YIndex] - verteices[leftEndVertexIndex].VertexY,
                                  x[pos2ZIndex] - x[centerZIndex]
                                  );
                              }
                              else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                              {
                                  //Console.WriteLine("3番目が呼ばれています");
                                  sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                  x[pos1XIndex] - verteices[leftEndVertexIndex].VertexX,
                                  x[pos1YIndex] - verteices[leftEndVertexIndex].VertexY,
                                  x[pos1ZIndex] - x[centerZIndex],
                                  verteices[pair[1]].VertexX - verteices[leftEndVertexIndex].VertexX,
                                  verteices[pair[1]].VertexY - verteices[leftEndVertexIndex].VertexY,
                                  x[pos2ZIndex] - x[centerZIndex]
                                  );
                              }
                              else
                              {
                                  //Console.WriteLine("4番目が呼ばれています");
                                  sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                  x[pos1XIndex] - verteices[leftEndVertexIndex].VertexX,
                                  x[pos1YIndex] - verteices[leftEndVertexIndex].VertexY,
                                  x[pos1ZIndex] - x[centerZIndex],
                                  x[pos2XIndex] - verteices[leftEndVertexIndex].VertexX,
                                  x[pos2YIndex] - verteices[leftEndVertexIndex].VertexY,
                                  x[pos2ZIndex] - x[centerZIndex]
                                  );
                              }
                              error += (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle) * (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle);
                              sumOfInnerVertexOnButtomEdgeAngle = 0;
                          }

                      }
                    */
                    //Console.WriteLine("aa = " + sumOfInnerVertexOnButtomEdgeAngle.ToString());
                }
                return error;
            };
            int a = 0;


            List <Func<double[], double>> funcs = new List<Func<double[], double>>();
            //底辺の内部頂点周りの角度の和
            double sumOfInnerVertexOnButtomEdgeAngle = 0;
            double arcCosSum = 0;
            double error = 0;
            int funcIndex = 0;
            //内部頂点が2piになるように最適化処理をする
            //ここは確率的最急降下法の処理なので消します．
            /*
            foreach (var innervertex in innerVertexIndexes)
            {
                Func<double[], double> fss = (double[] x) =>
                {
                    double arcCosSum = 0;
                    double error = 0;
                    foreach (var pair in verteices[innervertex].connectVertexId)
                    {
                        //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                        int pos1XIndex = pair[0] * 3;
                        int pos1YIndex = pair[0] * 3 + 1;
                        int pos1ZIndex = pair[0] * 3 + 2;
                        int pos2XIndex = pair[1] * 3;
                        int pos2YIndex = pair[1] * 3 + 1;
                        int pos2ZIndex = pair[1] * 3 + 2;
                        //TODO 以下の部分がおかしいので修正する)(
                        int centerXIndex = innervertex * 3;
                        int centerYIndex = innervertex * 3 + 1;
                        int centerZIndex = innervertex * 3 + 2;
                        if (FixedVertexIndexes.Contains(pair[0])
                        && FixedVertexIndexes.Contains(pair[1]))
                        {
                            arcCosSum += GetArcCos(
                            verteices[pair[0]].VertexX - x[centerXIndex],
                            verteices[pair[0]].VertexY - x[centerYIndex],
                            verteices[pair[0]].VertexZ - x[centerZIndex],
                            verteices[pair[1]].VertexX - x[centerXIndex],
                            verteices[pair[1]].VertexY - x[centerYIndex],
                            verteices[pair[1]].VertexZ - x[centerZIndex]
                            );
                        }
                        else if (FixedVertexIndexes.Contains(pair[0]))
                        {
                            if (VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                //Console.WriteLine("3番目が呼ばれています");
                                arcCosSum += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }
                            else
                            {
                                arcCosSum += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                x[pos2XIndex] - x[centerXIndex],
                                x[pos2YIndex] - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }
                        }
                        else if (FixedVertexIndexes.Contains(pair[1]))
                        {
                            if (VertexIndexOnUnitEdges.Contains(pair[0]))
                            {
                                arcCosSum += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                            else
                            {
                                arcCosSum += GetArcCos(
                                x[pos1XIndex] - x[centerXIndex],
                                x[pos1YIndex] - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                        }
                        else if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                        {
                            //Console.WriteLine("一番目が呼ばれています");
                            arcCosSum += GetArcCos(
                            verteices[pair[0]].VertexX - x[centerXIndex],
                            verteices[pair[0]].VertexY - x[centerYIndex],
                            x[pos1ZIndex] - x[centerZIndex],
                            verteices[pair[1]].VertexX - x[centerXIndex],
                            verteices[pair[1]].VertexY - x[centerYIndex],
                            x[pos2ZIndex] - x[centerZIndex]
                            );
                        }
                        else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                        {
                            //Console.WriteLine("2番目が呼ばれています");
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
                            //Console.WriteLine("3番目が呼ばれています");
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
                            //Console.WriteLine("4番目が呼ばれています");
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
                funcs.Add(fss);
            }
            //底辺上に存在する内部頂点の最適化
            foreach (var buttomvertex in innerVertexIndexesOnBottomEdge)
            {
                Func<double[], double> fss = (double[] x) =>
                {
                    double sumOfInnerVertexOnButtomEdgeAngle = 0;
                    double error = 0;
                    foreach (var pair in verteices[buttomvertex].connectVertexId)
                    {
                        //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                        int pos1XIndex = pair[0] * 3;
                        int pos1YIndex = pair[0] * 3 + 1;
                        int pos1ZIndex = pair[0] * 3 + 2;
                        int pos2XIndex = pair[1] * 3;
                        int pos2YIndex = pair[1] * 3 + 1;
                        int pos2ZIndex = pair[1] * 3 + 2;
                        int centerXIndex = buttomvertex * 3;
                        int centerYIndex = buttomvertex * 3 + 1;
                        int centerZIndex = buttomvertex * 3 + 2;
                        if (FixedVertexIndexes.Contains(pair[0])
                        && FixedVertexIndexes.Contains(pair[1]))
                        {
                            sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                            verteices[pair[0]].VertexX - x[centerXIndex],
                            verteices[pair[0]].VertexY - x[centerYIndex],
                            verteices[pair[0]].VertexZ - x[centerZIndex],
                            verteices[pair[1]].VertexX - x[centerXIndex],
                            verteices[pair[1]].VertexY - x[centerYIndex],
                            verteices[pair[1]].VertexZ - x[centerZIndex]
                            );
                        }
                        else if (FixedVertexIndexes.Contains(pair[0]))
                        {
                            if (VertexIndexOnUnitEdges.Contains(pair[1]))
                            {
                                //Console.WriteLine("3番目が呼ばれています");
                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }
                            else
                            {
                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                verteices[pair[0]].VertexZ - x[centerZIndex],
                                x[pos2XIndex] - x[centerXIndex],
                                x[pos2YIndex] - x[centerYIndex],
                                x[pos2ZIndex] - x[centerZIndex]
                                );
                            }
                        }
                        else if (FixedVertexIndexes.Contains(pair[1]))
                        {
                            if (VertexIndexOnUnitEdges.Contains(pair[0]))
                            {
                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                verteices[pair[0]].VertexX - x[centerXIndex],
                                verteices[pair[0]].VertexY - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                            else
                            {
                                sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                                x[pos1XIndex] - x[centerXIndex],
                                x[pos1YIndex] - x[centerYIndex],
                                x[pos1ZIndex] - x[centerZIndex],
                                verteices[pair[1]].VertexX - x[centerXIndex],
                                verteices[pair[1]].VertexY - x[centerYIndex],
                                verteices[pair[1]].VertexZ - x[centerZIndex]
                                );
                            }
                        }
                        else if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                        {
                            //Console.WriteLine("一番目が呼ばれています");
                            sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                            verteices[pair[0]].VertexX - verteices[buttomvertex].VertexX,
                            verteices[pair[0]].VertexY - verteices[buttomvertex].VertexY,
                            x[pos1ZIndex] - x[centerZIndex],
                            verteices[pair[1]].VertexX - verteices[buttomvertex].VertexX,
                            verteices[pair[1]].VertexY - verteices[buttomvertex].VertexY,
                            x[pos2ZIndex] - x[centerZIndex]
                            );
                        }
                        else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                        {
                            //Console.WriteLine("2番目が呼ばれています");
                            sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                            verteices[pair[0]].VertexX - verteices[buttomvertex].VertexX,
                            verteices[pair[0]].VertexY - verteices[buttomvertex].VertexY,
                            x[pos1ZIndex] - x[centerZIndex],
                            x[pos2XIndex] - verteices[buttomvertex].VertexX,
                            x[pos2YIndex] - verteices[buttomvertex].VertexY,
                            x[pos2ZIndex] - x[centerZIndex]
                            );
                        }
                        else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                        {
                            //Console.WriteLine("3番目が呼ばれています");
                            sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                            x[pos1XIndex] - verteices[buttomvertex].VertexX,
                            x[pos1YIndex] - verteices[buttomvertex].VertexY,
                            x[pos1ZIndex] - x[centerZIndex],
                            verteices[pair[1]].VertexX - verteices[buttomvertex].VertexX,
                            verteices[pair[1]].VertexY - verteices[buttomvertex].VertexY,
                            x[pos2ZIndex] - x[centerZIndex]
                            );
                        }
                        else
                        {
                            //Console.WriteLine("4番目が呼ばれています");
                            sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                            x[pos1XIndex] - verteices[buttomvertex].VertexX,
                            x[pos1YIndex] - verteices[buttomvertex].VertexY,
                            x[pos1ZIndex] - x[centerZIndex],
                            x[pos2XIndex] - verteices[buttomvertex].VertexX,
                            x[pos2YIndex] - verteices[buttomvertex].VertexY,
                            x[pos2ZIndex] - x[centerZIndex]
                            );
                        }
                    }
                    return (Math.PI - sumOfInnerVertexOnButtomEdgeAngle) * (Math.PI - sumOfInnerVertexOnButtomEdgeAngle);
                };
                funcs.Add(fss);
            }
            Func<double[], double> fs = (double[] x) =>
            {
                double sumOfInnerVertexOnButtomEdgeAngle = 0;
                double error = 0;
                foreach (var pair in verteices[leftEndVertexIndex].connectVertexId)
                {
                    //Console.WriteLine("Connect ID = " + pair[0].ToString() + " " + pair[1].ToString());
                    int pos1XIndex = pair[0] * 3;
                    int pos1YIndex = pair[0] * 3 + 1;
                    int pos1ZIndex = pair[0] * 3 + 2;
                    int pos2XIndex = pair[1] * 3;
                    int pos2YIndex = pair[1] * 3 + 1;
                    int pos2ZIndex = pair[1] * 3 + 2;
                    int centerXIndex = leftEndVertexIndex * 3;
                    int centerYIndex = leftEndVertexIndex * 3 + 1;
                    int centerZIndex = leftEndVertexIndex * 3 + 2;
                    if (VertexIndexOnUnitEdges.Contains(pair[0]) && VertexIndexOnUnitEdges.Contains(pair[1]))
                    {
                        //Console.WriteLine("一番目が呼ばれています");
                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                        verteices[pair[0]].VertexX - verteices[leftEndVertexIndex].VertexX,
                        verteices[pair[0]].VertexY - verteices[leftEndVertexIndex].VertexY,
                        x[pos1ZIndex] - x[centerZIndex],
                        verteices[pair[1]].VertexX - verteices[leftEndVertexIndex].VertexX,
                        verteices[pair[1]].VertexY - verteices[leftEndVertexIndex].VertexY,
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    else if (VertexIndexOnUnitEdges.Contains(pair[0]))
                    {
                        //Console.WriteLine("2番目が呼ばれています");
                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                        verteices[pair[0]].VertexX - verteices[leftEndVertexIndex].VertexX,
                        verteices[pair[0]].VertexY - verteices[leftEndVertexIndex].VertexY,
                        x[pos1ZIndex] - x[centerZIndex],
                        x[pos2XIndex] - verteices[leftEndVertexIndex].VertexX,
                        x[pos2YIndex] - verteices[leftEndVertexIndex].VertexY,
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    else if (VertexIndexOnUnitEdges.Contains(pair[1]))
                    {
                        //Console.WriteLine("3番目が呼ばれています");
                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                        x[pos1XIndex] - verteices[leftEndVertexIndex].VertexX,
                        x[pos1YIndex] - verteices[leftEndVertexIndex].VertexY,
                        x[pos1ZIndex] - x[centerZIndex],
                        verteices[pair[1]].VertexX - verteices[leftEndVertexIndex].VertexX,
                        verteices[pair[1]].VertexY - verteices[leftEndVertexIndex].VertexY,
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    else
                    {
                        //Console.WriteLine("4番目が呼ばれています");
                        sumOfInnerVertexOnButtomEdgeAngle += GetArcCos(
                        x[pos1XIndex] - verteices[leftEndVertexIndex].VertexX,
                        x[pos1YIndex] - verteices[leftEndVertexIndex].VertexY,
                        x[pos1ZIndex] - x[centerZIndex],
                        x[pos2XIndex] - verteices[leftEndVertexIndex].VertexX,
                        x[pos2YIndex] - verteices[leftEndVertexIndex].VertexY,
                        x[pos2ZIndex] - x[centerZIndex]
                        );
                    }
                    error += (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle) * (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle);
                    sumOfInnerVertexOnButtomEdgeAngle = 0;
                }
                return (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle) * (Math.PI / 2 - sumOfInnerVertexOnButtomEdgeAngle);
            };
            */
            //funcs.Add(fs);
            //とりあえず100頂点分用意する
            var initialX = new double[300];
            
            var initialX3 = new double[300];
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
            
            /*foreach (var verte in initialX)
            {
                Console.WriteLine(verte);
            }*/
            int iteration = 5000;
            double learningRate = 0.0000001;
            double[] answer = Seeker_Sys.SteepestDescentMethodMV.Compute(f, initialX, iteration, learningRate);

            for (int initialXIndex = 0; initialXIndex < 3 * verteices.Count; initialXIndex++)
            {
                if (initialXIndex % 3 == 0)
                {
                    initialX3[initialXIndex] = verteices[(int)(initialXIndex / 3)].VertexX;
                }
                else if (initialXIndex % 3 == 1)
                {
                    initialX3[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexY;
                }
                else
                {
                    initialX3[initialXIndex] = verteices[(int)(initialXIndex) / 3].VertexZ;
                }
            }

            Func<double[], double> f2 = (double[] x) =>
            {
                double e = 0;
                for(int i = 0; i < verteices.Count; i++)
                {

                    Vector3d init = new Vector3d(initialX2[i * 3], initialX2[i * 3 + 1], initialX2[i * 3 + 2]);
                    Vector3d v = new Vector3d(verteices[i].VertexX, verteices[i].VertexY, verteices[i].VertexZ);
                    e += (v - init).Length;
                }
                return e;
            };
            //double[] answer = Seeker_Sys.SGD.Compute(funcs, initialX, iteration, learningRate);
            /*var optimizer = new SteepestDescentMethodMV(f, initialX, learningRate);
            var xHistory = new List<double[]>();
            for (int i = 0; i < iteration; ++i)
            {
                optimizer.Update();
                xHistory.Add(optimizer.Xn);
            }
            foreach (var x in xHistory)
            {
                Console.WriteLine(string.Join(", ", x));
            }*/
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
                        //if(VertexIndexOnUnitButtomEdge.Contains((int)(i / 3)))
                        //{
                        //    verteices[(int)(i / 3)].VertexZ = answer[i - 1] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                        //}
                        //else
                        {
                            verteices[(int)(i / 3)].VertexZ = answer[i];
                        }
                        
                    }
                }
            }
            /*double[] answer2 = Seeker_Sys.SteepestDescentMethodMV.Compute(f2, initialX3, iteration - 495, learningRate );
            for (int i = 0; i < verteices.Count * 3; i++)
            {
                {
                    if (i % 3 == 0)
                    {
                        verteices[(int)(i / 3)].VertexX = answer2[i];
                    }
                    else if (i % 3 == 1)
                    {
                        verteices[(int)(i / 3)].VertexY = answer2[i];
                    }
                    else
                    {
                        //if(VertexIndexOnUnitButtomEdge.Contains((int)(i / 3)))
                        //{
                        //    verteices[(int)(i / 3)].VertexZ = answer[i - 1] * (-Math.Tan(Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2 / 180));
                        //}
                        //else
                        {
                            verteices[(int)(i / 3)].VertexZ = answer2[i];
                        }

                    }
                }
            }*/
            for (int i = 0; i < verteices.Count; i++)
            {
                verteices[i].VertexPosition = new Vector3d(verteices[i].VertexX, verteices[i].VertexY, verteices[i].VertexZ);
            }
            Console.WriteLine("計算後の数値");
            GetAllAngle(verteices);
            InnerBottomErrorZ = verteices[0].VertexZ;
            Console.WriteLine("底辺の内部頂点のz誤差は " + InnerBottomErrorZ.ToString()+"です。");
        }

        public static void test(List<Vertex> verteices)
        {
            Console.WriteLine("テストとして呼ばれました");
            GetInnerAngleSum(0, verteices);
            GetInnerAngleSum(1, verteices);
            GetInnerAngleSum(2, verteices);
            GetInnerAngleSum(3, verteices);
            //GetInnerAngleSum(4, verteices);
        }

        public static void GetAllAngle(List<Vertex> verteices)
        {
            Console.WriteLine("全頂点の角度を調査します．");
            foreach(var v in verteices)
            {
                GetInnerAngleSum(v.ID, verteices);
            }
        }

        public static double GetGradVecLength(double[] gradVec)
        {
            double ans = 0;
            foreach(var v in gradVec)
            {
                ans += v * v;
            }
            ans = Math.Sqrt(ans);
            return ans;
        }
    }
}
