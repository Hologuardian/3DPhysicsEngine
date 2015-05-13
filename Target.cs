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
    public class Target : Entity
    {
        public Target(Model m, World w) : base(m, w)
        {

        }

        public override void onCollideWithEntity(Entity entity, Collision vals, float delta, bool physics)
        {
            base.onCollideWithEntity(entity, vals, delta, physics);
            if (entity.GetType().Equals(typeof(Projectile)))
            {
                this.setDead();
            }
        }
    }
}
