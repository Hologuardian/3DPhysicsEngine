using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace MonogameFinal
{
    public class Util
    {

        //Generates an xna ModelPart from my custom polygon list, used to clean up code.
        public static ModelMeshPart makePartFromPolygon(Polygon p, GraphicsDevice graphics)
        {
            ModelMeshPart part = new ModelMeshPart();

            IndexBuffer i = new IndexBuffer(graphics, typeof(short), 3, BufferUsage.WriteOnly);
            i.SetData(new short[] { 0, 1, 2 });
            part.IndexBuffer = i;

            VertexPositionColor[] vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(p.points[0], p.colors[0]);
            vertices[1] = new VertexPositionColor(p.points[1], p.colors[1]);
            vertices[2] = new VertexPositionColor(p.points[2], p.colors[2]);
            VertexBuffer v = new VertexBuffer(graphics, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            v.SetData<VertexPositionColor>(vertices);
            part.VertexBuffer = v;
            part.PrimitiveCount = 1;
            part.NumVertices = 3;
            part.StartIndex = 0;
            part.Tag = p;
            

            return part;
        }

        //Generates a mesh from required parameters, used to simplify and clean up code.
        public static ModelMesh makeMeshFromPolygonList(List<Polygon> polys, GraphicsDevice graphics)
        {
            List<ModelMeshPart> parts = new List<ModelMeshPart>();
            ModelBone bone = new ModelBone();
            bone.Transform = Matrix.CreateTranslation(0, 0, 0);
            bone.ModelTransform = Matrix.CreateTranslation(0, 0, 0);

            Vector3 center = new Vector3(float.MaxValue, 0, 0);
            int pointCount = 0;

            foreach (Polygon p in polys)
            {
                ModelMeshPart part = makePartFromPolygon(p, graphics);
                parts.Add(part);
                foreach (Vector3 point in p.points)
                {
                    pointCount++;
                    if (center.X == float.MaxValue)
                        center = point;
                    else
                    {
                        center += point;
                    }
                }
            }

            center /= pointCount;
            float radius = 0;
            foreach (Polygon p in polys)
            {
                foreach (Vector3 point in p.points)
                {
                    radius = Vector3.Distance(point, center) > radius ? Vector3.Distance(point, center) : radius;
                }
            }

            ModelMesh mesh = new ModelMesh(graphics, parts);
            bone.AddMesh(mesh);
            mesh.ParentBone = bone;
            mesh.BoundingSphere = new BoundingSphere(center, radius);

            return mesh;
        }

        //Generates a mesh from required parameters, as well as specifying a parent bone to be used, used to simplify and clean up code.
        public static ModelMesh makeMeshFromPolygonList(List<Polygon> polys, GraphicsDevice graphics, ModelBone parentBone)
        {
            List<ModelMeshPart> parts = new List<ModelMeshPart>();
            ModelBone bone = new ModelBone();
            bone.Transform = Matrix.CreateTranslation(0, 0, 0);
            bone.ModelTransform = parentBone.ModelTransform;

            List<Vector3> points = new List<Vector3>();
            
            foreach (Polygon p in polys)
            {
                ModelMeshPart part = makePartFromPolygon(p, graphics);
                parts.Add(part);
                foreach (Vector3 point in p.points)
                {
                    points.Add(point);
                }
            }

            ModelMesh mesh = new ModelMesh(graphics, parts);
            bone.AddMesh(mesh);
            parentBone.AddChild(bone);
            bone.Parent = parentBone;
            mesh.ParentBone = bone;
            mesh.BoundingSphere = BoundingSphere.CreateFromPoints(points);

            return mesh;
        }

        //Generates a model from required parameters, used to simplify and clean up code.
        public static Model makeModel(List<ModelMesh> meshes, GraphicsDevice graphics, Effect effect)
        {
            List<ModelBone> boneList = new List<ModelBone>();
            ModelBone boneObj = new ModelBone();
            boneObj.Transform = Matrix.CreateTranslation(0, 0, 0);
            boneObj.ModelTransform = Matrix.CreateTranslation(0, 0, 0);

            foreach (ModelMesh modelMeshObj in meshes)
            {
                boneObj.AddChild(modelMeshObj.ParentBone);
                modelMeshObj.ParentBone.Parent = boneObj;
                boneList.Add(modelMeshObj.ParentBone);
            }

            Model modelObj = new Model(graphics, boneList, meshes);
            modelObj.Root = boneObj;

            foreach (ModelMesh modelMesh in modelObj.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }

            return modelObj;
        }

        //Generates a model from required parameters, as well as specifying a root bone to be used, used to simplify and clean up code.
        public static Model makeModel(List<ModelMesh> meshes, GraphicsDevice graphics, Effect effect, ModelBone rootBone)
        {
            List<ModelBone> boneList = new List<ModelBone>();

            Model modelObj = new Model(graphics, boneList, meshes);
            modelObj.Root = rootBone;

            foreach (ModelMesh modelMesh in modelObj.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }

            return modelObj;
        }

        //Generates a list of polygons from more simple numbers.
        public static List<Polygon> makeCube(float width, Color c)
        {
            return makeRectPrism(width, width, width, c);
        }

        //Generates a list of polygons from more simple numbers.
        public static List<Polygon> makeRectPrism(float xLen, float yLen, float zLen, Color c)
        {
            List<Polygon> polyList = new List<Polygon>();
            //x+
            Polygon p = new Polygon(new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //x-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //z+
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //z-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2f, yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //y-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2f, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2f, -yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //y+
            p = new Polygon(new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2f, yLen / 2, zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2f, yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);

            return polyList;
        }

        //Generates a list of polygons from more simple numbers.
        public static List<Polygon> makeMultiColRectPrism(float xLen, float yLen, float zLen, Color c, Color c1, Color c2, Color c3, Color c4, Color c5)
        {
            List<Polygon> polyList = new List<Polygon>();
            //x+
            Polygon p = new Polygon(new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Color[] { c, c, c });
            polyList.Add(p);
            //x-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Color[] { c1, c1, c1 });
            polyList.Add(p);
            p = new Polygon(new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Color[] { c1, c1, c1 });
            polyList.Add(p);
            //z+
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Color[] { c2, c2, c2 });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Color[] { c2, c2, c2 });
            polyList.Add(p);
            //z-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Color[] { c3, c3, c3 });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2f, yLen / 2, -zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Color[] { c3, c3, c3 });
            polyList.Add(p);
            //y-
            p = new Polygon(new Vector3(-xLen / 2, -yLen / 2, -zLen / 2), new Vector3(xLen / 2f, -yLen / 2, -zLen / 2), new Vector3(-xLen / 2, -yLen / 2, zLen / 2), new Color[] { c4, c4, c4 });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, -yLen / 2, zLen / 2), new Vector3(-xLen / 2f, -yLen / 2, zLen / 2), new Vector3(xLen / 2, -yLen / 2, -zLen / 2), new Color[] { c4, c4, c4 });
            polyList.Add(p);
            //y+
            p = new Polygon(new Vector3(-xLen / 2, yLen / 2, -zLen / 2), new Vector3(-xLen / 2f, yLen / 2, zLen / 2), new Vector3(xLen / 2, yLen / 2, -zLen / 2), new Color[] { c5, c5, c5 });
            polyList.Add(p);
            p = new Polygon(new Vector3(xLen / 2, yLen / 2, zLen / 2), new Vector3(xLen / 2f, yLen / 2, -zLen / 2), new Vector3(-xLen / 2, yLen / 2, zLen / 2), new Color[] { c5, c5, c5 });
            polyList.Add(p);

            return polyList;
        }

        //Used whenever a bone is moved, this also makes sure that the child bones are updates as well, since xna does not provide it by default
        public static void moveBone(Matrix matrix, ModelBone bone)
        {
            bone.Transform = matrix;
            foreach (ModelBone b in bone.Children)
            {
                b.ModelTransform = Matrix.Multiply(matrix, b.Transform);
            }
        }


        //Checks for a collision between two models.
        public static Collision? checkModelCollision(Model m1, Model m2, float time)
        {
            foreach (ModelMesh mesh1 in m1.Meshes)
            {
                foreach (ModelMesh mesh2 in m2.Meshes)
                {
                    //Collision first checks for bounding spheres around the meshes to improve performance, so that only when a collision is possible, polygon collision is used.
                    if (mesh1.BoundingSphere.Transform(mesh1.ParentBone.ModelTransform).Intersects(mesh2.BoundingSphere.Transform(mesh2.ParentBone.ModelTransform)))
                    {
                        foreach (ModelMeshPart part1 in mesh1.MeshParts)
                        {
                            foreach (ModelMeshPart part2 in mesh2.MeshParts)
                            {
                                Vector3[] p1 = ((Polygon)(part1.Tag)).points;
                                Vector3[] p2 = ((Polygon)(part2.Tag)).points;
                                Matrix transform1 = mesh1.ParentBone.ModelTransform;
                                Matrix transform2 = mesh2.ParentBone.ModelTransform;

                                //creates polygons with the bone transformation applied to the vertices.
                                Polygon polygon1 = new Polygon(Vector3.Transform(p1[0], transform1), Vector3.Transform(p1[1], transform1), Vector3.Transform(p1[2], transform1), ((Polygon)(part1.Tag)).colors);
                                Polygon polygon2 = new Polygon(Vector3.Transform(p2[0], transform2), Vector3.Transform(p2[1], transform2), Vector3.Transform(p2[2], transform2), ((Polygon)(part2.Tag)).colors);
                                if (polygon1.collides(polygon2))
                                {
                                    //I dislike that I return on the first polygon intersection, as sometimes it is not the best set of polygons to be using at the time
                                    //However, I am not aware of how to determine the best pair of polygons and as a result, this is used in its stead.
                                    return new Collision(m1, m2, polygon1, polygon2, CollisionType.None);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
