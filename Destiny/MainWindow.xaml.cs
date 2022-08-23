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

namespace Destiny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer myTimer = new DispatcherTimer();
        /// <summary>
        /// 投資投影行列
        /// </summary>
        private Matrix4 _perspectiveProjection;
        private Matrix4 _modelView;
        private int[] _viewport = new int[4];
        private int SelectedPartId = 0;
        private float scale = 1.0f;
        private float PENTA_radius = (float)Math.Sqrt(25 - 10 * Math.Sqrt(5))/10;
        private float OCTO_radius = 1 / (2*(float)Math.Sqrt(3));
        const float dihedralAngle_OCTO = 109.47f;
        //private float 
        public MainWindow()
        {
            InitializeComponent();
            myTimer.Tick += new EventHandler(Timer1_Tick);
            myTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            myTimer.Start();
            SetPENTA_UNIT_pos();
            SetOCTO_UNIT_pos();
            //Debug.Print("ANS = "+((4 * PENTA_radius * PENTA_radius - 1) / (4 * PENTA_radius * PENTA_radius + 1)).ToString());
        }

        int angle = 0;
        double w = 0.5, h = 0.2, d = 0.7;
        int vertexcount = 8;
        new List<Vertex> vertexes = new List<Vertex>();
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
        };
        private void SetPENTA_UNIT_pos()
        {
            PENTA_UNIT_pos[1] = new Vector3d(0, PENTA_radius, 0);
            PENTA_UNIT_pos[3] = new Vector3d(0, -PENTA_radius, 0);
        }
        private void SetOCTO_UNIT_pos()
        {
            OCTO_UNIT_pos[1] = new Vector3d(0, OCTO_radius, 0);
            OCTO_UNIT_pos[3] = new Vector3d(0, -OCTO_radius, 0);
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
        int[,] faceIndex_PENTA_UNIT = new int[,]
        {
            {0,2,1},
            {0,3,2}
        };
        int[,] faceIndex_OCTO_UNIT = new int[,]
{
            {0,2,1},
            {0,3,2}
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

        private void glControl_Load(object sender, EventArgs e)
        { //--(4)
            GL.ClearColor(System.Drawing.Color.White); // 背景色の設定
            GL.Enable(EnableCap.DepthTest); // デプスバッファの使用
            for (int vindex = 0; vindex < vertexcount; vindex++)
            {
                Vertex vertex = new Vertex((byte)vindex, pos[vindex]);
                vertexes.Add(vertex);
            }
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
        }

        private void glControl_Resize(object sender, EventArgs e)
        { //------(5)
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);
            _viewport = new int[] { 0, 0, glControl.Size.Width, glControl.Size.Height};
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
            float[] position = new float[] { 1.0f, 2.0f, 3.0f, 0.0f };
            // ライト 0 の設定と使用
            GL.Light(LightName.Light0, LightParameter.Position, position);
            GL.Enable(EnableCap.Light0); // 光源をオンにする
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        { //----(6)
            GL.Clear(ClearBufferMask.ColorBufferBit |
              ClearBufferMask.DepthBufferBit);
            //GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,System.Drawing.Color.Green);// 赤の直方体を描画
            //GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new Color4(0,254,0,0));
            DrawReferLine();
            //DrawVertexPoint();

            //drawBox(); //------------------------------------------------------(7)
            //DrawPENTA();
            DrawOCTO_UNIT(0);
            //drawOCTO();
            glControl.SwapBuffers(); //---------------------------------------(8)
        }

        private void glControl_KeyPress(object sender, KeyboardEventArgs e)
        {

        }
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
                GL.Vertex3(2,2, 0);
                GL.Vertex3(2.5, 2, 0);
                GL.End();
                GL.Begin(PrimitiveType.Lines);
                GL.Color3((byte)0, (byte)255, (byte)0);
                GL.Vertex3(2, 2, 0);
                GL.Vertex3(2, 2.5, 0);
                GL.End();
                GL.Begin(PrimitiveType.Lines);
                GL.Color3((byte)0, (byte)0, (byte)255);
                GL.Vertex3(2, 2, 0);
                GL.Vertex3(2, 2, 0.5);
                GL.End();
                GL.Enable(EnableCap.Lighting);
            }
            GL.PopMatrix();
        }
        private void DrawOCTO_UNIT(float faceAngle)
        {
            //faceAngle =　面内のユニットの回転角
            GL.PushMatrix();
            {
                for (int faceCount = 0; faceCount < faceIndex_OCTO_UNIT.GetLength(0); faceCount++)
                {
                    for (int faceID = 0; faceID < faceIndex_OCTO_UNIT.GetLength(1); faceID++)
                    {
                        GL.Scale(scale, scale, scale);
                    GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                        GL.PushMatrix();

                        GL.Translate(0, OCTO_radius, 0);
                GL.Rotate(faceAngle, 0, 0, 1);
                GL.Translate(0, -OCTO_radius, 0);


                    //angle =10; //-------------------------------------------(10)
                    GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                System.Drawing.Color.Red);// 赤の直方体を描画*/
                GL.Normal3(-Vector3.UnitZ);
                GL.Begin(PrimitiveType.Triangles);




                        int vertexIndex = faceIndex_OCTO_UNIT[faceCount, faceID];
                        double x = OCTO_UNIT_vertexes[vertexIndex].VertexX;
                        double y = OCTO_UNIT_vertexes[vertexIndex].VertexY;
                        double z = OCTO_UNIT_vertexes[vertexIndex].VertexZ;
   
                        GL.Vertex3(x, y, z);
                        
                    }
                }

                GL.End();
                GL.PopMatrix();
            }
            GL.PopMatrix();
        }
        private void DrawOCTO()
        {
            GL.PushMatrix();
            {
                for (int i = 0; i < 3; i++)
                {
                    DrawOCTO_UNIT(rotateface_OCTO_UNIT[i]);
                }
            }
            GL.PopMatrix();
        }
        private void DrawPENTA_UNIT(float faceAngle)
        {
            //faceAngle =　面内のユニットの回転角
            GL.PushMatrix();
            {
                GL.Scale(scale, scale, scale);
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Translate(0, PENTA_radius, 0);
                GL.Rotate(faceAngle, 0, 0, 1);
                GL.Translate(0, -PENTA_radius, 0);
                
                //angle =10; //-------------------------------------------(10)
                GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                System.Drawing.Color.Yellow);// 赤の直方体を描画*/
                GL.Normal3(-Vector3.UnitZ);
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
            GL.PopMatrix();
        }
        private void DrawPENTA()
        {
            GL.PushMatrix();
            {
                for(int i = 0; i<5;i++)
                {
                    DrawPENTA_UNIT(rotate_PENTA_UNIT[i]);
                }
            }
            GL.PopMatrix();
        }
        private void drawBox()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Scale(scale, scale, scale);
                //angle =10; //-------------------------------------------(10)
                for (int faceCount = 0; faceCount < faceIndex.GetLength(0); faceCount++)
                {
                    if (false)
                    {
                        
                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        System.Drawing.Color.Yellow);// 赤の直方体を描画*/
                    }
                    else 
                    {
                        
                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        colors[faceCount]);// 赤の直方体を描画*/
                    }
                    GL.Begin(PrimitiveType.TriangleFan);
                    {
                        int vertexIndex1 = faceIndex[faceCount, 0];
                        int vertexIndex2 = faceIndex[faceCount, 1];
                        int vertexIndex3 = faceIndex[faceCount, 2];
                        GL.Normal3(OpenTKExSys.GetNormalVector(vertexes[vertexIndex1].VertexPosition, vertexes[vertexIndex2].VertexPosition, vertexes[vertexIndex3].VertexPosition));
                        for (int faceID = 0; faceID < faceIndex.GetLength(1); faceID++)
                        {
                            int vertexIndex = faceIndex[faceCount, faceID];
                            double x = vertexes[vertexIndex].VertexX;
                            double y = vertexes[vertexIndex].VertexY;
                            double z = vertexes[vertexIndex].VertexZ;
                            GL.Vertex3(x, y, z);
                        }
                    }
                    GL.End();
                }
            }
            GL.PopMatrix();
        }
        private void drawOCTO()
        {
            GL.PushMatrix();
            {
                GL.Rotate(angle, 0, 1, 0);  //-------------------------(9)
                GL.Scale(scale, scale, scale);
                //angle =10; //-------------------------------------------(10)
                for (int faceCount = 0; faceCount < faceIndex_OCTO.GetLength(0); faceCount++)
                {
                    if (false)
                    {

                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        System.Drawing.Color.Yellow);// 赤の直方体を描画*/
                    }
                    else
                    {

                        GL.Material(MaterialFace.Front, MaterialParameter.Diffuse,
                        colors[faceCount]);// 赤の直方体を描画*/
                    }
                    GL.Begin(PrimitiveType.TriangleFan);
                    {

                        GL.Normal3(Vector3.UnitZ);
                        for (int faceID = 0; faceID < faceIndex_OCTO.GetLength(1); faceID++)
                        {
                            int vertexIndex = faceIndex_OCTO[faceCount, faceID];
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
                GL.Disable(EnableCap.Lighting);

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
            //GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            float[] pixels = new float[3];
            GL.ReadPixels(e.X, glControl.Size.Height - e.Y,  1, 1, PixelFormat.Rgb, PixelType.Float, pixels);
            pixels[0] = pixels[0] * 255;
            pixels[1] = pixels[1] * 255;
            pixels[2] = pixels[2] * 255;
            Debug.Print(((int)pixels[0]).ToString() + "," + ((int)pixels[1]).ToString() + "," + ((int)pixels[2]).ToString() + "," + glControl.Size.Width + "," +  glControl.Size.Height);
            for(int index = 0;index < vertexes.Count;index++)
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
            if(e.Delta > 0)
            {
                scale += 0.1f;
            }
            else
            {
                scale -= 0.1f;
                if(scale <= 0)
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
        }

        /// <summary>
        /// ボタンが押されたときの挙動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_saveObjFile_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter streamWriter = new StreamWriter(@"C:\Test\test.obj", false, Encoding.UTF8))
            {
                streamWriter.WriteLine("# Test Code");
                for(int vertexCount =0;vertexCount <vertexes.Count;vertexCount++)
                {
                    streamWriter.WriteLine("v " + vertexes[vertexCount].VertexX + " " + vertexes[vertexCount].VertexY + " " + vertexes[vertexCount].VertexZ);
                }
                streamWriter.WriteLine("vn 0 0 -1");
                streamWriter.WriteLine("vn -1 0 0");
                streamWriter.WriteLine("vn 1 0 0");
                streamWriter.WriteLine("vn 0 -1 0");
                streamWriter.WriteLine("vn 0 1 0");
                streamWriter.WriteLine("vn 0 0 1");

                for (int faceCount = 0; faceCount < faceIndex.GetLength(0); faceCount++)
                {
                    string s = "f ";
                        for (int faceID = 0; faceID < faceIndex.GetLength(1); faceID++)
                        {
                        int vertexIndex = faceIndex[faceCount, faceID];
                        s += (vertexIndex+1).ToString() + "//1 ";
                        }
                    streamWriter.WriteLine(s);
                }
            }
        }



        private void glControl_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.A)
            {
                angle += 1;
                Debug.Print("Unko");
            }
            glControl.Refresh();
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

            if(hits <= 0)
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
        /*
        [System.Security.Permissions.UIPermission(
        System.Security.Permissions.SecurityAction.Demand,
        Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            //キーの本来の処理を
            //させたくないときは、trueを返す
            if ((keyData & Keys.KeyCode) == Keys.Left)
            {
                OnDown();
                return true;
            }
            else if ((keyData & Keys.KeyCode) == Keys.Right)
            {
                OnUp();
                return true;
            }
            else if ((keyData & Keys.KeyCode) == Keys.Up)
            {
                OnUp();
                return true;
            }
            else if ((keyData & Keys.KeyCode) == Keys.Down)
            {
                OnDown();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }*/
    }
}
