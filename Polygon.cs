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
    public class Polygon
    {
        public Vector3[] points;
        public Plane alignmentPlane;
        public Color[] colors;

        //Polygons are an object that I use to store and calculate triangles and their collisions in 3D space, 
        //they each contain an array of points, a color, and an alignment plane
        public Polygon(Vector3 p1, Vector3 p2, Vector3 p3, Color[] c)
        {
            points = new Vector3[3];
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;
            alignmentPlane = new Plane(p1, p2, p3);
            colors = c;
        }

        /* Obsolete
        public float[] project(Vector3 proj)
        {
            float[] p = new float[3];

            p[0] = Vector3.Dot(proj, points[0]);
            p[1] = Vector3.Dot(proj, points[1]);
            p[2] = Vector3.Dot(proj, points[2]);

            return p;
        }
        */

        public float[] projectOntoRay(Ray axis, Plane opposite)
        {
            //In order to obtain the line segment intersections, I made rays from each point towards the previous point in the array.
            Ray ray1 = new Ray(points[0], Vector3.Normalize(points[1] - points[0]));
            Ray ray2 = new Ray(points[1], Vector3.Normalize(points[2] - points[1]));
            Ray ray3 = new Ray(points[2], Vector3.Normalize(points[0] - points[2]));

            //Obtaining the intersection of each ray on the plane of the opposite plane gives me the points.
            float? val1 = ray1.Intersects(opposite);
            float? val2 = ray2.Intersects(opposite);
            float? val3 = ray3.Intersects(opposite);


            //Using a min-max value system adresses more cases, because if there is ever a 3-segment intersection on a vertex, there will be no issue.
            float min = float.MaxValue;
            float max = float.MinValue;
            float projectedValue = 0;

            //Each ray is checked if they collided, and if they did, they are projected onto the collision axis.
            if (val1.HasValue && val1.Value <= Vector3.Distance(points[0], points[1]))
            {
                projectedValue = Vector3.Dot(ray1.Position + ray1.Direction * val1.Value, axis.Direction);
                min = Math.Min(min, projectedValue);
                max = Math.Max(max, projectedValue);
            }
            if (val2.HasValue && val2.Value <= Vector3.Distance(points[1], points[2]))
            {
                projectedValue = Vector3.Dot(ray2.Position + ray2.Direction * val2.Value, axis.Direction);
                min = Math.Min(min, projectedValue);
                max = Math.Max(max, projectedValue);
            }
            if (val3.HasValue && val3.Value <= Vector3.Distance(points[2], points[0]))
            {
                projectedValue = Vector3.Dot(ray3.Position + ray3.Direction * val3.Value, axis.Direction);
                min = Math.Min(min, projectedValue);
                max = Math.Max(max, projectedValue);
            }

            return new float[]{min, max};
        }

        //Collides is the method that is called, it simply returns whether a polygon collides with another in 3D space
        public bool collides(Polygon p)
        {
            Vector3 n1 = alignmentPlane.Normal;
            Vector3 n2 = p.alignmentPlane.Normal;

            //c1 is the cross product between both plane's normals
            Vector3 c1 = new Vector3(0);
            c1 = Vector3.Cross(n1, n2);

            //lDir is the direction from plane 2 towards plane 1
            Vector3 lDir = Vector3.Cross(n2, c1);
            lDir.Normalize();

            float numerator = Vector3.Dot(n1, lDir);
            Vector3 planeDelta = n1 * -alignmentPlane.D - n2 * -p.alignmentPlane.D;
            //Creates a ray originating on the second alignment plane, pointing towards the first alignment plane as well as it can
            //Is restricted to staying on the second alignment plane.
            Ray r = new Ray(n2 * -p.alignmentPlane.D, (Vector3.Dot(n2, planeDelta) / numerator) * lDir);
            float f;
            float? ff = r.Intersects(alignmentPlane);
            if (!ff.HasValue) //If the ray doesn't intersect anything the planes are not colliding and thus returns false
            {
                return false;
            }
            else
                f = ff.Value;

            Vector3 point = r.Position + lDir * f;  //Gets a point the planes intersect on

            Ray collisionAxis = new Ray(point, c1);
            //If two polygons are intersecting, then there must be two line segments of each polygon that intersect the collisionAxis
            //the point arrays represent each polygon's intersecting line segments as a value on the collision axis.
            float[] points1 = projectOntoRay(collisionAxis, p.alignmentPlane);
            float[] points2 = p.projectOntoRay(collisionAxis, alignmentPlane);


            //if the values are within each other, then there is a confirmed collision
            if (points1[0] >= points2[0] && points1[0] <= points2[1])
                return true;
            if (points1[1] >= points2[0] && points1[1] <= points2[1])
                return true;
            if (points2[0] >= points1[0] && points2[0] <= points1[1])
                return true;
            if (points2[1] >= points1[0] && points2[1] <= points1[1])
                return true;

            
            return false;
        }

        /*  OBSOLETE CODE
        public Vector3 getCollisionPoint(Polygon p)
        {
            Vector3 n1 = alignmentPlane.Normal;
            Vector3 n2 = p.alignmentPlane.Normal;
            Vector3 c1 = new Vector3(0);
            c1 = Vector3.Cross(n1, n2);

            Vector3 lDir = Vector3.Cross(n2, c1);
            lDir.Normalize();

            float numerator = Vector3.Dot(n1, lDir);
            Vector3 planeDelta = n1 * -alignmentPlane.D - n2 * -p.alignmentPlane.D;
            Ray r = new Ray(n2 * -p.alignmentPlane.D, (Vector3.Dot(n2, planeDelta) / numerator) * lDir);
            float f;
            float? ff = 0;
            ff = r.Intersects(alignmentPlane);
            if (!ff.HasValue)
            {
                r.Direction = -r.Direction;
                ff = r.Intersects(alignmentPlane);
                if (!ff.HasValue)
                    f = 0;
                else
                    f = ff.Value;
            }
            else
                f = ff.Value;

            if (float.IsNaN(f) || float.IsInfinity(f))
                return Vector3.Zero;

            Vector3 point = r.Position + lDir * f;

            Ray r1 = new Ray(point, c1);
            float[] points1 = intersects(r1, p.alignmentPlane);
            float[] points2 = p.intersects(r1, alignmentPlane);

            return r1.Position + r1.Direction * ((points1[0] + points1[1] + points2[0] + points2[1]) / 4);
        }
        */
    }
}
