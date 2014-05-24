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
        float ballRadius = 50f;
        static IrrlichtDevice device;
        CameraSceneNode camera;
        int mouseX;
        int mouseY;
        bool mouseL;
        bool mouseR;
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
                mouseL = e.Mouse.IsLeftPressed();
                mouseR = e.Mouse.IsRightPressed();
            }
            return false;
        }

        static bool IsKeyDown(KeyCode keyCode)
        {
            return KeyIsDown.ContainsKey(keyCode) ? KeyIsDown[keyCode] : false;
        }

        public void deformMesh(MeshSceneNode t, Vector3Df position, Vector3Df direction, Triangle3Df triangle)
        {
            //get the closest vector to this point
            //USE BRUTE FORCE FOR RIGHT NOW

            Vertex3D[] v = (Vertex3D[])t.Mesh.MeshBuffers[0].Vertices;
            TriangleSelector tri = device.SceneManager.CreateTriangleSelector(t.Mesh, t);
            t.TriangleSelector = tri;
            tri.Drop();
            int size = t.Mesh.MeshBuffers[0].VertexCount;

            
            int min = 0;
            float minDist = v[0].Position.GetDistanceFromSQ(position);
            for (int i = 1; i < size; i++)
            {
                //nsole.WriteLine(v[i].Position.SphericalCoordinateAngles.X);
                //query verts
                 float currDist = v[i].Position.GetDistanceFromSQ(position);
                // float currDist = v[i].TCoords - position;
                 if (currDist < minDist)
                 {
                     min = i;
                     minDist = currDist;
                 }
                 
            }

            int radius = 2;
            //for (; radius > 0; radius--)
            //{// this should give a staircase like effect
                //v[min - radius].Position = new Vector3Df(v[min - radius].Position + v[min - radius].Normal * direction);
                v[min].Position = new Vector3Df(v[min].Position + v[min].Normal * 2*direction);
            //}
            t.Mesh.MeshBuffers[0].UpdateVertices(v, 0);
            t.Mesh.MeshBuffers[0].SetDirty(HardwareBufferType.VertexAndIndex);
            //device.SceneManager.MeshManipulator.RecalculateNormals(t.Mesh);
        }

        public Triangle3Df interpolateFrom2D(Vector2Di input)
        {
            //We can assume two things:
            //That the hand will be considered in front of the object
            //And that the hand will always be orbiting around the object
            //So we calculate based off of sin and cos and relative positions
            SceneCollisionManager collisionManager = device.SceneManager.SceneCollisionManager;
            Line3Df ray = device.SceneManager.SceneCollisionManager.GetRayFromScreenCoordinates(input);

            //calcLine.End = calcLine.End.Normalize();
            //calcLine.End *= new Vector3Df(20);
            // Tracks the current intersection point with the level or a mesh
            Vector3Df intersection;
            // Used to show with triangle has been hit
            Triangle3Df hitTriangle;

            SceneNode selectedSceneNode =
                device.SceneManager.SceneCollisionManager.GetSceneNodeAndCollisionPointFromRay(
                    ray,
                    out intersection, // This will be the position of the collision
                    out hitTriangle); // This ensures that only nodes that we have set up to be pickable are considered
            SceneNode highlightedSceneNode = null;
            // If the ray hit anything, move the billboard to the collision position
            // and draw the triangle that was hit.
            if (selectedSceneNode != null)
            {
                //bill.Position = new Vector3Df(intersection);

                // We need to reset the transform before doing our own rendering.
                device.VideoDriver.SetTransform(TransformationState.World, new Matrix());
                //device.VideoDriver.SetMaterial(material);
                device.VideoDriver.Draw3DTriangle(hitTriangle, new Color(255, 255, 0, 0));

                // We can check the flags for the scene node that was hit to see if it should be
                // highlighted. The animated nodes can be highlighted, but not the Quake level mesh

                highlightedSceneNode = selectedSceneNode;

                // Highlighting in this case means turning lighting OFF for this node,
                // which means that it will be drawn with full brightness.
                //highlightedSceneNode.SetMaterialFlag(MaterialFlag.Lighting, false);

            }
            return hitTriangle;
        }
        /*
        void gpu_OnSetConstants(MaterialRendererServices services, int userData)
        {
            	//set constants
            int worldId = services.GetVertexShaderConstantID("World");
            int worldViewID = services.GetVertexShaderConstantID("matWorldViewProj");
            int lightDirID = services.GetVertexShaderConstantID("LightDirection");
            int eyePos = services.GetVertexShaderConstantID("EyePosition");
	        Matrix projectionMatrix = services.VideoDriver.GetTransform(TransformationState.Projection);
	        Matrix viewMatrix = services.VideoDriver.GetTransform(TransformationState.View);
	        Matrix worldMatrix = services.VideoDriver.GetTransform(TransformationState.World);
	        Matrix projectionViewWorldMatrix = projectionMatrix*viewMatrix*worldMatrix;

	        services.SetVertexShaderConstant(worldId, worldMatrix.ToArray());
	        services.SetVertexShaderConstant(worldViewID, projectionViewWorldMatrix.ToArray());

            float[] dir = {1.0f,0.0f,0.0f,0.0f};
	        services.SetVertexShaderConstant(lightDirID, dir);

            float[] p = { device.SceneManager.ActiveCamera.Position.X, device.SceneManager.ActiveCamera.Position.Y, device.SceneManager.ActiveCamera.Position.Z, 0 };
	        
	        services.SetVertexShaderConstant(eyePos, p.ToArray());


        }*/
        static bool useHighLevelShaders = true;
        static bool useCgShaders = false;

        static bool shaderFirstUpdate = true;
        static int shaderInvWorldId;
        static int shaderWorldViewProjId;
        static int shaderLightPosId;
        static int shaderLightColorId;
        static int shaderTransWorldId;
        static int shaderTextureId;
        static int shaderNewLightPosId;
        static int shaderNewLightColorId;
        static void gpu_OnSetConstants(MaterialRendererServices services, int userData)
        {
            VideoDriver driver = services.VideoDriver;

            if (useHighLevelShaders && shaderFirstUpdate)
            {
                shaderWorldViewProjId = services.GetVertexShaderConstantID("mWorldViewProj");
                shaderTransWorldId = services.GetVertexShaderConstantID("mTransWorld");
                shaderInvWorldId = services.GetVertexShaderConstantID("mInvWorld");
                shaderLightPosId = services.GetVertexShaderConstantID("mLightPos");
                shaderLightColorId = services.GetVertexShaderConstantID("mLightColor");

                shaderFirstUpdate = false;
            }

            // set inverted world matrix
            // if we are using highlevel shaders (the user can select this when
            // starting the program), we must set the constants by name

            Matrix invWorld = driver.GetTransform(TransformationState.World);
            invWorld.MakeInverse();

            if (useHighLevelShaders)
                services.SetVertexShaderConstant(shaderInvWorldId, invWorld.ToArray());
            else
                services.SetVertexShaderConstant(0, invWorld.ToArray());

            // set clip matrix

            Matrix worldViewProj = driver.GetTransform(TransformationState.Projection);
            worldViewProj *= driver.GetTransform(TransformationState.View);
            worldViewProj *= driver.GetTransform(TransformationState.World);

            if (useHighLevelShaders)
                services.SetVertexShaderConstant(shaderWorldViewProjId, worldViewProj.ToArray());
            else
                services.SetVertexShaderConstant(4, worldViewProj.ToArray());

            // set camera position

            Vector3Df pos = device.SceneManager.ActiveCamera.AbsolutePosition;

            if (useHighLevelShaders)
                services.SetVertexShaderConstant(shaderLightPosId, pos.ToArray());
            else
                services.SetVertexShaderConstant(8, pos.ToArray());


            // set light color

            Colorf col = new Colorf(0.0f, 0.5f, 0.5f, 0.5f);

            if (useHighLevelShaders)
                services.SetVertexShaderConstant(shaderLightColorId, col.ToArray());
            else
                services.SetVertexShaderConstant(9, col.ToArray());

            // set transposed world matrix

            Matrix transpWorld = driver.GetTransform(TransformationState.World).Transposed;

            if (useHighLevelShaders)
            {
                services.SetVertexShaderConstant(shaderTransWorldId, transpWorld.ToArray());
                services.SetPixelShaderConstant(shaderTextureId, new int[] { 0 });
            }
            else
            {
                services.SetVertexShaderConstant(10, transpWorld.ToArray());
            }
        }

        static void Main(string[] args)
        {
            Program p = new Program();
        }
		public Program()
		{
            mouseX = 0; mouseY = 0; mouseL = false; mouseR = false;
            //device = IrrlichtDevice.CreateDevice(
            //    DriverType.Direct3D9, new Dimension2Di(800, 600), 16, false, true, false);
            device = IrrlichtDevice.CreateDevice(
                DriverType.Direct3D9, new Dimension2Di(800, 600), 32, false, true, false);
            
			device.SetWindowCaption("Kinect Modeller");

			VideoDriver driver = device.VideoDriver;
			SceneManager smgr = device.SceneManager;
			GUIEnvironment gui = device.GUIEnvironment;
           
            device.OnEvent += new IrrlichtDevice.EventHandler(device_OnEvent);
            smgr.AmbientLight = new Colorf(128, 128, 128, 128);
            //smgr.AddLightSceneNode(null, new Vector3Df(0, 70, 0), new Colorf(122,0,122,0), (float)10);
            MeshSceneNode box = smgr.AddCubeSceneNode(100, null, 9001, new Vector3Df(0.0f, -ballRadius, 0.0f));
            box.Scale = new Vector3Df(100.0f, 0.1f, 100.0f);
            MeshSceneNode t = smgr.AddSphereSceneNode(ballRadius,64);
            TriangleSelector triselect = smgr.CreateTriangleSelector(t.Mesh, t);
            t.TriangleSelector = triselect;
            triselect.Drop();
            //t = smgr.AddMeshSceneNode(smgr.GetMesh("../../media/sphere.x"));
            //smgr
            t.SetMaterialTexture(0, driver.GetTexture("../../media/rockwall.jpg"));
            //t.SetMaterialFlag(MaterialFlag.Lighting, true);
            t.GetMaterial(0).SpecularColor.Set(0, 0, 0);
            //t.GetMaterial(0).Lighting = true;
            t.GetMaterial(0).NormalizeNormals = false;
 //           driver.GPUProgrammingServices.OnSetConstants += new GPUProgrammingServices.SetConstantsHandler(gpu_OnSetConstants);
            /*
             MaterialType shaderMat = MaterialType.Solid;
             shaderMat = driver.GPUProgrammingServices.AddHighLevelShaderMaterialFromFiles("C:/IrrlichtLime-1.4/examples/01.HelloWorld/bumpmap.hlsl", "VertexShaderFunction", VertexShaderType.VS_3_0,
             "C:/IrrlichtLime-1.4/examples/01.HelloWorld/bumpmap.hlsl", "PixelShaderFunction", PixelShaderType.PS_3_0, MaterialType.Solid);
  
            t.SetMaterialType(shaderMat);
            t.SetMaterialTexture(1, driver.GetTexture("../../media/rockwall_height.bmp"));*/
            
			GPUProgrammingServices gpu = driver.GPUProgrammingServices;
			MaterialType newMaterialType1 = MaterialType.Solid;
			MaterialType newMaterialType2 = MaterialType.TransparentAddColor;

		    gpu.OnSetConstants += new GPUProgrammingServices.SetConstantsHandler(gpu_OnSetConstants);

				// create the shaders depending on if the user wanted high level or low level shaders

                newMaterialType1 = gpu.AddHighLevelShaderMaterialFromFiles(
                    "C:/IrrlichtLime-1.4/examples/01.HelloWorld/d3d9.hlsl", "vertexMain", VertexShaderType.VS_1_1,
                    "C:/IrrlichtLime-1.4/examples/01.HelloWorld/d3d9.hlsl", "pixelMain", PixelShaderType.PS_1_1,
                    MaterialType.Solid, 0,GPUShadingLanguage.Default);
                t.SetMaterialType(newMaterialType1);
            //t.GetMaterial(0).Wireframe = true;
            //t.DebugDataVisible = DebugSceneType.Full;
            //t.AddShadowVolumeSceneNode(null, -1, false, 1000.0f);
            smgr.AddLightSceneNode(null, new Vector3Df(40,150,-50), new Colorf(255,255,255,255), 250.0f);
            //CSampleSceneNode sceneNode = new CSampleSceneNode(smgr.RootNode, smgr, 667);
            camera = smgr.AddCameraSceneNode(null, new Vector3Df(0, 50, -140), new Vector3Df(0, 5, 0));
        

            //camera.Target = new Vector3Df(-70, 30, -60);
            //smgr.AddCameraSceneNodeFPS(null, (float)50.0);
            Material m = new Material();
            m.Lighting = false;
            double PI = 3.1415926f;
            float distance = 200.0f;
            double angle = 180.0f;
            double angleY = 20.0f;
            int oldMouseX = mouseX;
            int oldMouseY = mouseY;
			while (device.Run())
            {
                angle -= mouseX - oldMouseX;
                oldMouseX = mouseX;
                if (angle > 360)
                    angle -= 360;
                else if (angle < 0)
                    angle += 360;

                if (angleY > 360)
                    angle -= 360;
                else if (angleY < 0)
                    angleY += 360;
				driver.BeginScene(true, true, new Color(100, 101, 140));
                camera.Target = new Vector3Df(0, 0, 0);
                double temp = Math.Cos(angleY * PI / 180.0) * distance;
                double X = Math.Sin(angle * PI / 180.0) * temp;
                double Y = Math.Sin(angleY * PI / 180.0) * distance;
                double Z = Math.Cos(angle * PI / 180.0) * temp;
                camera.Position = new Vector3Df((float)X, (float)Y, (float)Z);
				smgr.DrawAll();

                gui.DrawAll();
                driver.SetMaterial(m);
                Triangle3Df triangle = interpolateFrom2D(new Vector2Di(mouseX, mouseY));
                if (IsKeyDown(KeyCode.KeyW))
                {
                    //Console.WriteLine("PRESSED KEY");
                    triangle.A *= new Vector3Df(0.5f);
                    triangle.B *= new Vector3Df(0.5f);
                    triangle.C *= new Vector3Df(0.5f);
                    deformMesh(t, triangle.A, new Vector3Df(1), triangle);

                }
                if (IsKeyDown(KeyCode.KeyS))
                {
                    //Console.WriteLine("PRESSED KEY");
                    triangle.A *= new Vector3Df(1.5f);
                    triangle.B *= new Vector3Df(1.5f);
                    triangle.C *= new Vector3Df(1.5f);
                    deformMesh(t, triangle.A, new Vector3Df(-1), triangle);
                }
				driver.EndScene();
			}

			device.Drop();
		}
	}
}
