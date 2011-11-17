// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Windows.Forms;
using Soundfingerprinting.SoundTools.BassResampler;
using Soundfingerprinting.SoundTools.DbFiller;
using Soundfingerprinting.SoundTools.DrawningTool;
using Soundfingerprinting.SoundTools.FFMpegResampler;
using Soundfingerprinting.SoundTools.FilePermutations;
using Soundfingerprinting.SoundTools.Misc;
using Soundfingerprinting.SoundTools.NetworkEnsembling;
using Soundfingerprinting.SoundTools.NetworkTrainer;
using Soundfingerprinting.SoundTools.PermutationGenerator;
using Soundfingerprinting.SoundTools.Properties;
using Soundfingerprinting.SoundTools.QueryDb;
using Soundfingerprinting.SoundTools.WaveletDecomposition;

namespace Soundfingerprinting.SoundTools
{
    public partial class WinMain : Form
    {
        /// <summary>
        ///   Main window constructor
        /// </summary>
        public WinMain()
        {
            InitializeComponent();
            Icon = Resources.Sound;
        }

        /// <summary>
        ///   Db filler called
        /// </summary>
        private void FillDatabaseToolStripClick(object sender, EventArgs e)
        {
            WinDBFiller filler = new WinDBFiller();
            filler.Show();
        }

        /// <summary>
        ///   Db filler called
        /// </summary>
        private void BtnFillDatabaseClick(object sender, EventArgs e)
        {
            FillDatabaseToolStripClick(sender, e);
        }

        /// <summary>
        ///   Query database window
        /// </summary>
        private void BtnQueryDbClick(object sender, EventArgs e)
        {
            QueryDatabaseToolStripClick(sender, e);
        }

        /// <summary>
        ///   Query database window
        /// </summary>
        private void QueryDatabaseToolStripClick(object sender, EventArgs e)
        {
            WinCheckHashBins queryDatabase = new WinCheckHashBins();
            queryDatabase.Show();
        }

        /// <summary>
        ///   Min-Hash permutations generator
        /// </summary>
        private void MinHashPermGeneratorToolStripClick(object sender, EventArgs e)
        {
            WinPermGenerator win = new WinPermGenerator();
            win.Show();
        }

        /// <summary>
        ///   Win Drawing tool
        /// </summary>
        private void AudioToolToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinDrawningTool win = new WinDrawningTool();
            win.Show();
        }

        /// <summary>
        ///   Permute files and select them
        /// </summary>
        private void RandomPermutationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFilePermutation win = new WinFilePermutation();
            win.ShowDialog();
        }

        /// <summary>
        ///   FFMpeg resample
        /// </summary>
        private void FFMpegResamplerToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFfMpegResampler win = new WinFfMpegResampler();
            win.Show();
        }

        /// <summary>
        ///   Train neural networks
        /// </summary>
        private void BtnTrainNetworksClick(object sender, EventArgs e)
        {
            WinNetworkTrainer win = new WinNetworkTrainer();
            win.Show();
        }

        /// <summary>
        ///   Hash fingerprints
        /// </summary>
        private void BtnHashFingersClick(object sender, EventArgs e)
        {
            WinEnsembleHash win = new WinEnsembleHash();
            win.Show();
        }

        /// <summary>
        ///   Train networks
        /// </summary>
        private void TrainToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinNetworkTrainer win = new WinNetworkTrainer();
            win.Show();
        }

        /// <summary>
        ///   Hash fingerprints
        /// </summary>
        private void HashFingerprintsToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinEnsembleHash win = new WinEnsembleHash();
            win.Show();
        }

        /// <summary>
        ///   Close window
        /// </summary>
        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void BassResamplerToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinBassResampler win = new WinBassResampler();
            win.Show();
        }

        private void SimilarityCalculationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinMisc win = new WinMisc();
            win.Show();
        }

        private void WaveletDecompositionToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinHaarWavelet win = new WinHaarWavelet();
            win.Show();
        }
    }
}