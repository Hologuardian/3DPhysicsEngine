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
    public class PhysicsObject
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 momentum;
        public float mass;
        public float coeffR;
        public bool gravity = true;
        public bool staticPhysics = false;
        public static float step = 0.5f;
        public static int count = 0;
        public bool friction = true;

        Model model;
        World world;

        public PhysicsObject(Model m, float objectMass, World world)
        {
            model = m;
            this.world = world;
            mass = objectMass;

            position = new Vector3(0);
            position = Vector3.Transform(position, model.Root.ModelTransform);
            velocity = new Vector3(0);
            momentum = new Vector3(0);
            //Due to the coefficient of restitution being based off of a lot of different factors, I effectively made it static, but kept it dynamic for possible changes in the future
            coeffR = 0.9f;
        }

        public void addImpulse(Vector3 force, float time)
        {
            momentum += force * time;
        }

        public void update(float time)
        {
            if (!staticPhysics)
            {
                if (gravity)
                    addImpulse(Vector3.Down * 0.1f * mass, time);
                //if (friction)
                //    addImpulse(-momentum * 0.01f, time);
                velocity = momentum / mass;

                //Somewhat irrelevant, however, this makes it so that a very slowly moving object is stopped to promote efficiency
                //This can be combined with flagging entities for no updates to greatly increase performance
                if (velocity.Length() <= 0.01f)
                    velocity = Vector3.Zero;
                position += velocity * time;
            }
            Util.moveBone(Matrix.CreateTranslation(position), model.Root);
        }

        public void physicsCollide(PhysicsObject otherObj, Plane collisionPlane, float time)
        {
            //Moves the objects back to their position at the start of the tick
            otherObj.position -= otherObj.velocity * time;
            position -= velocity * time;

            //Takes the normal from the collision plane
            Vector3 n = collisionPlane.Normal;
            n.Normalize();

            //If the normal is pointing the wrong direction, this corrects it.
            if (Vector3.Dot(position, n) <= 0)
                n = -n;

            Vector3 velocityDifference = otherObj.velocity - velocity;

            float inverseMass = (float)(Math.Pow(mass, -1));
            if (staticPhysics) //If an object is static, it's inverse mass is treated as 0
                inverseMass = 0.0f;

            float otherInverseMass = (float)(Math.Pow(otherObj.mass, -1));
            if (otherObj.staticPhysics) //If an object is static, it's inverse mass is treated as 0
                otherInverseMass = 0.0f;

            //The magnitude of the impulse that is applied is based off of the difference in the velocities of each object and their masses.
            //Both objects recieve the same amount of force, however that is modified by their mass.
            float impulseMagnitude = Vector3.Dot(((-(1 + coeffR)) * velocityDifference), n) /(inverseMass + otherInverseMass);
            addImpulse(-n * impulseMagnitude, 1.0f);
            otherObj.addImpulse(n * impulseMagnitude, 1.0f);
            //count++; //Debug messages
            //System.Diagnostics.Debug.WriteLine("Boop" + count);
        }

        //Only ever called by projectiles to each other, uses sphere-sphere collision response due to how similar in size they are.
        public void physicsCollide(PhysicsObject otherObj, Vector3 normal, float time)
        {
            //Moves the objects back to their position at the start of the tick
            otherObj.position -= otherObj.velocity * time;
            position -= velocity * time;

            //in this case, this method is called if a normal vector can be easily supplied.
            //Since this is the case only between projectiles and targets, both of which are of similar size, a normal between their centers is used.
            Vector3 n = normal;
            n.Normalize();
            //If the normal is pointing the wrong direction, this corrects it.
            if (Vector3.Dot(position, n) <= 0)
                n = -n;
            Vector3 velocityDifference = otherObj.velocity - velocity;

            float inverseMass = (float)(Math.Pow(mass, -1));
            float otherInverseMass = (float)(Math.Pow(otherObj.mass, -1));

            //The magnitude of the impulse that is applied is based off of the difference in the velocities of each object and their masses.
            //Both objects recieve the same amount of force, however that is modified by their mass.
            float impulseMagnitude = Vector3.Dot(((-(1 + coeffR)) * velocityDifference), n) / (inverseMass + otherInverseMass);
            addImpulse(-n * impulseMagnitude, 1.0f);
            otherObj.addImpulse(n * impulseMagnitude, 1.0f);
            //count++; //Debug messages
            //System.Diagnostics.Debug.WriteLine("Boop" + count);
        }

        public void staticCollide(Plane collisionPlane, float time)
        {
            //If an object collides with a static object, the object is perfectly reflected on the plane of the object it hit.

            position -= velocity * time;
            Vector3 n = Vector3.Normalize(collisionPlane.Normal);
            if (Vector3.Dot(position, n) <= 0)
                n = -n;
            Vector3 vr = -velocity;
            float inverseMass = (float)(Math.Pow(mass, -1));
            float otherInverseMass = 0.0f;
            float jr = Vector3.Dot(((-(1 + coeffR)) * vr), n) / (inverseMass + otherInverseMass);
            addImpulse(-n * jr, 1.0f);
            //count++; //For debug purposes
        }
    }
}
