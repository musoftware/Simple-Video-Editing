using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Simple_Video_Editing
{
    public partial class VideoBrowse : UserControl
    {
        public VideoBrowse()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog open = new OpenFileDialog())
            {
                open.Filter = "Mp4 Videos|*.mp4";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var memStream = new MemoryStream())
                        {
                            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                            ffMpeg.GetVideoThumbnail(open.FileName, memStream);
                            pictureBox1.Image = Image.FromStream(memStream);
                        }
                        textBox1.Text = open.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Not valid mp4 video");
                    }

                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        public string Filename { get { return textBox1.Text; } }
    }
}
