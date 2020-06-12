using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCropper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                imageCropperBox1.Image = new Bitmap(ofd.FileName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            imageCropperBox1.PreviewImageChanged += ImageCropperBox1_PreviewImageChanged;
        }

        private void ImageCropperBox1_PreviewImageChanged(object sender, PreviewImageChangedArgs e)
        {
            pbPreview.Image = e.PreviewImage;
        }
    }
}
