using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using RanksGenerator.Properties;

namespace RanksGenerator
{
    public partial class FormMain : Form
    {
        // posílá hodnoty přes jiné vlákno
        private delegate void SetImageCallback(PictureBox pb, Data data);

        private Thread th = null; // vlákno co generuje ranky
        private bool running = false; // pokud vlákno běži
        private bool wasLaunch = false; // pokud se generovalo aspoň jednou

        private ColorPoint bg;
        private ColorPoint lvl1;
        private ColorPoint lvl2;
        private ColorPoint lvl3;
        private ColorPoint lvl4;
        private ColorPoint lvl5;
        private ColorPoint lvl6;
        private Color text;
        private Color area;
        private Color border;
        private long size = 0; // velikost v bytech celé složky

        public FormMain()
        {
            InitializeComponent();
            Icon = Resources.Letter_R_red;
            InitColors();
            cboxMode.Text = cboxMode.Items[0].ToString();
        }

        private void InitColors()
        {
            // area background
            area = Color.White;
            pbArea.Image = createColor(area);

            // border
            border = Color.Black;
            pbBorder.Image = createColor(border);

            // text
            text = Color.White;
            pbText.Image = createColor(text);

            // background
            bg.CX = Color.Black;
            pbBg1.Image = createColor(bg.CX);
            bg.CY = Color.Gray;
            pbBg2.Image = createColor(bg.CY);

            // gray
            lvl1.CX = Color.FromArgb(128, 128, 128);
            pbLvl11.Image = createColor(lvl1.CX);
            lvl1.CY = Color.FromArgb(220, 220, 220);
            pbLvl12.Image = createColor(lvl1.CY);

            // green
            lvl2.CX = Color.FromArgb(0, 128, 0);
            pbLvl21.Image = createColor(lvl2.CX);
            lvl2.CY = Color.FromArgb(0, 220, 0);
            pbLvl22.Image = createColor(lvl2.CY);

            // blue
            lvl3.CX = Color.FromArgb(0, 128, 220);
            pbLvl31.Image = createColor(lvl3.CX);
            lvl3.CY = Color.FromArgb(0, 220, 220);
            pbLvl32.Image = createColor(lvl3.CY);

            // yellow
            lvl4.CX = Color.FromArgb(128, 128, 0);
            pbLvl41.Image = createColor(lvl4.CX);
            lvl4.CY = Color.FromArgb(220, 220, 0);
            pbLvl42.Image = createColor(lvl4.CY);

            // red
            lvl5.CX = Color.FromArgb(128, 0, 0);
            pbLvl51.Image = createColor(lvl5.CX);
            lvl5.CY = Color.FromArgb(220, 0, 0);
            pbLvl52.Image = createColor(lvl5.CY);

            // purple / pink
            lvl6.CX = Color.FromArgb(128, 0, 128);
            pbLvl61.Image = createColor(lvl6.CX);
            lvl6.CY = Color.FromArgb(220, 0, 220);
            pbLvl62.Image = createColor(lvl6.CY);

            reloadPreviews();
        }

        private void reloadPreviews()
        {
            pbPreviewLvl1.Image = createRank(65, 1234567890, lvl1.CX, lvl1.CY, text);
            pbPreviewLvl2.Image = createRank(65, 1234567890, lvl2.CX, lvl2.CY, text);
            pbPreviewLvl3.Image = createRank(65, 1234567890, lvl3.CX, lvl3.CY, text);
            pbPreviewLvl4.Image = createRank(65, 1234567890, lvl4.CX, lvl4.CY, text);
            pbPreviewLvl5.Image = createRank(65, 1234567890, lvl5.CX, lvl5.CY, text);
            pbPreviewLvl6.Image = createRank(65, 1234567890, lvl6.CX, lvl6.CY, text);
            pbPreviewSpecial.Image = createRank(tbTextSpecial.Text, bg.CX, bg.CY, text);
        }

        private LinearGradientMode getMode()
        {
            switch (cboxMode.Text)
            {
                case "Vertical": return LinearGradientMode.Vertical;
                case "Horizontal": return LinearGradientMode.Horizontal;
                case "ForwardDiagonal": return LinearGradientMode.ForwardDiagonal;
                case "BackwardDiagonal": return LinearGradientMode.BackwardDiagonal;
                default: return LinearGradientMode.Vertical;
            }
        }

        private Image createColor(Color c)
        {
            Bitmap img = new Bitmap(Const.ColorViewSize, Const.ColorViewSize);
            Graphics g = Graphics.FromImage(img);
            g.Clear(c);
            g.Dispose();
            return img;
        }

