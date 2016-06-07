using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace adahf1
{
    public partial class Form1 : Form
    {

        List<List<int>> Images = new List<List<int>>(); //ezek a képek az adatbázisban, vektor formában
        List<List<int>> ZImages = new List<List<int>>(); //ezek a képek az adatbázisban, vektor formában
        private List<int> b = new List<int>(); 
        public image currImage; // az épp szerkesztés alatt álló
        public int LPheight = 0;
        public int LPtoppadding = 30;
        public int LPleftpadding = 30; //a bal panel értékei
        private int matrixsize; //N: vektordimenzió
        public double[,] TheWMatrix = new double[64, 64];
        public double[,] TheMMatrix = new double[64, 64];
        public double[,] TheZMatrix = new double[64, 64];
        private image currRightImage;
        private DateTime startTime;
        private int timeriters = 0;

        //számolás
        private int itercount;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        #region spurious

        public void GenerateZVectors()
        {
            ZImages.Clear();
            foreach (var image in Images)
            {
                ZImages.Add(GenerateZVector());
            }
        }

        public List<int> GenerateZVector()
        {
            Random r = new Random(ZImages.Count * DateTime.Now.Millisecond);
            RandomNumberGenerator r2 = new RNGCryptoServiceProvider();
            byte[] asd = new byte[64];
            r2.GetNonZeroBytes(asd);
            List<int> n = new List<int>();
            for (int i = 0; i < 64; i++)
            {
                if(asd[i]>127)
                    n.Add(-1);
                else
                {
                    n.Add(1);
                }
            }
            return n;
        }

        public void BuildMMatrix()
        {
            if (Images.Count == 0) return;
            matrixsize = Images[0].Count();
            DateTime start = DateTime.Now;
            for (int i = 0; i < matrixsize; i++)
            {
                for (int j = 0; j < matrixsize; j++)
                {
                    double ertek = 0;
                    for (int z = 0; z < Images.Count; z++)
                    {
                        ertek += Images[z][j] * ZImages[z][i];
                    }

                    TheMMatrix[i, j] = (1.0 / matrixsize) * ertek;
                    //label3.Text += TheWMatrix[i, j].ToString("0.000 ");
                }
                //label3.Text += "\n";
            }
            DateTime finish = DateTime.Now;
            TimeSpan len = finish - start;
            //toolStripStatusLabel2.Text = matrixsize * matrixsize + " sized matrix generated in " + len.TotalMilliseconds +
             //                             "ms.";
           // button7.Enabled = true;
           // button8.Enabled = true;
        }

        public void BuildZMatrix()
        {
            if (Images.Count == 0) return;
            matrixsize = Images[0].Count();
            DateTime start = DateTime.Now;
            for (int i = 0; i < matrixsize; i++)
            {
                for (int j = 0; j < matrixsize; j++)
                {
                    double ertek = 0;
                    for (int z = 0; z < Images.Count; z++)
                    {
                        ertek += ZImages[z][j] * ZImages[z][i];
                    }

                    TheZMatrix[i, j] = (1.0 / matrixsize) * ertek;
                    //label3.Text += TheWMatrix[i, j].ToString("0.000 ");
                }
                //label3.Text += "\n";
            }
            DateTime finish = DateTime.Now;
            TimeSpan len = finish - start;
            //toolStripStatusLabel2.Text = matrixsize * matrixsize + " sized matrix generated in " + len.TotalMilliseconds +
            //                             "ms.";
            // button7.Enabled = true;
            // button8.Enabled = true;
        }

        #endregion

        public void GetHammingDsts()
        {
            DateTime start = DateTime.Now;
            labelHamming.Text = "";
            labelHamming.Text += "Hamming distances:\n";
            List<int> rightImageAsVector = currRightImage.ToVector();
            int cnt = 0;

            foreach (var image in Images)
            {
                labelHamming.Text += "\n[" + cnt + "]: " +
                GetHammingDistance(rightImageAsVector, image);
                cnt++;
            }
            DateTime finish = DateTime.Now;
            TimeSpan len = finish - start;
            labelHamming.Text += "\n\nTime spent: " + len.TotalMilliseconds + " ms.";
        }

        public int GetHammingDistance(List<int> imageAsVector1, List<int> imageAsVector2)
        {
            int ret = 0;
            for (int i = 0; i < 64; i++)
            {
                if (!imageAsVector1[i].Equals(imageAsVector2[i]))
                    ret++;
            }
            return ret;
        }

        public void BuildMatrix() //Learn button
        {
            label3.Text = "";
            if (Images.Count == 0) return;
            matrixsize = Images[0].Count();
            DateTime start = DateTime.Now;
            for (int i = 0; i < matrixsize; i++)
            {
                for (int j = 0; j < matrixsize; j++)
                {
                    double ertek = 0;
                    foreach (var image in Images)
                    {
                        ertek += image[j] * image[i]; //előadás alapján képlet (w mátrix)
                    }
                    TheWMatrix[i, j] = (1.0 / matrixsize) * ertek;
                    //label3.Text += TheWMatrix[i, j].ToString("0.000 ");
                }
                //label3.Text += "\n";
            }
            DateTime finish = DateTime.Now;
            TimeSpan len = finish - start;
            toolStripStatusLabel2.Text = matrixsize * matrixsize + " sized matrix generated in " + len.TotalMilliseconds +
                                          "ms.";
            button7.Enabled = true;
            button8.Enabled = true;
        }

        public class image
        {
            public List<imgPixel> imgPixels = new List<imgPixel>();
            public Panel parent;
            public Point location; //bal felső sarka

            //konstruktor
            public image(Point Location, Panel Parent)
            {
                parent = Parent;
                location = Location;

                for (int i = 0; i < 80; i += 10)
                {
                    for (int j = 0; j < 80; j += 10)
                    {
                        imgPixel current = new imgPixel(new Point(location.X + j, location.Y + i), parent);
                        imgPixels.Add(current);
                    }
                }
            }

            public List<int> ToVector()
            {
                return imgPixels.Select(imgPixel => imgPixel.value).ToList();
            }

            public void reset()
            {
                foreach (var imgPixel in imgPixels)
                {
                    if (imgPixel.value == 1)
                        imgPixel.Invert();
                }
            }
        }


        public class imgPixel
        {
            //hely (bal felső)
            public Point location;
            //érték
            public int value; // bármikor állítható
            //parent
            public Control ParentControl;
            //panel
            public Panel p;
            //konstruktor
            public imgPixel(Point Location, Control Parent, int Value = -1)
            {
                location = Location;
                ParentControl = Parent;
                value = Value;
                p = GenPanel(location, Parent);
            }

            public void Invert(object o, MouseEventArgs args)
            {
                //színcsere
                p.BackColor = p.BackColor == Color.Black ? Color.White : Color.Black;
                //számcsere
                value *= -1;
            }

            public void Invert()
            {
                //színcsere
                p.BackColor = p.BackColor == Color.Black ? Color.White : Color.Black;
                //számcsere
                value *= -1;
            }

            public void Set(int color) //-1: fehér, 1: fekete
            {
                //színcsere
                p.BackColor = color == -1 ? Color.White : Color.Black;
                //számcsere
                value = color;
            }

            private Panel GenPanel(Point locationPoint, Control parent)
            {
                Panel panel1 = new System.Windows.Forms.Panel
                {
                    BackColor = Color.White,
                    Location = locationPoint,
                    Name = "panel1",
                    Size = new System.Drawing.Size(10, 10),
                };
                panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Invert);
                parent.Controls.Add(panel1);
                return panel1;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (currImage == null)
                currImage = new image(new Point(300, 100), splitContainer1.Panel1);
            else
            {
                currImage.reset();
            }
        }

        public void RefreshLeftPanel() //a bal panel képeit tölti újra
        {
            LeftPanel.Controls.Clear();
            LeftPanel.Invalidate();
            LPheight = 0;
            //minden kép kirajzolása balra
            foreach (var image in Images)
            {
                int k = 0;
                for (int i = 0; i < 80; i += 10)
                {
                    for (int j = 0; j < 80; j += 10)
                    {
                        GenStaticPanel(new Point(j + LPleftpadding, i + LPtoppadding + LPheight), LeftPanel, image[k]);
                        k++;
                    }
                }

                LPheight += 100;
            }
            LPheight = 0;
            foreach (var image in ZImages)
            {
                int k = 0;
                for (int i = 0; i < 80; i += 10)
                {
                    for (int j = 0; j < 80; j += 10)
                    {
                        GenStaticPanel(new Point(j + LPleftpadding + 100, i + LPtoppadding + LPheight), LeftPanel, image[k]);
                        k++;
                    }
                }

                LPheight += 100;
            }
            toolStripStatusLabel1.Text = Images.Count() + " patterns loaded.";
        }

        public void DisplayLastLP()
        {
            int k = 0;
            for (int i = 0; i < 80; i += 10)
            {
                for (int j = 0; j < 80; j += 10)
                {
                    GenStaticPanel(new Point(j + LPleftpadding, i + LPtoppadding + LPheight), LeftPanel, Images.Last()[k]);
                    k++;
                }
            }
            LPheight += 100;
            toolStripStatusLabel1.Text = Images.Count() + " patterns loaded.";
        }

        public Panel GenStaticPanel(Point locationPoint, Control parent, int value)
        {
            Panel panel1 = new System.Windows.Forms.Panel
            {
                BackColor = value == -1 ? Color.White : Color.Black,
                Location = locationPoint,
                Name = "panel1",
                Size = new System.Drawing.Size(10, 10),
            };
            parent.Controls.Add(panel1);
            return panel1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Images.Add(currImage.ToVector());
            DisplayLastLP();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RefreshLeftPanel();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure to delete all patterns?",
                                     "Confirm Delete!",
                                     MessageBoxButtons.YesNo);
            if (confirmResult != DialogResult.Yes) return;
            Images.Clear();
            RefreshLeftPanel();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            BuildMatrix();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currRightImage == null)
                currRightImage = new image(new Point(37, 100), splitContainer1.Panel2);
            else
            {
                currRightImage.reset();
            }
        }

        public void doIterate()
        {
            if (itercount == 0)
                startTime = DateTime.Now;
            int res = -1;
            double resd = 0; //az egész eredménye signum nélkül még
            int l = itercount % matrixsize;
            toolStripStatusLabel4.Text = l.ToString();
            for (int j = 0; j < matrixsize; j++)
            {
                double de = TheWMatrix[l, j] * currRightImage.imgPixels[j].value;
                if (l == j) de = 0; //diagonál elemek
                resd += de;
            }
            
            //b vektor kivonása
            //resd -= b[l];

            //signum
            if (resd >= 0)
                res = 1;
            toolStripStatusLabel4.Text += " " + resd;
            currRightImage.imgPixels[l].Set(res);
            itercount++;
        }

        public void doZIterate()
        {
            if (itercount == 0)
                startTime = DateTime.Now;
            int res = -1;
            double resd = 0; //az egész eredménye signum nélkül még
            int l = itercount % matrixsize;
            toolStripStatusLabel4.Text = l.ToString();
            for (int j = 0; j < matrixsize; j++)
            {
                double de = TheZMatrix[l, j] * currRightImage.imgPixels[j].value;
                if (l == j) de = 0; //diagonál elemek
                resd += de;
            }

            //b vektor kivonása
            //resd -= b[l];

            //signum
            if (resd >= 0)
                res = 1;
            toolStripStatusLabel4.Text += " " + resd;
            currRightImage.imgPixels[l].Set(res);
            itercount++;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            doIterate();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer1.Interval = int.Parse(speedbox.Text);
            timeriters = 0;
            timer1.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            itercount = 0;
            currRightImage.reset();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                doZIterate();
            else
            {
                doIterate();
            }
            toolStripStatusLabel3.Text = itercount + " steps in " + (DateTime.Now - startTime).TotalMilliseconds +
                                         " ms.";
            timeriters++;
            if (timeriters > int.Parse(iterbox.Text))
                timer1.Enabled = false;

            //hasonlítsuk össze meglévővel?
            //spurious steady states

        }

        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {

        }

        private void buttonHamming_Click(object sender, EventArgs e)
        {
            GetHammingDsts();
        }

        private void mtest_Click(object sender, EventArgs e)
        {
            GenerateZVectors();

        }

        private void vektorszorz_Click(object sender, EventArgs e)
        {
            int i = 0;
            
            foreach (var imgPixel in currRightImage.imgPixels)
            {
                int res = -1;
                double eredmeny = 0;
                for (int j = 0; j < matrixsize; j++)
                {
                    eredmeny += TheMMatrix[i, j]*imgPixel.value;
                }
                if (eredmeny >= 0)
                    res = 1;
                imgPixel.Set(res);
                i++;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            BuildMMatrix();
            BuildZMatrix();
        }
    }
}
