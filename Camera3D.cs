using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonogameFinal
{
    public class Camera3D
    {
        public Vector3 position;
        public Vector3 look;
        Matrix fieldOfView;
        public Vector2 cameraYawPitch;

        public Camera3D(Matrix fov)
        {
            position = new Vector3(0);
            look = new Vector3(0);
            fieldOfView = fov;
            cameraYawPitch = new Vector2(0);
        }

        public Camera3D(Matrix fov, Vector3 pos, Vector3 l)
        {
            position = pos;
            look = l;
            fieldOfView = fov;
            look.Normalize();
            cameraYawPitch = new Vector2((float)Math.Asin((l.X) / (1 + Math.Cos(Math.Asin(l.Y)))), (float)Math.Asin(l.Y));
            setLook(cameraYawPitch);
        }

        public void changeLook(Vector3 delta)
        {
            look += delta;
            look.Normalize();
        }

        public void setLook(Vector2 yawPitch)
        {
            //Creates a 3D look vector from a yaw and pitch vector
            cameraYawPitch = yawPitch;
            look = new Vector3((float)Math.Sin(yawPitch.X) + (float)Math.Cos(yawPitch.Y) * (float)Math.Sin(yawPitch.X), (float)Math.Sin(yawPitch.Y), (float)Math.Cos(yawPitch.X) + (float)Math.Cos(yawPitch.Y) * (float)Math.Cos(yawPitch.X));
            look.Normalize();
        }

        public Vector2 getHorizontalLook()
        {
            return new Vector2(look.X, look.Z);
        }

        public void move(float distance, int direction)
        {
            //Uses a switch statement to apply a distance vector onto the position vector relative to where the camera is looking.
            Vector2 dir = getHorizontalLook();
            dir.Normalize();
            dir *= distance;
            switch (direction)
            {
                case 0:
                    position.Y += distance;
                    break;
                case 5:
                    position.Y -= distance;
                    break;
                case 1:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI * 5 / 4)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 2:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 3:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI * 3 / 4)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 4:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI * 3 / 2)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 6:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI / 2)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 7:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI * 7 / 4)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 8:
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
                case 9:
                    dir = Vector2.Transform(dir, Matrix.CreateRotationZ((float)(Math.PI * 1 / 4)));
                    position.X += dir.X;
                    position.Z += dir.Y;
                    break;
            }
        }

        public Matrix getLookMatrix()
        {
            return Matrix.CreateLookAt(position, position + look * 1, Vector3.Up);
        }

        public Matrix getFOVMatrix()
        {
            return fieldOfView;
        }

        public Matrix getWorldMatrix()
        {
            return Matrix.CreateTranslation(position);
        }
    }
}