        private Image getAndSetColor(ref Color c)
        {
            colorChooser.Color = c;

            if (colorChooser.ShowDialog() == DialogResult.OK)
                c = colorChooser.Color;

            return createColor(c);
        }

        private void updater(PictureBox pb, Data d)
        {
            if (pb.InvokeRequired)
            {
                pb.BeginInvoke(new SetImageCallback(updater), pb, d);
            }
            else
            {
                pb.Image = d.Img;
                lbProgress.Text = (!d.Progress.Equals("Done!")) ? d.Progress + "/" + 100 : d.Progress;
                lbPack.Text = d.Pack.X + "-" + d.Pack.Y;
                lbDirSize.Text = (size / 1024) + "KB" + "  (" + size + ")";
                btnStart.Enabled = d.BtnEnable;
                btnStart.Text = d.BtnText;
            }
        }

        private void runGenerate()
        {
            tryCreateDir(Const.Dir);
            wasLaunch = false;

            if (cboxLvl1.Checked)
                createAndSaveRank("0-100", "lvl1", 0, lvl1.CX, lvl1.CY, text);

            if (cboxLvl2.Checked)
                createAndSaveRank("100-200", "lvl2", 100, lvl2.CX, lvl2.CY, text);

            if (cboxLvl3.Checked)
                createAndSaveRank("200-300", "lvl3", 200, lvl3.CX, lvl3.CY, text);

            if (cboxLvl4.Checked)
                createAndSaveRank("300-400", "lvl4", 300, lvl4.CX, lvl4.CY, text);

            if (cboxLvl5.Checked)
                createAndSaveRank("400-500", "lvl5", 400, lvl5.CX, lvl5.CY, text);

            if (cboxLvl6.Checked)
                createAndSaveRank("500-600", "lvl6", 500, lvl6.CX, lvl6.CY, text);

            if (!wasLaunch)
            {
                new SetImageCallback(updater)(pbRender,
                new Data(null, "0", Point.Empty, true, "Generate"));
            }
        }

        private string genName(string subDir, string prefix, int index, int offset)
        {
            tryCreateDir(Const.Dir + "/" + subDir);
            return Const.Dir + "/" + subDir + "/" + prefix + "_" + (index - offset) + ".png";
        }

        private void tryCreateDir(string dirName)
        {
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
        }

