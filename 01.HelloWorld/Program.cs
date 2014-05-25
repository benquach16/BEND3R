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
	class Application
    {
        double deltaAngle = 1.0d;
        float ballRadius = 50f;
        static IrrlichtDevice device;
        CameraSceneNode camera;
        int mouseX;
        int mouseY;
        bool mouseL;
        bool mouseR;
        bool potterWheelActivate = false;
        bool potterWheelDown = false;
        bool leftKeyPressed = false;
        bool rightKeyPressed = false;
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
                //mouseX = e.Mouse.X;
                //mouseY = e.Mouse.Y;
                mouseL = e.Mouse.IsLeftPressed();
                mouseR = e.Mouse.IsRightPressed();
            }
            return false;
        }

        static bool IsKeyDown(KeyCode keyCode)
        {
            return KeyIsDown.ContainsKey(keyCode) ? KeyIsDown[keyCode] : false;
        }
        static float avgDist = 0;
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
            //int radius = 2;
            //for (; radius > 0; radius--)
            //{// this should give a staircase like effect
            //v[min - radius].Position = new Vector3Df(v[min - radius].Position + v[min - radius].Normal * direction);
            if (v[min].Position.GetDistanceFromSQ(new Vector3Df(0, 0, 0)) > 100 || direction.X > 0)
            {
                v[min].Position = new Vector3Df(v[min].Position + v[min].Normal * 2 * direction);
                //}
                t.Mesh.MeshBuffers[0].UpdateVertices(v, 0);
                t.Mesh.MeshBuffers[0].SetDirty(HardwareBufferType.VertexAndIndex);
            }
            //device.SceneManager.MeshManipulator.RecalculateNormals(t.Mesh);
        }
        public void deformCyl(MeshSceneNode t, Vector3Df position, Vector3Df direction, Triangle3Df triangle)
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
            //int radius = 2;
            //for (; radius > 0; radius--)
            //{// this should give a staircase like effect
            //v[min - radius].Position = new Vector3Df(v[min - radius].Position + v[min - radius].Normal * direction);
            if ((v[min].Position.X * v[min].Position.X + v[min].Position.Z * v[min].Position.Z) > 400 || direction.X > 0)
            {
                Console.WriteLine("Min = " + min);
                v[min].Position = new Vector3Df(v[min].Position + v[min].Normal * 2 * direction);
                //}
                t.Mesh.MeshBuffers[0].UpdateVertices(v, 0);
                t.Mesh.MeshBuffers[0].SetDirty(HardwareBufferType.VertexAndIndex);
            }
            //device.SceneManager.MeshManipulator.RecalculateNormals(t.Mesh);
        }

        byte[] stringToByte(string str)
        {
            Encoding enc = Encoding.GetEncoding(1252);
            byte[] ret = enc.GetBytes(str.ToCharArray());
            return ret;
        }

        void getVectorAsStringLine(Vector3Df v, ref string s)
        {
            s = (-v.X).ToString();
            s += " ";
            s += v.Y.ToString();
            s += " ";
            s += v.Z.ToString();
            s += "\n";
        }

        void getVectorAsStringLine(Vector2Df v, ref string s)
        {
            s = v.X.ToString();
            s += " ";
            s += (-v.Y).ToString();
            s += "\n";
        }


        void getColorAsStringLine(Color color, string prefix, ref string s)
        {
            s = prefix;
            s += " ";
            s += ((double)(color.Red / 255.0f)).ToString();
            s += " ";
            s += ((double)(color.Green / 255.0f)).ToString();
            s += " ";
            s += ((double)(color.Blue / 255.0f)).ToString();
            s += "\n";
        }

        //! writes a mesh
        bool writeMesh(IrrlichtLime.IO.WriteFile file, Mesh mesh, int flags)
        {
            if (file == null)
                return false;
            Console.WriteLine("Writing Mesh");

            // write OBJ MESH header

            string name = (device.FileSystem.GetFileBasename(device.SceneManager.MeshCache.GetMeshName(mesh).ToString(), false) + ".mtl");
            file.Write(stringToByte("# exported by Irrlicht\n"));
            file.Write(stringToByte("mtllib "));
            file.Write(stringToByte(name));
            file.Write(stringToByte("\n\n"));

            // write mesh buffers

            //core::array<video::SMaterial*> mat;
            List<Material> mat = new List<Material>();
            int allVertexCount = 1; // count vertices over the whole file
            for (int i = 0; i < mesh.MeshBufferCount; ++i)
            {
                string num = new string((i + 1).ToString().ToCharArray());
                MeshBuffer buffer = mesh.GetMeshBuffer(i);
                if (buffer.VertexCount > 0)
                {
                    file.Write(stringToByte("g grp"));
                    file.Write(stringToByte(num));
                    file.Write(stringToByte("\n"));

                    int j;
                    int vertexCount = buffer.VertexCount;
                    for (j = 0; j < vertexCount; ++j)
                    {
                        file.Write(stringToByte("v "));
                        getVectorAsStringLine(buffer.GetPosition(j), ref num);
                        file.Write(stringToByte(num));
                    }

                    for (j = 0; j < vertexCount; ++j)
                    {
                        file.Write(stringToByte("vt "));
                        getVectorAsStringLine(buffer.GetTCoords(j), ref num);
                        file.Write(stringToByte(num));
                    }

                    for (j = 0; j < vertexCount; ++j)
                    {
                        file.Write(stringToByte("vn "));
                        getVectorAsStringLine(buffer.GetNormal(j), ref num);
                        file.Write(stringToByte(num));
                    }

                    file.Write(stringToByte("usemtl mat"));
                    num = "";
                    for (j = 0; j < mat.Count; ++j)
                    {
                        if (mat[j] == buffer.Material)
                        {
                            num = j.ToString();
                            break;
                        }
                    }
                    if (num == "")
                    {
                        num = mat.Count.ToString();
                        mat.Add(buffer.Material);
                    }
                    file.Write(stringToByte(num));
                    file.Write(stringToByte("\n"));

                    int indexCount = buffer.IndexCount;
                    for (j = 0; j < indexCount; j += 3)
                    {
                        ushort[] indicies = (ushort[])buffer.Indices;
                        file.Write(stringToByte("f "));
                        num = (indicies[j + 2] + allVertexCount).ToString();
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte(" "));

                        num = (indicies[j + 1] + allVertexCount).ToString();
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte(" "));

                        num = (indicies[j] + allVertexCount).ToString();
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte("/"));
                        file.Write(stringToByte(num));
                        file.Write(stringToByte(" "));

                        file.Write(stringToByte("\n"));
                    }
                    file.Write(stringToByte("\n"));
                    allVertexCount += vertexCount;
                }
            }

            if (mat.Count == 0)
            {
                return true;
            }

            file = device.FileSystem.CreateWriteFile(name);
            if (file != null)
            {
                //os::Printer::log("Writing material", file->getFileName());

                file.Write(stringToByte("# exported by Irrlicht\n\n"));
                for (int i = 0; i < mat.Count; ++i)
                {
                    string num = (i.ToString());
                    file.Write(stringToByte("newmtl mat"));
                    file.Write(stringToByte(num));
                    file.Write(stringToByte("\n"));

                    getColorAsStringLine(mat[i].AmbientColor, "Ka", ref num);
                    file.Write(stringToByte(num));
                    getColorAsStringLine(mat[i].DiffuseColor, "Kd", ref num);
                    file.Write(stringToByte(num));
                    getColorAsStringLine(mat[i].SpecularColor, "Ks", ref num);
                    file.Write(stringToByte(num));
                    getColorAsStringLine(mat[i].EmissiveColor, "Ke", ref num);
                    file.Write(stringToByte(num));
                    num = ((double)(mat[i].Shininess / 0.128f)).ToString();
                    file.Write(stringToByte("Ns "));
                    file.Write(stringToByte(num));

                    file.Write(stringToByte("\n"));
                    if (mat[i].GetTexture(0) != null)
                    {
                        file.Write(stringToByte("map_Kd "));
                        file.Write(stringToByte(mat[i].GetTexture(0).Name.Path));

                        file.Write(stringToByte("\n"));
                    }
                    file.Write(stringToByte("\n"));
                }
                file.Drop();
            }
            return true;
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
        static float mfX = 0;
        static float mfY = 0;
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
                shaderNewLightColorId = services.GetVertexShaderConstantID("mNewLightColor");
                shaderNewLightPosId = services.GetVertexShaderConstantID("mNewLightPos");
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

            Vector3Df newPos = new Vector3Df(100, 40, -40);
            services.SetVertexShaderConstant(shaderNewLightPosId, newPos.ToArray());

            // set light color

            Colorf col = new Colorf(0.5f, 0.5f, 0.7f, 1.0f);
            Colorf col2 = new Colorf(1.0f, 0.5f, 0.5f, 0.0f);
            services.SetVertexShaderConstant(shaderNewLightColorId, col2.ToArray());
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
            Application p = new Application();
        }
		public Application()
		{

            _01.HelloWorld.Kinect kinect = new _01.HelloWorld.Kinect();
            mfX = mouseX = 512; mfY = mouseY = 375; mouseL = false; mouseR = false;
            //device = IrrlichtDevice.CreateDevice(
            //    DriverType.Direct3D9, new Dimension2Di(800, 600), 16, false, true, false);
            //                                                           |
            device = IrrlichtDevice.CreateDevice(                    // \|/ Fullscreen
                DriverType.Direct3D9, new Dimension2Di(1680, 1050), 32, true, true, false);
            
			device.SetWindowCaption("BENder3D");

			VideoDriver driver = device.VideoDriver;
			SceneManager smgr = device.SceneManager;
			GUIEnvironment gui = device.GUIEnvironment;
           
            device.OnEvent += new IrrlichtDevice.EventHandler(device_OnEvent);
            smgr.AmbientLight = new Colorf(128, 128, 128, 128);
            //smgr.AddLightSceneNode(null, new Vector3Df(0, 70, 0), new Colorf(122,0,122,0), (float)10);
            MeshSceneNode box = smgr.AddCubeSceneNode(100, null, 9001, new Vector3Df(0.0f, -ballRadius*3/2, 0.0f));
            box.Scale = new Vector3Df(100.0f, 0.1f, 100.0f);
            //Mesh cyl = smgr.GeometryCreator.CreateCylinderMesh(ballRadius, 50, 256);
            //Mesh sphere = smgr.GeometryCreator.CreateSphereMesh(ballRadius, 16,16);
            //MeshSceneNode t = smgr.AddSphereSceneNode(ballRadius, 32);
            //MeshSceneNode t = smgr.AddOctreeSceneNode(sphere);
            MeshSceneNode t = smgr.AddMeshSceneNode(smgr.GetMesh("pill.obj"));
            //MeshSceneNode t = smgr.AddMeshSceneNode(cyl);
            TriangleSelector triselect = smgr.CreateTriangleSelector(t.Mesh, t);
            t.TriangleSelector = triselect;
            triselect.Drop();
            //t = smgr.AddMeshSceneNode(smgr.GetMesh("../../media/sphere.x"));
            //smgr
            t.SetMaterialTexture(0, driver.GetTexture("rockwall.jpg"));
            //t.SetMaterialFlag(MaterialFlag.Lighting, true);
            t.GetMaterial(0).SpecularColor.Set(0, 0, 0);
            //t.GetMaterial(0).Lighting = true;
            t.GetMaterial(0).NormalizeNormals = false;
            Texture citrus = driver.AddTexture(new Dimension2Di(200,200), "citrus.png");
            gui.AddImage(citrus, new Vector2Di(824, 0), true);
            gui.AddStaticText("Hey, Listen! Press C to switch the mesh to a cylinder!\n Press S to change to a sphere, and enter to send yourself the obj file!", new Recti(0,0,400,60));
            //t.AddShadowVolumeSceneNode();
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
                    "d3d9.hlsl", "vertexMain", VertexShaderType.VS_1_1,
                    "d3d9.hlsl", "pixelMain", PixelShaderType.PS_1_1,
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
            
			uint then = device.Timer.Time;
            while (device.Run())
            {
                uint now = device.Timer.Time;
                float frameDeltaTime = (float)(now - then) / 1000.0f;
                then = now;
                if (kinect.isTranslating && (kinect.translation.X < 30 && kinect.translation.X > -30))
                {
                    mfX -= (int)(kinect.translation.X);
                    mfY -= (int)(kinect.translation.Y);

                    Console.WriteLine(mouseX + ", " + mouseY + " ----------------- " + (int)(kinect.translation.X) + ", " + (int)(kinect.translation.Y));
                }

                kinect.resetTranslation();
                /*
                if (getDistance((int)mfX, (int)mfY, 512, 384) > 150)
                {
                    mfX = 512; mfY= 384;
                }*/
                
                
                mouseX = Math.Abs((int)mfX) % 1024;
                mouseY = Math.Abs((int)mfY) % 768;
                //mouseX = kinect.position.X;
                
                device.CursorControl.Position = new Vector2Di(mouseX, mouseY);
                
                if (!potterWheelDown && IsKeyDown(KeyCode.Up))
                {
                    potterWheelDown = true;
                    deltaAngle = 1.0d;
                    potterWheelActivate = !potterWheelActivate;
                }
                else if(!IsKeyDown(KeyCode.Up))
                {
                    potterWheelDown = false;
                }
                if (!leftKeyPressed && IsKeyDown(KeyCode.Left))
                {
                    leftKeyPressed = true;
                    deltaAngle /= 2;
                }
                else if (!IsKeyDown(KeyCode.Left))
                {
                    leftKeyPressed = false;
                }
                if (!rightKeyPressed && IsKeyDown(KeyCode.Right))
                {
                    rightKeyPressed = true;
                    deltaAngle *= 2;
                }
                else if (!IsKeyDown(KeyCode.Right))
                {
                    rightKeyPressed = false;
                }
                if (potterWheelActivate)
                {
                    angle -= 700.0f * deltaAngle * frameDeltaTime;
                }

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
                if(kinect.isMorphing && kinect.morphDist > 0)
                //if (IsKeyDown(KeyCode.KeyW))
                {
                    //Console.WriteLine("PRESSED KEY");
                    triangle.A *= new Vector3Df(0.5f);
                    triangle.B *= new Vector3Df(0.5f);
                    triangle.C *= new Vector3Df(0.5f);
                    deformCyl(t, triangle.A, new Vector3Df(.5f / (potterWheelActivate ? (float)(1/deltaAngle) : 60f)), triangle);

                }
                if (kinect.isMorphing && kinect.morphDist < 0)
                {
                    //Console.WriteLine("PRESSED KEY");
                    triangle.A *= new Vector3Df(1.5f);
                    triangle.B *= new Vector3Df(1.5f);
                    triangle.C *= new Vector3Df(1.5f);
                    deformCyl(t, triangle.A, new Vector3Df(-.5f / (potterWheelActivate ? (float)(1/deltaAngle) : 60f)), triangle);
                }
                if (kinect.isZoom && kinect.zoomDist < 0)
                {
                    if (distance < 300.0f)
                    {
                        distance += .0625f;
                    }
                }
                if (kinect.isZoom && kinect.zoomDist > 0)
                {
                    if (distance > 150)
                    {
                        distance -= .0625f;
                    }
                }
                if (kinect.isRotating && kinect.rotation > 0)
                {
                    angle += 200 * frameDeltaTime;
                }
                if (kinect.isRotating && kinect.rotation < 0)
                {
                    angle -= 200 * frameDeltaTime;
                }

                //Change shape

                if (IsKeyDown(KeyCode.KeyC))
                {
                    t.Remove();
                    t = smgr.AddMeshSceneNode(smgr.GetMesh("pill.obj"));
                    //MeshSceneNode t = smgr.AddMeshSceneNode(cyl);
                    triselect = smgr.CreateTriangleSelector(t.Mesh, t);
                    t.TriangleSelector = triselect;
                    triselect.Drop();
                    //t = smgr.AddMeshSceneNode(smgr.GetMesh("../../media/sphere.x"));
                    //smgr
                    t.SetMaterialTexture(0, driver.GetTexture("rockwall.jpg"));
                    //t.SetMaterialFlag(MaterialFlag.Lighting, true);
                    t.GetMaterial(0).SpecularColor.Set(0, 0, 0);
                    //t.GetMaterial(0).Lighting = true;
                    t.GetMaterial(0).NormalizeNormals = false;
                    t.SetMaterialType(newMaterialType1);
                }
                else if (IsKeyDown(KeyCode.KeyS))
                {
                    t.Remove();
                    t = smgr.AddSphereSceneNode(ballRadius, 32);
                    triselect = smgr.CreateTriangleSelector(t.Mesh, t);
                    t.TriangleSelector = triselect;
                    triselect.Drop();
                    //t = smgr.AddMeshSceneNode(smgr.GetMesh("../../media/sphere.x"));
                    //smgr
                    t.SetMaterialTexture(0, driver.GetTexture("rockwall.jpg"));
                    //t.SetMaterialFlag(MaterialFlag.Lighting, true);
                    t.GetMaterial(0).SpecularColor.Set(0, 0, 0);
                    //t.GetMaterial(0).Lighting = true;
                    t.GetMaterial(0).NormalizeNormals = false;
                    t.SetMaterialType(newMaterialType1);
                }
				driver.EndScene();
			}

			device.Drop();
		}
        public double getDistance(int x1, int x2, int y1, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
	}
}
