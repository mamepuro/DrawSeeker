using System;
using System.IO;
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
using Destiny.Seeker_Sys;
using System.Collections;

namespace Destiny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer myTimer = new DispatcherTimer();
        /// <summary>
        /// 右クリック中かどうか
        /// </summary>
        private bool _isDraggingRightButton = false;
        private float _mouseX = 0;
        private float _mouseY = 0;
        private Vector3d _rotato;
        private float rot = 0.0f;
        /// <summary>
        /// y軸周りに回転させる角度
        /// </summary>
        private float _rotateAngleY = 0;
        /// <summary>
        /// z軸周りに回転させる角度
        /// </summary>
        private float _rotateAngleZ = 0;
        /// <summary>
        /// 投資投影行列
        /// </summary>
        private Matrix4 _perspectiveProjection;
        private Matrix4 _modelView;
        private int[] _viewport = new int[4];
        private int SelectedPartId = 0;
        private float scale = 1.0f;
        private float PENTA_radius = 1 / (2 * MathF.Tan(MathF.PI / 5));
        //正十二面体の内接球半径
        private float PENTA_innerball_radius = ((MathF.Sqrt(5) + 1) * (MathF.Sqrt(25 + 10 * MathF.Sqrt(5)))) / (20);
        private float OCTO_radius = 1 / (2 * (float)Math.Sqrt(3));
        int angle = 45;
        double w = 0.5, h = 0.2, d = 0.7;
        float rad = (90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2) * (float)Math.PI / 180;
        float rotatey;
        float rotatez;
        private bool isDisplayUnit = false;
        int manipulateVertexIndex = 5;
        bool isDrawReferLine = false;
        Seeker_Sys.Arcball arcball;
        /// <summary>
        /// 多面体の面の中心部分の回転軸
        /// </summary>
        Vector3d nor_axis;
        //private float 
        /// <summary>
        /// ユニットの下端の(上下ひっくり返した部分)頂点インデックス
        /// </summary>
        int bottomVertexIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            myTimer.Tick += new EventHandler(Timer1_Tick);
            myTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            myTimer.Start();
            SetPENTA_UNIT_pos();
            SetOCTO_UNIT_pos();
            GetPos();
            SubWindow subwindow = new SubWindow();
            subwindow.Show();
            //Debug.Print("ANS = "+((4 * PENTA_radius * PENTA_radius - 1) / (4 * PENTA_radius * PENTA_radius + 1)).ToString());
        }


        int vertexcount = 8;
        public static List<Vertex> vertexes = new List<Vertex>();
        public static List<int[]> edges = new List<int[]>();
        Vector3d[] pos = new Vector3d[] {
            /*0*/new Vector3d(0, 0, 0),
            /*1*/new Vector3d(0.5, 0, 0),
            /*2*/new Vector3d(0, 0.5, 0),
            /*3*/new Vector3d(0.5, 0.5, 0),
            /*4*/new Vector3d(0, 0, 0.5),
            /*5*/new Vector3d(0.5, 0, 0.5),
            /*6*/new Vector3d(0, 0.5, 0.5),
            /*7*/new Vector3d(0.5, 0.5, 0.5)
        };
        new List<Vertex> PENTA_UNIT_vertexes = new List<Vertex>();
        Vector3d[] PENTA_UNIT_pos = new Vector3d[] {
            /*0*/new Vector3d(-0.5, 0, 0),
/*2(SQRTはコンパイル時に定数でないためとりあえず0,0,0で宣言する)*/new Vector3d(0, 0, 0),
            /*1*/new Vector3d(0.5, 0, 0),

            /*3*/new Vector3d(0, 0, 0),
        };
        new List<Vertex> OCTO_UNIT_vertexes = new List<Vertex>();
        Vector3d[] OCTO_UNIT_pos = new Vector3d[] {
            /*0*/new Vector3d(-0.5, 0, 0),
/*2(SQRTはコンパイル時に定数でないためとりあえず0,0,0で宣言する)*/new Vector3d(0, 0, 0),
            /*1*/new Vector3d(0.5, 0, 0),

            /*3*/new Vector3d(0, 0, 0),
        };
        new List<Vertex> OCTO_vertexes = new List<Vertex>();
        Vector3d[] OCTO_pos = new Vector3d[] {
            /*0*/new Vector3d(-0.5, 0, 0),
            /*1*/new Vector3d(0.5, 0, 0),
            /*2*/new Vector3d(0, 0.5, 0),
            /*3*/new Vector3d(0, -0.5, 0),
            /*4*/new Vector3d(0, 0, 0.5),
            /*5*/new Vector3d(0, 0, -0.5),
            /*6*/new Vector3d(0.25, 0.25, 0.25),
            /*7*/new Vector3d(0.25, -0.25, 0.25),
            /*8*/new Vector3d(-0.25, 0.25, 0.25),
            /*9*/new Vector3d(-0.25, -0.25, 0.25),
            /*10*/new Vector3d(0.25, 0.25, -0.25),
            /*11*/new Vector3d(0.25, -0.25, -0.25),
            /*12*/new Vector3d(-0.25, 0.25, -0.25),
            /*13*/new Vector3d(-0.25, -0.25, -0.25),
        };
        /// <summary>
        /// roatateYtを計算する
        /// </summary>
        private void GetPos()
        {
            rotatey = OCTO_radius * (float)Math.Cos(rad);
            rotatez = OCTO_radius * (float)Math.Sin(rad);
            nor_axis = OpenTKExSys.GetNormalVector(new Vector3d(-0.5, 0, 0), new Vector3d(0.5, 0, 0), new Vector3d(0, rotatey, rotatez));
        }

        private void SetPENTA_UNIT_pos()
        {
            PENTA_UNIT_pos[1] = new Vector3d(0, PENTA_radius, 0);
            PENTA_UNIT_pos[3] = new Vector3d(0, -PENTA_radius, 0);
        }
        private void SetOCTO_UNIT_pos()
        {
            OCTO_UNIT_pos[1] = new Vector3d(0, OCTO_radius, 0);
            //OCTO UNIT pos[3]のz座標-0.3で複雑な形に
            OCTO_UNIT_pos[3] = new Vector3d(0, OCTO_radius / 2, 0);
        }

        
        System.Drawing.Color[] colors = new System.Drawing.Color[]
        {
            System.Drawing.Color.Yellow,
            System.Drawing.Color.Blue,
            System.Drawing.Color.Red,
            System.Drawing.Color.Green,
            System.Drawing.Color.OrangeRed,
            System.Drawing.Color.SkyBlue,
            System.Drawing.Color.Purple,
            System.Drawing.Color.Brown,
            System.Drawing.Color.LightGreen,
        };

        /*ユニットの基本形を描画する*/
        private void DrawUnit(List<Vertex> vertices, List<int[]> faces, bool isRotate)
        {

            //頂点の描画
            GL.Enable(EnableCap.Normalize);
            
            GL.PushMatrix();
            GL.Scale(scale, scale, scale);
            GL.Rotate(_rotateAngleY, 0, 0, 1);
            GL.Rotate(angle, 0, 1, 0);
            if (isDisplayUnit)
            {
                GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
            }
            GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
            GL.PointSize(6);
            GL.Disable(EnableCap.Lighting);
            GL.Begin(BeginMode.Points);
            for (int vertexpoint = 0; vertexpoint < vertices.Count; vertexpoint++)
            {
                if (!Seeker_MainSystem.FixedVertexIndexes.Contains(vertexpoint) && !Seeker_MainSystem.VertexIndexOnUnitEdges.Contains(vertexpoint))
                {
                    GL.Color3((byte)0,(byte) 0, (byte)0xFF);
                }
                else if(Seeker_MainSystem.FixedVertexIndexes.Contains(vertexpoint))
                {
                    GL.Color3((byte)0xFF, (byte)0, (byte)0);
                }
                else
                {
                    GL.Color3((byte)0, (byte)0xFF, (byte)0);
                }
                Vertex vertex = vertices[vertexpoint];
                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.003);
                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.003);
            }
            GL.End();
            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);

            //エッジの描画

            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                int faceCount = faces[faceIndex].Length;
                int[] indexes = new int[faceCount];
                for (int j = 0; j < faceCount; j++)
                {
                    indexes[j] = faces[faceIndex][j];
                }
                /*
                GL.PushMatrix();
                GL.Scale(scale, scale, scale);
                GL.Rotate(angle, 0, 1, 0);
                GL.Begin(BeginMode.Triangles);
                for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                {
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                }
                GL.End();
                GL.PopMatrix();
                */

                //右上
                GL.PushMatrix();
                GL.Scale(scale, scale, scale);
                GL.Rotate(_rotateAngleY, 0, 0, 1);
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                
                if (isDisplayUnit)
                {
                    GL.Rotate(rot, -1, 0, 0);
                }
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0,0,-Seeker_MainSystem.InnerBottomErrorZ);
                GL.Disable(EnableCap.Light0);
                GL.Begin(BeginMode.Lines);
                for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                {
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % faceCount]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ-0.001);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ-0.001);
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.001);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ + 0.001);
                }
                GL.End();
                GL.Enable(EnableCap.Light0);
                GL.PopMatrix();
                
                //左右対称
                /*
                GL.PushMatrix();
                GL.Scale(-1, 1, 1);
                GL.Scale(scale, scale, scale);
                GL.Rotate(_rotateAngleY, 0, 0, -1);
                GL.Rotate(angle, 0, -1, 0);  //-------------------------(9)
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Disable(EnableCap.Light0);
                GL.Begin(BeginMode.Lines);
                for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                {
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % faceCount]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.001);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ - 0.001);
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.001);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ + 0.001);
                }
                GL.End();
                GL.Enable(EnableCap.Light0);
                //右上
                GL.PopMatrix();
                */
                GL.PushMatrix();
                GL.Scale(-1, 1, 1);
                GL.Scale(scale, scale, scale);
                GL.Rotate(_rotateAngleY, 0, 0, 1);
                GL.Rotate(angle, 0, -1, 0);
                if(isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                Vector3d p0 = vertices[indexes[0]].VertexPosition;
                Vector3d p1 = vertices[indexes[1]].VertexPosition;
                Vector3d p2 = vertices[indexes[2]].VertexPosition;
                GL.Begin(BeginMode.Polygon);
                //GL.Color4(0x0, 0xff, 0xff, 0x20);
                for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                {
                    GL.Normal3(Vector3d.Cross(p2 - p1, p0 - p1));
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);

                }
                GL.End();
                GL.PopMatrix();
                /*
                GL.PushMatrix();
                GL.Scale(scale, scale, scale);
                GL.Rotate(_rotateAngleY, 0, 0, -1);
                GL.Rotate(angle, 0, 1, 0);
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Begin(BeginMode.Polygon);
                //GL.Color4(0x0, 0xff, 0xff, 0x20);
                for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                {
                        GL.Normal3(Vector3d.Cross(p2 - p1, p0 - p1));
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                    
                }
                GL.End();
                GL.PopMatrix();
                
                //上下反転
                    //左右対称
                    GL.PushMatrix();
                    GL.Scale(1, -1, 1);
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(_rotateAngleY, 0, 0, 1);
                    GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Disable(EnableCap.Light0);
                    GL.Begin(BeginMode.Lines);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % faceCount]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ - 0.001);
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ + 0.001);
                    }
                    GL.End();
                    GL.Enable(EnableCap.Light0);
                    GL.PopMatrix();

                    GL.PushMatrix();
                    GL.Scale(1, -1, 1);
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(_rotateAngleY, 0, 0, 1);
                    GL.Rotate(angle, 0, 1, 0);
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Begin(BeginMode.Polygon);
                    //GL.Color4(0x0, 0xff, 0xff, 0x20);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        GL.Normal3(Vector3d.Cross(p2 - p1, p0 - p1));
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);

                    }
                    GL.End();
                GL.PopMatrix();
                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(180, 0, 0, 1);
                    GL.Rotate(_rotateAngleY, 0, 0, -1);
                    GL.Rotate(angle, 0, -1, 0);
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                
                    GL.Begin(BeginMode.Polygon);
                    //GL.Color4(0x0, 0xff, 0xff, 0x20);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        GL.Normal3(Vector3d.Cross(p1 - p2, p1 - p0));
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                    }
                    GL.End();
                    GL.PopMatrix();

                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(180, 0, 0, 1);
                    GL.Rotate(_rotateAngleY, 0, 0, -1);
                    GL.Rotate(angle, 0, -1, 0);  //-------------------------(9)
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Disable(EnableCap.Light0);
                    GL.Begin(BeginMode.Lines);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % 3]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ - 0.001);
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ + 0.001);
                    }


                    GL.End();
                    GL.PopMatrix();
                    GL.Enable(EnableCap.Light0);
                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(180, 0, 0, 1);
                    GL.Rotate(_rotateAngleY, 0, 0, -1);
                    GL.Rotate(angle, 0, -1, 0);
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);

                GL.Begin(BeginMode.Polygon);
                    //GL.Color4(0x0, 0xff, 0xff, 0x20);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        GL.Normal3(Vector3d.Cross(p1 - p2, p1 - p0));
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                    }
                    GL.End();
                    GL.PopMatrix();

                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    GL.Rotate(180, 0, 0, 1);
                    GL.Rotate(_rotateAngleY, 0, 0, -1);
                    GL.Rotate(angle, 0, -1, 0);  //-------------------------(9)
                if (isDisplayUnit)
                {
                    GL.Rotate(90 - Seeker_Sys.Seeker_ShapeData.dihedralAngle_OCTO / 2, -1, 0, 0);
                }
                GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                GL.Disable(EnableCap.Light0);
                    GL.Begin(BeginMode.Lines);
                    for (int vertexpoint = 0; vertexpoint < faceCount; vertexpoint++)
                    {
                        Vertex vertex = vertices[indexes[vertexpoint]];
                        Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % 3]];
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ - 0.001);
                        GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ + 0.001);
                        GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ + 0.001);
                    }
                    GL.End();
                    GL.PopMatrix();
                    GL.Enable(EnableCap.Light0);
                
                */
            }

        }
        private void glControl_Load(object sender, EventArgs e)
        { //--(4)

            GL.ClearColor(System.Drawing.Color.White); // 背景色の設定
            GL.Enable(EnableCap.DepthTest); // デプスバッファの使用
            /*
            for (int vindex = 0; vindex < vertexcount; vindex++)
            {
                Vertex vertex = new Vertex((byte)vindex, pos[vindex]);
                vertexes.Add(vertex);
            }*/
            for (int vindex = 0; vindex < OCTO_pos.Length; vindex++)
            {
                Vertex vertex = new Vertex((byte)vindex, OCTO_pos[vindex]);
                OCTO_vertexes.Add(vertex);
            }
            for (int vindex = 0; vindex < PENTA_UNIT_pos.Length; vindex++)
            {
                Vertex vertex = new Vertex((byte)vindex, PENTA_UNIT_pos[vindex]);
                PENTA_UNIT_vertexes.Add(vertex);
            }
            for (int vindex = 0; vindex < OCTO_UNIT_pos.Length; vindex++)
            {
                Vertex vertex = new Vertex((byte)vindex, OCTO_UNIT_pos[vindex]);
                OCTO_UNIT_vertexes.Add(vertex);
            }
            System.Console.WriteLine("===============DEBUG LOG===============");
            /*
            if(Seeker_MainSystem.isDebugging)
            {
                Seeker_MainSystem.GetHalfTriangleUnitObjFile(3, "halftriangle");
                //Seeker_MainSystem.LoadObjFlie(@"test2.obj", vertexes, edges, angle, scale);
                Seeker_MainSystem.LoadObjFlie(@"halftriangle.obj", vertexes, edges, angle, scale);
            }
            else
            {
                Seeker_MainSystem.GetTriangleUnitObjFile(2, "aaa");
                Seeker_MainSystem.LoadObjFlie(@"testData.obj", vertexes, edges, angle, scale);
            }*/
            //Seeker_MainSystem.GetTriangleUnitObjFile(2, "aaa");
            //Seeker_MainSystem.GetHalfTriangleUnitObjFile(5,"halftriangle");
            Seeker_MainSystem.GetPleatHalfTriangleUnitObjFile(4, "halftriangle");
            //Seeker_MainSystem.LoadObjFlie(@"halftriangle2.obj", vertexes, edges, angle, scale);
            //Seeker_MainSystem.LoadObjFlie(@"halftriangle.obj", vertexes, edges, angle, scale);
            Seeker_MainSystem.LoadObjFlie(@"untitled2.obj", vertexes, edges, angle, scale);
            //Seeker_MainSystem.LoadObjFlie(@"testData.obj", vertexes, edges, angle, scale);
            Func<double[], double> f = x => Math.Cos(x[0]) * Math.Cos(x[0]) * Math.Cos(x[1]) * Math.Cos(x[1]);// * Math.Cos(x[2]) * Math.Cos(x[2]);//x[0] * x[0] + x[1] * x[1] + 1.0;
            var initialX = new double[] { 3.14, 3.14, 1};
            int iteration = 1000;
            double learningRate = 0.01;
            double[] answer = Seeker_Sys.SteepestDescentMethodMV.Compute(f, initialX, iteration, learningRate);
            Console.WriteLine("最小値は"+answer[0].ToString() + " " + answer[1].ToString() + " " + answer[2].ToString() + " (" + Math.Cos(answer[0]) * Math.Cos(answer[0]) * Math.Cos(answer[1]) * Math.Cos(answer[1]) + ")");
             arcball = new Seeker_Sys.Arcball(glControl.Size.Height / 2);
            //Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
        }

        private void glControl_Resize(object sender, EventArgs e)
        { //------(5)
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);
            _viewport = new int[] { 0, 0, glControl.Size.Width, glControl.Size.Height };
            GL.MatrixMode(MatrixMode.Projection); // projectionの設定
            Matrix4 projection =
              Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,
              (float)glControl.Size.Width / (float)glControl.Size.Height,
              0.1f, 64.0f);
            _perspectiveProjection = projection;
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview); // 視界の設定
            Matrix4 look = Matrix4.LookAt(3.0f * Vector3.UnitX + 2.0f * Vector3.UnitY,
              Vector3.Zero, Vector3.UnitY);
            _modelView = look;
            GL.LoadMatrix(ref look);
            GL.Enable(EnableCap.Lighting); // 光源の利用を宣言
            float[] position = new float[] { 3.0f, 2.0f, 3.0f, 0.0f };
            // ライト 0 の設定と使用
            GL.Light(LightName.Light0, LightParameter.Position, position);
            //GL.Enable(EnableCap.Light0); // 光源をオンにする
            GL.Enable(EnableCap.Lighting);
            float[] position1 = new float[] { 3.0f, 2.0f, 3.0f, 0.0f };
            GL.Light(LightName.Light0, LightParameter.Position, position);
            GL.Enable(EnableCap.Light0); // 光源をオンにする
            float[] position2 = new float[] { 3.0f, 2.0f, 3.0f, 0.0f };
            // ライト 0 の設定と使用
            GL.Light(LightName.Light2, LightParameter.Position, position2);
            GL.Enable(EnableCap.Light2); // 光源をオンにする
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        { //----(6)
            GL.Clear(ClearBufferMask.ColorBufferBit |
              ClearBufferMask.DepthBufferBit);
            glControl.MakeCurrent();
            //GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,System.Drawing.Color.Green);// 赤の直方体を描画
            //GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new Color4(0,254,0,0));
            if (isDrawReferLine)
            {
                DrawReferLine();
            }
            //DrawVertexPoint();
            DrawUnit(vertexes, edges, false);
            //drawBox(); //------------------------------------------------------(7)
            //DrawPENTA();
            //DrawOCTO_UNIT(0);
            //DrawOCTO();

            glControl.SwapBuffers(); //---------------------------------------(8)
        }

        
        /// <summary>
        /// 軸線を表示する(赤はx軸, 緑はy軸)
        /// </summary>
        private void DrawReferLine()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Scale(scale, scale, scale);
                GL.Disable(EnableCap.Lighting);
                //angle =10; //-------------------------------------------(10)
                GL.Begin(PrimitiveType.Lines);
                GL.Color3((byte)255, (byte)0, (byte)0);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(3, 0, 0);
                GL.End();
                GL.Begin(PrimitiveType.Lines);
                GL.Color3((byte)0, (byte)255, (byte)0);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 3, 0);
                GL.End();
                GL.Begin(PrimitiveType.Lines);
                GL.Color3((byte)0, (byte)0, (byte)255);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 3);
                GL.End();
                GL.Enable(EnableCap.Lighting);
            }
            GL.PopMatrix();
        }
        /// <summary>
        /// マウスでクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            float[] pixels = new float[3];
            GL.ReadPixels(e.X, glControl.Size.Height - e.Y, 1, 1, PixelFormat.Rgb, PixelType.Float, pixels);
            pixels[0] = pixels[0] * 255;
            pixels[1] = pixels[1] * 255;
            pixels[2] = pixels[2] * 255;
            Debug.Print(((int)pixels[0]).ToString() + "," + ((int)pixels[1]).ToString() + "," + ((int)pixels[2]).ToString() + "," + glControl.Size.Width + "," + glControl.Size.Height);
            for (int index = 0; index < vertexes.Count; index++)
            {
                //Green の色をIDとする
                if (vertexes[index].ID == pixels[2])
                {
                    vertexes[index].VertexX += 0.1;
                    break;
                }
            }
            glControl.Refresh();

            /*
            GL.RenderMode(RenderingMode.Select);
            Debug.Print("Clicked");
            GL.RenderMode(RenderingMode.Render);
            */
            /*
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int winW = viewport[2];
            int winH = viewport[3];
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int sizeBuffer = 2048;
                int[] pickSelectBuffer = new int[sizeBuffer];

                PreProcessOfObjectPick(
                    sizeBuffer, pickSelectBuffer,
                    (uint)e.X, (uint)e.Y, 5, 5);
                //DrawSelection();

                IList<SelectedObject> selectedObjs = ObjectPickAfterProcessing(pickSelectBuffer,
                    (uint)e.X, (uint)e.Y);

                SelectedPartId = 0;
                if (selectedObjs.Count > 0)
                {
                    int[] selectFlg = selectedObjs[0].Name;
                    System.Diagnostics.Debug.WriteLine("selectFlg[1] = " + selectFlg[1]);
                    SelectedPartId = selectFlg[1];
                }
                glControl.Invalidate();
            }
            */
        }

        private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //上向きにホイールを回した
            if (e.Delta > 0)
            {
                scale += 0.1f;
            }
            else
            {
                scale -= 0.1f;
                if (scale <= 0)
                {
                    scale = 0.1f;
                }
            }
            glControl.Refresh();
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            //glControl.Refresh();// 3次元表示を更新する 	//----------------(12)
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            //Debug.Print(viewport[0].ToString() + "," + viewport[1].ToString() + "," + viewport[2].ToString() + "," + viewport[3].ToString());
            glControl.Refresh();
            Debug.Print("_isDragging" + _isDraggingRightButton);
        }

        /// <summary>
        /// ボタンが押されたときの挙動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_saveObjFile_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter(@"testing.obj", false, Encoding.UTF8))
            {
                    streamWriter.WriteLine("# Test Code");
                    /*
                     ユニットの右上,右下,左上,左下から順に1,2,3,4番とする
                     */
                    //ユニット右下部分の頂点オフセット(0番から数えるため1つずれてる)
                    int offsetUnit2 = vertexes.Count;
                    int offsetUnit3 = offsetUnit2 + (vertexes.Count - Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count);
                    int offsetUnit4 = offsetUnit3 + (vertexes.Count - Seeker_MainSystem.VertexIndexOnUnitLeftEdge.Count);
                    //+1はleftEndVertexが二回引かれている分の補正
                    int VertexAllCountInPerfectUnit = offsetUnit4 + vertexes.Count - Seeker_MainSystem.VertexIndexOnUnitLeftEdge.Count - Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count + 1;
                    //ユニットの完成形の頂点データを全て格納する
                    for (int vertexCount = 0; vertexCount < vertexes.Count * 4; vertexCount++)
                    {
                        if (vertexCount < vertexes.Count)
                        {
                            streamWriter.WriteLine("v " + vertexes[vertexCount].VertexX + " " + vertexes[vertexCount].VertexY + " " + vertexes[vertexCount].VertexZ);
                        }
                        else if (vertexCount < vertexes.Count * 2)
                        {
                            streamWriter.WriteLine("v " + vertexes[vertexCount - vertexes.Count].VertexX
                                + " -" + vertexes[vertexCount - vertexes.Count].VertexY
                                + " " + vertexes[vertexCount - vertexes.Count].VertexZ);
                        }
                        else if (vertexCount < vertexes.Count * 3)
                        {
                            streamWriter.WriteLine("v -" + vertexes[vertexCount - vertexes.Count * 2].VertexX
                                + " " + vertexes[vertexCount - vertexes.Count * 2].VertexY
                                + " " + vertexes[vertexCount - vertexes.Count * 2].VertexZ);
                        }
                        else if (vertexCount < vertexes.Count * 4)
                        {
                            streamWriter.WriteLine("v -" + vertexes[vertexCount - vertexes.Count * 3].VertexX
                                + " -" + vertexes[vertexCount - vertexes.Count * 3].VertexY
                                + " " + vertexes[vertexCount - vertexes.Count * 3].VertexZ);
                        }

                    }
                    /*
                    //ユニットの頂点数 * 2 - 底辺の頂点数分頂点情報を格納する
                    for (int vertexCount = 0; vertexCount < vertexes.Count + (vertexes.Count - Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count); vertexCount++)
                    {
                        if (vertexCount < vertexes.Count)
                        {
                            streamWriter.WriteLine("v " + vertexes[vertexCount].VertexX + " " + vertexes[vertexCount].VertexY + " " + vertexes[vertexCount].VertexZ);
                        }
                        else if (offsetUnit2 <= vertexCount && vertexCount < offsetUnit3)
                        {
                            streamWriter.WriteLine("v " + vertexes[vertexCount - vertexes.Count + Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count].VertexX
                                + " -" + vertexes[vertexCount - vertexes.Count + Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count].VertexY
                                + " " + vertexes[vertexCount - vertexes.Count + Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count].VertexZ);
                        }

                    }*/
                    streamWriter.WriteLine("vn 0 0 1");
                    streamWriter.WriteLine("vn -1 0 0");
                    streamWriter.WriteLine("vn 1 0 0");
                    streamWriter.WriteLine("vn 0 -1 0");
                    streamWriter.WriteLine("vn 0 1 0");
                    streamWriter.WriteLine("vn 0 0 1");

                    for (int faceCount = 0; faceCount < edges.Count * 4; faceCount++)
                    {
                        string s = "f ";
                        //ひっくり返したユニットを表示する際、objファイルで出力する面の順番がひっくり返ってしまうため修正するために処理を変更する
                        if (faceCount < edges.Count)
                        {
                            for (int faceID = 0; faceID < edges[0].Length; faceID++)
                            {
                                int vertexIndex = edges[faceCount][faceID];
                                s += (vertexIndex + 1).ToString() + "//1 ";
                            }
                        }
                        else if(faceCount < edges.Count * 2)
                        {
                            for (int faceID = edges[0].Length - 1; faceID >= 0; faceID--)
                            {

                                int vertexIndex = edges[faceCount - edges.Count][faceID];
                                {
                                    s += (vertexIndex + vertexes.Count + 1).ToString() + "//1 ";
                                }
                            }
                        }
                        else if (faceCount < edges.Count * 3)
                        {
                            for (int faceID = edges[0].Length - 1; faceID >= 0; faceID--)
                            {

                                int vertexIndex = edges[faceCount - edges.Count * 2][faceID];
                                {
                                    s += (vertexIndex + vertexes.Count * 2 + 1).ToString() + "//1 ";
                                }
                            }
                        }
                        else if (faceCount < edges.Count * 4)
                        {
                            for (int faceID = 0; faceID < edges[0].Length; faceID++)
                            {

                                int vertexIndex = edges[faceCount - edges.Count * 3][faceID];
                                {
                                    s += (vertexIndex + vertexes.Count * 3 + 1).ToString() + "//1 ";
                                }
                            }
                        }
                        /*
                        else if(faceCount < edges.Count * 3)
                        {
                            for (int faceID = edges[0].Length - 1; faceID >= 0; faceID--)
                            {

                                int vertexIndex = edges[faceCount - edges.Count * 2][faceID];
                                //底辺上の頂点の場合は何もしない
                                if (Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Contains(vertexIndex))
                                {
                                    s += (vertexIndex + 1).ToString() + "//1 ";
                                }

                                else
                                {
                                    s += (vertexIndex - Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count + 1 + edges.Count + 1).ToString() + "//1 ";
                                }
                            }
                        }
                        else if(faceCount < edges.Count * 4)
                        {

                        }*/

                        streamWriter.WriteLine(s);
                    }
                    /*
                    for (int faceCount = 0; faceCount < edges.Count * 2; faceCount++)
                    {
                        string s = "f ";
                        //ひっくり返したユニットを表示する際、objファイルで出力する面の順番がひっくり返ってしまうため修正するために処理を変更する
                        if (faceCount < edges.Count)
                        {
                            for (int faceID = 0; faceID < edges[0].Length; faceID++)
                            {
                                int vertexIndex = edges[faceCount][faceID];
                                s += (vertexIndex + 1).ToString() + "//1 ";
                            }
                        }
                        else
                        {
                            for (int faceID = edges[0].Length - 1; faceID >= 0; faceID--)
                            {

                                int vertexIndex = edges[faceCount - edges.Count][faceID];
                                //底辺上の頂点の場合は何もしない
                                if (Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Contains(vertexIndex))
                                {
                                    s += (vertexIndex + 1).ToString() + "//1 ";
                                }

                                else
                                {
                                    s += (vertexIndex - Seeker_MainSystem.VertexIndexOnUnitButtomEdge.Count + 1 + edges.Count + 1).ToString() + "//1 ";
                                }
                            }
                        }

                        streamWriter.WriteLine(s);
                    }
                    */
                

            }
        }

        private void button_AssembleUnitShape(object sender, RoutedEventArgs e)
        {
            if (isDisplayUnit)
            {
                isDisplayUnit = false;
            }
            else
            {
                isDisplayUnit = true;
            }
        }

        private void buttonDrawReferLine(object sender, RoutedEventArgs e)
        {
            if(!isDrawReferLine)
            {
                isDrawReferLine = true;
            }
            else
            {
                isDrawReferLine = false;
            }
        }
        private void glControl_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                angle += 1;
            }
            if(e.KeyCode == Keys.Q)
            {
                _rotateAngleY += 1;
            }
            if (e.KeyCode == Keys.W)
            {
                vertexes[manipulateVertexIndex].VertexZ -= 0.001;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Console.WriteLine("VertexZ = " + vertexes[manipulateVertexIndex].VertexPosition.Z);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.R)
            {
                _rotateAngleY = 0;
                _rotateAngleZ = 0;
            }
            if (e.KeyCode == Keys.S)
            {
                vertexes[manipulateVertexIndex].VertexZ += 0.01;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.F)
            {
                vertexes[manipulateVertexIndex].VertexX -= 0.001;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.H)
            {
                vertexes[manipulateVertexIndex].VertexX += 0.001;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.G)
            {
                vertexes[manipulateVertexIndex].VertexY -= 0.001;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.T)
            {
                vertexes[manipulateVertexIndex].VertexY += 0.001;
                vertexes[manipulateVertexIndex].VertexPosition = new Vector3d(vertexes[manipulateVertexIndex].VertexX, vertexes[manipulateVertexIndex].VertexY, vertexes[manipulateVertexIndex].VertexZ);
                Seeker_MainSystem.SetAdjustedUnitVertexes(vertexes, 5, Seeker_MainSystem.InnnerVertexIndex, Seeker_MainSystem.InnerVertexIndexOnButtomEdge);
            }
            if (e.KeyCode == Keys.M)
            {
                rot += 0.5f;
            }
            if(e.KeyCode == Keys.Down || e.KeyCode == Keys.Z)
            {
                manipulateVertexIndex++;
                if(manipulateVertexIndex >= vertexes.Count)
                {
                    manipulateVertexIndex = 0;
                }
                Console.WriteLine("Now ManiPulate Index is " + manipulateVertexIndex.ToString());

            }
            if(e.KeyCode == Keys.Enter)
            {
                Console.WriteLine("ENTER KEY PRESSED");
                if(!Seeker_MainSystem.FixedVertexIndexes.Contains(manipulateVertexIndex))
                {
                    Console.WriteLine("Now Vertex " + manipulateVertexIndex.ToString() + "is Fixed Point");
                    Seeker_MainSystem.FixedVertexIndexes.Add(manipulateVertexIndex);
                }
            }
            if(e.KeyCode == Keys.X)
            {
                Console.WriteLine("X key PRESSED");
                if(Seeker_MainSystem.FixedVertexIndexes.Contains(manipulateVertexIndex))
                {
                    Console.WriteLine("Now Vertex " + manipulateVertexIndex.ToString() + "is removed from Fixed Point");
                    Seeker_MainSystem.FixedVertexIndexes.Remove(manipulateVertexIndex);
                }
            }
            glControl.Refresh();
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (_isDraggingRightButton)
            {
                float currentMouseX = e.X;
                float currentMouseY = e.Y;
                Vector3d mouseMove = new Vector3d(0, currentMouseX - _mouseX, currentMouseY - _mouseY);
                Vector3d v1 = new Vector3d(0, _mouseX, _mouseY);
                Vector3d v2 = new Vector3d(0, currentMouseY, currentMouseY);
                //_rotato;
                _rotateAngleY = arcball.GetRotateAngle(_mouseX, _mouseY, currentMouseX, currentMouseY, glControl.Size.Width / 2, glControl.Size.Height / 2);
                _rotateAngleZ = MathF.Sqrt((currentMouseY - _mouseY) * (currentMouseY - _mouseY));
                _mouseX = currentMouseX;
                _mouseY = currentMouseY;
            }
            glControl.Refresh();

        }

        private void glControl_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_isDraggingRightButton)
                {
                    _isDraggingRightButton = false;
                }
            }
        }

        private void glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (!_isDraggingRightButton)
                {
                    _isDraggingRightButton = true;
                    //_mouseX = e.X;
                    //_mouseY = e.Y;
                    Debug.Print(_mouseX.ToString() + ", " + _mouseY.ToString() + ": ");
                }
            }
        }



        /*オブジェクトのピック処理を以下に記述*/
        private void PreProcessOfObjectPick(int sizebuffer, int[] selectBuffer,
            uint pointX, uint pointY,
            uint deltaX, uint deltaY)
        {
            //Selection初期化
            GL.SelectBuffer(sizebuffer, selectBuffer);
            GL.RenderMode(RenderingMode.Select);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            //名前スタックの初期化
            GL.InitNames();

            GL.MatrixMode(MatrixMode.Projection);
            //GL.PushMatrix();
            GL.LoadIdentity();
            OpenTKExSys.GluPickMatrix(pointX, viewport[3] - pointY, deltaX, deltaY, viewport);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,
                                 (float)glControl.Size.Width / (float)glControl.Size.Height,
                                 0.1f, 64.0f);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview); // 視界の設定
            Matrix4 look = Matrix4.LookAt(3.0f * Vector3.UnitX + 2.0f * Vector3.UnitY,
              Vector3.Zero, Vector3.UnitY);
        }

        private IList<SelectedObject> ObjectPickAfterProcessing(int[] selecetedBuffer,
            uint pointX, uint pointY)
        {
            GL.MatrixMode(MatrixMode.Projection); // projectionの設定
            Matrix4 projection =
              Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4,
              (float)glControl.Size.Width / (float)glControl.Size.Height,
              0.1f, 64.0f);
            _perspectiveProjection = projection;
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview); // 視界の設定
            Matrix4 look = Matrix4.LookAt(3.0f * Vector3.UnitX + 2.0f * Vector3.UnitY,
              Vector3.Zero, Vector3.UnitY);
            _modelView = look;
            GL.LoadMatrix(ref look);

            IList<SelectedObject> selectedObjects = new List<SelectedObject>();

            //ヒットしたオブジェクトの数
            int hits = GL.RenderMode(RenderingMode.Render);
            Debug.Print("Hits = " + hits.ToString());

            if (hits <= 0)
            {
                return selectedObjects;
            }

            IList<PickedObject> pickedObjects = new List<PickedObject>();
            for (int i = 0; i < hits; i++)
            {
                pickedObjects.Add(new PickedObject());
            }
            int iSel = 0;
            for (int i = 0; i < pickedObjects.Count; i++)
            {
                uint nameDepth = (uint)selecetedBuffer[iSel];
                System.Diagnostics.Debug.Assert(nameDepth <= 4);
                pickedObjects[i].NameDepth = nameDepth;
                iSel++;
                pickedObjects[i].MinDepth = (float)selecetedBuffer[iSel] / 0x7fffffff;
                iSel++;
                pickedObjects[i].MaxDepth = (float)selecetedBuffer[iSel] / 0x7fffffff;
                iSel++;
                for (int j = 0; j < nameDepth; j++)
                {
                    pickedObjects[i].Name[j] = selecetedBuffer[iSel];
                    iSel++;
                }
            }
            // sort picked object in the order of min depth
            for (int i = 0; i < pickedObjects.Count; i++)
            {
                for (int j = i + 1; j < pickedObjects.Count; j++)
                {
                    if (pickedObjects[i].MinDepth > pickedObjects[j].MinDepth)
                    {
                        PickedObject tmp = pickedObjects[i];
                        pickedObjects[i] = pickedObjects[j];
                        pickedObjects[j] = tmp;
                    }
                }
            }
            for (int i = 0; i < pickedObjects.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("pickedObjects[" + i + "]");
                System.Diagnostics.Debug.WriteLine("NameDepth = " + pickedObjects[i].NameDepth + " " +
                    "MinDepth = " + pickedObjects[i].MinDepth + " " +
                    "MaxDepth = " + pickedObjects[i].MaxDepth);
                for (int j = 0; j < pickedObjects[i].NameDepth; j++)
                {
                    System.Diagnostics.Debug.Write("Name[" + j + "] = " + pickedObjects[i].Name[j] + " ");
                }
                System.Diagnostics.Debug.WriteLine("");
            }

            selectedObjects.Clear();
            for (int i = 0; i < pickedObjects.Count; i++)
            {
                System.Diagnostics.Debug.Assert(pickedObjects[i].NameDepth <= 4);
                SelectedObject selectedObj = new SelectedObject();
                selectedObj.NameDepth = 3;
                for (int itmp = 0; itmp < 3; itmp++)
                {
                    selectedObj.Name[itmp] = pickedObjects[i].Name[itmp];
                }
                selectedObjects.Add(selectedObj);

                double ox, oy, oz;
                {
                    double[] mvMatrix = new double[16];
                    double[] pjMatrix = new double[16];
                    int[] viewport = new int[4];

                    GL.GetInteger(GetPName.Viewport, viewport);

                    GL.GetDouble(GetPName.ModelviewMatrix, mvMatrix);

                    GL.GetDouble(GetPName.ProjectionMatrix, pjMatrix);

                    //Tao.OpenGl.Glu.gluUnProject(
                    //    (double)pointX,
                    //    (double)viewport[3] - pointY,
                    //    pickedObjects[i].MinDepth * 0.5,
                    //    mvMatrix, pjMatrix, viewport,
                    //    out ox, out oy, out oz);
                    OpenTKExSys.GluUnProject(
                        pointX,
                        viewport[3] - pointY,
                        pickedObjects[i].MinDepth * 0.5,
                        mvMatrix, pjMatrix, viewport,
                        out ox, out oy, out oz);
                }
                selectedObj.PickedPos = new Vector3((float)ox, (float)oy, (float)oz);
            }
            return selectedObjects;
        }
    }
}
