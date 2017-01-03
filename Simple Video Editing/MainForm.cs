using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Simple_Video_Editing
{
    public partial class MainForm : Form
    {
        NReco.VideoConverter.FFMpegConverter ffMpeg = null;
        MyFFMpegProgress fFMpegProgress = null;
        public MainForm()
        {
            InitializeComponent();
            init();
        }

        void init()
        {
            ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.LogReceived += ffMpeg_LogReceived;
            fFMpegProgress = new MyFFMpegProgress(new Action<ConvertProgressEventArgs>(this.OnConvertProgress), true);
        }

        private void OnConvertProgress(ConvertProgressEventArgs e)
        {
            progressBar1.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = (int)(e.Processed.TotalSeconds * 100 / e.TotalDuration.TotalSeconds);
            });
        }

        void ffMpeg_LogReceived(object sender, FFMpegLogEventArgs e)
        {
            fFMpegProgress.ParseLine(e.Data);
        }
 

        ~MainForm()
        {
            ffMpeg.Abort();
        }
        private void button1_Click(object sender, EventArgs e)
        {
          
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            init();

            string dirPath = System.IO.Path.GetDirectoryName(videoBrowse1.Filename);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(videoBrowse1.Filename);
            string editedFileName = fileName + "_rotated";
            string ext = System.IO.Path.GetExtension(videoBrowse1.Filename);
            string fullPath = System.IO.Path.Combine(dirPath, editedFileName + ext);

            int transpose = 0;

            if (radioButton1.Checked)
            {
                transpose = 0;
            }
            else if (radioButton2.Checked)
            {
                transpose = 1;
            }
            else if (radioButton3.Checked)
            {
                transpose = 2;
            }
            else
            {
                transpose = 3;
            }
            button1.Enabled = false;
            button2.Enabled = true;

            backgroundWorker1.RunWorkerAsync(new Args()
            {
                inputFile = videoBrowse1.Filename,
                transpose = transpose,
                fullPath = fullPath,
            });

         
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ffMpeg != null)
            {
                button2.Enabled = false;
                new Thread(() =>
                {
                    ffMpeg.Stop();
                }).Start();
            }
        }

        public object ConvertProgress { get; set; }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (Args)e.Argument;
            ffMpeg.Invoke(string.Format(
               "-i \"{0}\" -y -loglevel info -vf \"transpose={1}\" \"{2}\"",
                args.inputFile,
                args.transpose,
                args.fullPath));
            fFMpegProgress.Complete();

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;

        }
    }
}
