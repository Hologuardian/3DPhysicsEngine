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
    public enum CollisionType
    {
        Entity_Static,
        Static_Entity,
        Static_Static,
        Entity_Entity,
        None
    }

    public class Entity
    {
        public Model model;
        bool isAlive;
        World world;

        public Entity(Model m, World w)
        {
            model = m;
            isAlive = true;
            world = w;
        }

        public void update(float delta)
        {
            if (isAlive)
            {
                ((PhysicsObject)model.Tag).update(delta);
            }
        }

        public void setDead()
        {
            isAlive = false;
            world.removeEntity(this);
        }

        public void checkCollisions(float delta, List<Entity> validCollidables)
        {
            //Checks if the current entity is colliding with any entities in the provided list and responds accordingly.
            foreach (Entity ent in validCollidables)
            {
                Collision? check = checkCollision(ent.model, delta);
                if (check.HasValue)
                {
                    Collision response = check.Value;
                    if (!(response.type == CollisionType.None))
                    {
                        onCollide(ent, response, delta);
                    }
                }
            }
        }

        public Collision? checkCollision(Model m, float delta)
        {
            //The individual collision check between two entities.
            //A collision class is a nullable class that has a value if two objects are colliding, it contains relevant data to the collision
            Collision? collision = Util.checkModelCollision(this.model, m, delta);
            if (collision.HasValue)
            {
                //An enum is used so that collision is reponded to accordingly.
                Collision data = collision.Value;
                if (!((PhysicsObject)model.Tag).staticPhysics && !((PhysicsObject)m.Tag).staticPhysics)
                {
                    data.type = CollisionType.Entity_Entity;
                    return data;
                }
                else if (!((PhysicsObject)model.Tag).staticPhysics && ((PhysicsObject)m.Tag).staticPhysics)
                {
                    data.type = CollisionType.Entity_Static;
                    return data;
                }
                else if (((PhysicsObject)model.Tag).staticPhysics && !((PhysicsObject)m.Tag).staticPhysics)
                {
                    data.type = CollisionType.Static_Entity;
                    return data;
                }
                else if (((PhysicsObject)model.Tag).staticPhysics && ((PhysicsObject)m.Tag).staticPhysics)
                {
                    data.type = CollisionType.Static_Static;
                    return data;
                }
            }

            return null;
        }

        public void onCollide(Entity hit, Collision vals, float delta)
        {
            //Calls each entity's respective collision function based on the CollisionType enum.
            switch (vals.type)
            {
                case CollisionType.Entity_Entity:
                    {
                        onCollideWithEntity(hit, vals, delta, true);
                        hit.onCollideWithEntity(this, vals, delta, false);
                        break;
                    }
                case CollisionType.Entity_Static:
                    {
                        onCollideWithStatic(hit, vals, delta, true);
                        hit.onCollideWithEntity(this, vals, delta, false);
                        break;
                    }
                case CollisionType.Static_Entity:
                    {
                        onCollideWithEntity(hit, vals, delta, true);
                        hit.onCollideWithStatic(this, vals, delta, true);
                        break;
                    }
                case CollisionType.Static_Static:
                    {
                        onCollideWithStatic(hit, vals, delta, true);
                        hit.onCollideWithStatic(this, vals, delta, false);
                        return;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        //Since the PhysicsObject class updates both objects on a collision, a physics boolean is needed so that the second object isn't updated.
        //however the method is still called, making sure both entities recieve the notice that they are colliding and react accordingly
        public virtual void onCollideWithEntity(Entity entity, Collision vals, float delta, bool physics)
        {
            //Specific case checking so that collisions will work more effectively
            //Somewhat hacky
            if (!((PhysicsObject)model.Tag).staticPhysics && physics)
            {
                if ((this.GetType().Equals(typeof(Projectile))) || (this.GetType().Equals(typeof(Target))))
                {
                    if ((entity.GetType().Equals(typeof(Projectile))) || (entity.GetType().Equals(typeof(Target))))
                    {
                        Vector3 normal = Vector3.Zero;

                        normal = (((PhysicsObject)entity.model.Tag).position + entity.model.Meshes[0].BoundingSphere.Center) - (((PhysicsObject)model.Tag).position + model.Meshes[0].BoundingSphere.Center);

                        ((PhysicsObject)vals.m1.Tag).physicsCollide((PhysicsObject)vals.m2.Tag, normal, delta);
                    }
                    else
                    {
                        Plane plane = vals.p2.alignmentPlane;
                        ((PhysicsObject)vals.m1.Tag).physicsCollide((PhysicsObject)vals.m2.Tag, plane, delta);
                    }
                }
                else
                {
                    Plane plane = vals.p2.alignmentPlane;
                    ((PhysicsObject)vals.m1.Tag).physicsCollide((PhysicsObject)vals.m2.Tag, plane, delta);
                }
            }
        }

        //Colliding with a static object is simply a mirror reflection and does not have nearly as many cases.
        public virtual void onCollideWithStatic(Entity entity, Collision vals, float delta, bool physics)
        {
            if (!((PhysicsObject)model.Tag).staticPhysics && physics)
            {
                ((PhysicsObject)vals.m1.Tag).staticCollide(Plane.Normalize(vals.p2.alignmentPlane), delta);
            }
        }
    }
}