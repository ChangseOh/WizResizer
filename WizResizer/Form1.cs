using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WizResizer
{
    [Serializable]
    struct Profile
    {
        public int widthNum;//가로 수치
        public int heightNum;//세로 수치
        public int widthType;//가로 픽셀/퍼센트
        public int heightType;//세로 픽셀/퍼센트
        public bool keepRatio;//가로세로 비율유지
        public bool sameFolderAsSrc;//false = 원본과 같은 폴더에 저장
        public string targetFolder;//저장할 폴더
        public string sourceFolder;//파일 선택창 열 때 부를 폴더
        public List<string> listTarget;
        public int dupMode;//덮어쓰기 모드
        public string prefix;//파일명 접두사
        public string suffix;//파일명 접미사
    };

    public partial class Form1 : Form
    {
        Profile profile = new Profile();
        string nowSourcePath = "";
        string defaultPFname = "default.pf";

        public Form1()
        {
            InitializeComponent();
        }

        private string NumberOnly(string src)
        {
            string ret = "";

            var chstr = src.ToCharArray();
            for (int i = 0; i < chstr.Length; i++)
            {
                //if ((chstr[i] < '0' || chstr[i] > '9') && chstr[i] != '.')
                if (chstr[i] < '0' || chstr[i] > '9')
                    continue;
                ret += chstr[i];
            }

            return ret;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            NumberOnly(textBox1.Text);//숫자와 피리어드만 남긴다
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            NumberOnly(textBox2.Text);
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            textBox5.Enabled = !cb.Checked;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("List Clear");
            listBox1.Items.Clear();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //Keep Ratio
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Delete Selection
            if (listBox1.SelectedItems.Count == 0)
            {
                listBox1.Items.Clear();
            }
            else
            {
                while (listBox1.SelectedItems.Count > 0)
                {
                    foreach (var temp in listBox1.SelectedItems)
                    {
                        listBox1.Items.Remove(temp);
                        break;
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //run resize
            RunResize();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //overwrite
            profile.dupMode = radioButton1.Checked ? 0 : 1;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //suffix
            profile.dupMode = radioButton2.Checked ? 1 : 0;
            textBox3.Enabled = true;
            textBox4.Enabled = true;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            profile.prefix = textBox3.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            profile.suffix = textBox4.Text;
        }

        private object prevImgIndex;
        List<int> listBox1_selection = new List<int>();
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                TrackSelectionChanged((ListBox)sender, listBox1_selection);
                int lidx = listBox1_selection[listBox1_selection.Count - 1];
                object si = listBox1.Items[lidx];
                if (prevImgIndex != si)
                {
                    var img = System.Drawing.Image.FromFile(si.ToString());
                    pictureBox1.Image = img;
                    prevImgIndex = si;
                    label4.Text = string.Format("Size {0}x{1}", img.Width, img.Height);
                }
            }
            else
            {
                pictureBox1.Image = null;
                prevImgIndex = -1;
                label4.Text = "Size";
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] temps = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            List<string> s = new List<string>();
            foreach (var ss in temps)
            {
                if(Directory.Exists(ss))
                {
                    nowSourcePath = ss;
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(ss);
                    foreach(var _ss in di.GetFiles())
                    {
                        if (Directory.Exists(_ss.ToString()))
                            continue;

                        switch (Path.GetExtension(_ss.ToString()).ToLower())
                        {
                            case ".bmp":
                            case ".gif":
                            case ".png":
                            case ".jpg":
                            case ".jpeg":
                            case ".tiff":
                                s.Add(ss + "\\" + _ss.ToString());
                                break;
                        }
                    }
                }

                switch (Path.GetExtension(ss).ToLower())
                {
                    case ".bmp":
                    case ".gif":
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".tiff":
                        s.Add(ss);
                        break;
                }
            }

            int n;
            if (profile.listTarget == null)
            {
                n = 0;
                profile.listTarget.Clear();
            }
            else
            {
                n = profile.listTarget.Count;
                //Array.Resize<string>(ref profile.listTarget, n + s.Count);
            }
            for (int i = 0; i < s.Count; i++)
            {
                listBox1.Items.Add(s[i]);
                profile.listTarget.Add(s[i]);
            }
            //중복제거 처리할 것
            string[] tempArr = profile.listTarget.Distinct().ToArray();
            if (tempArr.Length < profile.listTarget.Count)
            {
                listBox1.Items.Clear();
                //Array.Resize<string>(ref profile.listTarget, tempArr.Length);
                for (int i = 0; i < tempArr.Length; i++)
                {
                    listBox1.Items.Add(tempArr[i]);
                    profile.listTarget.Add(tempArr[i]);//[i] = tempArr[i];
                }
            }

            label3.Text = string.Format("File List ({0} files)", listBox1.Items.Count);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void TrackSelectionChanged(ListBox lb, List<int> selection)
        {
            ListBox.SelectedIndexCollection sic = lb.SelectedIndices;
            foreach (int index in sic)
            {
                if (!selection.Contains(index))
                    selection.Add(index);
            }
            List<int> tempList = new List<int>(selection);
            foreach (int index in tempList)
            {
                if (!sic.Contains(index))
                    selection.Remove(index);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileInfo fi = new FileInfo(defaultPFname);
            if (fi.Exists)
            {
                var pf = loadPF(defaultPFname);
            }
            else
            {
                //프로필 초기화
                profile.widthNum = 0;
                profile.heightNum = 0;
                profile.widthType = 0;
                profile.heightType = 0;
                profile.keepRatio = true;
                profile.sameFolderAsSrc = false;
                profile.sourceFolder = "";
                profile.targetFolder = "";
                profile.listTarget = new List<string>();
                profile.dupMode = 0;//덮어쓰기 모드
                profile.prefix = "";//파일명 접두사
                profile.suffix = "";//파일명 접미사

                initForm();
            }
        }
        private bool loadPF(string filename)
        {
            bool isVeri = false;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                //Profile _pf = (Profile)bf.Deserialize(fs);
                //setProfile(_pf);
                isVeri = true;
            }
            return isVeri;
        }
        private bool savePF(string filename)
        {
            bool isVeri = false;
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, profile);
                isVeri = true;
            }
            return isVeri;
        }
        private void initForm()
        {
            comboBox1.SelectedIndex = profile.widthType;
            textBox1.Text = string.Format("{0}", profile.widthNum);
            textBox2.Text = string.Format("{0}", profile.heightNum);
            radioButton1.Checked = profile.dupMode == 0;
            radioButton2.Checked = profile.dupMode == 1;
            checkBox2.Checked = true;
        }

        private void RunResize()
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                {
                    string fname = listBox1.SelectedItems[i].ToString();
                    DoResize(fname);
                }
            }
            else
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    string fname = listBox1.Items[i].ToString();
                    DoResize(fname);
                }
            }

            SystemSounds.Beep.Play();
        }
        private void DoResize(string fname)
        {
            //선택된 항목의 이미지 크기를 얻어온다
            string _fname = fname;
            var img = System.Drawing.Image.FromFile(_fname);
            var srcBitmap = new Bitmap(img);
            int srcWidth = img.Width, srcHeight = img.Height;
            //img.dpi

            int targetWidth = int.Parse(textBox1.Text);
            int targetHeight = int.Parse(textBox2.Text);

            //percent인 경우 pixel로 환산
            if (comboBox1.SelectedIndex == 1)
            {
                targetWidth = targetWidth * srcWidth / 100;
                targetHeight = targetHeight * srcHeight / 100;
            }

            using (Bitmap bitmap = new Bitmap(targetWidth, targetHeight))
            {
                Point[] pointArray =
                {
                    new Point(0,0),
                    new Point(targetWidth, 0),
                    new Point(0, targetHeight)
                };

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(srcBitmap, pointArray);
                }
                bitmap.SetResolution(img.HorizontalResolution, img.VerticalResolution);

                string filePath = Path.GetDirectoryName(fname);
                string fileName = Path.GetFileNameWithoutExtension(fname);
                string fileExt = Path.GetExtension(fname);
                string targetFname = "";
                if (!checkBox1.Checked)
                {
                    filePath = textBox5.Text;
                }
                targetFname = filePath + "\\" + fileName + fileExt;
                if (!radioButton1.Checked)
                    targetFname = string.Format("{0}\\{1}{2}{3}{4}", filePath, textBox3.Text, fileName, textBox4.Text, fileExt);

                img.Dispose();

                switch (fileExt.ToLower())
                {
                    case ".bmp":
                        bitmap.Save(targetFname, ImageFormat.Bmp);
                        break;
                    case ".png":
                        bitmap.Save(targetFname, ImageFormat.Png);
                        break;
                    case ".gif":
                        bitmap.Save(targetFname, ImageFormat.Gif);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        bitmap.Save(targetFname, ImageFormat.Jpeg);
                        break;
                    //case ".exif":
                    //    bitmap.Save(targetFname, ImageFormat.Exif);
                    //    break;
                    case ".tif":
                    case ".tiff":
                        bitmap.Save(targetFname, ImageFormat.Tiff);
                        break;
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                //선택이 하나뿐이고 비율 유지가 되어있으면 textBox2의 값도 연동해서 변하게 한다
                if (listBox1.SelectedItems.Count == 1 && checkBox2.Checked)
                {
                    var height = getPair(int.Parse(textBox1.Text), comboBox1.SelectedIndex, true);
                    textBox2.Text = string.Format("{0}", height);
                }
                else
                    if (checkBox2.Checked && comboBox1.SelectedIndex == 1)
                {
                    //단위가 percent면 선택여부와 상관없이 연동
                    textBox2.Text = textBox1.Text;
                }
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                //선택이 하나뿐이고 비율 유지가 되어있으면 textBox2의 값도 연동해서 변하게 한다
                if (listBox1.SelectedItems.Count == 1 && checkBox2.Checked)
                {
                    var width = getPair(int.Parse(textBox2.Text), comboBox1.SelectedIndex, false);
                    textBox1.Text = string.Format("{0}", width);

                }
                else
                    if (checkBox2.Checked && comboBox1.SelectedIndex == 1)
                {
                    //단위가 percent면 선택여부와 상관없이 연동
                    textBox1.Text = textBox2.Text;
                }
            }
        }
        private float getPair(int val, int mode, bool isX)
        {
            float ret = 0;

            //선택된 항목의 이미지 크기를 얻어온다
            string _fname = listBox1.SelectedItem.ToString();
            var img = System.Drawing.Image.FromFile(_fname);
            int srcWidth = img.Width, srcHeight = img.Height;

            if (mode == 0)
            {
                //값1의 형태가 pixel이면, 값1/이미지너비가 변환비율
                float rate = (float)val / (float)(isX ? srcWidth : srcHeight);
                if (mode == 0)
                {
                    ret = (int)((float)(isX ? srcHeight : srcWidth) * rate);
                }
                else
                {
                    ret = (int)(rate * 100);
                }
            }
            else
            {
                //값1의 형태가 percent면 값1 자체가 변환값
                float rate = val * 0.01f;
                if (mode == 0)
                {
                    ret = (int)((float)(isX ? srcHeight : srcWidth) * rate);
                }
                else
                {
                    ret = (int)(rate * 100);
                }
            }

            return ret;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.SelectedPath == "")
            {
                if (Directory.Exists(textBox5.Text))
                    fbd.SelectedPath = textBox5.Text;
                else
                    fbd.SelectedPath = nowSourcePath;
            }
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = fbd.SelectedPath;
                profile.targetFolder = fbd.SelectedPath;
            }
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            ListBox lb = sender as ListBox;
            int idx = lb.IndexFromPoint(e.Location);
            if (idx == -1)
                lb.ClearSelected();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int idx = listBox1.IndexFromPoint(e.Location);

            if (idx != -1)
            {
                System.Diagnostics.Process.Start(string.Format("{0}", listBox1.SelectedItem.ToString()));
                return;
            }


            //파일 추가 다이얼로그 열기
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image(s)";
            ofd.Filter = "Image(jpg, bmp, png, gif, tiff)|*.gif;*.png;*.tiff;*.jpg;*.bmp";
            ofd.Multiselect = true;
            ofd.CustomPlaces.Add(nowSourcePath);
            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                nowSourcePath = Path.GetDirectoryName(ofd.FileNames[0]);

                string[] filename = ofd.FileNames;

                List<string> strs = new List<string>();
                foreach (var lst in listBox1.Items)
                {
                    strs.Add(string.Format("{0}", lst));
                }
                foreach (var sld in filename)
                {
                    strs.Add(sld);
                }
                strs.Distinct().ToList();
                listBox1.Items.Clear();
                //profile.listTarget = new string[strs.Count];
                int id = 0;
                foreach (var fn in strs)
                {
                    listBox1.Items.Add(fn);
                    //profile.listTarget[id] = fn;
                    id++;
                }

                label3.Text = string.Format("File List ({0} files)", strs.Count());
            }
        }
    }
}
