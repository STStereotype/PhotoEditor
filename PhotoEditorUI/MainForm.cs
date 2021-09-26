using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PhotoEditor
{
    public partial class MainForm : Form
    {
        private Image image;
        private int _currentWidth;
        private int _currentHeight;
        private int _staticWidth;
        private int _staticHeight;
        private float _scaleImage = 100;
        string fileName;

        private Bitmap ResizeNow(int targetWidth, int targetHeight)
        {
            Rectangle destRect = new Rectangle(0, 0, targetWidth, targetHeight);
            Bitmap destImage = new Bitmap(targetWidth, targetHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var wrapmode = new ImageAttributes())
                {
                    wrapmode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapmode);
                }
            }

            return destImage;
        }

        private void UploadImage()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Images|*.bmp;*.png;*.jpg|All files|*.*";
            if (openDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                if (image != null)
                {
                    image = null;
                }
                image = Image.FromFile(openDialog.FileName);
                image = new Bitmap(openDialog.FileName);
                fileName = openDialog.FileName;
                _currentWidth = image.Width;
                _currentHeight = image.Height;

                if (_currentHeight >= panelImage.Size.Height)
                {
                    float v = (int)Math.Pow(10, (image.Height.ToString().Length - 1));
                    float g = panelImage.Size.Height / (image.Height / v) / v;
                    _currentWidth = _staticWidth = Convert.ToInt32(Convert.ToDouble(image.Width) * Convert.ToDouble(g));
                    _currentHeight = _staticHeight = Convert.ToInt32(Convert.ToDouble(image.Height) * Convert.ToDouble(g));
                }

                if (_currentWidth >= panelImage.Size.Width)
                {
                    float v = (int)Math.Pow(10, (image.Width.ToString().Length - 1));
                    float g = panelImage.Size.Width / (image.Width / v) / v;
                    _currentWidth = _staticWidth = Convert.ToInt32(Convert.ToDouble(image.Width) * Convert.ToDouble(g));
                    _currentHeight = _staticHeight = Convert.ToInt32(Convert.ToDouble(image.Height) * Convert.ToDouble(g));
                }
                pictureBoxImage.Image = image;
                pictureBoxImage.Image = ResizeNow(_currentWidth, _currentHeight);
                _scaleImage = 100;

            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Unsupported format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void ImageRotation(RotateFlipType rotateFlipType)
        {
            if (image == null)
            {
                MessageBox.Show("Image is not open. Select image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            image.RotateFlip(rotateFlipType);
            pictureBoxImage.Image = ResizeNow(_currentWidth, _currentHeight);
        }

        private void ChangeSize(float scaleImage)
        {
            if (image == null)
            {
                MessageBox.Show("Image is not open. Select image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var TempHeight = (Convert.ToInt32(_staticHeight * scaleImage)) / 100;
            var TempWifth = (Convert.ToInt32(_staticWidth * scaleImage)) / 100;
            if(TempHeight >= 1 && TempWifth >= 1 && TempWifth <= 10000  && TempWifth <= 10000)
            {
                _scaleImage = scaleImage;
                _currentWidth = TempWifth;
                _currentHeight = TempHeight;
                pictureBoxImage.Image = ResizeNow(TempHeight, TempWifth);
                Size size = new Size(TempWifth, TempHeight);
                panelImage.AutoScrollMinSize = size;
            }
        }

        private void EnlargeImage()
        {
            if (_scaleImage < 32000)
            {
                ChangeSize(_scaleImage *= 1.1f);
            }
            else if (_scaleImage >= 32000)
            {
                ChangeSize(32000);
            }
        }

        private void ReduceImage()
        {
            if (_scaleImage > 0.01f)
            {
                ChangeSize(_scaleImage / 1.1f);
            }
            else if (_scaleImage <= 0.01f)
            {
                ChangeSize(0.01f);
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void map_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 120 && ModifierKeys == Keys.Alt)
            {
                EnlargeImage();
            }
            else if (e.Delta == -120 && ModifierKeys == Keys.Alt)
            {
                ReduceImage();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Do you want to save the file?", "Save", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (image == null || fileName == null)
                {
                    MessageBox.Show("Image is not open. Select image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (Bitmap bmb = (Bitmap)pictureBoxImage.Image.Clone())
                {
                    bmb.Save(@"" + fileName, bmb.RawFormat);
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image == null || fileName == null)
            {
                MessageBox.Show("Image is not open. Select image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "Save image as...";
            saveFile.OverwritePrompt = true;
            saveFile.CheckPathExists = true;
            saveFile.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.PNG)|*.PNG|Image Files(*.JPG)|*.JPG|All files(*.*)|*.*";
            saveFile.ShowHelp = true;
            saveFile.FileName = "";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Bitmap bmb = (Bitmap)pictureBoxImage.Image.Clone())
                    {
                        bmb.Save(saveFile.FileName, bmb.RawFormat);
                    }
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("Unable to save image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
        }

        private void pictureBoxAddImage_Click(object sender, EventArgs e)
        {
            UploadImage();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UploadImage();
        }

        private void pictureBoxPlus_Click(object sender, EventArgs e)
        {
            EnlargeImage();
        }

        private void pictureBoxMinus_Click(object sender, EventArgs e)
        {
            ReduceImage();
        }

        private void increaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnlargeImage();
        }

        private void decreaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReduceImage();
        }

        private void pictureBoxLeft_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.Rotate270FlipNone);
        }

        private void pictureBoxRight_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.Rotate90FlipNone);
        }

        private void turnLeft90DegreesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.Rotate270FlipNone);
        }

        private void turnRight90DegreesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.Rotate90FlipNone);
        }

        private void pictureBoxVertical_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.RotateNoneFlipX);
        }

        private void pictureBoxHorizontal_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.RotateNoneFlipY);
        }

        private void verticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.RotateNoneFlipX);
        }

        private void horizontallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageRotation(RotateFlipType.RotateNoneFlipY);
        }

        private void pictureBoxImage_MouseEnter(object sender, EventArgs e)
        {
            MouseWheel += map_MouseWheel;
        }

        private void pictureBoxImage_MouseLeave(object sender, EventArgs e)
        {
            MouseWheel -= map_MouseWheel;
        }

        private void pictureBoxAddImage_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxAddImage.Image = Image.FromFile("../../Image/AddHover.png");
        }

        private void pictureBoxAddImage_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxAddImage.Image = Image.FromFile("../../Image/AddStandart.png");
        }

        private void pictureBoxLeft_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxLeft.Image = Image.FromFile("../../Image/LeftHover.png");
        }

        private void pictureBoxLeft_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxLeft.Image = Image.FromFile("../../Image/LeftStandart.png");
        }

        private void pictureBoxRight_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxRight.Image = Image.FromFile("../../Image/RightHover.png");
        }

        private void pictureBoxRight_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxRight.Image = Image.FromFile("../../Image/RightStandart.png");
        }

        private void pictureBoxPlus_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxPlus.Image = Image.FromFile("../../Image/PlusHover.png");
        }

        private void pictureBoxPlus_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxPlus.Image = Image.FromFile("../../Image/PlusStandart.png");
        }

        private void pictureBoxMinus_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxMinus.Image = Image.FromFile("../../Image/MinusHover.png");
        }

        private void pictureBoxMinus_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxMinus.Image = Image.FromFile("../../Image/MinusStandart.png");
        }

        private void pictureBoxVertical_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxVertical.Image = Image.FromFile("../../Image/VerticalHover.png");
        }

        private void pictureBoxVertical_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxVertical.Image = Image.FromFile("../../Image/VerticalStandart.png");
        }

        private void pictureBoxHorizontal_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxHorizontal.Image = Image.FromFile("../../Image/HorizontalHover.png");
        }

        private void pictureBoxHorizontal_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxHorizontal.Image = Image.FromFile("../../Image/HorizontalStandart.png");
        }
    }
}
