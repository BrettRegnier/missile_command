﻿using missile_command.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace missile_command
{
    public class Bomb : GameObject
    {
        const int CURSOR_OFFSET = 5;
        static Graphics g;

        Rectangle circle;
        PType player;
        int radius = 5; // TODO REMOVE MAGIC NUMBER
        float speed = 5; // TODO REMOVE MAGIC NUMBER
        PointF velocity;

        bool atDestination = false;

        Point origin;
        Point destination;

        SolidBrush color;
        

        public Bomb(Point pos, Point des, Size dim, Graphics g, PType p) : base(pos, des, dim, g, p)
        {
            player = p;

            origin = pos;
            destination = ;

            // 10, 10 is the bomb size?
            circle = new Rectangle(200, 200, 10, 10);

            setColor();
            calculateVelocity();
        }

        public void Move()
        {
            if (!atDestination)
            {
                float remainingDiffX = Math.Abs(destination.X - circle.X);
                float remainingDiffY = Math.Abs(destination.Y - circle.Y);

                if ((circle.Y != destination.Y) || (circle.X != destination.X))
                {
                    float newX;
                    float newY;

                    if (remainingDiffX < Math.Abs(velocity.X) && remainingDiffY < Math.Abs(velocity.Y))
                    {
                        newX = destination.X;
                        newY = destination.Y;
                    }
                    else
                    {
                        newX = circle.X + velocity.X;
                        newY = circle.Y + velocity.Y;
                    }

                    circle.X = Convert.ToInt32(newX);
                    circle.Y = Convert.ToInt32(newY);
                }
                else
                {
                    popToDestination();
                }
            }
        }

        private void collide()
        {
            throw new NotImplementedException();
        }

        private void explosionCalc()
        {
            throw new NotImplementedException();
        }

        private void popToDestination()
        {

        }

        private void calculateVelocity()
        {
            // Difference between the origin and where it will hit.
            double diffX = origin.X - destination.X;
            double diffY = origin.Y - destination.Y;
            double tanAngle = 0; //Trajectory angle

            tanAngle = Math.Atan(diffX / diffY); //Gets the Tangent Angle 

            velocity.X = speed * (float)Math.Cos(tanAngle);
            velocity.Y = speed * (float)Math.Sin(tanAngle);

            if (destination.X < origin.X)
            {
                velocity.X *= -1.0F;
                velocity.Y *= -1.0F;
            }
        }

        private void setColor()
        {
            color = new SolidBrush(Config.GetInstance().getPlayerColor(player));
        }

        public override void Draw()
        {
            // TODO Use the move and collision detection
            g.FillEllipse(color, circle);
        }

        public override void Collided()
        {
        }

        public int getWidth { get { return circle.Width; } }
        public int getHeight { get { return circle.Height; } }
        public int getX { get { return circle.X; } }
        public int getY { get { return circle.Y; } }
        public Point getCenter { get { return new Point(circle.X + (circle.Width / 2), circle.Y + (circle.Height / 2)); } }
        public int getRadius { get { return radius; } }
    }
}
