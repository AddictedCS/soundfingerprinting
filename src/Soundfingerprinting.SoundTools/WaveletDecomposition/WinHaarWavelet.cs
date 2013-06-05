namespace Soundfingerprinting.SoundTools.WaveletDecomposition
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    using Soundfingerprinting.Image;
    using Soundfingerprinting.SoundTools.Properties;
    using Soundfingerprinting.Wavelets;

    public partial class WinHaarWavelet : Form
    {
        private readonly IImageService imageService;

        public WinHaarWavelet(IImageService imageService)
        {
            this.imageService = imageService;
            InitializeComponent();
            Icon = Resources.Sound;
        }

        private void BtnDecomposeClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(_tbImageToDecompose.Text))
            {
                MessageBox.Show(
                    Resources.SelectFile, Resources.SelectFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(Path.GetFullPath(_tbImageToDecompose.Text)))
            {
                MessageBox.Show(
                    Resources.NoSuchFile, Resources.NoSuchFile, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (String.IsNullOrEmpty(_tbSaveImage.Text))
            {
                SaveFileDialog sfd = new SaveFileDialog
                                         {
                                             Filter = Resources.FileFilterJPeg,
                                             FileName = "transformed.jpg"
                                         };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    _tbSaveImage.Text = Path.GetFullPath(sfd.FileName);
                }
                else return;
            }
            FadeControls(false);
            Action action = () =>
                {
                    Image img = Image.FromFile(_tbImageToDecompose.Text);
                    Bitmap bmp = new Bitmap(img);
                    float[][] argb = new float[bmp.Height][];
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        argb[i] = new float[bmp.Width];
                        for (int j = 0; j < bmp.Width; j++) argb[i][j] = bmp.GetPixel(j, i).ToArgb();
                    }

                    Image image = imageService.GetWaveletTransformedImage(argb, new StandardHaarWaveletDecomposition());
                    image.Save(_tbSaveImage.Text, ImageFormat.Jpeg);
                    img.Dispose();
                    bmp.Dispose();
                    image.Dispose();
                };
            action.BeginInvoke(
                (result) =>
                    {
                        action.EndInvoke(result);
                        FadeControls(true);
                        MessageBox.Show(
                            Resources.ImageDecomposedSuccessfuly,
                            Resources.Success,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    },
                null);
        }

        /// <summary>
        ///   Select image to decompose
        /// </summary>
        private void TbImageToDecomposeMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog {Filter = Resources.FileFilterJPeg, FileName = "image.jpg"};
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbImageToDecompose.Text = Path.GetFullPath(ofd.FileName);
            }
        }

        /// <summary>
        ///   Save image
        /// </summary>
        private void TbSaveImageMouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog {Filter = Resources.FileFilterJPeg, FileName = "transformed.jpg"};
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _tbSaveImage.Text = Path.GetFullPath(sfd.FileName);
            }
        }

        /// <summary>
        ///   Fade controls on the windows form
        /// </summary>
        /// <param name = "isVisible">Is Visible parameter</param>
        private void FadeControls(bool isVisible)
        {
            Invoke(new Action(
                () =>
                {
                    _tbImageToDecompose.Enabled = isVisible;
                    _tbSaveImage.Enabled = isVisible;
                    _btnDecompose.Enabled = isVisible;
                }), null);
        }
    }
}