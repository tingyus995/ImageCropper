using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCropper
{

    class PreviewImageChangedArgs : EventArgs
    {
        public Image PreviewImage { get; private set; }

        public PreviewImageChangedArgs(Image img)
        {
            PreviewImage = img;
        }
    }

    class ImageCropperBox : PictureBox
    {
        CropArea cropArea = new CropArea(10, 10, 100, 100);
        private bool isDragging = false;
        Dragger currentDragger = null;
        

        int orgX; int orgY;

        public new Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                base.Image = value;
                // find ratio
            }
        }

        public event EventHandler<PreviewImageChangedArgs> PreviewImageChanged;

        private Point ToImageCoordinate(Point p)
        {
            // source: https://stackoverflow.com/questions/10473582/how-to-retrieve-zoom-factor-of-a-winforms-picturebox           
            
            Point unscaled_p = new Point();

            // image and container dimensions
            int w_i = Image.Width;
            int h_i = Image.Height;
            int w_c = Width;
            int h_c = Height;

            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio

            if (imageRatio >= containerRatio)
            {
                // horizontal image
                float scaleFactor = w_c / (float)w_i;
                float scaledHeight = h_i * scaleFactor;
                // calculate gap between top of container and top of image
                float filler = Math.Abs(h_c - scaledHeight) / 2;
                unscaled_p.X = (int)(p.X / scaleFactor);
                unscaled_p.Y = (int)((p.Y - filler) / scaleFactor);
            }
            else
            {
                // vertical image
                float scaleFactor = h_c / (float)h_i;
                float scaledWidth = w_i * scaleFactor;
                float filler = Math.Abs(w_c - scaledWidth) / 2;
                unscaled_p.X = (int)((p.X - filler) / scaleFactor);
                unscaled_p.Y = (int)(p.Y / scaleFactor);
            }

            return unscaled_p;
        }

        void UpdateCroppedPreview()
        {
            
            if(Image != null)
            {

                Point topLeft = ToImageCoordinate(new Point(cropArea.X, cropArea.Y));
                Point bottomRight = ToImageCoordinate(new Point(cropArea.X + cropArea.Width, cropArea.Y + cropArea.Height));

                int width = bottomRight.X - topLeft.X;
                int height = bottomRight.Y - topLeft.Y;

                Bitmap result = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(Image, new Rectangle(0, 0, width, height), new Rectangle(topLeft.X, topLeft.Y, width, height), GraphicsUnit.Pixel);
                }

                PreviewImageChanged?.Invoke(this, new PreviewImageChangedArgs(result));
            }            
        }


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

            if (currentDragger != null) // check if is we're resizing right now
            {
                int dx = e.X - orgX;
                int dy = e.Y - orgY;

                cropArea.DragResize(currentDragger, dx, dy);
                UpdateCroppedPreview();

                orgX = e.X; orgY = e.Y;
            }
            else if (isDragging)
            {
                cropArea.X += e.X - orgX;
                cropArea.Y += e.Y - orgY;
                UpdateCroppedPreview();
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
                if (dragger.IsMouseHovering(e.X, e.Y))
                {
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
            if(isDragging || currentDragger != null) { 
                isDragging = false;
                currentDragger = null;
                UpdateCroppedPreview();
            }
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

        public int MinimumWidth { get; set; } = 30;
        public int MinimumHeight { get; set; } = 30;
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
        private float aspectRatio;

        public CropArea(int x, int y, int width, int height, float ratio = -1)
        {
            // initialize values
            X = x; Y = y;
            Width = width; Height = height;
            Hover = false;
            aspectRatio = ratio;

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

        public void DragResize(Dragger dragger, int dx, int dy)
        {
            switch (dragger.Location)
            {
                case DraggerLocation.TOP_LEFT:

                    if ((Width - dx) >= MinimumWidth)
                    {
                        X += dx; // intent
                        Width -= dx; // compensate
                    }

                    if ((Height - dy) >= MinimumHeight)
                    {
                        Y += dy; // intent
                        Height -= dy; // compensate
                    }

                    break;
                case DraggerLocation.TOP_RIGHT:

                    if ((Width + dx) >= MinimumWidth)
                    {
                        Width += dx; // intent
                    }

                    if ((Height - dy) >= MinimumHeight)
                    {
                        Y += dy; // intent
                        Height -= dy; // compensate
                    }


                    break;
                case DraggerLocation.BOTTOM_LEFT:

                    if ((Width - dx) >= MinimumWidth)
                    {
                        X += dx; // intent
                        Width -= dx; // compensate
                    }

                    if ((Height + dy) >= MinimumHeight)
                    {
                        Height += dy; // intent
                    }


                    break;
                case DraggerLocation.BOTTOM_RIGHT:


                    if ((Width + dx) >= MinimumWidth)
                    {
                        Width += dx; // intent
                    }
                    if ((Height + dy) >= MinimumHeight)
                    {
                        Height += dy; // intent
                    }
                    break;
            }
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
