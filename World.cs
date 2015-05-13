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
    public class World
    {
        public Camera3D camera;
        List<Entity> projectiles = new List<Entity>();
        List<Entity> targets = new List<Entity>();
        List<Entity> obstacles = new List<Entity>();
        List<Entity> entityAddList = new List<Entity>();
        List<Entity> entityRemoveList = new List<Entity>();

        public World(Camera3D cam)
        {
            camera = cam;
        }

        public void update(float delta)
        {
                //Steps are used in the case that an object is moving very fast
                //Steps split the tick up to create more accurate collisions
                float steps = 0;
                foreach (Entity ent in projectiles)
                {
                    float vel = ((PhysicsObject)ent.model.Tag).velocity.Length() * delta;
                    float tempS = (vel / PhysicsObject.step);
                    steps = tempS > steps ? tempS : steps;
                }
                foreach (Entity ent in targets)
                {
                    float vel = ((PhysicsObject)ent.model.Tag).velocity.Length() * delta;
                    float tempS = (vel / PhysicsObject.step);
                    steps = tempS > steps ? tempS : steps;
                }

                float stepTime = delta / steps;
                if (steps <= 1)
                {
                    steps = 1;
                    stepTime = delta;
                }
                else if (steps >= 100)
                    steps = 100;
                
            //This is an inefficient system however, since all objects must be updated many times even if only a single object has a high velocity.
                for (float n = 0; n < steps; n++)
                {
                    //Projectiles check their collisions against all eligible entities
                    for (int i = 0; i < projectiles.Count; i++)
                    {
                        //Only a sub-set of projectiles is checked as to prevent checking the same collision twice
                        projectiles[i].checkCollisions(stepTime, projectiles.GetRange(i + 1, Math.Max(0, (projectiles.Count - 1) - (i))));
                        projectiles[i].checkCollisions(stepTime, targets);
                        projectiles[i].checkCollisions(stepTime, obstacles);
                    }
                    //targets don't collide with projectiles since they were already checked in projectiles
                    for (int i = 0; i < targets.Count; i++)
                    {
                        targets[i].checkCollisions(stepTime, targets.GetRange(i + 1, Math.Max(0, (targets.Count - 1) - (i))));
                        targets[i].checkCollisions(stepTime, obstacles);
                    }
                    //Obstacles can only collide with other obstacles.
                    for (int i = 0; i < obstacles.Count; i++)
                    {
                        obstacles[i].checkCollisions(stepTime, obstacles.GetRange(i + 1, Math.Max(0, (obstacles.Count - 1) - (i))));
                    }


                    //Fires an update towards each entity to move them after collision has been resolved this step.
                    foreach (Entity ent in projectiles)
                    {
                        ent.update(stepTime);
                    }

                    foreach (Entity ent in targets)
                    {
                        ent.update(stepTime);
                    }

                    foreach (Entity ent in obstacles)
                    {
                        ent.update(stepTime);
                    }
                }

            //To prevent concurrent write exeptions, I used add and remove lists to be safe
            //They also sort entities into their respective arrays based on type

            foreach (Entity ent in entityRemoveList)
            {
                if (ent.GetType().Equals(typeof(Projectile)))
                    projectiles.Remove(ent);
                else if (ent.GetType().Equals(typeof(Target)))
                    targets.Remove(ent);
                else if (ent.GetType().Equals(typeof(Obstacle)))
                    obstacles.Remove(ent);
            }

            foreach (Entity ent in entityAddList)
            {
                if (ent.GetType().Equals(typeof(Projectile)))
                    projectiles.Add(ent);
                else if (ent.GetType().Equals(typeof(Target)))
                    targets.Add(ent);
                else if (ent.GetType().Equals(typeof(Obstacle)))
                    obstacles.Add(ent);
            }

            entityRemoveList.Clear();
            entityAddList.Clear();
        }

        public void draw()
        {
            //Calls the draw method in each mesh object within all of the entity models.
            foreach (Entity ent in projectiles)
            {
                foreach (ModelMesh mesh in ent.model.Meshes)
                {
                    foreach (BasicEffect e in mesh.Effects)
                    {
                        //The effect is modified to be accurate to the tick's updates.
                        e.World = mesh.ParentBone.ModelTransform;
                        e.View = camera.getLookMatrix();
                    }
                    mesh.Draw();
                }
            }
            foreach (Entity ent in targets)
            {
                foreach (ModelMesh mesh in ent.model.Meshes)
                {
                    foreach (BasicEffect e in mesh.Effects)
                    {
                        e.World = mesh.ParentBone.ModelTransform;
                        e.View = camera.getLookMatrix();
                    }
                    mesh.Draw();
                }
            }
            foreach (Entity ent in obstacles)
            {
                foreach (ModelMesh mesh in ent.model.Meshes)
                {
                    foreach (BasicEffect e in mesh.Effects)
                    {
                        e.World = mesh.ParentBone.ModelTransform;
                        e.View = camera.getLookMatrix();
                    }
                    mesh.Draw();
                }
            }
        }

        public void registerEntity(Entity ent)
        {
            entityAddList.Add(ent);
        }

        public void removeEntity(Entity ent)
        {
            entityRemoveList.Add(ent);
        }
    }
}
