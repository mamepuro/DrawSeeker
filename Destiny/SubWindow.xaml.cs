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
using GlmSharp;


namespace Destiny
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class SubWindow : Window
    {
        public SubWindow()
        {
            InitializeComponent();
            myTimer.Tick += new EventHandler(Timer1_Tick);
            myTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            myTimer.Start();
            SetPENTA_UNIT_pos();
            SetOCTO_UNIT_pos();
            GetPos();
        }

        private DispatcherTimer myTimer = new DispatcherTimer();
        /// <summary>
        /// 右クリック中かどうか
        /// </summary>
        private bool _isDraggingRightButton = false;
        private float _mouseX = 0;
        private float _mouseY = 0;
        private Vector3d _rotato;
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
        private float scale = 1.5f;
        private float HEXA_radius = 0.5f;
        private float PENTA_radius = 1 / (2 * MathF.Tan(MathF.PI / 5));
        //正十二面体の内接球半径
        private float PENTA_innerball_radius = ((MathF.Sqrt(5) + 1) * (MathF.Sqrt(25 + 10 * MathF.Sqrt(5)))) / (20);
        private float ICOSA_outerball_radius = MathF.Sqrt(10 + 2 * MathF.Sqrt(5))  / (4);
        private float OCTO_radius = 1 / (2 * (float)Math.Sqrt(3));
        int angle = 0;
        double w = 0.5, h = 0.2, d = 0.7;
        float rad = (90 - dihedralAngle_OCTO / 2) * (float)Math.PI / 180;
        float rad_ICOSA = (90 - dihedralAngle_ICOSA / 2) * (float)Math.PI / 180;
        float rotatey;
        float rotatez;
        Seeker_Sys.Arcball arcball;
        private bool isDisplayUnit = false;
        /// <summary>
        /// 多面体の面の中心部分の回転軸
        /// </summary>
        Vector3d nor_axis;
        const float dihedralAngle_HEXA = 90.0f;
        const float dihedralAngle_OCTO = 109.47f;
        const float dihedralAngle_PENTA = 116.56f;
        const float dihedralAngle_ICOSA = 138.11f;

        int vertexcount = 8;
        List<Vertex> vertexes = new List<Vertex>();
        List<int[]> edges = new List<int[]>();
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

        int[,] faceIndex = new int[,]
        {
            {7,3,2,6},
            {4, 5, 7,6},
            {1,0,4,5},//
            {1,0,2,3},
            {5,1,3,7 },
            {0,4,6,2 }
            /**/
        };
        int[,] faceIndex_OCTO = new int[,]
        {
            {1,2,4},
            {0,2,4},
            {0,3,4},//
            {3,1,4},
            {1,2,5},
            {0,2,5},
            {0,3,5},//
            {3,1,5},
            /**/
        };
        int[,] faceIndex_OCTO2 = new int[,]
{
    //{ 1,2,4},
            {6,1,2},
            {6,4,1 },
            {6,4,2 },

            //{0,2,4},
            { 8,0,2},
            { 8,4,2},
            { 8,0,4},
            //{0,3,4},//
            { 9,0,3},
            { 9,0,4},
            { 9,3,4},
            //{3,1,4},
            {7,3,4},
            {7,1,4},
            {7,1,3},
            //{1,2,5},
            {10,1,2},
            {10,5,1 },
            {10,5,2 },
            //{0,2,5},
            { 12,0,2},
            { 12,5,2},
            { 12,0,5},
            //{0,3,5},//
            { 13,0,3},
            { 13,0,5},
            { 13,3,5},
            //{3,1,5},
            {11,3,5},
            {11,1,5},
            { 11,1,3},
    /**/
};
        int[,] faceIndex_PENTA_UNIT = new int[,]
        {
            {0,2,1},
        };
        int[,] faceIndex_OCTO_UNIT = new int[,]
{
           //{2,1,0},
           {3,1,0 },
           {3,1,2 },
           {3,2,0 },
};
        int[] rotate_PENTA_UNIT = new int[]
            {
                0,
                72,
                72*2,
                72*3,
                72*4,
                72*5,
            };
        int[] rotateface_HEXA_UNIT = new int[]
{
                0,
                90,
                180,
                270
};
        int[] rotateUnit_HEXA_UNIT = new int[]
        {
            0,
            90,
            180,
            270,
            0
        };
        int[] rotateface_OCTO_UNIT = new int[]
    {
                0,
                120,
                240
    };
        int[] rotateUnit_OCTO_UNIT = new int[]
        {
            0,
            90,
            180,
            270,
        };
        int[] rotateface_ICOSA_UNIT = new int[]
{
                0,
                120,
                240
};
        int[] rotateUnit_ICOSA_UNIT = new int[]
        {
                0,
                72,
                72*2,
                72*3,
                72*4,
        };

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

        private void DrawUnit(List<Vertex> vertices, List<int[]> faces)
        {
            GL.PushMatrix();
            GL.Scale(scale, scale, scale);
            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
            GL.Begin(BeginMode.Points);
            for (int vertexpoint = 0; vertexpoint < vertices.Count; vertexpoint++)
            {
                Vertex vertex = vertices[vertexpoint];
                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
            }
            GL.End();
            GL.PopMatrix();
            //エッジの描画

            int[] indexes = new int[3];
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                indexes[0] = faces[faceIndex][0];
                indexes[1] = faces[faceIndex][1];
                indexes[2] = faces[faceIndex][2];
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
                GL.PushMatrix();
                GL.Scale(scale, scale, scale);
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Begin(BeginMode.Lines);
                for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                {
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % 3]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ);
                }
                GL.End();
                GL.PopMatrix();

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
            System.Console.WriteLine("---------------------Sub Window Successfuly built.-----------------------");
            arcball = new Seeker_Sys.Arcball(glControl.Size.Height / 2);
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
            Matrix4 look = Matrix4.LookAt(3.0f * Vector3.UnitX + 1.0f * Vector3.UnitY,
              Vector3.Zero, Vector3.UnitY);
            _modelView = look;
            GL.LoadMatrix(ref look);
            GL.Enable(EnableCap.Lighting); // 光源の利用を宣言
            float[] position = new float[] { 1.50f, 1.0f, 1.50f, 0.0f };
            // ライト 0 の設定と使用
            GL.Light(LightName.Light0, LightParameter.Position, position);
            GL.Enable(EnableCap.Light0); // 光源をオンにする
            float[] position1 = new float[] { 3.0f, 2.0f, 3.0f, 0.0f };
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
            //DrawReferLine();
            //DrawVertexPoint();
            //DrawTETRA();
            //DrawHEXA();
            //DrawOCTO();
            //drawBox(); //------------------------------------------------------(7)
            //DrawPENTA();
            DrawICOSA();

            glControl.SwapBuffers(); //---------------------------------------(8)
        }

        private void DrawReferLine()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Scale(scale, scale, scale);
                //GL.Disable(EnableCap.Lighting);
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
        /// 正四面体ベースのユニット折り紙を表示する
        /// </summary>
        private void DrawTETRA() 
        {

                GL.PushMatrix();
                {
                    //正八面体の上半分と下半分をflagで場合分け
                    for (int flag = 0; flag < 1; flag++)
                    {
                        //正八面体の上/下半分の4面をiで場合分け
                        for (int i = 0; i < 4; i++)
                        {
                            //正八面体の１つの面を3つの
                            for (int j = 0; j < 3; j++)
                            {
                                for (int mirror = 0; mirror < 2; mirror++)
                                {
                                    DrawOCTO_UNIT(rotateface_OCTO_UNIT[j], rotateUnit_OCTO_UNIT[i], flag, i, j, mirror);
                                }
                            }

                        }
                    }

                }
                GL.PopMatrix();
        }
        /// <summary>
        /// 正六面体ベースのユニット折り紙を表示する
        /// </summary>
        private void DrawHEXA()
        {
            GL.PushMatrix();
            {
                //正八面体の上半分と下半分をflagで場合分け
                for (int flag = 0; flag < 1; flag++)
                {
                    //面の数
                    for (int i = 0; i < 5; i++)
                    {
                        //正八面体の１つの面を4つのユニットから作る
                        for (int j = 0; j < 4; j++)
                        {
                            for (int mirror = 0; mirror < 2; mirror++)
                            {
                                DrawHEXA_UNIT(rotateface_HEXA_UNIT[j], rotateUnit_HEXA_UNIT[i], flag, i, j, mirror);
                            }
                        }

                    }
                }

            }
            GL.PopMatrix();
        }
        /// <summary>
        /// 正六面体ベースのユニット折り紙を表示する
        /// </summary>
        /// <param name="faceAngle"></param>
        /// <param name="UnitAngle"></param>
        /// <param name="flag"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void DrawHEXA_UNIT(float faceAngle, float UnitAngle, int flag, int i, int j, int mirror)
        {
            //faceAngle =　面内のユニットの回転角
            {
                for (int faceCount = 0; faceCount < rotateface_HEXA_UNIT.Length; faceCount++)
                {
                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    if (flag == 0)
                    {

                        GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                        GL.Rotate(_rotateAngleY, 0, 1, 0);
                        GL.Rotate(_rotateAngleZ, 0, 0, -1);
                        if (i < 4)
                        {
                            GL.Rotate(UnitAngle, 0, 1, 0);
                            GL.Translate(0, 0, -0.5);
                            //ここから面をつくる
                            GL.Translate(0, 0.5, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -0.5, 0);
                            //ここまで
                            GL.Rotate(90 - dihedralAngle_HEXA, 1, 0, 0);
                        }
                        else
                        {
                            GL.Translate(0, 0.5, 0);
                            GL.Translate(0, 0, -0.5);
                            GL.Rotate(90, 1, 0, 0);
                            GL.Translate(0, 0, -0.5);
                            //ここから面をつくる
                            GL.Translate(0, 0.5, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -0.5, 0);
                            //ここまで
                            GL.Rotate(90 - dihedralAngle_HEXA, 1, 0, 0);
                        }

                        
                        GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                        if (mirror != 0)
                        {
                            GL.Scale(-1, 1, 1);
                        }
                        //GL.Normal3(-Vector3.UnitZ);
                    }
                    else
                    //下半分
                    {
                        GL.Rotate(180, 1, 0, 0);
                        GL.Rotate(angle, 0, -1, 0);  //-------------------------(9)
                        GL.Rotate(_rotateAngleY, 0, -1, 0);
                        GL.Rotate(_rotateAngleZ, 0, 0, 1);
                        GL.Rotate(UnitAngle, 0, 1, 0);
                        GL.Translate(0, 0, -0.5);
                        GL.Translate(0, rotatey, rotatez);
                        GL.Rotate(faceAngle, -nor_axis);
                        GL.Translate(0, -rotatey, -rotatez);
                        GL.Rotate(90 - dihedralAngle_HEXA / 2, 1, 0, 0);
                        GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                        if (mirror != 0)
                        {
                            GL.Scale(-1, 1, 1);
                        }
                        //GL.Normal3(Vector3.UnitZ);
                    }
                    DrawUnitPart(MainWindow.vertexes, MainWindow.edges, i + j + flag);

                    GL.PopMatrix();

                }


            }
            GL.PopMatrix();
        }

        /// <summary>
        /// 正八面体ベースのユニット折り紙を表示する
        /// </summary>
        private void DrawOCTO()
        {
            GL.PushMatrix();
            {
                //正八面体の上半分と下半分をflagで場合分け
                for (int flag = 0; flag < 2; flag++)
                {
                    //正八面体の上/下半分の4面をiで場合分け
                    for (int i = 0; i < 4; i++)
                    {
                        //正八面体の１つの面を3つの
                        for (int j = 0; j < 3; j++)
                        {
                            for (int mirror = 0; mirror < 2; mirror++)
                            {
                                DrawOCTO_UNIT(rotateface_OCTO_UNIT[j], rotateUnit_OCTO_UNIT[i], flag, i, j, mirror);
                            }
                        }

                    }
                }

            }
            GL.PopMatrix();
        }
        /// <summary>
        /// 正八面体ベースのユニット折り紙を表示する
        /// </summary>
        /// <param name="faceAngle"></param>
        /// <param name="UnitAngle"></param>
        /// <param name="flag"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void DrawOCTO_UNIT(float faceAngle, float UnitAngle, int flag, int i, int j, int mirror)
        {
            //faceAngle =　面内のユニットの回転角
            {
                for (int faceCount = 0; faceCount < faceIndex_OCTO_UNIT.GetLength(0); faceCount++)
                {
                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    //
                    if (flag == 0)
                    {

                        GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                        GL.Rotate(_rotateAngleY, 0, 1, 0);
                        GL.Rotate(_rotateAngleZ, 0, 0, -1);
                        GL.Rotate(UnitAngle, 0, 1, 0);
                        GL.Translate(0, 0, -0.5);
                        //ここから面をつくる
                        GL.Translate(0, rotatey, rotatez);
                        GL.Rotate(faceAngle, -nor_axis);
                        GL.Translate(0, -rotatey, -rotatez);
                        //ここまで
                        GL.Rotate(90 - dihedralAngle_OCTO / 2, 1, 0, 0);
                        GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                        if (mirror != 0)
                        {
                            GL.Scale(-1, 1, 1);
                        }
                        //GL.Normal3(-Vector3.UnitZ);
                    }
                    else
                    //下半分
                    {
                        GL.Rotate(180, 1, 0, 0);
                        GL.Rotate(angle, 0, -1, 0);  //-------------------------(9)
                        GL.Rotate(_rotateAngleY, 0, -1, 0);
                        GL.Rotate(_rotateAngleZ, 0, 0, 1);
                        GL.Rotate(UnitAngle, 0, 1, 0);
                        GL.Translate(0, 0, -0.5);
                        GL.Translate(0, rotatey, rotatez);
                        GL.Rotate(faceAngle, -nor_axis);
                        GL.Translate(0, -rotatey, -rotatez);
                        GL.Rotate(90 - dihedralAngle_OCTO / 2, 1, 0, 0);
                        GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                        if (mirror != 0)
                        {
                            GL.Scale(-1, 1, 1);
                        }
                        //GL.Normal3(Vector3.UnitZ);
                    }

                    /*
                    //angle =10; //-------------------------------------------(10)
                    GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                colors[faceCount]);
                    GL.Normal3(Vector3d.Cross((OCTO_UNIT_vertexes[1].VertexPosition - OCTO_UNIT_vertexes[0].VertexPosition), OCTO_UNIT_vertexes[2].VertexPosition - OCTO_UNIT_vertexes[0].VertexPosition));
                    GL.Begin(PrimitiveType.Polygon);
                    for (int faceID = 0; faceID < faceIndex_OCTO_UNIT.GetLength(1); faceID++)
                    {

                        int vertexIndex = faceIndex_OCTO_UNIT[faceCount, faceID];
                        double x = OCTO_UNIT_vertexes[vertexIndex].VertexX;
                        double y = OCTO_UNIT_vertexes[vertexIndex].VertexY;
                        double z = OCTO_UNIT_vertexes[vertexIndex].VertexZ;

                        GL.Vertex3(x, y, z);
                    }
                    GL.End();
                    GL.PopMatrix();*/
                    DrawUnitPart(MainWindow.vertexes, MainWindow.edges, i + j + flag);

                    GL.PopMatrix();

                }


            }
            GL.PopMatrix();
        }


        private void DrawUnitPart(List<Vertex> vertices, List<int[]> faces, int ind)
        {
            int[] indexes = new int[3];


            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                indexes[0] = faces[faceIndex][0];
                indexes[1] = faces[faceIndex][1];
                indexes[2] = faces[faceIndex][2];
                GL.Disable(EnableCap.Light0);
                GL.Begin(BeginMode.Lines);
                for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                {
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    Vertex vertex1 = vertices[indexes[(vertexpoint + 1) % 3]];
                    GL.Color4(0x0, 0x0, 0x255, 0x255);
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ - 0.001);
                    GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ - 0.001);
                }
                GL.End();
                GL.Enable(EnableCap.Light0);

                /*
                    if(ind==0)
                    {
                        GL.Color4((byte)0, (byte)255, (byte)255, (byte)255);
                    }
                    else
                    {
                        GL.Color4((byte)200, (byte)200, (byte)200, (byte)255);
                    }
                */
                GL.Begin(BeginMode.Triangles);
                //GL.Color4(0x0, 0xff, 0xff, 0x20);
                Vector3d p0 = vertices[indexes[0]].VertexPosition;
                Vector3d p1 = vertices[indexes[1]].VertexPosition;
                Vector3d p2 = vertices[indexes[2]].VertexPosition;
                for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                {
                    Vector3d nom = Vector3d.Cross(p2 - p1, p0 - p1);
                    GL.Normal3(-nom.Normalized());
                    Vertex vertex = vertices[indexes[vertexpoint]];
                    GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                }
                GL.End();



            }
            //GL.Enable(EnableCap.Lighting);
        }

        private void DrawPENTA_UNIT(float faceAngle, int count, int is_halfUp)
        {
            //faceAngle =　面内のユニットの回転角
            GL.PushMatrix();
            {
                GL.Scale(scale, scale, scale);
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Rotate(_rotateAngleY, 0, 1, 0);
                GL.Rotate(_rotateAngleZ, 0, 0, -1);
                if (is_halfUp == 1)
                {
                    GL.Rotate(180, 0, 0, 1);
                    GL.Rotate(36, 0, 1, 0);
                }
                GL.Translate(0, -PENTA_innerball_radius, 0);
                //立体部分の表示
                if (count != 0)
                {
                    GL.Rotate(rotate_PENTA_UNIT[count - 1], 0, 1, 0);
                    GL.Translate(0, 0, -PENTA_radius);
                    GL.Rotate(dihedralAngle_PENTA, -1, 0, 0);
                    GL.Translate(0, 0, PENTA_radius);
                }
                if (count != 0)
                {
                    DrawPENTA_FACE(faceAngle);
                }
                else
                {
                    DrawPENTA_FACE_xPararel(faceAngle);
                }
            }
            GL.PopMatrix();
        }
        /// <summary>
        /// 正五角形の表示
        /// </summary>
        private void DrawPENTA_FACE(float faceAngle)
        {
            //ここまでxz表面に平行なの正五角形の描画

            GL.Rotate(90, 1, 0, 0);
            GL.Translate(0, -PENTA_radius, 0);
            //ここまでユニットの描画
            GL.Translate(0, PENTA_radius, 0);
            GL.Rotate(faceAngle, 0, 0, 1);
            GL.Translate(0, -PENTA_radius, 0);
            //angle =10; //-------------------------------------------(10)
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse,
            System.Drawing.Color.Yellow);// 赤の直方体を描画*/  
            //GL.Normal3(Vector3.UnitX);
            GL.Normal3(Vector3d.Cross((PENTA_UNIT_vertexes[1].VertexPosition - PENTA_UNIT_vertexes[0].VertexPosition), PENTA_UNIT_vertexes[2].VertexPosition - PENTA_UNIT_vertexes[0].VertexPosition));
            GL.Begin(PrimitiveType.TriangleFan);
            for (int faceCount = 0; faceCount < faceIndex_PENTA_UNIT.GetLength(0); faceCount++)
            {

                for (int faceID = 0; faceID < faceIndex_PENTA_UNIT.GetLength(1); faceID++)
                {

                    int vertexIndex = faceIndex_PENTA_UNIT[faceCount, faceID];
                    double x = PENTA_UNIT_vertexes[vertexIndex].VertexX;
                    double y = PENTA_UNIT_vertexes[vertexIndex].VertexY;
                    double z = PENTA_UNIT_vertexes[vertexIndex].VertexZ;
                    GL.Vertex3(x, y, z);
                }
            }

            GL.End();
        }
        private void DrawPENTA_FACE_xPararel(float faceAngle)
        {
            //ここまでxz表面に平行なの正五角形の描画



            GL.Translate(0, 2 * PENTA_innerball_radius, 0);
            GL.Rotate(36, 0, 1, 0);
            GL.Rotate(90, 1, 0, 0);
            GL.Translate(0, -PENTA_radius, 0); 
            //ここまでユニットの描画
            GL.Translate(0, PENTA_radius, 0);
            GL.Rotate(faceAngle, 0, 0, 1);
            GL.Translate(0, -PENTA_radius, 0);
            //angle =10; //-------------------------------------------(10)
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse,
            System.Drawing.Color.Yellow);
            //GL.Normal3(Vector3.UnitX);
            GL.Normal3(Vector3d.Cross((PENTA_UNIT_vertexes[1].VertexPosition - PENTA_UNIT_vertexes[0].VertexPosition), PENTA_UNIT_vertexes[2].VertexPosition - PENTA_UNIT_vertexes[0].VertexPosition));
            GL.Begin(PrimitiveType.TriangleFan);
            for (int faceCount = 0; faceCount < faceIndex_PENTA_UNIT.GetLength(0); faceCount++)
            {

                for (int faceID = 0; faceID < faceIndex_PENTA_UNIT.GetLength(1); faceID++)
                {

                    int vertexIndex = faceIndex_PENTA_UNIT[faceCount, faceID];
                    double x = PENTA_UNIT_vertexes[vertexIndex].VertexX;
                    double y = PENTA_UNIT_vertexes[vertexIndex].VertexY;
                    double z = PENTA_UNIT_vertexes[vertexIndex].VertexZ;
                    GL.Vertex3(x, y, z);
                }
            }

            GL.End();
        }
        private void DrawPENTA()
        {
            GL.PushMatrix();
            {
                //正十二面体の上半分と下半分
                for (int is_hafUP = 0; is_hafUP < 2; is_hafUP++)
                {
                    //正十二面体の半分
                    for (int PENTAhalf = 0; PENTAhalf < 7; PENTAhalf++)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            DrawPENTA_UNIT(rotate_PENTA_UNIT[i], PENTAhalf, is_hafUP);
                        }
                    }
                }


            }
            GL.PopMatrix();
        }

        /// <summary>
        /// 正二十面体ベースのユニット折り紙を表示する
        /// </summary>
        private void DrawICOSA()
        {
            GL.PushMatrix();
            {
                //
                for(int is_under = 0; is_under < 2; is_under++)
                {
                    for (int flag = 0; flag < 2; flag++)
                    {
                        //正二十面体一周あたり5個のユニット
                        for (int i = 0; i < 5; i++)
                        {
                            //正八面体の１つの面を3つのユニット
                            for (int j = 0; j < 3; j++)
                            {
                                for (int mirror = 0; mirror < 2; mirror++)
                                {
                                    DrawICOSA_UNIT(rotateface_ICOSA_UNIT[j], rotateUnit_ICOSA_UNIT[i], flag, i, j, mirror, is_under);
                                }
                            }

                        }
                    }
                }
                //正八面体の上半分と下半分をflagで場合分け


            }
            GL.PopMatrix();
        }
        /// <summary>
        /// 正二十面体ベースのユニット折り紙を表示する
        /// </summary>
        /// <param name="faceAngle"></param>
        /// <param name="UnitAngle"></param>
        /// <param name="flag"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void DrawICOSA_UNIT(float faceAngle, float UnitAngle, int flag, int i, int j, int mirror, int is_under)
        {
            float anglePENTA = MathF.Acos((float)PENTA_radius / (0.5f * MathF.Sqrt(3))) * 180 / MathF.PI;

            float height = MathF.Tan(MathF.Acos((float)PENTA_radius / (0.5f * MathF.Sqrt(3)))) * (PENTA_radius);
            float move = (ICOSA_outerball_radius - height);
            Console.WriteLine(move);
            //faceAngle =　面内のユニットの回転角
            {
                for (int faceCount = 0; faceCount < faceIndex_OCTO_UNIT.GetLength(0); faceCount++)
                {
                    GL.PushMatrix();
                    GL.Scale(scale, scale, scale);
                    //

                    if(is_under == 0)
                    {
                        if (flag == 0)
                        {

                            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)

                            //GL.Rotate(UnitAngle, 0, 1, 0);
                            //GL.Translate(0, 0, -0.5);
                            ////ここから面をつくる
                            GL.Translate(0, -move, 0);
                            GL.Rotate(UnitAngle, 0, 1, 0);
                            //正五角形の内接円の半径分だけ移動(双対関係より)
                            GL.Translate(0, 0, -PENTA_radius);
                            //二面角分だけ回転する
                            GL.Rotate(dihedralAngle_ICOSA - anglePENTA - 90, -1, 0, 0);
                            //ここまでxy平面に乗る正三角形の描画(底辺がy=0)
                            GL.Translate(0, Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                            if (mirror != 0)
                            {
                                GL.Scale(-1, 1, 1);
                            }
                            //GL.Normal3(-Vector3.UnitZ);
                        }
                        else
                        //下半分
                        {

                            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                            GL.Translate(0, -move, 0);
                            GL.Rotate(UnitAngle, 0, 1, 0);
                            //正五角形の内接円の半径分だけ移動(双対関係より)
                            GL.Translate(0, 0, -PENTA_radius);
                            //二面角分だけ回転する
                            GL.Rotate(180, 0, 0, 1);
                            GL.Rotate(90 - anglePENTA, 1, 0, 0);
                            //ここまでxy平面に乗る正三角形の描画(底辺がy=0)

                            GL.Translate(0, Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                            if (mirror != 0)
                            {
                                GL.Scale(-1, 1, 1);
                            }
                            //GL.Normal3(Vector3.UnitZ);
                        }
                    }
                    else
                    {
                        if (flag == 0)
                        {

                            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)

                            //GL.Rotate(UnitAngle, 0, 1, 0);
                            //GL.Translate(0, 0, -0.5);
                            ////ここから面をつくる
                            //GL.Translate(0, rotatey, rotatez);
                            GL.Rotate(180, 1, 0, 0);
                            GL.Rotate(72, 0, 1, 0);
                            GL.Translate(0, -move, 0);
                            GL.Rotate(UnitAngle, 0, 1, 0);
                            //正五角形の内接円の半径分だけ移動(双対関係より)
                            GL.Translate(0, 0, -PENTA_radius);
                            //二面角分だけ回転する
                            GL.Rotate(dihedralAngle_ICOSA - anglePENTA - 90, -1, 0, 0);
                            //ここまでxy平面に乗る正三角形の描画(底辺がy=0)
                            GL.Translate(0, Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                            if (mirror != 0)
                            {
                                GL.Scale(-1, 1, 1);
                            }
                            //GL.Normal3(-Vector3.UnitZ);
                        }
                        else
                        //下半分
                        {

                            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                            GL.Rotate(180, 1, 0, 0);
                            GL.Rotate(72, 0, 1, 0);
                            GL.Translate(0, -move, 0);
                            GL.Rotate(UnitAngle, 0, 1, 0);
                            //正五角形の内接円の半径分だけ移動(双対関係より)
                            GL.Translate(0, 0, -PENTA_radius);
                            //二面角分だけ回転する
                            GL.Rotate(180, 0, 0, 1);
                            GL.Rotate(90 - anglePENTA, 1, 0, 0);
                            //ここまでxy平面に乗る正三角形の描画(底辺がy=0)

                            GL.Translate(0, Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Rotate(faceAngle, 0, 0, 1);
                            GL.Translate(0, -Seeker_Sys.Seeker_ShapeData.OCTO_radius, 0);
                            GL.Translate(0, 0, -Seeker_MainSystem.InnerBottomErrorZ);
                            if (mirror != 0)
                            {
                                GL.Scale(-1, 1, 1);
                            }
                            //GL.Normal3(Vector3.UnitZ);
                        }
                    }

                    /*
                    //angle =10; //-------------------------------------------(10)
                    GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                colors[faceCount]);
                    GL.Normal3(Vector3d.Cross((OCTO_UNIT_vertexes[1].VertexPosition - OCTO_UNIT_vertexes[0].VertexPosition), OCTO_UNIT_vertexes[2].VertexPosition - OCTO_UNIT_vertexes[0].VertexPosition));
                    GL.Begin(PrimitiveType.Polygon);
                    for (int faceID = 0; faceID < faceIndex_OCTO_UNIT.GetLength(1); faceID++)
                    {

                        int vertexIndex = faceIndex_OCTO_UNIT[faceCount, faceID];
                        double x = OCTO_UNIT_vertexes[vertexIndex].VertexX;
                        double y = OCTO_UNIT_vertexes[vertexIndex].VertexY;
                        double z = OCTO_UNIT_vertexes[vertexIndex].VertexZ;

                        GL.Vertex3(x, y, z);
                    }
                    GL.End();
                    GL.PopMatrix();*/
                    DrawUnitPart(MainWindow.vertexes, MainWindow.edges, i + j + flag);

                    GL.PopMatrix();

                }


            }
            GL.PopMatrix();
        }

        /// <summary>
        /// テスト用コード(描画テストに使う)
        /// </summary>
        private void drawBox()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0); 	//-------------------------(9)
                angle++; //-------------------------------------------(10)
                double w = 0.5, h = 0.2, d = 0.7;
                GL.Begin(PrimitiveType.TriangleStrip);
                { //------------(11)
                    GL.Normal3(Vector3.UnitX);
                    GL.Vertex3(w, h, -d); GL.Vertex3(w, h, d);
                    GL.Vertex3(w, -h, -d); GL.Vertex3(w, -h, d);
                }
                GL.End();
                GL.Begin(PrimitiveType.TriangleStrip);
                {
                    GL.Normal3(-Vector3.UnitX);
                    GL.Vertex3(-w, -h, -d); GL.Vertex3(-w, -h, d);
                    GL.Vertex3(-w, h, -d); GL.Vertex3(-w, h, d);
                }
                GL.End();
                GL.Begin(PrimitiveType.TriangleStrip);
                {
                    GL.Normal3(Vector3.UnitY);
                    GL.Vertex3(w, h, d); GL.Vertex3(-w, h, d);
                    GL.Vertex3(w, h, -d); GL.Vertex3(-w, h, -d);
                }
                GL.End();
                GL.Begin(PrimitiveType.TriangleStrip);
                {
                    GL.Normal3(-Vector3.UnitY);
                    GL.Vertex3(w, -h, d); GL.Vertex3(w, -h, -d);
                    GL.Vertex3(-w, -h, d); GL.Vertex3(-w, -h, -d);
                }
                GL.End();
                GL.Begin(PrimitiveType.TriangleStrip);
                {
                    GL.Normal3(Vector3.UnitZ);
                    GL.Vertex3(w, h, d); GL.Vertex3(-w, h, d);
                    GL.Vertex3(w, -h, d); GL.Vertex3(-w, -h, d);
                }
                GL.End();
                GL.Begin(PrimitiveType.TriangleStrip);
                {
                    GL.Normal3(-Vector3.UnitZ);
                    GL.Vertex3(w, h, -d); GL.Vertex3(-w, h, -d);
                    GL.Vertex3(w, -h, -d); GL.Vertex3(-w, -h, -d);
                }
                GL.End();
            }
            GL.PopMatrix();
        }
        /// <summary>
        /// テスト用(実行しない)
        /// </summary>
        private void drawOCTO()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Scale(scale, scale, scale);
                //angle =10; //-------------------------------------------(10)
                for (int faceCount = 0; faceCount < faceIndex_OCTO2.GetLength(0); faceCount++)
                {
                    if (false)
                    {

                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        System.Drawing.Color.Yellow);// 赤の直方体を描画*/
                    }
                    else
                    {

                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        colors[faceCount % (colors.Length - 1)]);// 赤の直方体を描画*/
                    }
                    GL.Begin(PrimitiveType.Polygon);
                    {

                        GL.Normal3(Vector3.UnitZ);
                        for (int faceID = 0; faceID < faceIndex_OCTO2.GetLength(1); faceID++)
                        {
                            int vertexIndex = faceIndex_OCTO2[faceCount, faceID];
                            double x = OCTO_vertexes[vertexIndex].VertexX;
                            double y = OCTO_vertexes[vertexIndex].VertexY;
                            double z = OCTO_vertexes[vertexIndex].VertexZ;
                            GL.Vertex3(x, y, z);
                        }
                    }
                    GL.End();
                }
            }
            GL.PopMatrix();
        }
        private void DrawVertexPoint()
        {
            GL.PushMatrix();
            GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
            GL.Scale(scale, scale, scale);
            for (int faceCount = 0; faceCount < faceIndex.GetLength(0); faceCount++)
            {
                GL.PointSize(15);
                //GL.Disable(EnableCap.Lighting);

                //GL.ClearColor(0.0f, 1.0f, 0.0f, 0.0f);
                GL.Begin(PrimitiveType.Points);

                GL.Scale(2, 2, 2);
                {
                    for (int faceID = 0; faceID < faceIndex.GetLength(1); faceID++)
                    {
                        int vertexIndex = faceIndex[faceCount, faceID];
                        double x = vertexes[vertexIndex].VertexX;
                        double y = vertexes[vertexIndex].VertexY;
                        double z = vertexes[vertexIndex].VertexZ;
                        GL.Color3((byte)0, (byte)255, (byte)vertexes[vertexIndex].ID);
                        GL.Vertex3(x, y, z);
                    }
                }
                GL.End();
            }
            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);
        }
        /// <summary>
        /// マウスでクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //メインウィンドウのみで操作を受け付ける
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

        /*以下操作系統のコード*/

        /// <summary>
        /// ボタンが押されたときの挙動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_saveObjFile_Click(object sender, RoutedEventArgs e)
        {
            //サブウィンドウは何もしない
        }

        private void button_AssembleUnitShape(object sender, RoutedEventArgs e)
        {

        }

        private void glControl_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                angle += 1;
            }
        }

        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (_isDraggingRightButton)
            {
                Console.WriteLine("~~~~~~~~~~~~~~~SubWindow Activatesd~~~~~~~~~~~~~~~~~~~~");
                Console.WriteLine("MouseMove Detected: isDragging = " + _isDraggingRightButton.ToString());
                float currentMouseX = e.X;
                float currentMouseY = e.Y;
                Vector3d mouseMove = new Vector3d(0, currentMouseX - _mouseX, currentMouseY - _mouseY);
                Vector3d v1 = new Vector3d(0, _mouseX, _mouseY);
                Vector3d v2 = new Vector3d(0, currentMouseY, currentMouseY);
                _rotato = OpenTKExSys.GetNormalVector(mouseMove, mouseMove);
                _rotateAngleY = arcball.GetRotateAngle(_mouseX, _mouseY, currentMouseX, currentMouseY, glControl.Size.Width / 2, glControl.Size.Height / 2) * 2 * MathF.PI;
                _rotateAngleZ = arcball.GetRotateAngle(_mouseX, _mouseY, currentMouseX, currentMouseY, glControl.Size.Width / 2, glControl.Size.Height / 2) * 2 * MathF.PI;
                _mouseX = e.X;
                _mouseY = e.Y;

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
    }
}
