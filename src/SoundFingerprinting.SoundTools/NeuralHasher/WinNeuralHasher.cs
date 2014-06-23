namespace SoundFingerprinting.SoundTools.NeuralHasher
{
    using System;
    using System.Windows.Forms;

    using Encog.Engine.Network.Activation;

    using SoundFingerprinting.NeuralHasher;

    public partial class WinNeuralHasher : Form
    {
        private readonly INetworkTrainer trainer;

        public WinNeuralHasher(INetworkTrainer trainer)
        {
            this.trainer = trainer;
            InitializeComponent();
        }

        private void BtnTrainClick(object sender, EventArgs e)
        {
            int hiddenLayerCount = (int)nudHiddenLayers.Value;
            int outputCount = (int)nudOutputCount.Value;
            var activationFunction = new ActivationTANH();

            var networkConfiguration = new NetworkConfiguration()
                {
                    ActivationFunction = activationFunction,
                    HiddenLayerCount = hiddenLayerCount,
                    OutputCount = outputCount
                };

            trainer.Train(
                networkConfiguration,
                (int)Math.Pow(2, outputCount),
                new[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
                (status, outputs, rate, iteration) => { }).ContinueWith(
                    task =>
                        {
                            var network = task.Result;
                            network.Save("network.encog");
                        });
        }
    }
}
