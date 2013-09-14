namespace SoundFingerprinting.SoundTools.FFMpegResampler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    using SoundFingerprinting.Audio;
    using SoundFingerprinting.SoundTools.Properties;

    public partial class WinFfMpegResampler : Form
    {
        private readonly ITagService tagService;

        private readonly IExtendedAudioService audioService;

        private readonly List<string> fileList = new List<string>();
        private int bitRate;
        private int currentProceesedFiles;
        private string outputPath;
        private bool pause;
        private string rootPath;
        private int samplingRate;
        private int skipped;
        private bool stopped;

        public WinFfMpegResampler(ITagService tagService, IExtendedAudioService audioService)
        {
            this.tagService = tagService;
            this.audioService = audioService;
            InitializeComponent();
            Icon = Resources.Sound;
            currentProceesedFiles = 0;
            skipped = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                rootPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = rootPath;
                fileList.Clear();
                foreach (string f in Directory.GetFiles(rootPath, "*.mp3"))
                {
                    fileList.Add(f);
                }

                foreach (string f in Directory.GetFiles(rootPath, "*.flac"))
                {
                    fileList.Add(f);
                }

                DirSearch(rootPath);
                textBox2.Text = fileList.Count.ToString(CultureInfo.InvariantCulture);
                labelTotalFiles.Text = textBox2.Text;
                skipped = 0;
                currentProceesedFiles = 0;
                labelSkipped.Text = skipped.ToString(CultureInfo.InvariantCulture);
                labelCurrentProcessed.Text = currentProceesedFiles.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void buttonOutputBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                outputPath = folderBrowserDialog2.SelectedPath;
                textBox3.Text = outputPath;
                buttonStartConversion.Enabled = true;
            }
        }


        private void DirSearch(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, "*.mp3"))
                    {
                        fileList.Add(f);
                    }

                    foreach (string f in Directory.GetFiles(d, "*.flac"))
                    {
                        fileList.Add(f);
                    }

                    DirSearch(d);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void ButtonRefreshClick(object sender, EventArgs e)
        {
            fileList.Clear();
            rootPath = textBox1.Text;
            foreach (string f in Directory.GetFiles(rootPath, "*.mp3"))
            {
                fileList.Add(f);
            }

            foreach (string f in Directory.GetFiles(rootPath, "*.flac"))
            {
                fileList.Add(f);
            }

            DirSearch(rootPath);
            textBox2.Text = fileList.Count.ToString(CultureInfo.InvariantCulture);
            labelTotalFiles.Text = textBox2.Text;
            buttonStartConversion.Enabled = true;
            buttonStop.Enabled = false;
            currentProceesedFiles = 0;
            skipped = 0;
            labelSkipped.Text = skipped.ToString();
            labelCurrentProcessed.Text = currentProceesedFiles.ToString(CultureInfo.InvariantCulture);
        }

        private void ProcessFiles()
        {
            Action finishDel = delegate
                {
                    buttonStop.Enabled = false;
                    buttonPause.Enabled = false;
                    buttonStartConversion.Enabled = true;
                    stopped = false;
                };

            Process process = new Process
                {
                    StartInfo =
                        {
                            FileName = "ffmpeg.exe",
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            RedirectStandardInput = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }
                };

            Action del = delegate
                {
                    labelCurrentProcessed.Text = currentProceesedFiles.ToString();
                    labelSkipped.Text = skipped.ToString();
                    richTextBox1.AppendText(process.StandardError.ReadToEnd() + "\n");
                    richTextBox1.Focus();
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                };


            foreach (string f in fileList)
            {
                if (stopped)
                {
                    Invoke(finishDel);
                    Thread.CurrentThread.Abort();
                }

                while (pause)
                {
                    if (stopped)
                    {
                        Invoke(finishDel);
                        Thread.CurrentThread.Abort();
                    }
                    Thread.Sleep(1000);
                }

                TagInfo tags = null;

                try
                {
                    //Read Tags from the file
                    tags = tagService.GetTagInfo(f);
                }
                catch
                {
                    skipped++;
                    currentProceesedFiles++;
                    Invoke(del);
                    continue;
                }

                //Compose the output name of the wav file
                if (String.IsNullOrEmpty(tags.Title) || tags.Title.Length == 0)
                {
                    //Skip file
                    skipped++;
                    currentProceesedFiles++;
                    Invoke(del);
                    continue;
                }
                string artist = "";
                if (String.IsNullOrEmpty(tags.Artist))
                {
                    if (tags.Composer == null)
                    {
                        skipped++;
                        currentProceesedFiles++;
                        Invoke(del);
                        continue;
                    }
                    artist = tags.Composer;
                }
                else artist = tags.Artist;

                string outfilename = tags.Title + " + " + artist + ".wav";

                string outfile = outputPath + "\\" + outfilename;
                string arguments = "-i \"" + f + "\" -ac 1 -ar " + samplingRate + " -ab " + bitRate + " \"" + outfile
                                   + "\"";

                process.StartInfo.Arguments = arguments;
                process.Start();
                process.StandardInput.Write("n");
                currentProceesedFiles++;
                Invoke(del);
            }

            Invoke(finishDel);
        }

        private void ButtonStartConversionClick(object sender, EventArgs e)
        {
            if (!Int32.TryParse(textBoxSamplingRate.Text, out samplingRate))
                samplingRate = 5512;
            if (!Int32.TryParse(textBoxBitRate.Text, out bitRate))
                bitRate = 64000;
            Thread thread = new Thread(ProcessFiles);
            thread.Start();
            currentProceesedFiles = 0;
            labelCurrentProcessed.Text = currentProceesedFiles.ToString();
            buttonPause.Enabled = true;
            buttonStartConversion.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            pause = !pause;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopped = true;
            buttonStop.Enabled = false;
        }
    }
}