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

        class CSampleSceneNode : SceneNode
        {
            AABBox bbox = new AABBox();
            public Vertex3D[] vertices;
            Material material = new Material();

            public CSampleSceneNode(SceneNode parent, SceneManager smgr, int id)
                : base(parent, smgr, id)
            {
                this.OnRegisterSceneNode += new RegisterSceneNodeEventHandler(CSampleSceneNode_OnRegisterSceneNode);
                this.OnRender += new RenderEventHandler(CSampleSceneNode_OnRender);
                this.OnGetBoundingBox += new GetBoundingBoxEventHandler(CSampleSceneNode_OnGetBoundingBox);
                this.OnGetMaterialCount += new GetMaterialCountEventHandler(CSampleSceneNode_OnGetMaterialCount);
                this.OnGetMaterial += new GetMaterialEventHandler(CSampleSceneNode_OnGetMaterial);

                material.Wireframe = false;
                material.Lighting = false;

                vertices = new Vertex3D[4];
                
                vertices[0] = new Vertex3D(0, 0, 10, 1, 1, 0, new Color(0, 255, 255), 0, 1);
                vertices[1] = new Vertex3D(10, 0, -10, 1, 0, 0, new Color(255, 0, 255), 1, 1);
                vertices[2] = new Vertex3D(0, 20, 0, 0, 1, 1, new Color(255, 255, 0), 1, 0);
                vertices[3] = new Vertex3D(-10, 0, -10, 0, 0, 1, new Color(0, 255, 0), 0, 0);

                bbox.Set(vertices[0].Position);
                for (int i = 1; i < vertices.Length; i++)
                    bbox.AddInternalPoint(vertices[i].Position);
            }

            void CSampleSceneNode_OnRegisterSceneNode()
            {
                if (Visible)
                    SceneManager.RegisterNodeForRendering(this);
            }

            void CSampleSceneNode_OnRender()
            {
                ushort[] indices = new ushort[] { 0, 2, 3, 2, 1, 3, 1, 0, 3, 2, 0, 1 };
                VideoDriver driver = SceneManager.VideoDriver;

                driver.SetMaterial(material);
                driver.SetTransform(TransformationState.World, AbsoluteTransformation);
                driver.DrawVertexPrimitiveList(vertices, indices);
            }

            AABBox CSampleSceneNode_OnGetBoundingBox()
            {
                return bbox;
            }

            int CSampleSceneNode_OnGetMaterialCount()
            {
                return 1;
            }

            Material CSampleSceneNode_OnGetMaterial(int index)
            {
                return material;
            }
        }

   
}
