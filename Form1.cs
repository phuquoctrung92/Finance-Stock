using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FApp
{
    public partial class Form1 : Form
    {
        class info
        {
            public DateTime dt { get; set; }
            public string open { get; set; }
            public string high { get; set; }
            public string low { get; set; }
            public double close { get; set; }
            public string volume
            {
                get { return _volume; }
                set
                {
                    uint val = uint.Parse(value);
                    if (val >= 1000 && val < 1000000)
                    {
                        _volume = (val / 1000).ToString() + "K";
                    }
                    else if (val >= 1000000)
                    {
                        _volume = (val / 1000000).ToString() + "M";
                    }
                    else
                    {
                        _volume = val.ToString();
                    }
                }
            }
            private string _volume;
            public string dividends { get; set; }
            public string split { get; set; }
            public info(string[] data)
            {
                dt = DateTime.Parse(data[0].Split('+')[0]);
                open = data[1];
                high = data[2];
                low = data[3];
                close = double.Parse(data[4]);
                volume = data[5];
                dividends = data[6];
                split = data[7];
            }
            public info()
            {
                dt = DateTime.Now;
                open = high = low = volume = dividends = split = "0";
                close = 0;
            }
        }

        List<string> list = new List<string>();
        Process process;
        public Form1()
        {
            InitializeComponent();
            process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1_Tick(null, null);
        }
        public void loadF()
        {
            DateTime now = DateTime.Now.AddMinutes(-15);
            dataGridView1.Rows.Clear();
            foreach (string item in list)
            {
                string filePath = item + ".csv";
                string firstLine = "";
                string lastLine = "";
                int retry = 0;
                while (File.Exists(filePath) && retry < 5 && string.IsNullOrEmpty(lastLine))
                {
                    try
                    {
                        var allLine = File.ReadAllLines(filePath);
                        if (allLine.Count() < 2)
                        {
                            break;
                        }
                        firstLine = allLine[1];
                        lastLine = allLine[2];
                    }
                    catch
                    {
                    }
                    finally
                    {
                        retry++;
                    }
                }
                var info_first = new info();
                if (!string.IsNullOrEmpty(firstLine))
                {
                    info_first = new info(firstLine.Split(','));
                }
                var info_last = new info();
                if (!string.IsNullOrEmpty(lastLine))
                {
                    info_last = new info(lastLine.Split(','));
                }
                double change = info_last.close - info_first.close;
                string change_str = change > 0 ? "+" + change.ToString() : change.ToString();
                dataGridView1.Rows.Add(item, $"{now.Hour.ToString().PadLeft(2, '0')}:{now.Minute.ToString().PadLeft(2, '0')}", info_last.close, change_str, info_last.volume);
            }
            dataGridView1.ClearSelection();
        }

        public async Task getF()
        {
            var tasks = new List<Task>();

            foreach (string item in list)
            {
                tasks.Add(Task.Run(() =>
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = "/c python f.py " + item;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();
                        process.WaitForExit();
                    }
                }));
            }

            await Task.WhenAll(tasks); // đảm bảo tất cả process hoàn tất
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            list.Clear();
            if (File.Exists("list.txt"))
            {
                list = File.ReadAllLines("list.txt").ToList();
            }
            if (list.Count > 0)
            {
                await getF();
                loadF();
            }
            
        }
    }
}