        private void createAndSaveRank(string subDir, string prefix, int percOffset, Color c1, Color c2, Color fontColor)
        {
            Image img = null;
            wasLaunch = true;

            for (int i = 0; i <= 100; i++)
            {
                int x = i + Const.Offset;
                img = createRank((int)Math.Round(x * 1.09 - 3), i + percOffset, c1, c2, fontColor);

                lock (img)
                {
                    new SetImageCallback(updater)(pbRender,
                        new Data(img, i.ToString(),
                            new Point(percOffset, percOffset + 100), false, "Running..."));
                    img.Save(genName(subDir, prefix, x + percOffset, Const.Offset), ImageFormat.Png);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);
                        size += ms.Length;
                    }
                }
                Thread.Sleep(10);
            }
            new SetImageCallback(updater)(pbRender,
                new Data(img, "Done!", new Point(Const.Max, Const.Max), true, "Generate"));
        }

        private Image createRank(int filled, int index, Color c1, Color c2, Color fontColor)
        {
            Bitmap img = new Bitmap(Const.RankX, Const.RankY);
            Graphics g = Graphics.FromImage(img);

            GraphicsPath path = RoundedRectangle.Create(0, 0, Const.RankX - 1,
                Const.RankY - 1, Const.R);

            GraphicsPath pathFilled = RoundedRectangle.Create(1, 1, filled,
                Const.RankY - 3, Const.R);

            string text = index.ToString() + "%";

            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (!area.Equals(Color.White))
                g.Clear(area);

            g.FillPath(new LinearGradientBrush(
                new Rectangle(0, 0, Const.RankX - 1, Const.RankY - 1),
                bg.CX, bg.CY, getMode()), path);

            g.FillPath(new LinearGradientBrush(
                new Rectangle(0, 0, Const.RankX - 1, Const.RankY - 1),
                c1, c2, getMode()), pathFilled);

            g.DrawString(text,
                new Font("verdana", 7f, FontStyle.Bold),
                new SolidBrush(fontColor), img.Width / 2 - text.Length * 2 - 4, 4);

            g.DrawPath(new Pen(border), path);
            g.Dispose();
            return img;
        }

        private Image createRank(string text, Color c1, Color c2, Color fontColor)
        {
            Bitmap img = new Bitmap(Const.RankX, Const.RankY);
            Graphics g = Graphics.FromImage(img);

            GraphicsPath path = RoundedRectangle.Create(0, 0, Const.RankX - 1,
                Const.RankY - 1, Const.R);

            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (!area.Equals(Color.White))
                g.Clear(area);

            g.FillPath(new LinearGradientBrush(
                new Rectangle(0, 0, Const.RankX - 1, Const.RankY - 1),
                bg.CX, bg.CY, getMode()), path);

            g.DrawString(text,
                new Font("Verdana", 7f, FontStyle.Bold),
                new SolidBrush(fontColor), Const.Offset, 4);

            g.DrawPath(new Pen(border), path);
            g.Dispose();
            return img;
        }

        #region Events

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                th = new Thread(new ThreadStart(runGenerate));
                th.Start();
                btnStart.Enabled = false;
            }
        }

        private void btnCreateSpecial_Click(object sender, EventArgs e)
        {
            string dir = Const.Dir + "/special/" + Path.GetRandomFileName() + ".png";
            Image img = createRank(tbTextSpecial.Text, bg.CX, bg.CY, text);

            tryCreateDir(Const.Dir + "/special/");
            img.Save(dir, ImageFormat.Png);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            tbTextSpecial.Text = "          mimic";
            InitColors();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null && th.IsAlive)
                th.Abort();
        }

        private void pbArea_Click(object sender, EventArgs e)
        {
            pbArea.Image = getAndSetColor(ref area);
            reloadPreviews();
        }

        private void pbBg1_Click(object sender, EventArgs e)
        {
            pbBg1.Image = getAndSetColor(ref bg.CX);
            reloadPreviews();
        }

        private void pbBg2_Click(object sender, EventArgs e)
        {
            pbBg2.Image = getAndSetColor(ref bg.CY);
            reloadPreviews();
        }

        private void pbBorder_Click(object sender, EventArgs e)
        {
            pbBorder.Image = getAndSetColor(ref border);
            reloadPreviews();
        }

        private void pbText_Click(object sender, EventArgs e)
        {
            pbText.Image = getAndSetColor(ref text);
            reloadPreviews();
        }

        private void pbLvl11_Click(object sender, EventArgs e)
        {
            pbLvl11.Image = getAndSetColor(ref lvl1.CX);
            reloadPreviews();
        }

        private void pbLvl12_Click(object sender, EventArgs e)
        {
            pbLvl12.Image = getAndSetColor(ref lvl1.CY);
            reloadPreviews();
        }

        private void lbLvl21_Click(object sender, EventArgs e)
        {
            pbLvl21.Image = getAndSetColor(ref lvl2.CX);
            reloadPreviews();
        }

        private void pbLvl22_Click(object sender, EventArgs e)
        {
            pbLvl22.Image = getAndSetColor(ref lvl2.CY);
            reloadPreviews();
        }

        private void pbLvl31_Click(object sender, EventArgs e)
        {
            pbLvl31.Image = getAndSetColor(ref lvl3.CX);
            reloadPreviews();
        }

        private void pbLvl32_Click(object sender, EventArgs e)
        {
            pbLvl32.Image = getAndSetColor(ref lvl3.CY);
            reloadPreviews();
        }

        private void pbLvl41_Click(object sender, EventArgs e)
        {
            pbLvl41.Image = getAndSetColor(ref lvl4.CX);
            reloadPreviews();
        }

        private void pbLvl42_Click(object sender, EventArgs e)
        {
            pbLvl42.Image = getAndSetColor(ref lvl4.CY);
            reloadPreviews();
        }

        private void pbLvl51_Click(object sender, EventArgs e)
        {
            pbLvl51.Image = getAndSetColor(ref lvl5.CX);
            reloadPreviews();
        }

        private void pbLvl52_Click(object sender, EventArgs e)
        {
            pbLvl52.Image = getAndSetColor(ref lvl5.CY);
            reloadPreviews();
        }

        private void pbLvl61_Click(object sender, EventArgs e)
        {
            pbLvl61.Image = getAndSetColor(ref lvl6.CX);
            reloadPreviews();
        }

        private void pbLvl62_Click(object sender, EventArgs e)
        {
            pbLvl62.Image = getAndSetColor(ref lvl6.CY);
            reloadPreviews();
        }

        private void OnChangeMode(object sender, EventArgs e)
        {
            reloadPreviews();
        }

        private void tbTextSpecial_TextChanged(object sender, EventArgs e)
        {
            reloadPreviews();
        }

        #endregion Events
    }
}