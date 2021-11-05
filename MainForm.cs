using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace HardwareDictionary
{
    public partial class MainForm : Form
    {
        StreamWriter logger = new StreamWriter("log.txt", true);

        public MainForm()
        {
            InitializeComponent();
            logger.AutoFlush = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log("[Инфо] Запускается программа");
            StreamReader reader = null;
            try
            {
                if(!File.Exists(Environment.CurrentDirectory + @"\Data\Texts.txt") ||
                    !File.Exists(Environment.CurrentDirectory + @"\Data\Titles.txt"))
                    throw new FileNotFoundException();
                reader = new StreamReader(Environment.CurrentDirectory + @"\Data\Texts.txt");
                var articlesContent = Regex.Matches(reader.ReadToEnd(), @"\[id=(?<id>\d+)(pic=(?<picid>\d+))?\](?<text>.+)");
                reader.Close();
                Match[] articlesContentArray = new Match[articlesContent.Count];
                articlesContent.CopyTo(articlesContentArray, 0);

                reader = new StreamReader(Environment.CurrentDirectory + @"\Data\Titles.txt");
                var titles = Regex.Matches(reader.ReadToEnd(), 
                    @"\[id=(?<id>\d+)type=(?<type>\d+)(url=(?<url>.+))?(need=(?<need>\d))?(year=(?<year>\d+)" + 
                    @"country=(?<country>.+))?\](?<title>.+)");
                reader.Close();
                Match[] titlesArray = new Match[titles.Count];
                titles.CopyTo(titlesArray, 0);

                int count = 0;
                foreach (Match item in articlesContent)
                    count = int.Parse(item.Groups["id"].Value) + 1;
                if (count != titles.Count)
                    throw new FileNotFoundException();

                for (int i = 0; i < count; i++)
                {
                    var arr = articlesContentArray.Where(m => m.Groups["id"].Value == i.ToString());
                    switch (titlesArray[i].Groups["type"].Value)
                    {
                        case "0":
                            Article.articles.Add(new Site(titlesArray[i].Groups["title"].Value, 
                                arr.Select(m => m.Groups["text"].Value).ToArray(), titlesArray[i].Groups["url"].Value));
                            break;
                        case "1":
                            Article.articles.Add(new Hardware(titlesArray[i].Groups["title"].Value,
                                arr.Select(m => m.Groups["text"].Value).ToArray(), titlesArray[i].Groups["need"].Value != "0"));
                            break;
                        case "2":
                            Article.articles.Add(new Manufacturer(titlesArray[i].Groups["title"].Value,
                                arr.Select(m => m.Groups["text"].Value).ToArray(), int.Parse(titlesArray[i].Groups["year"].Value), 
                                titlesArray[i].Groups["country"].Value));
                            break;
                    }
                    var current = Article.articles[Article.articles.Count - 1];
                    if (current is IPictures)
                    {
                        var pics = new Bitmap[arr.Count()];
                        Group temp;
                        for(int j = 0; j < pics.Length; j++)
                        {
                            temp = arr.ElementAt(j).Groups["picid"];
                            if (!temp.Success)
                                continue;
                            if (!File.Exists(Environment.CurrentDirectory + @"\Data\" + temp.Value + ".png"))
                                throw new FileNotFoundException();
                            pics[j] = new Bitmap(Environment.CurrentDirectory + @"\Data\" + temp.Value + ".png");
                        }
                        (current as IPictures).Init(pics);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Log("[Ошибка] Один или несколько файлов программы не были обнаружены");
                MessageBox.Show("Один или несколько файлов программы не были обнаружены. Дальнейшая работа невозможна",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            Fill<Hardware>(tabPage1);
            Fill<Manufacturer>(tabPage2);
            Fill<Site>(tabPage3);

            Fill<Hardware>(panel1);
            Fill<Manufacturer>(panel1);
            Fill<Site>(panel1);
            Log("[Инфо] Программа готова к работе");
        }

        private void Fill<T>(Control container, string filter = null) where T: Article
        {
            foreach (var item in Article.articles.OfType<T>())
            {
                if (!string.IsNullOrWhiteSpace(filter) && !item.Search(filter))
                    continue;
                var gb = new GroupBox();
                gb.Text = item.Title;
                gb.Dock = DockStyle.Top;
                gb.AutoSize = false;
                gb.Height = 15;
                gb.Click += (s, ea) =>
                {
                    var temp = s as GroupBox;
                    if (temp.AutoSize)
                    {
                        temp.AutoSize = false;
                        temp.Height = 15;
                        Log("[Инфо] Закрыта статья " + temp.Text);
                    }
                    else
                    {
                        temp.AutoSize = true;
                        Log("[Инфо] Открыта статья " + temp.Text);
                    }
                };
                if (item is Hardware)
                {
                    var l = new Label();
                    l.Text = "Является " + ((item as Hardware).Need ? "обязательным" : "необязательным") + 
                        " компонентом для работы компьютера";
                    l.AutoSize = true;
                    l.Dock = DockStyle.Top;
                    l.Font = new Font(l.Font, FontStyle.Bold);
                    gb.Controls.Add(l);
                    l.BringToFront();
                }
                else if (item is Manufacturer)
                {
                    var l = new Label();
                    l.Text = "Основана в " + (item as Manufacturer).Year.ToString() + "году";
                    l.AutoSize = true;
                    l.Dock = DockStyle.Top;
                    l.Font = new Font(l.Font, FontStyle.Bold);
                    gb.Controls.Add(l);
                    l.BringToFront();
                    l = new Label();
                    l.Text = "Расположение: " + (item as Manufacturer).Country;
                    l.AutoSize = true;
                    l.Dock = DockStyle.Top;
                    l.Font = new Font(l.Font, FontStyle.Bold);
                    gb.Controls.Add(l);
                    l.BringToFront();
                }
                for (int i = 0; i < item.Text.Length; i++)
                {
                    var l = new Label();
                    l.Text = item.Text[i];
                    l.AutoSize = true;
                    l.Dock = DockStyle.Top;
                    gb.Controls.Add(l);
                    l.BringToFront();
                    if (item is IPictures && (item as IPictures).Pictures[i] != null)
                    {
                        var pb = new PictureBox();
                        pb.Image = (item as IPictures).Pictures[i];
                        pb.Dock = DockStyle.Top;
                        pb.SizeMode = PictureBoxSizeMode.AutoSize;
                        gb.Controls.Add(pb);
                        pb.BringToFront();
                    }
                }
                if (item is Site)
                {
                    var l = new LinkLabel();
                    l.Text = (item as Site).Url;
                    l.AutoSize = true;
                    l.Dock = DockStyle.Top;
                    l.Font = new Font(l.Font, FontStyle.Bold);
                    gb.Controls.Add(l);
                    l.BringToFront();
                }
                container.Controls.Add(gb);
                gb.BringToFront();
            }
        }

        private void Log(string msg)
        {
            logger.WriteLine("[{0}][{1}]{2}", DateTime.Now.ToString("G"), Environment.UserName, msg);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            Log("[Инфо]Поиск по ключевому(ым) слову(ам): " + textBox1.Text);
            Fill<Hardware>(panel1, textBox1.Text);
            Fill<Manufacturer>(panel1, textBox1.Text);
            Fill<Site>(panel1, textBox1.Text);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log("[Инфо] Завершение работы программы. Причина: " + e.CloseReason.ToString());
            logger.Close();

        }
    }
}
