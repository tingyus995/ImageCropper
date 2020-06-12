using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCropper
{
    
    static class Utility
    {
        
    }
    
    class ImageCropperBox : PictureBox
    {
        CropArea cropArea = new CropArea(10, 10, 100, 100);
        private bool isDragging = false;
        Dragger currentDragger = null;

        int orgX; int orgY;

        
        public ImageCropperBox()
        {

        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            cropArea.Draw(g);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);



            foreach (Dragger dragger in cropArea.Draggers)
            {
                if (dragger.IsMouseHovering(e.X, e.Y))
                {
                    dragger.Hover = true;
                    currentDragger = dragger;
                    Cursor.Current = Cursors.Hand;
                    orgX = e.X; orgY = e.Y;
                    break;
                }
            }

            if (currentDragger == null && cropArea.IsMouseHovering(e.X, e.Y))
            {

                Cursor.Current = Cursors.Hand;
                isDragging = true;
                orgX = e.X; orgY = e.Y;
            }
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);


            
            // check if is dragging

             if(currentDragger != null) // check if is we're resizing right now
            {
                int dx = e.X - orgX;
                int dy = e.Y - orgY;

                switch (currentDragger.Location)
                {
                    case DraggerLocation.TOP_LEFT:
                        cropArea.X += dx;
                        cropArea.Y += dy;

                        cropArea.Width -= dx;
                        cropArea.Height -= dy;
                        
                        break;
                    case DraggerLocation.TOP_RIGHT:
                        cropArea.Width += dx;
                        cropArea.Y += dy;
                         
                        cropArea.Height -= dy;
                        break;
                    case DraggerLocation.BOTTOM_LEFT:
                        cropArea.X += dx;
                        cropArea.Height += dy;                        
                        
                        cropArea.Width -= dx;

                        break;
                    case DraggerLocation.BOTTOM_RIGHT:
                        cropArea.Width += dx;
                        cropArea.Height += dy;
                        break;
                }

                orgX = e.X; orgY = e.Y;
            }else if (isDragging)
            {
                cropArea.X += e.X - orgX;
                cropArea.Y += e.Y - orgY;
                orgX = e.X; orgY = e.Y;               
            }
            


           // check if mouse is in the crop area

           if (cropArea.IsMouseHovering(e.X, e.Y))
            {
                Cursor.Current = Cursors.Hand;
                cropArea.Hover = true;
            }
            else
            {
                cropArea.Hover = false;
            }
            
            // check if the mouse is hovering on dragger
            foreach (Dragger dragger in cropArea.Draggers)
            {
                int half = dragger.Size / 2;
                if (dragger.IsMouseHovering(e.X, e.Y)){
                    dragger.Hover = true;
                }
                else
                {
                    dragger.Hover = false;
                }                    
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDragging = false;
            currentDragger = null;
        }
    }

    enum DraggerLocation
    {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT
    }
    class Dragger
    {
        public int X
        {
            get
            {

                switch (Location)
                {
                    case DraggerLocation.TOP_LEFT:
                        return area.X;
                    case DraggerLocation.TOP_RIGHT:
                        return area.X + area.Width;
                    case DraggerLocation.BOTTOM_LEFT:
                        return area.X;
                    case DraggerLocation.BOTTOM_RIGHT:
                        return area.X + area.Width;
                }

                return 0;
            }
        }
        public int Y
        {
            get
            {

                switch (Location)
                {
                    case DraggerLocation.TOP_LEFT:
                        return area.Y;
                    case DraggerLocation.TOP_RIGHT:
                        return area.Y;
                    case DraggerLocation.BOTTOM_LEFT:
                        return area.Y + area.Height;
                    case DraggerLocation.BOTTOM_RIGHT:
                        return area.Y + area.Height;

                }
                return 0;
            }
        }
        public int Size { get; private set; }

        private CropArea area;
        public DraggerLocation Location { get; private set; }

        public Color BackColor { get; set; }
        public Color HoverColor { get; set; }

        public bool Hover { get; set; } = false;

        public Dragger(CropArea parent, DraggerLocation loc, int size = 10)
        {
            // initial values
            area = parent; Location = loc; Size = size;
            BackColor = parent.DraggerBackColor;
            HoverColor = parent.DraggerHoverColor;

        }

        public bool IsMouseHovering(int mouseX, int mouseY)
        {
            int half = Size / 2;
            return mouseX >= X - half && mouseX <= (X - half + Size) && mouseY >= Y - half && mouseY <= (Y - half + Size);
        }

        public void Draw(Graphics g)
        {
            int half = Size / 2;

            Brush brush;
            if (Hover) brush = new SolidBrush(HoverColor);
            else brush = new SolidBrush(BackColor);

            g.FillEllipse(brush, new Rectangle(X - half, Y - half, Size, Size));
        }


    }
    class CropArea
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Hover { get; set; }

        public Color BorderColor { get; set; } = Color.FromArgb(255, 0, 0);
        public Color HoverColor { get; set; } = Color.FromArgb(50, 255, 0, 0);

        public Color DraggerBackColor { get; set; } = Color.FromArgb(100, 0, 255, 0);
        public Color DraggerHoverColor { get; set; } = Color.FromArgb(255, 0, 0, 255);

        public Dragger[] Draggers
        {
            get
            {
                return draggers.ToArray();
            }
        }


        private List<Dragger> draggers = new List<Dragger>();

        public CropArea(int x, int y, int width, int height)
        {
            // initialize values
            X = x; Y = y; Width =
            width; Height = height; Hover = false;

            // assign draggers
            draggers.Add(new Dragger(this, DraggerLocation.TOP_LEFT));
            draggers.Add(new Dragger(this, DraggerLocation.TOP_RIGHT));
            draggers.Add(new Dragger(this, DraggerLocation.BOTTOM_LEFT));
            draggers.Add(new Dragger(this, DraggerLocation.BOTTOM_RIGHT));

        }

        public bool IsMouseHovering(int mouseX, int mouseY)
        {
            return mouseX >= X && mouseX <= (X + Width) && mouseY >= Y && mouseY <= (Y + Width);
        }
        public void Draw(Graphics g)
        {
            // draw bonding box
            g.DrawRectangle(new Pen(BorderColor), new Rectangle(X, Y, Width, Height));
            if (Hover)
            {
                g.FillRectangle(new SolidBrush(HoverColor), new Rectangle(X, Y, Width, Height));
            }

            // draw draggers
            foreach (Dragger dragger in draggers)
            {
                dragger.Draw(g);
            }
        }

    }
}
