namespace SoundFingerprinting.SoundTools
{
    using System;
    using System.Windows.Forms;

    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.SoundTools.BassResampler;
    using SoundFingerprinting.SoundTools.DbFiller;
    using SoundFingerprinting.SoundTools.DrawningTool;
    using SoundFingerprinting.SoundTools.FFMpegResampler;
    using SoundFingerprinting.SoundTools.FilePermutations;
    using SoundFingerprinting.SoundTools.Misc;
    using SoundFingerprinting.SoundTools.PermutationGenerator;
    using SoundFingerprinting.SoundTools.Properties;
    using SoundFingerprinting.SoundTools.QueryDb;
    using SoundFingerprinting.SoundTools.WaveletDecomposition;

    public partial class WinMain : Form
    {
        public WinMain()
        {
            InitializeComponent();
            Icon = Resources.Sound;
        }

        private void FillDatabaseToolStripClick(object sender, EventArgs e)
        {
            WinDbFiller filler = DependencyResolver.Current.Get<WinDbFiller>();
            filler.Show();
        }

        private void BtnFillDatabaseClick(object sender, EventArgs e)
        {
            FillDatabaseToolStripClick(sender, e);
        }

        private void BtnQueryDbClick(object sender, EventArgs e)
        {
            QueryDatabaseToolStripClick(sender, e);
        }

        private void QueryDatabaseToolStripClick(object sender, EventArgs e)
        {
            WinCheckHashBins queryDatabase = DependencyResolver.Current.Get<WinCheckHashBins>();
            queryDatabase.Show();
        }

        private void MinHashPermGeneratorToolStripClick(object sender, EventArgs e)
        {
            WinPermGenerator win = DependencyResolver.Current.Get<WinPermGenerator>();
            win.Show();
        }

        private void AudioToolToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinDrawningTool win = DependencyResolver.Current.Get<WinDrawningTool>();
            win.Show();
        }

        private void RandomPermutationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFilePermutation win = DependencyResolver.Current.Get<WinFilePermutation>();
            win.ShowDialog();
        }

        private void FFMpegResamplerToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinFfMpegResampler win = DependencyResolver.Current.Get<WinFfMpegResampler>();
            win.Show();
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void BassResamplerToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinBassResampler win = DependencyResolver.Current.Get<WinBassResampler>();
            win.Show();
        }

        private void SimilarityCalculationToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinMisc win = DependencyResolver.Current.Get<WinMisc>();
            win.Show();
        }

        private void WaveletDecompositionToolStripMenuItemClick(object sender, EventArgs e)
        {
            WinHaarWavelet win = DependencyResolver.Current.Get<WinHaarWavelet>();
            win.Show();
        }
    }
}