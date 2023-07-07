using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace LandOfColor_2._0
{
    public partial class Form1 : Form
    {
        private BufferedGraphicsContext _buffer;
        private BufferedGraphics buffer;

        private bool Wresize;
        private Bitmap bitmap;

        private int size = 500;
        public int d = 200;
        Box[] boxes = new Box[10];
        //List<Box> boxes = new List<Box>();

        Button seed = new Button()
        {
            Text = "Seed",
            Size = new Size(20, 20),
            
            //TabIndex = 10
        };

        public Form1()
        {
            InitializeComponent();

            _buffer = BufferedGraphicsManager.Current;
            buffer = _buffer.Allocate(this.CreateGraphics(), this.ClientRectangle);
            bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Wresize = true;
            this.MouseWheel += new MouseEventHandler(Form1_MouseMove);
            this.seed.Click += (sender, e) => {
                generateBoxes();
                buffer.Dispose();
                buffer = _buffer.Allocate(this.CreateGraphics(), this.ClientRectangle);
                drawBoxes();
                buffer.Render(this.CreateGraphics());
            };
            this.Controls.Add(seed);
        }

        Point[] gridPoints;
        int gridSize = 20;
        int gridIndex = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.Location = new Point(0, 0);

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            gridPoints = new Point[(width * height)];

            int indexX = 0;
            int indexY = 0;

            for (int x = 0; x < width; x += gridSize)
            {
                for (int y = 0; y < height; y += gridSize)
                {
                    gridPoints[indexX++].X = x;
                    gridPoints[indexY++].Y = y;
                }
            }
            
            do
            {
                gridIndex++;
            } while (gridPoints[gridIndex].X != 0 || gridPoints[gridIndex].Y != 0);
            buffer.Graphics.DrawImage(drawGrid(), 0, 0);

            //---------------------BOX--------------------------------------------------------------//
            generateBoxes();
        }
        private bool doOverlap(Rectangle rect1, Rectangle rect2)
        {
            int rect1MaxX = rect1.X + rect1.Width;
            int rect1MaxY = rect1.Y + rect1.Height;
            int rect2MaxX = rect2.X + rect2.Width;
            int rect2MaxY = rect2.Y + rect2.Height;

            return (rect1.X < rect2MaxX && rect1MaxX > rect2.X &&
                    rect1.Y < rect2MaxY && rect1MaxY > rect2.Y);
        }
        private void generateBoxes()
        {
            Random random = new Random();
            int i = new int();
            int boxSize = 5;
            int index = 0;
            while (index != 10)
            {
                i = random.Next(gridIndex);
                if (gridPoints[i].X + boxSize * gridSize != this.ClientSize.Width && gridPoints[i].Y + boxSize * gridSize != this.ClientSize.Height)
                {
                    bool breaked = false;
                    for (int x = 0; x < boxes.Count(); x++)
                    {
                        if (boxes[x] == null)
                        {
                            boxes[index] = new Box(gridPoints[i], new Size(boxSize * gridSize, boxSize * gridSize), Color.FromArgb(255, random.Next(255), random.Next(255), random.Next(255)), i);
                            index++;
                            break;
                        }
                        if (!doOverlap(new Rectangle(boxes[x].Point, new Size(boxSize * gridSize, boxSize * gridSize)), new Rectangle(gridPoints[i], new Size(boxSize * gridSize, boxSize * gridSize))))
                        {
                            breaked = true;
                            break;
                        }
                    }
                    if (breaked)
                    {
                        //boxes.Add(new Box(gridPoints[i], new Size(boxSize * gridSize, boxSize * gridSize), Color.FromArgb(255, random.Next(255), random.Next(255), random.Next(255)), i));
                        boxes[index] = new Box(gridPoints[i], new Size(boxSize * gridSize, boxSize * gridSize), Color.FromArgb(255, random.Next(255), random.Next(255), random.Next(255)), i);
                        index++;
                    }
                }
            }
        }
        private void drawBoxes()
        {
            for (int i = 0; i < boxes.Count(); i++)
            {
                buffer.Graphics.FillRectangle(new SolidBrush(boxes[i].Color), new Rectangle(boxes[i].Point.X, boxes[i].Point.Y, boxes[i].Size.Width, boxes[i].Size.Height));
                for (int j = 0; j < Lights.Count(); j++)
                {
                    buffer.Graphics.DrawLines(new Pen(Color.FromArgb(255, Color.White), 2), boxes[i].highlight(Lights[j].Point, boxes[i].Index, size));
                    buffer.Graphics.FillPolygon(new SolidBrush(Color.FromArgb((int)(200), 13, 13, 13)), boxes[i].ShadowBound(boxes[i].Index, Lights[j].Point));
                }
            }
        }
        private Bitmap drawGrid()
        {
            if (Wresize)
            {
                Graphics g = Graphics.FromImage(bitmap);

                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 13, 13, 13)), new Rectangle(0, 0, this.Width, this.Height)); // BLACK
                //g.FillRectangle(new SolidBrush(Color.FromArgb(255, 242, 242, 242)), new Rectangle(0, 0, this.Width, this.Height)); // WHITE

                Wresize = false;

                return bitmap;
            }

            return bitmap;
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            buffer = _buffer.Allocate(this.CreateGraphics(), this.ClientRectangle);

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            gridPoints = new Point[(width * height)];

            int index = 0;
            for (int x = 0; x < width; x += gridSize)
            {
                gridPoints[index++].X = x;
            }

            index = 0;
            for (int y = 0; y < height; y += gridSize)
            {
                gridPoints[index++].Y = y;
            }

            Wresize = true;
            buffer.Graphics.DrawImage(drawGrid(), 0, 0);
        }

        List<Light> Lights = new List<Light>();
        Point mouse = new Point();
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                size += 10;
            }
            else if (e.Delta < 0)
            {
                if (size > 15)
                    size -= 10;
            }

            int index = 28;
            buffer = _buffer.Allocate(this.CreateGraphics(), this.ClientRectangle);
            buffer.Graphics.DrawImage(drawGrid(), 0, 0);
            mouse = new Point(e.Location.X, e.Location.Y);

            //---------------------LIGHT-------------------------------------------//
            for (int i = 0; i < Lights.Count; i++)
            {
                Lights.Last().Point = mouse;
                Lights.Last().Size = new Size(size, size);
                Lights.Last().create();
                buffer.Graphics.FillEllipse(Lights[i].ColorBrush, Lights[i].bounds);
            }
            //---------------------LIGHT-------------------------------------------//
            drawBoxes();

            buffer.Render(this.CreateGraphics());
            
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            Random random = new Random();
            Lights.Add(new Light(mouse, new Size(size, size), Color.FromArgb(255, random.Next(255), random.Next(255), random.Next(255))));
            Lights.Last().create();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            generateBoxes();
        }
    }

    public class Light
    {
        public Light() { }
        public Light(int x, int y, int width, int height, Color color) 
        {
            this.Point = new Point(x, y);
            this.Size = new Size(width, height);
            this.Color = color;
        }
        public Light(Point point, int width, int height, Color color)
        {
            this.Point = point;
            this.Size = new Size(width, height);
            this.Color = color;
        }
        public Light(int x, int y, Size size, Color color)
        {
            this.Point = new Point(x, y);
            this.Size = size;
            this.Color = color;
        }
        public Light(Point point, Size size, Color color)
        {
            this.Point = point;
            this.Size = size;
            this.Color = color;
        }
        public Color Color { get; set; }
        public Point Point { get; set; }
        public Size Size { get; set; }
        public PathGradientBrush ColorBrush { get; set; }
        public Rectangle bounds { get; set; }
        public void create()
        {
            Rectangle bounds = new Rectangle(this.Point.X - this.Size.Width / 2, this.Point.Y - this.Size.Height / 2, this.Size.Width, this.Size.Height);
            this.bounds = bounds;
            using (var ellipsePath = new GraphicsPath())
            {
                ellipsePath.AddEllipse(bounds);
                ellipsePath.AddEllipse(bounds);
                this.ColorBrush = new PathGradientBrush(ellipsePath);
                this.ColorBrush.CenterPoint = new PointF(this.Point.X, this.Point.Y);
                this.ColorBrush.CenterColor = this.Color;
                this.ColorBrush.SurroundColors = new[] { Color.Transparent };
                this.ColorBrush.FocusScales = new PointF(0, 0);
            }
        }
    }

    public class Box
    {
        public Box() { }
        public Box(Point point, Size size, Color color, int index)
        {
            Point = point;
            Size = size;
            Color = color;
            Distance = 200;
            Index = index;
        }
        public Box(int x, int y, Size size, Color color, int index)
        {
            Point = new Point(x, y);
            Size = size;
            Color = color;
            Distance = 200;
            Index = index;
        }
        public Box(Point point, int width, int height, Color color, int index)
        {
            Point = point;
            Size = new Size(width, height);
            Color = color;
            Distance = 200;
            Index = index;
        }
        public Box(int x, int y, int width, int height, Color color, int index)
        {
            Point = new Point(x, y);
            Size = new Size(width, height);
            Color = color;
            Distance = 200;
            Index = index;
        }
        public Point Point { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
        public Point nPoint { get; set; }
        public int Distance { get; set; }
        public int Index { get; set; }
        public Point[] ShadowBound(int index, Point mouse)
        {
            Point c0 = closestPoint(mouse, index, 0);
            Point c1 = closestPoint(mouse, index, 1);
            Point c2 = closestPoint(mouse, index, 2);
            Point c3 = closestPoint(mouse, index, 3);
            Point[] shadow = new Point[1];

            if (mouse.Y <= closestPoint(mouse, index, 1).Y && mouse.Y >= closestPoint(mouse, index, 0).Y ||
                mouse.Y >= closestPoint(mouse, index, 1).Y && mouse.Y <= closestPoint(mouse, index, 0).Y ||
                mouse.X <= closestPoint(mouse, index, 1).X && mouse.X >= closestPoint(mouse, index, 0).X ||
                mouse.X >= closestPoint(mouse, index, 1).X && mouse.X <= closestPoint(mouse, index, 0).X)
            {
                shadow = new Point[]
                {
                    c0, c1,
                    new Point(c1.X + (c1.X - mouse.X) * d, c1.Y + (c1.Y - mouse.Y) * d),
                    new Point(c2.X + (c2.X - mouse.X) * d, c2.Y + (c2.Y - mouse.Y) * d),
                    new Point(c0.X + (c0.X - mouse.X) * d, c0.Y + (c0.Y - mouse.Y) * d),
                    c0,
                };
            }
            else
            {
                shadow = new Point[]
                {
                    c0, c1,
                    new Point(c1.X + (c1.X - mouse.X) * d, c1.Y + (c1.Y - mouse.Y) * d),
                    new Point(c2.X + (c2.X - mouse.X) * d, c2.Y + (c2.Y - mouse.Y) * d),
                    c2, c0,
                };
            }
            //buffer.Graphics.FillPolygon(new SolidBrush(Color.FromArgb(255, 13, 13, 13)), shadow);

            return shadow;
        }
        public Point[] highlight(Point mouse, int index, int size)
        {
            int s = 10;
            Point[] mid = new Point[]
           {
                new Point((closestPoint(mouse, index, 0).X + closestPoint(mouse, index, 1).X)/2 - s/2,
                          (closestPoint(mouse, index, 0).Y + closestPoint(mouse, index, 1).Y)/2 - s/2),
                new Point((closestPoint(mouse, index, 0).X + closestPoint(mouse, index, 2).X)/2 - s/2,
                          (closestPoint(mouse, index, 0).Y + closestPoint(mouse, index, 2).Y)/2 - s/2),
                new Point((closestPoint(mouse, index, 2).X + closestPoint(mouse, index, 3).X)/2 - s/2,
                          (closestPoint(mouse, index, 2).Y + closestPoint(mouse, index, 3).Y)/2 - s/2),
                new Point((closestPoint(mouse, index, 1).X + closestPoint(mouse, index, 3).X)/2 - s/2,
                          (closestPoint(mouse, index, 1).Y + closestPoint(mouse, index, 3).Y)/2 - s/2),
           };

            Point c0 = closestPoint(mouse, index, 0);
            Point c1 = closestPoint(mouse, index, 1);

            Point[] lines = new Point[0];

            if (mouse.Y <= closestPoint(mouse, index, 1).Y && mouse.Y >= closestPoint(mouse, index, 0).Y ||
                mouse.Y >= closestPoint(mouse, index, 1).Y && mouse.Y <= closestPoint(mouse, index, 0).Y ||
                mouse.X <= closestPoint(mouse, index, 1).X && mouse.X >= closestPoint(mouse, index, 0).X ||
                mouse.X >= closestPoint(mouse, index, 1).X && mouse.X <= closestPoint(mouse, index, 0).X)
            {
                lines = new Point[2];
                if (Math.Sqrt(Math.Pow(mouse.X - c0.X, 2) + Math.Pow(mouse.Y - c0.Y, 2)) < size / 2)
                {
                    lines[0] = closestPoint(mouse, index, 0);
                    lines[1] = closestPoint(mouse, index, 1);
                };
            }
            else
            {
                lines = new Point[4];
                if (Math.Sqrt(Math.Pow(mouse.X - c0.X, 2) + Math.Pow(mouse.Y - c0.Y, 2)) < size / 2)
                {
                    lines[0] = closestPoint(mouse, index, 0);
                    lines[1] = closestPoint(mouse, index, 1);
                    lines[2] = closestPoint(mouse, index, 1);
                    lines[3] = closestPoint(mouse, index, 1);
                };
                if (Math.Sqrt(Math.Pow(mouse.X - c1.X, 2) + Math.Pow(mouse.Y - c1.Y, 2)) < size / 2)
                {
                    lines[2] = closestPoint(mouse, index, 0);
                    lines[3] = closestPoint(mouse, index, 2);
                };
            }

            try
            {
                return lines;
                //buffer.Graphics.DrawLines(new Pen(Color.FromArgb(100, Color.White), 2), lines);
            }
            catch 
            {
                return null;
            }
        }

        private int d = 200;
        private Point closestPoint(Point mouse, int index, int i/*, Light light*/)
        {
            switch (closestIndex(mouse, index, i))
            {
                case 0: return new Point(this.Point.X, this.Point.Y);
                case 1: return new Point(this.Point.X + this.Size.Width, this.Point.Y);
                case 2: return new Point(this.Point.X, this.Point.Y + this.Size.Height);
                case 3: return new Point(this.Point.X + this.Size.Width, this.Point.Y + this.Size.Height);
            }

            return new Point(0, 0);
        }
        private int closestIndex(Point mouse, int index, int i/*, Light light*/)
        {
            double[] c = new double[4];
            c[0] = (Math.Sqrt(Math.Pow(this.Point.X - mouse.X, 2) + Math.Pow(this.Point.Y - mouse.Y, 2)));
            c[1] = (Math.Sqrt(Math.Pow((this.Point.X + this.Size.Width) - mouse.X, 2) + Math.Pow(this.Point.Y - mouse.Y, 2)));
            c[2] = (Math.Sqrt(Math.Pow(this.Point.X - mouse.X, 2) + Math.Pow((this.Point.Y + this.Size.Height) - mouse.Y, 2)));
            c[3] = (Math.Sqrt(Math.Pow((this.Point.X + this.Size.Width) - mouse.X, 2) + Math.Pow((this.Point.Y + this.Size.Height) - mouse.Y, 2)));

            try
            {
                for (int x = 0; x < i; x++)
                {
                    c[Array.IndexOf(c, c.Min())] = 10000;
                }
            }
            catch { }

            return Array.IndexOf(c, c.Min());
        }
    }
}
