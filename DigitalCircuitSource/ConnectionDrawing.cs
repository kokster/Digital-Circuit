using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalCircuit.Library;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalCircuitSource
{
    public class ConnectionDrawing
    {
        private Connection connection;
        public Connection Connection
        {
            get
            {
                return connection;
            }
        }

        private Viewport viewport;
        private MainForm mainForm;

        public bool hasHoveredPort
        {
            get
            {
                if ((inputPortDrawing != null && inputPortDrawing.hovered) || (outputPortDrawing != null && outputPortDrawing.hovered))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool lineHovered;

        private bool hovered
        {
            get
            {
                return hasHoveredPort || lineHovered;
            }

        }

        public bool IsVisible;

        private Point boundingBoxLocation
        {
            get
            {
                int x = inputPortDrawingLocation.X < outputPortDrawingLocation.X ? inputPortDrawingLocation.X : outputPortDrawingLocation.X;
                int y = inputPortDrawingLocation.Y < outputPortDrawingLocation.Y ? inputPortDrawingLocation.Y : outputPortDrawingLocation.Y;
                return viewport.getPixelLocation(new Point(x, y));
            }
        }

        private int boundingBoxWidth
        {
            get
            {
                return viewport.getPixelLength(Math.Abs(inputPortDrawingLocation.X - outputPortDrawingLocation.X) + inputPortDrawing.PictureBox.Width);
            }
        }

        private int boundingBoxHeight
        {
            get
            {
                return viewport.getPixelLength(Math.Abs(inputPortDrawingLocation.Y - outputPortDrawingLocation.Y) + inputPortDrawing.PictureBox.Height);
            }
        }

        private Point inputPortDrawingLocation
        {
            get
            {
                return new Point(inputPortDrawing.location.X + connection.inputPort.item.Location.X, inputPortDrawing.location.Y + connection.inputPort.item.Location.Y);
            }
        }

        private PortDrawing inputPortDrawing
        {
            get
            {
                return mainForm.canvas.getPortDrawing(connection.inputPort);
            }
        }

        private Point outputPortDrawingLocation
        {
            get
            {
                return new Point(outputPortDrawing.location.X + connection.outputPort.item.Location.X, outputPortDrawing.location.Y + connection.outputPort.item.Location.Y);
            }
        }

        private PortDrawing outputPortDrawing
        {
            get
            {
                return mainForm.canvas.getPortDrawing(connection.outputPort);
            }
        }

        

        public ConnectionDrawing(Connection connection, Viewport viewport, MainForm mainForm)
        {
            this.connection = connection;
            this.viewport = viewport;
            this.mainForm = mainForm;
        }

        public Point[] getLine()
        {
                Point[] points = new Point[2];
                int padding = Convert.ToInt32(inputPortDrawing.PictureBox.Height / 2);

                if (inputPortDrawingLocation.X > outputPortDrawingLocation.X && inputPortDrawingLocation.Y > outputPortDrawingLocation.Y
                    || inputPortDrawingLocation.X < outputPortDrawingLocation.X && inputPortDrawingLocation.Y < outputPortDrawingLocation.Y)
                {
                    points[0] = new Point(padding, padding);
                    points[1] = new Point(this.BoundingBox.Width - 7, this.BoundingBox.Height - padding);
                }
                else
                {
                    points[0] = new Point(padding, this.BoundingBox.Height - padding);
                    points[1] = new Point(this.BoundingBox.Width - padding, padding);
                }

                points[0].X += BoundingBox.X;
                points[0].Y += BoundingBox.Y;
                points[1].X += BoundingBox.X;
                points[1].Y += BoundingBox.Y;

                return points;
        }

        public double distanceToPoint(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2-x1,2) + Math.Pow(y2-y1,2));
        }

        public int distanceToPoint(Point point1)
        {
            Point[] linePoints = getLine();
            Point point2 = linePoints[0];
            Point point3 = linePoints[1];
            double r1 = distanceToPoint(point1.X, point1.Y, point2.X, point2.Y);
            double r2 = distanceToPoint(point1.X, point1.Y, point3.X, point3.Y);
            double r12 = distanceToPoint(point2.X, point2.Y, point3.X, point3.Y);
            if (r1 >= distanceToPoint(r2, r12, 0, 0))
                return Convert.ToInt32(Math.Abs(r2));
            else if (r2 >= distanceToPoint(r1, r12, 0, 0))
                return Convert.ToInt32(Math.Abs(r1));
            else
            {
                double a = point3.Y - point2.Y;
                double b = point2.X - point3.X;
                double c = point2.Y * (point3.X - point2.X) - point2.X * (point3.Y - point2.Y);
                double t = distanceToPoint(a, b, 0, 0);
                if (c < 0)
                {
                    a = a * (-1);
                    b = b * (-1);
                    c = c * (-1);
                }
                double r0 = (a * point1.X + b * point1.Y + c) / t;

                return Convert.ToInt32(Math.Abs(r0));
            }
        }

        public Color getColor()
        {
            if (hovered)
            {
                return Color.Blue;
            }
            else if (this.connection.isPowered == null)
            {
                return Color.Red;
            }
            else if (Convert.ToBoolean(this.connection.isPowered))
            {
                return Color.DarkOrange;
            }
            else
            {
                return Color.Black;
            }
        }

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle(boundingBoxLocation.X, boundingBoxLocation.Y, boundingBoxWidth, boundingBoxHeight);
            }
        }

        public void refresh()
        {
            if(inputPortDrawing != null && outputPortDrawing != null)
            {
                if (hasHoveredPort || lineHovered)
                {
                    inputPortDrawing.connectionHovered = true;
                    outputPortDrawing.connectionHovered = true;
                }
                else
                {
                    inputPortDrawing.connectionHovered = false;
                    outputPortDrawing.connectionHovered = false;
                }

                inputPortDrawing.refresh();
                outputPortDrawing.refresh();
            }
        }

        public void Disconnect()
        {
            inputPortDrawing.connectionHovered = false;
            outputPortDrawing.connectionHovered = false;
        }
    }
}
