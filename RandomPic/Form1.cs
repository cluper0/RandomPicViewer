using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace RandomPic
{
	public partial class Form1 : Form
    {
		public Form1()
		{
			InitializeComponent();
		}

		public Form1(string[] args)
        {
            InitializeComponent();
			ReadList(args[0]);
			if (picList.Count() != 0)
			{
				PlayList();
			}
		}

		List<PicGroup> picList = new List<PicGroup>();
		private void ReadList()
        {
			OpenFileDialog file = new OpenFileDialog();
			file.InitialDirectory = ".";
			file.Filter = "文本文件|*.txt";
			file.ShowDialog();
			if (file.FileName != string.Empty)
			{
				try
				{
					string pathname = file.FileName;   //获得文件的绝对路径
					Stream s = file.OpenFile();
					StreamReader sr = new StreamReader(s);
					string text;
					text = sr.ReadLine();
					while(text != null)
					{
						if (text == string.Empty || text.StartsWith("*") == true)
						{
							text = sr.ReadLine();
							continue;
						}
						string[] split = text.Split(new Char[] { ' ','\t' }, StringSplitOptions.RemoveEmptyEntries);

						PicGroup p = new PicGroup
						{
							prefix = split[0],
							extension = split[1],
							stepTime = double.Parse(split[2]),
							path = pathname.Substring(0, pathname.LastIndexOf('\\'))      //不包含最后的反斜杠
						};

						int iFile = 1;
						while (File.Exists(p.path + "\\" + p.prefix + " (" + iFile.ToString() + ")." + p.extension))
						{
							iFile++;
						}
						p.numPic = iFile - 1;

						picList.Add(p);
						text = sr.ReadLine();
					}
					sr.Close();
					s.Close();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}

		}

		private void ReadList(string listPath)
		{
			try
			{
				Stream s = File.Open(listPath, FileMode.Open) ;
				StreamReader sr = new StreamReader(s);
				string text;
				text = sr.ReadLine();
				while (text != null)
				{
					if (text == string.Empty || text.StartsWith("*") == true)
					{
						text = sr.ReadLine();
						continue;
					}
					string[] split = text.Split(new Char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					PicGroup p = new PicGroup
					{
						prefix = split[0],
						extension = split[1],
						stepTime = double.Parse(split[2]),
						path = listPath.Substring(0, listPath.LastIndexOf('\\'))      //不包含最后的反斜杠
					};

					int iFile = 1;
					while (File.Exists(p.path + "\\" + p.prefix + " (" + iFile.ToString() + ")." + p.extension))
					{
						iFile++;
					}
					p.numPic = iFile - 1;


					picList.Add(p);
					text = sr.ReadLine();
				}
				sr.Close();
				s.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			

		}

		private int iGroup = 0;
		private int iPic = 1;
		private PicGroup p = new PicGroup();
		private string picPath = "";
		private System.Timers.Timer timer = new System.Timers.Timer();
		private Random random = new Random();
		private List<int> rdmList = new List<int>();

		private void PlayList()
		{
			
			int currentIndex;
			for (int i = 0; i < picList.Count; i++)
			{
				currentIndex = random.Next(0, picList.Count - i);
				PicGroup p_temp = picList[currentIndex];
				picList[currentIndex] = picList[picList.Count - 1 - i];
				picList[picList.Count - 1 - i] = p_temp;
			}

			timer.AutoReset = true;
			iGroup = 0;

			p = picList[iGroup];
			iPic = 1;
			picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")."+p.extension;
			this.pictureBox1.Load(picPath);
			timer.Interval = (int)(p.stepTime * 1000);
			timer.Start();
			
			timer.Elapsed += new System.Timers.ElapsedEventHandler((sender, e) =>
			{
				if (iPic < p.numPic)
				{
					iPic++;
					picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
					this.pictureBox1.Load(picPath);
				}
				else
				{
					iGroup = (iGroup + 1) % picList.Count();
					p = picList[iGroup];
					iPic = 1;
					picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
					this.pictureBox1.Load(picPath);
					timer.Interval = (int)(p.stepTime * 1000);
				}
				
			});
		}

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.O:
                    ReadList();
					if (picList.Count() != 0)
					{
						PlayList();
					}
					return true;
				case Keys.Left:
				case Keys.Up:
					///这里有一个不在播放状态下按方向键会崩溃的问题
					if (iPic > 1)
					{
						iPic--;
						picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
						this.pictureBox1.Load(picPath);
					}
					timer.Stop();
					timer.Start();
					return true;
				case Keys.Right:
				case Keys.Down:
					if (iPic < p.numPic)
					{
						iPic++;
						picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
						this.pictureBox1.Load(picPath);
					}
					timer.Stop();
					timer.Start();
					return true;
				case Keys.PageUp:
					iGroup = (iGroup + picList.Count - 1) % picList.Count();
					p = picList[iGroup];
					iPic = 1;
					picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
					this.pictureBox1.Load(picPath);
					timer.Interval = (int)(p.stepTime * 1000);
					return true;
				case Keys.PageDown:
					iGroup = (iGroup + 1) % picList.Count();
					p = picList[iGroup];
					iPic = 1;
					picPath = p.path + "\\" + p.prefix + " (" + iPic.ToString() + ")." + p.extension;
					this.pictureBox1.Load(picPath);
					timer.Interval = (int)(p.stepTime * 1000);
					return true;
			}
            return false;
        }

		private void 扫描文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}
	}

	public class PicGroup
	{
		public int numPic;      //组内图片总数
		public string path;     //组地址
		public string prefix;   //组共同前缀
		public string extension;//组共同拓展名
		public double stepTime; //图片显示时间
	}
}
