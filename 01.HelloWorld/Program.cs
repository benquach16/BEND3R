using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Video;
using IrrlichtLime.Scene;
using IrrlichtLime.GUI;

namespace Program
{
	class Program
	{
        IrrlichtDevice device;
        int mouseX;
        int mouseY;
        static Dictionary<KeyCode, bool> KeyIsDown = new Dictionary<KeyCode, bool>();

        public bool device_OnEvent(Event e)
        {
            if (e.Type == EventType.Key)
            {
                if (KeyIsDown.ContainsKey(e.Key.Key))
                    KeyIsDown[e.Key.Key] = e.Key.PressedDown;
                else
                    KeyIsDown.Add(e.Key.Key, e.Key.PressedDown);
            }
            if (e.Type == EventType.Mouse)
            {
                //handle mouse events
                mouseX = e.Mouse.X;
                mouseY = e.Mouse.Y;
            }
            return false;
        }

        static bool IsKeyDown(KeyCode keyCode)
        {
            return KeyIsDown.ContainsKey(keyCode) ? KeyIsDown[keyCode] : false;
        }

        public void deformMesh(MeshSceneNode t, Vector3Df position, Vector3Df direction)
        {
            //get the closest vector to this point
            //USE BRUTE FORCE FOR RIGHT NOW

            Vertex3D[] v = (Vertex3D[])t.Mesh.MeshBuffers[0].Vertices;
            int size = t.Mesh.MeshBuffers[0].VertexCount;
            int min = 0;
            float minDist = v[0].Position.GetDistanceFromSQ(position);
            for (int i = 1; i < size; i++)
            {
                //query verts
                float currDist = v[i].Position.GetDistanceFromSQ(position);
                if (currDist < minDist)
                {
                    min = i;
                    minDist = currDist;
                }
                
            }
            int radius = 20;
            v[min].Position = new Vector3Df(v[min].Position + v[min].Normal * new Vector3Df(5));
            for (int i = 0; i < radius; i++)
            {
                v[min + i % size].Position = new Vector3Df(v[min + i % size].Position + v[min + i % size].Normal * new Vector3Df(5));
            }
            t.Mesh.MeshBuffers[0].UpdateVertices(v, 0);            
        }

        public Vector3Df interpolateFrom2D(Vector2Di input)
        {
            //We can assume two things:
            //That the hand will be considered in front of the object
            //And that the hand will always be orbiting around the object
            //So we calculate based off of sin and cos and relative positions
            Line3Df calcLine = device.SceneManager.SceneCollisionManager.GetRayFromScreenCoordinates(input);
            double dist;
            calcLine.GetIntersectionWithSphere(new Vector3Df(0, 0, 0), 50.0f, out dist);

            calcLine.End = calcLine.End.Normalize();
            calcLine.End *= new Vector3Df(20);
            return calcLine.Start;
        }
        static void Main(string[] args)
        {
            Program p = new Program();
        }
		public Program()
		{
            mouseX = 0; mouseY = 0;
			device = IrrlichtDevice.CreateDevice(
				DriverType.Direct3D9, new Dimension2Di(800, 600), 32, false, true, false);

			device.SetWindowCaption("Kinect Modeller");

			VideoDriver driver = device.VideoDriver;
			SceneManager smgr = device.SceneManager;
			GUIEnvironment gui = device.GUIEnvironment;

            device.OnEvent += new IrrlichtDevice.EventHandler(device_OnEvent);
            
            smgr.AmbientLight = new Colorf(128, 128, 128, 128);
            //smgr.AddLightSceneNode(null, new Vector3Df(0, 70, 0), new Colorf(122,0,122,0), (float)10);
            MeshSceneNode box = smgr.AddCubeSceneNode(100,null,-1, new Vector3Df(0.0f,-50.0f,0.0f));
            box.Scale = new Vector3Df(100.0f, 0.1f, 100.0f);
            MeshSceneNode t = smgr.AddSphereSceneNode((float)50,32);
            //t = smgr.AddMeshSceneNode(smgr.GetMesh("../../media/sphere.x"));
            t.SetMaterialTexture(0, driver.GetTexture("../../media/rockwall.jpg"));
            //t.SetMaterialFlag(MaterialFlag.Lighting, true);
            t.GetMaterial(0).SpecularColor.Set(0, 0, 0);
            //t.GetMaterial(0).Lighting = true;
            t.GetMaterial(0).NormalizeNormals = true;

            //t.GetMaterial(0).Wireframe = true;
            //t.DebugDataVisible = DebugSceneType.Full;
            t.AddShadowVolumeSceneNode(null, -1, false, 1000.0f);
            smgr.AddLightSceneNode(null, new Vector3Df(40,150,-50), new Colorf(255,255,255,255), 250.0f);
            //CSampleSceneNode sceneNode = new CSampleSceneNode(smgr.RootNode, smgr, 667);
			smgr.AddCameraSceneNode(null, new Vector3Df(0, 50, -140), new Vector3Df(0, 5, 0));
            smgr.AddCameraSceneNodeFPS(null, (float)50.0);

			while (device.Run())
			{
				driver.BeginScene(true, true, new Color(100, 101, 140));
                Vector3Df pos = interpolateFrom2D(new Vector2Di(mouseX, mouseY));
                if(IsKeyDown(KeyCode.KeyW))
                {
                    //Console.WriteLine("PRESSED KEY");
                    deformMesh(t, pos, new Vector3Df());

                }

				smgr.DrawAll();
				gui.DrawAll();

				driver.EndScene();
			}

			device.Drop();
		}
	}
}
