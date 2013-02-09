namespace Soundfingerprinting.SoundTools
{
    using System;
    using System.Windows.Forms;

    using Soundfingerprinting.SoundTools.BassResampler;
    using Soundfingerprinting.SoundTools.DbFiller;
    using Soundfingerprinting.SoundTools.DI;
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

    public partial class WinMain : Form
    {
        private readonly IDependencyResolver dependencyResolver;

        public WinMain(IDependencyResolver dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
            InitializeComponent();
            Icon = Resources.Sound;
        }

        private void FillDatabaseToolStripClick(object sender, EventArgs e)
        {
            WinDbFiller filler = dependencyResolver.Get<WinDbFiller>();
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
            WinCheckHashBins queryDatabase = dependencyResolver.Get<WinCheckHashBins>();
            queryDatabase.Show();
        }

        /// <summary>
        ///   Min-Hash permutations generator
        /// </summary>
        private void MinHashPermGeneratorToolStripClick(object sender, EventArgs e)
        {
            WinPermGenerator win = dependencyResolver.Get<WinPermGenerator>();
            win.Show();
        }

        /// <summary>
        ///   Win Drawing tool
        /// </summary>
        private void AudioToolToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinDrawningTool win = dependencyResolver.Get<WinDrawningTool>();
            win.Show();
        }

        /// <summary>
        ///   Permute files and select them
        /// </summary>
        private void RandomPermutationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFilePermutation win = dependencyResolver.Get<WinFilePermutation>();
            win.ShowDialog();
        }

        /// <summary>
        ///   FFMpeg resample
        /// </summary>
        private void FFMpegResamplerToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFfMpegResampler win = dependencyResolver.Get<WinFfMpegResampler>();
            win.Show();
        }

        /// <summary>
        ///   Train neural networks
        /// </summary>
        private void BtnTrainNetworksClick(object sender, EventArgs e)
        {
            WinNetworkTrainer win = dependencyResolver.Get<WinNetworkTrainer>();
            win.Show();
        }

        /// <summary>
        ///   Hash fingerprints
        /// </summary>
        private void BtnHashFingersClick(object sender, EventArgs e)
        {
            WinEnsembleHash win = dependencyResolver.Get<WinEnsembleHash>();
            win.Show();
        }

        /// <summary>
        ///   Train networks
        /// </summary>
        private void TrainToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinNetworkTrainer win = dependencyResolver.Get<WinNetworkTrainer>();
            win.Show();
        }

        /// <summary>
        ///   Hash fingerprints
        /// </summary>
        private void HashFingerprintsToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinEnsembleHash win = dependencyResolver.Get<WinEnsembleHash>();
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
            WinBassResampler win = dependencyResolver.Get<WinBassResampler>();
            win.Show();
        }

        private void SimilarityCalculationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinMisc win = dependencyResolver.Get<WinMisc>();
            win.Show();
        }

        private void WaveletDecompositionToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinHaarWavelet win = dependencyResolver.Get<WinHaarWavelet>();
            win.Show();
        }
    }
}