using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using PhotoEditor.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PhotoEditor
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private string FileName { get; set; }

        List<Effect> Effects { get; set; }
        List<Filter> Filters { get; set; }

        Bitmap originalImage { get; set; }
        Histogram histogram { get; set; }

        Bitmap editedImage { get; set; }
        Bitmap showImage { get; set; }
        static int IntEdit { get; set; }
        bool Cancel { get; set; }

        private float zoom = 1f;
        private const float ZoomIncrement = 0.05f;
        private int accX;
        private int accY;
        private bool ChangePosition = false;
        private int startX = 0;
        private int startY = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadImage(openFileDialog1.FileName);
                txtPasta.Text = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
            }
        }

        private void txtPasta_TextChanged(object sender, EventArgs e)
        {
            string[] files = System.IO.Directory.GetFiles(txtPasta.Text).Where(q =>
            System.IO.Path.GetExtension(q).ToLower() == ".jpg"
            || System.IO.Path.GetExtension(q).ToLower() == ".jpeg"
            || System.IO.Path.GetExtension(q).ToLower() == ".png"
            || System.IO.Path.GetExtension(q).ToLower() == ".gif"
            || System.IO.Path.GetExtension(q).ToLower() == ".bmp"
            ).ToArray();

            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(64, 64);
            imageList.Images.Add(Properties.Resources.Image);

            lstImages.Items.Clear();
            lstImages.LargeImageList = imageList;

            foreach (string file in files)
            {
                lstImages.Items.Add(System.IO.Path.GetFileName(file), System.IO.Path.GetFileName(file), 0);
            }

            (new System.Threading.Thread(new System.Threading.ThreadStart(LoadImageList))).Start();
        }

        private void LoadImageList()
        {
            try
            {
                if (System.IO.Directory.Exists(txtPasta.Text))
                {
                    string[] files = System.IO.Directory.GetFiles(txtPasta.Text).Where(q =>
                    System.IO.Path.GetExtension(q).ToLower() == ".jpg"
                    || System.IO.Path.GetExtension(q).ToLower() == ".jpeg"
                    || System.IO.Path.GetExtension(q).ToLower() == ".png"
                    || System.IO.Path.GetExtension(q).ToLower() == ".gif"
                    || System.IO.Path.GetExtension(q).ToLower() == ".bmp"
                    ).ToArray();

                    ImageList imageList = new ImageList();
                    imageList.ImageSize = new Size(64, 64);
                    imageList.Images.Add(Properties.Resources.Image);

                    for (int i = 0; i < files.Length; i++)
                    {
                        imageList.Images.Add(ProcessImage.ResizeImage(LoadFromFile(files[i]), 64, 64));
                    }

                    try
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            lstImages.LargeImageList = imageList;
                            for (int i = 0; i < files.Length; i++)
                            {
                                lstImages.Items[i].ImageIndex = i + 1;
                            }
                        }));
                    }
                    catch { }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: LoadImageList " + ex.Message);
            }
        }



        private void UpdateListViewFilters()
        {
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(64, 64);
            imageList.Images.Add(Properties.Resources.Image);

            lstFilters.Items.Clear();
            lstFilters.LargeImageList = imageList;

            Image image64 = ProcessImage.ResizeImage((Image)originalImage.Clone(), 64, 64);
            Histogram histogram64 = ImageEditor.GetHistogram((Bitmap)image64, ImageEditorProgress);

            for (int i = 0; i < Filters.Count; i++)
            {
                imageList.Images.Add(ImageEditor.GetAdjusts((Bitmap)image64.Clone(), histogram64, Filters[i].Effects, ImageEditorProgress));
                lstFilters.Items.Add(Filters[i].Name, Filters[i].Name, i + 1);
            }

        }

        private Bitmap copyBytes(Bitmap sourceBitmap)
        {
            // Assuming you have a Bitmap object called sourceBitmap
            // Assuming you have a sourceBitmap and want to copy its pixel data to a newBitmap
            Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, sourceBitmap.PixelFormat);

            // Lock the source bitmap
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                          ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);

            // Lock the destination bitmap
            BitmapData newData = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                                                    ImageLockMode.WriteOnly, newBitmap.PixelFormat);

            // Calculate the total number of bytes in the image
            int bytesPerPixel = Image.GetPixelFormatSize(sourceBitmap.PixelFormat) / 8;
            int byteCount = sourceData.Stride * sourceBitmap.Height;

            // Create byte arrays to hold the source and destination pixel data
            byte[] sourcePixelData = new byte[byteCount];
            byte[] destinationPixelData = new byte[byteCount];

            sourcePixelData.CopyTo(destinationPixelData, 0);

            // Copy the pixel data from the source bitmap to the sourcePixelData array
            Marshal.Copy(sourceData.Scan0, destinationPixelData, 0, byteCount);

            // Copy the pixel data from the sourcePixelData array to the destination bitmap
            Marshal.Copy(destinationPixelData, 0, newData.Scan0, byteCount);

            // Unlock the source and destination bitmaps
            sourceBitmap.UnlockBits(sourceData);
            newBitmap.UnlockBits(newData);

            sourceBitmap.Dispose();
            sourceBitmap = null;

            // Now the newBitmap contains a copy of the pixel data from the sourceBitmap
            return newBitmap;
        }

        private Bitmap LoadFromFile(string fileName)
        {
            return copyBytes((Bitmap)Image.FromFile(fileName).Clone());
        }

        private void LoadImage(string fileName)
        {
            try
            {
                this.histogram = null;

                ResetControls(null);

                this.FileName = fileName;
                Effects = new List<Effect>();
                UpdateListaEfeitos();

                Image image = LoadFromFile(fileName);//copyBytes((Bitmap)Image.FromFile(fileName));

                lblFileName.Text = string.Format("{0} ({1}x{2})", System.IO.Path.GetFileName(this.FileName), image.Width, image.Height);

                if (originalImage != null)
                {
                    originalImage.Dispose();
                    originalImage = null;
                }
                originalImage = (Bitmap)image.Clone();
                UpdateEditedImage((Bitmap)image);
                UpdateListViewFilters();

                (new System.Threading.Thread(new System.Threading.ThreadStart(LoadHistogram))).Start();
            }
            catch
            {
                MessageBox.Show("Erro ao carregar a imagem");
            }
        }

        private void LoadHistogram()
        {

            this.histogram = ImageEditor.GetHistogram((Bitmap)ResizeImage((Image)originalImage.Clone(), 720 + 360), ImageEditorProgress);
        }

        private void UpdateEditedImage(Image image)
        {
            try
            {
                editedImage = (Bitmap)image.Clone();
                ShowEditedImage();
            }
            catch { }
        }

        private void ShowOriginalImage()
        {
            SetShowImage(OriginalImageResized());
        }

        private void ShowEditedImage()
        {
            if (editedImage != null)
            {
                SetShowImage((Bitmap)editedImage.Clone());
            }

        }

        private Image ResizeImage(Image image, int compatibility)
        {
            int percentEditeed = (int)((compatibility * 100 / (image.Width + image.Height)) * zoom);
            return ProcessImage.ResizeImage(image, percentEditeed);
        }
        private void SetShowImage(Bitmap image)
        {
            //int percentEditeed = (int)(((imgFoto.Width+ imgFoto.Width) * 100 / (image.Width + image.Height))*zoom);
            showImage = (Bitmap)ResizeImage(image, imgFoto.Width + imgFoto.Height); //ProcessImage.ResizeImage(image, percentEditeed) ;
            imgFoto.Invalidate();
        }

        private Bitmap OriginalImageResized()
        {
            //int percent = 100;
            if (!rbIllimited.Checked)
            {
                int sizeMax = 0;

                if (rbSD.Checked) { sizeMax = 720 + 480; }
                else if (rbHD.Checked) { sizeMax = 1280 + 720; }
                else if (rbFHD.Checked) { sizeMax = 1920 + 1080; }
                else if (rb4K.Checked) { sizeMax = 3840 + 2160; }

                if ((originalImage.Width + originalImage.Height) > sizeMax)
                {
                    //percent = sizeMax * 100 / (originalImage.Width + originalImage.Height);
                    return (Bitmap)ResizeImage((Image)originalImage.Clone(), sizeMax);//  ProcessImage.ResizeImage((Image)originalImage.Clone(), percent);

                }
            }

            return (Bitmap)originalImage.Clone();
        }

        private void EditaImagem(object target)
        {
            if (originalImage == null)
            {
                return;
            }

            ImageEditor.Cancel = true;

            lock (originalImage)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);

                ParamEditor param = ((ParamEditor)target);
                if (param.Counter != IntEdit)
                {
                    return;
                }

                List<Effect> effects = new List<Effect>();
                effects.AddRange(this.Effects);
                effects.Add(param.Effect);

                Bitmap bitmap = ImageEditor.GetAdjusts(OriginalImageResized(), histogram, effects, ImageEditorProgress);
                if (bitmap != null)
                {
                    UpdateEditedImage(bitmap);
                }
            }
        }



        public void ImageEditorProgress(int step, int count)
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    pbBig.Maximum = count;
                    pbBig.Value = step;
                }));
            }
            catch (Exception)
            {
            }

        }


        private void MakeAdjusts()
        {
            IntEdit++;

            lblTemperatura.Text = trTemperatura.Value.ToString();
            lblSaturation.Text = trSaturation.Value.ToString();

            lblBrilho.Text = trBrilho.Value.ToString();
            lblContraste.Text = trContrast.Value.ToString();
            
            lblExposition.Text= trExposition.Value.ToString();
            lblRealce.Text = trHighlight.Value.ToString();
            

            ParamEditor param = new ParamEditor
            {
                Counter = IntEdit,
                Effect = GetEffect()
            };

            (new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(EditaImagem))).Start(param);

        }

        public static string GetVersion()
        {
            System.Reflection.Assembly entryPoint = System.Reflection.Assembly.GetEntryAssembly();
            System.Reflection.AssemblyName entryPointName = entryPoint.GetName();
            return entryPointName.Version.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Photo Editor " + GetVersion();
            cmbEffects.Sorted = true;
            this.Filters = new List<Filter>();
            this.LoadFilters();
            cmbBalanceLeft.SelectedIndex = 2;
            cmbBalanceRight.SelectedIndex = 0;
            imgFoto.MouseWheel += pictureBox1_MouseWheel;
            //imgFoto.Paint += pictureBox1_Paint;
            //lstFilters.AutoScrollOffset=
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            int numberOfSteps = e.Delta / SystemInformation.MouseWheelScrollDelta;
            if (numberOfSteps > 0 && zoom < 2f)
            {
                zoom += ZoomIncrement;
            }
            else if (numberOfSteps < 0 && zoom > 0.5f)
            {
                zoom -= ZoomIncrement;
            }

            imgFoto.Invalidate();
        }



        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            /*if (editedImage!=null) {
                SetShowImage((Bitmap)editedImage.Clone());
                float rw = (showImage.Width * zoom) * 1.25f;
                float rh = (showImage.Height * zoom) * 1.25f;
                float x = (((imgFoto.Width / 2) - (rw / 2)) / 2) + accX;
                float y = (((imgFoto.Height / 2) - (rh / 2)) / 2) + accY;
                e.Graphics.DrawImage(showImage, x + (100 / zoom), y);
            }*/

            if (showImage != null)
            {
                e.Graphics.ScaleTransform(zoom, zoom);
                float rw = (showImage.Width * zoom) * 1.25f;
                float rh = (showImage.Height * zoom) * 1.25f;
                float x = (((imgFoto.Width / 2) - (rw / 2)) / 2) + accX;
                float y = (((imgFoto.Height / 2) - (rh / 2)) / 2) + accY;
                e.Graphics.DrawImage(showImage, x + (100 / zoom), y);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowOriginalImage();
            }
            else if (e.Button == MouseButtons.Left)
            {
                imgFoto.Focus();


                ChangePosition = true;


                if (Math.Abs(accX) > imgFoto.Width)
                {
                    accX = 0;
                }

                if (Math.Abs(accY) > imgFoto.Height)
                {
                    accY = 0;
                }

                startX = (int)(e.X - accX);
                startY = (int)(e.Y - accY);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowEditedImage();
            }
            ChangePosition = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ChangePosition)
            {
                float add = 1;
                accX = (int)((e.X - startX) * add);
                accY = (int)((e.Y - startY));

                imgFoto.Invalidate();
            }
        }

        private void Control_MakeAdjusts(object sender, EventArgs e)
        {
            MakeAdjusts();
        }

        private void btnBrilho_Click(object sender, EventArgs e)
        {
            trBrilho.Value = 0;
            MakeAdjusts();
        }

        private void btnContraste_Click(object sender, EventArgs e)
        {
            trContrast.Value = 0;
            MakeAdjusts();
        }

        private void btnTemperatura_Click(object sender, EventArgs e)
        {
            trTemperatura.Value = 0;
            MakeAdjusts();
        }
        private void btnRealce_Click(object sender, EventArgs e)
        {
            trHighlight.Value = 0;
            MakeAdjusts();
        }

        private int ConvertToInt(string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }


        private void btnAplicar_Click(object sender, EventArgs e)
        {
            this.Effects.Add(GetEffect());
            UpdateListaEfeitos();
            ResetControls(sender);
        }

        private Effect GetEffect()
        {
            Effect effect = new Effect()
            {
                Brightness = trBrilho.Value,
                Contrast = trContrast.Value,
                Saturation = trSaturation.Value,
                Temperature = trTemperatura.Value,
                Highlight = trHighlight.Value,
                Exposition = trExposition.Value,
                BalanceLeft = cmbBalanceLeft.Text,
                BalanceRight = cmbBalanceRight.Text,
                Matrix = new int[3, 3]{
                    { ConvertToInt(bm1.Text),ConvertToInt(bm2.Text),ConvertToInt(bm3.Text) },
                    { ConvertToInt(bm4.Text),ConvertToInt(bm5.Text),ConvertToInt(bm6.Text) },
                    { ConvertToInt(bm7.Text),ConvertToInt(bm8.Text),ConvertToInt(bm9.Text) }
                },
                NoiseReduce = new NoiseReduce()
                {
                    Size = (int)numSizeNoise.Value,
                    Limit = (int)Math.Pow(2, (int)numLimitNoise.Value),
                },
                Tone = new EffectTone()
                {
                    Red = cbTR.Checked,
                    Green = cbTG.Checked,
                    Blue = cbTB.Checked,
                    Yellow = cbTY.Checked,
                    Cyan = cbTC.Checked,
                    Magenta = cbTM.Checked,
                    Gray = cbTS.Checked,
                    Weight = (int)numTones.Value
                },
                Pixel = new EffectPixel()
                {
                    Red = cbR.Checked,
                    Green = cbG.Checked,
                    Blue = cbB.Checked,
                },

                Light = new EffectLight()
                {
                    DarkTones = cbEscuro.Checked,
                    MidTones = cbMedios.Checked,
                    LightTones = cbClaro.Checked,
                }
            };
            return effect;
        }

        private void ResetControls(object sender)
        {
            if (sender != cmbEffects)
            {
                cmbEffects.SelectedIndex = 0;
            }

            trBrilho.Value = 0;
            trContrast.Value = 0;
            trSaturation.Value = 0;
            trTemperatura.Value = 0;
            trHighlight.Value = 0;
            trExposition.Value = 0;

            cbEscuro.Checked = true;
            cbMedios.Checked = true;
            cbClaro.Checked = true;

            cbTR.Checked = true;
            cbTG.Checked = true;
            cbTB.Checked = true;
            cbTY.Checked = true;
            cbTC.Checked = true;
            cbTM.Checked = true;
            cbTS.Checked = true;

            cbR.Checked = true;
            cbG.Checked = true;
            cbB.Checked = true;

            bm1.Text = "0";
            bm2.Text = "0";
            bm3.Text = "0";
            bm4.Text = "0";
            bm5.Text = "1";
            bm6.Text = "0";
            bm7.Text = "0";
            bm8.Text = "0";
            bm9.Text = "0";

            numSizeNoise.Value = 0;

        }

        private void UpdateListaEfeitos()
        {
            lstEffects.Items.Clear();
            lstEffects.Items.AddRange(this.Effects.ToArray());
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            ShowEditedImage();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPasta.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void lstFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstEffects.SelectedIndex >= 0)
            {
                if (this.Effects.Count > 0)
                {
                    this.Effects.RemoveAt(lstEffects.SelectedIndex);
                }
                UpdateListaEfeitos();
                MakeAdjusts();
            }
        }

        public string GetDirAppCondig()
        {
            string Dir =
              Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/" +
              Application.ProductName;

            if (!Directory.Exists(Dir))
            { Directory.CreateDirectory(Dir); }

            return Dir;
        }

        private void SaveFilters()
        {
            string filePath = GetDirAppCondig() + "/" + "filters.json";
            System.IO.File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(Filters));
        }

        private void LoadFilters()
        {
            string filePath = GetDirAppCondig() + "/" + "filters.json";
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                Filters = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Filter>>(json);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (this.Effects.Count > 0)
            {
                FormFiltro formFiltro = new FormFiltro();
                if (formFiltro.ShowDialog() == DialogResult.OK)
                {
                    Filter filter = new Filter();
                    filter.Name = formFiltro.FilterName;
                    filter.Effects.AddRange(this.Effects);
                    Filters.Add(filter);
                    SaveFilters();
                    UpdateListViewFilters();
                }
            }
        }

        private void number_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica se a tecla pressionada é numérica ou um sinal de menos
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Verifica se a tecla pressionada é numérica do teclado numérico ou um sinal de menos do teclado numérico
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    if (e.KeyCode != Keys.Subtract) // Verifica se a tecla pressionada é um sinal de menos normal
                    {
                        // Verifica se a tecla pressionada é uma tecla de controle, como backspace ou delete
                        if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right)
                        {
                            // Se a tecla pressionada não for um número, um sinal de menos ou uma tecla de controle, ignora a entrada do usuário
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        // Verifica se já há um sinal de menos no início do texto
                        if (((System.Windows.Forms.TextBox)sender).Text.IndexOf('-') > 0 || ((System.Windows.Forms.TextBox)sender).SelectionStart > 0)
                        {
                            // Se já houver um sinal de menos ou o cursor não estiver no início do texto, ignora a entrada do usuário
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private void cmbEffects_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetControls(sender);
            if (cmbEffects.Text == "mais nítido")
            {
                bm1.Text = "0";
                bm2.Text = "-1";
                bm3.Text = "0";
                bm4.Text = "-1";
                bm5.Text = "5";
                bm6.Text = "-1";
                bm7.Text = "0";
                bm8.Text = "-1";
                bm9.Text = "0";
            }
            else if (cmbEffects.Text == "nítidez exagerada")
            {
                bm1.Text = "-1";
                bm2.Text = "-1";
                bm3.Text = "-1";
                bm4.Text = "-1";
                bm5.Text = "9";
                bm6.Text = "-1";
                bm7.Text = "-1";
                bm8.Text = "-1";
                bm9.Text = "-1";
            }
            else if (cmbEffects.Text == "mais borrado")
            {
                bm1.Text = "1";
                bm2.Text = "1";
                bm3.Text = "1";
                bm4.Text = "1";
                bm5.Text = "0";
                bm6.Text = "1";
                bm7.Text = "1";
                bm8.Text = "1";
                bm9.Text = "1";
            }
            else if (cmbEffects.Text == "apenas bordas")
            {
                bm1.Text = "-1";
                bm2.Text = "-1";
                bm3.Text = "-1";
                bm4.Text = "-1";
                bm5.Text = "8";
                bm6.Text = "-1";
                bm7.Text = "-1";
                bm8.Text = "-1";
                bm9.Text = "-1";
            }
            else if (cmbEffects.Text == "aparência de vidro")
            {
                bm1.Text = "-1";
                bm2.Text = "-1";
                bm3.Text = "-1";
                bm4.Text = "-1";
                bm5.Text = "7";
                bm6.Text = "-1";
                bm7.Text = "-1";
                bm8.Text = "-1";
                bm9.Text = "-1";
            }
            else if (cmbEffects.Text == "imagem tremida")
            {
                bm1.Text = "-1";
                bm2.Text = "1";
                bm3.Text = "1";
                bm4.Text = "-1";
                bm5.Text = "1";
                bm6.Text = "-1";
                bm7.Text = "1";
                bm8.Text = "1";
                bm9.Text = "-1";
            }
            else if (cmbEffects.Text == "abrir sombras")
            {
                cbMedios.Checked = false;
                cbClaro.Checked = false;
                trBrilho.Value = 100;
            }
            else if (cmbEffects.Text == "aumentar luzes")
            {
                cbEscuro.Checked = false;
                trExposition.Value = 50;
                trBrilho.Value = 50;
            }
            else if (cmbEffects.Text == "aumentar azul")
            {
                cbTR.Checked = false;
                cbTY.Checked = false;
                cbTG.Checked = false;
                cbTC.Checked = false;
                cbTM.Checked = false;
                cbTS.Checked = false;
                cbR.Checked = false;
                cbG.Checked = false;
                trSaturation.Value = 100;
            }
            else if (cmbEffects.Text == "reduzir vermelho")
            {
                cbTY.Checked = false;
                cbTG.Checked = false;
                cbTC.Checked = false;
                cbTB.Checked = false;
                cbTM.Checked = false;
                cbTS.Checked = false;
                cbG.Checked = false;
                cbB.Checked = false;
                trSaturation.Value = -50;
            }
            else if (cmbEffects.Text == "reduzir ruído")
            {
                numSizeNoise.Value = 3;
                numLimitNoise.Value = 5;
            }
            else if (cmbEffects.Text == "desenho")
            {
                numSizeNoise.Value = 4;
                numLimitNoise.Value = 5;
                bm1.Text = "-1";
                bm2.Text = "-1";
                bm3.Text = "-1";
                bm4.Text = "-1";
                bm5.Text = "9";
                bm6.Text = "-1";
                bm7.Text = "-1";
                bm8.Text = "-1";
                bm9.Text = "-1";
                trContrast.Value = 50;
                trHighlight.Value = -40;
                trSaturation.Value = -20;
            }
            else if (cmbEffects.Text == "outono")
            {
                numTones.Value = 9;
                cbR.Checked = false;
                cbB.Checked = false;
                cbClaro.Checked = false;
                cbMedios.Checked = false;
                cmbBalanceLeft.Text = "Red";
                cmbBalanceRight.Text = "Cyan";
                trTemperatura.Value = -80;
                trSaturation.Value = -40;
            }
            else if (cmbEffects.Text == "nivelar cores") {
                trContrast.Value = 50;
                trHighlight.Value = -50;
            }
            MakeAdjusts();
        }

        private void rbSize_CheckedChanged(object sender, EventArgs e)
        {
            MakeAdjusts();
        }

        private void lstFilters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstFilters.SelectedItems.Count >= 0)
            {
                this.Filters.RemoveAt(lstFilters.SelectedItems[0].Index);
                SaveFilters();
                UpdateListViewFilters();
            }
        }

        private void cmbBalanceLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbBalanceRight.Items.Clear();
            if (cmbBalanceLeft.Text == "Green" || cmbBalanceLeft.Text == "Blue" || cmbBalanceLeft.Text == "Cyan")
            {
                cmbBalanceRight.Items.Add("Red");
            }

            if (cmbBalanceLeft.Text == "Red" || cmbBalanceLeft.Text == "Blue" || cmbBalanceLeft.Text == "Magenta")
            {
                cmbBalanceRight.Items.Add("Green");
            }

            if (cmbBalanceLeft.Text == "Red" || cmbBalanceLeft.Text == "Green" || cmbBalanceLeft.Text == "Yellow")
            {
                cmbBalanceRight.Items.Add("Blue");
            }

            if (cmbBalanceLeft.Text == "Blue")
            {
                cmbBalanceRight.Items.Add("Yellow");
            }

            if (cmbBalanceLeft.Text == "Red")
            {
                cmbBalanceRight.Items.Add("Cyan");
            }

            if (cmbBalanceLeft.Text == "Green")
            {
                cmbBalanceRight.Items.Add("Magenta");
            }
            cmbBalanceRight.SelectedIndex = 0;
        }

        private void num_ValueChanged(object sender, EventArgs e)
        {
            MakeAdjusts();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                this.FileName = dlgSave.FileName;
                this.SaveFile();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.SaveFile();
        }

        private void SaveFile()
        {
            try
            {
                string strformato = "";

                if (rbSD.Checked) { strformato = string.Format("para o formato SD"); }
                if (rbHD.Checked) { strformato = string.Format("para o formato HD"); }
                if (rbFHD.Checked) { strformato = string.Format("para o formato Full HD"); }
                if (rb4K.Checked) { strformato = string.Format("para o formato 4K"); }

                Bitmap saveImage = (Bitmap)editedImage.Clone();
                Bitmap saveImage64 = (Bitmap)ProcessImage.ResizeImage((Image)editedImage.Clone(), 64, 64);

                string img_path = this.FileName;
                string img_name = System.IO.Path.GetFileName(this.FileName);

                if (!System.IO.File.Exists(img_path) || rbIllimited.Checked || (System.IO.File.Exists(img_path) && MessageBox.Show(string.Format("Tem certeza que deseja sobrescrever o arquivo {0} {1}?", img_name, strformato), "Atenção", MessageBoxButtons.YesNo) == DialogResult.Yes))
                {
                    saveImage.Save(img_path);

                    LoadImage(img_path);

                    for (int i = 0; i < lstImages.Items.Count; i++)
                    {
                        if (lstImages.Items[i].Name == img_name)
                        {
                            if (lstImages.LargeImageList.Images.Count > i + 1)
                            {
                                lstImages.LargeImageList.Images[i + 1] = saveImage64;
                            }
                            lstImages.Invalidate();
                        }
                    }
                }
                else
                {
                    rbIllimited.Checked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            originalImage = (Bitmap)ProcessImage.RotateImage(originalImage);
            editedImage = (Bitmap)ProcessImage.RotateImage(editedImage);
            ShowEditedImage();
            UpdateListViewFilters();
        }

        private void btnSaturation_Click(object sender, EventArgs e)
        {
            trSaturation.Value = 0;
            MakeAdjusts();
        }

        private void lstImages_MouseClick(object sender, MouseEventArgs e)
        {
            ResetControls(sender);
            if (lstImages.SelectedItems.Count > 0)
            {
                LoadImage(txtPasta.Text + "/" + lstImages.SelectedItems[0].Name.ToString());
            }
        }

        private void lstFilters_MouseClick(object sender, MouseEventArgs e)
        {
            ResetControls(sender);
            if (lstFilters.SelectedItems.Count > 0)
            {
                Effects.Clear();
                Effects.AddRange(Filters[lstFilters.SelectedItems[0].Index].Effects);
                UpdateListaEfeitos();
                MakeAdjusts();
            }
        }

        private void btnExposition_Click(object sender, EventArgs e)
        {
            trExposition.Value = 0;
            MakeAdjusts();
        }
    }
}
