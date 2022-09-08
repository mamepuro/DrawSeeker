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
    internal static class Seeker_MainSystem
    {
        public static void LoadObjFlie(string filename, List<Vertex> vertices)
        {
            byte iD = 0;
            //頂点配列を使用可能にする
            GL.EnableClientState(ArrayCap.VertexArray);
            //GL.VertexPointer(3, VertexPointerType.Float, 0, vertex);
            using (StreamReader streamReader = new StreamReader(filename))
            {
                while(streamReader.EndOfStream == false)
                {
                    string line = streamReader.ReadLine();
                    //頂点情報が記載されている場合
                    if(line.StartsWith("v "))
                    {
                        string[] points = line.Split(' ');
                        //頂点座標がxyzの3点分ない場合
                        if(points.Length != 4)
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
                    else if(line.StartsWith("f"))
                    {
                        Console.WriteLine(line);
                        string[] param = line.Split(' ');
                        //面が三角形で張られていない場合
                        if(param.Length != 4)
                        {
                            Console.WriteLine("ERROR! OBJファイルの面のフォーマットが三角形ではありません");
                        }
                        else
                        {
                            int[] indexes = new int[3];
                            //インデックスが1ずれるので-1をする。(0スタートと1スタートの違い)
                            indexes[0] = int.Parse((param[1].Split('/'))[0]) - 1;
                            indexes[1] = int.Parse((param[2].Split('/'))[0]) - 1;
                            indexes[2] = int.Parse((param[3].Split('/'))[0]) - 1;
                            //頂点の描画
                            GL.Begin(BeginMode.Points);
                            for(int vertexpoint = 0;vertexpoint<3;vertexpoint++)
                            {
                                Vertex vertex = vertices[indexes[vertexpoint]];
                                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                            }
                            GL.End();

                            //エッジの描画
                            GL.Begin(BeginMode.Lines);
                            for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                            {
                                Vertex vertex = vertices[indexes[vertexpoint]];
                                Vertex vertex1 = vertices[indexes[(vertexpoint + 1)%3]];
                                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                                GL.Vertex3(vertex1.VertexX, vertex1.VertexY, vertex1.VertexZ);
                            }
                            GL.End();

                            //面の描画
                            /*
                            GL.Begin(BeginMode.Triangles);
                            for (int vertexpoint = 0; vertexpoint < 3; vertexpoint++)
                            {
                                Vertex vertex = vertices[indexes[vertexpoint]];
                                GL.Vertex3(vertex.VertexX, vertex.VertexY, vertex.VertexZ);
                            }
                            GL.End();
                            */
                        }

                    }
                }
            }
        }
    }
}
