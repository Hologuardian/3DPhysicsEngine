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
    public struct Collision
    {
        public Polygon p1;
        public Polygon p2;
        public Model m1;
        public Model m2;
        public CollisionType type;

        public Collision(Model model1, Model model2, Polygon poly1, Polygon poly2, CollisionType ct)
        {
            p1 = poly1;
            p2 = poly2;
            m1 = model1;
            m2 = model2;
            type = ct;
        }
    }
}
