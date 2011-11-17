// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Soundfingerprinting.AudioProxies;
using Soundfingerprinting.SoundTools.Properties;
using Un4seen.Bass.AddOn.Tags;

namespace Soundfingerprinting.SoundTools.FFMpegResampler
{
    public partial class WinFfMpegResampler : Form
    {
        private readonly List<string> _fileList = new List<string>();
        private int _bitRate;
        private int _currentProceesedFiles;
        private string _outputPath;
        private bool _pause;
        private string _rootPath;
        private int _samplingRate;
        private int _skipped;
        private bool _stopped;

        public WinFfMpegResampler()
        {
            InitializeComponent();
            Icon = Resources.Sound;
            _currentProceesedFiles = 0;
            _skipped = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                _rootPath = folderBrowserDialog1.SelectedPath;
                textBox1.Text = _rootPath;
                _fileList.Clear();
                foreach (string f in Directory.GetFiles(_rootPath, "*.mp3"))
                {
                    _fileList.Add(f);
                }
                foreach (string f in Directory.GetFiles(_rootPath, "*.flac"))
                {
                    _fileList.Add(f);
                }
                DirSearch(_rootPath);
                textBox2.Text = _fileList.Count.ToString();
                labelTotalFiles.Text = textBox2.Text;
                _skipped = 0;
                _currentProceesedFiles = 0;
                labelSkipped.Text = _skipped.ToString();
                labelCurrentProcessed.Text = _currentProceesedFiles.ToString();
            }
        }

        private void buttonOutputBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                _outputPath = folderBrowserDialog2.SelectedPath;
                textBox3.Text = _outputPath;
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
                        _fileList.Add(f);
                    }
                    foreach (string f in Directory.GetFiles(d, "*.flac"))
                    {
                        _fileList.Add(f);
                    }
                    DirSearch(d);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            _fileList.Clear();
            _rootPath = textBox1.Text;
            foreach (string f in Directory.GetFiles(_rootPath, "*.mp3"))
            {
                _fileList.Add(f);
            }
            foreach (string f in Directory.GetFiles(_rootPath, "*.flac"))
            {
                _fileList.Add(f);
            }
            DirSearch(_rootPath);
            textBox2.Text = _fileList.Count.ToString();
            labelTotalFiles.Text = textBox2.Text;
            buttonStartConversion.Enabled = true;
            buttonStop.Enabled = false;
            _currentProceesedFiles = 0;
            _skipped = 0;
            labelSkipped.Text = _skipped.ToString();
            labelCurrentProcessed.Text = _currentProceesedFiles.ToString();
        }

        private void ProcessFiles()
        {
            Action finishDel = delegate
                               {
                                   buttonStop.Enabled = false;
                                   buttonPause.Enabled = false;
                                   buttonStartConversion.Enabled = true;
                                   _stopped = false;
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
                             labelCurrentProcessed.Text = _currentProceesedFiles.ToString();
                             labelSkipped.Text = _skipped.ToString();
                             richTextBox1.AppendText(process.StandardError.ReadToEnd() + "\n");
                             richTextBox1.Focus();
                             richTextBox1.SelectionStart = richTextBox1.Text.Length;
                         };

            using (BassProxy proxy = new BassProxy())
            {
                foreach (string f in _fileList)
                {
                    if (_stopped)
                    {
                        Invoke(finishDel);
                        Thread.CurrentThread.Abort();
                    }

                    while (_pause)
                    {
                        if (_stopped)
                        {
                            Invoke(finishDel);
                            Thread.CurrentThread.Abort();
                        }
                        Thread.Sleep(1000);
                    }

                    TAG_INFO tags = null;

                    try
                    {
                        //Read Tags from the file
                        tags = proxy.GetTagInfoFromFile(f);
                    }
                    catch
                    {
                        _skipped++;
                        _currentProceesedFiles++;
                        Invoke(del);
                        continue;
                    }

                    //Compose the output name of the wav file
                    if (String.IsNullOrEmpty(tags.title) || tags.title.Length == 0)
                    {
                        //Skip file
                        _skipped++;
                        _currentProceesedFiles++;
                        Invoke(del);
                        continue;
                    }
                    string artist = "";
                    if (String.IsNullOrEmpty(tags.artist))
                    {
                        if (tags.composer == null)
                        {
                            _skipped++;
                            _currentProceesedFiles++;
                            Invoke(del);
                            continue;
                        }
                        artist = tags.composer;
                    }
                    else
                        artist = tags.artist;

                    string outfilename = tags.title + " + " + artist + ".wav";

                    string outfile = _outputPath + "\\" + outfilename;
                    string arguments = "-i \"" + f + "\" -ac 1 -ar " + _samplingRate + " -ab " +
                                       _bitRate + " \"" + outfile + "\"";

                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.StandardInput.Write("n");
                    _currentProceesedFiles++;
                    Invoke(del);
                }
            }
            Invoke(finishDel);
        }

        private void ButtonStartConversionClick(object sender, EventArgs e)
        {
            if (!Int32.TryParse(textBoxSamplingRate.Text, out _samplingRate))
                _samplingRate = 5512;
            if (!Int32.TryParse(textBoxBitRate.Text, out _bitRate))
                _bitRate = 64000;
            Thread thread = new Thread(ProcessFiles);
            thread.Start();
            _currentProceesedFiles = 0;
            labelCurrentProcessed.Text = _currentProceesedFiles.ToString();
            buttonPause.Enabled = true;
            buttonStartConversion.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            _pause = !_pause;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _stopped = true;
            buttonStop.Enabled = false;
        }
    }
}