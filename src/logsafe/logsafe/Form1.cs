using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace logsafe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string filePath = null;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Choose unsafe log file",
                Filter= "Text Files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofd.FileName))
                {
                    filePath = ofd.FileName;
                    label2.Text = filePath;
                    return;
                }
            }
        }
        private const string regexEndpoint = @"(\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b:\d{1,5})";

        private static string Md5Base64(string inp)
        {
            using (MD5 md5Hasher = MD5.Create())
            {
                byte[] by = Encoding.UTF8.GetBytes(inp);
                return Convert.ToBase64String(md5Hasher.ComputeHash(by, 0, by.Length));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("input file does not exist");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "save safe log file where?",
                FileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_safe" + Path.GetExtension(filePath))

            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    MessageBox.Show("output file already exists");
                    return;
                }
                using (StreamReader p = File.OpenText(filePath))
                {
                    using (FileStream g = File.OpenWrite(sfd.FileName))
                    {
                        using (StreamWriter sw = new StreamWriter(g))
                        {
                            while (!p.EndOfStream)
                            {
                                string line = p.ReadLine();


                                Match match = Regex.Match(line, regexEndpoint);
                                if (match.Success)
                                {
                                    while (match.Success)
                                    {
                                        Regex regex = new Regex(regexEndpoint);
                                        line = regex.Replace(line, Md5Base64(match.Groups[1].Value), 1);
                                        match = Regex.Match(line, regexEndpoint);
                                    }
                                  

                                    sw.WriteLine(line);
                                }
                                else
                                {
                                    sw.WriteLine(line);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
