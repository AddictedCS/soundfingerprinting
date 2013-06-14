namespace SoundFingerprinting.SoundTools.NetworkTrainer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    using Encog.Engine.Network.Activation;
    using Encog.Neural.Networks.Layers;
    using Encog.Util;

    using SoundFingerprinting.Dao;
    using SoundFingerprinting.Hashing.NeuralHashing;
    using SoundFingerprinting.Hashing.NeuralHashing.NeuralTrainer;
    using SoundFingerprinting.SoundTools.Properties;

    public partial class WinNetworkTrainer : Form
    {
        /// <summary>
        ///   Default input counts
        /// </summary>
        private const int InputsCount = 4096;

        /// <summary>
        ///   Default output counts
        /// </summary>
        private const int OutputsCount = 10;

        /// <summary>
        ///   Default comma separator
        /// </summary>
        private const string CommaSeparator = ",";

        /// <summary>
        ///   Activation bipolar function
        /// </summary>
        private const string ActivationTanh = "ActivationTANH";

        /// <summary>
        ///   Activation sigmoid function
        /// </summary>
        private const string ActivationSigmoid = "ActivationSigmoid";

        private const string ActivationLinear = "ActivationLinear";

        /// <summary>
        ///   ModelService used to access the music storage
        /// </summary>
        private readonly IModelService modelService;

        /// <summary>
        ///   Types of Possible Activation Functions used in Training
        /// </summary>
        private readonly object[] dataProvider =
            {
                ActivationTanh,
                ActivationSigmoid,
                ActivationLinear
            };

        /// <summary>
        ///   Duration of the training
        /// </summary>
        private TimeSpan duration = new TimeSpan(0);

        /// <summary>
        ///   Logger
        /// </summary>
        private StreamWriter logger;

        /// <summary>
        ///   Network to train
        /// </summary>
        private Network netToTrain;

        /// <summary>
        ///   Network trainer
        /// </summary>
        private NetTrainer netTrainer;

        /// <summary>
        ///   Paused
        /// </summary>
        private bool paused;

        /// <summary>
        ///   Start time
        /// </summary>
        private DateTime startTime;

        private bool stopped;

        #region Constructors

        public WinNetworkTrainer(IModelService modelService)
        {
            this.modelService = modelService;
            InitializeComponent();
            Icon = Resources.Sound;
            _cmbActivationFunction.DataSource = new List<object>(dataProvider);
            _cmbActivationFunction.SelectedIndex = 0;
            _cmbActivationFunctionHidden.DataSource = new List<object>(dataProvider);
            _cmbActivationFunctionHidden.SelectedIndex = 0;
            _cmbActivationFunctionOutput.DataSource = new List<object>(dataProvider);
            _cmbActivationFunctionOutput.SelectedIndex = 0;
        }

        #endregion

        #region Event Handlers

        private void BtnSaveClick(object sender, EventArgs e)
        {
            string hiddenUnits = _tbHiddenUnits.Text;
            string correctOutputs = Convert.ToDouble(_tbCorrectOutputs.Text).ToString(CultureInfo.InvariantCulture);
            SaveFileDialog sfdSaveNetwork = new SaveFileDialog
                {
                    FileName = hiddenUnits + "_hidden_" + correctOutputs + Resources.NetworkExtension,
                    Filter = "(*" + Resources.NetworkExtension + ")|*" + Resources.NetworkExtension
                };
            if (sfdSaveNetwork.ShowDialog() == DialogResult.OK)
            {
                SerializeObject.Save(Path.GetFullPath(sfdSaveNetwork.FileName), netToTrain);
            }
        }

        /// <summary>
        ///   Create network to train
        /// </summary>
        private void BtnCreateClick(object sender, EventArgs e)
        {
            // Create Network
            int hiddenNeuronsCount = Convert.ToInt32(_tbHiddenUnits.Text);
            netToTrain = new Network();
            Type typeInput = GetActivationFunctionType(_cmbActivationFunction.SelectedItem.ToString());
            Type typeHidden = GetActivationFunctionType(_cmbActivationFunctionHidden.SelectedItem.ToString());
            Type typeOutput = GetActivationFunctionType(_cmbActivationFunctionOutput.SelectedItem.ToString());
            netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeInput), true, InputsCount)); /*4096*/
            netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeHidden), true, hiddenNeuronsCount));
            netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeOutput), false, OutputsCount)); /*10*/
            netToTrain.Structure.FinalizeStructure();
            netToTrain.Reset();
            _buttonStart.Enabled = true;
            _buttonSave.Enabled = true;
            if (_cbLog.Checked)
                logger = new StreamWriter(hiddenNeuronsCount + "_hidden_log.csv", true);
        }

        /// <summary>
        ///   Start training, fade all controls
        /// </summary>
        private void ButtonStartClick(object sender, EventArgs e)
        {
            netTrainer = new NetTrainer(modelService);
            _tbLearningRate.Enabled = false;
            _tbMomentum.Enabled = false;
            _buttonStart.Enabled = false;
            _buttonPause.Enabled = true;
            _buttonAbort.Enabled = true;
            _cbLog.Enabled = false;
            _tbHiddenUnits.Enabled = false;
            _cmbActivationFunction.Enabled = false;
            _cmbActivationFunctionHidden.Enabled = false;
            _cmbActivationFunctionOutput.Enabled = false;
            startTime = DateTime.Now;
            _tElapsedTime.Enabled = true;
            _tElapsedTime.Start();
            netTrainer.StartTrainingAsync(netToTrain, CallBack);
        }


        /// <summary>
        ///   Training callback
        /// </summary>
        /// <param name = "status">Training status</param>
        /// <param name = "correctOutputs">Correct outputs percentage</param>
        /// <param name = "error">Error RMS</param>
        /// <param name = "iteration">Iteration</param>
        private void CallBack(TrainingStatus status, double correctOutputs, double error, int iteration)
        {
            if (status == TrainingStatus.Finished)
            {
                _buttonPause.Enabled = false;
                _buttonAbort.Enabled = false;
                _cbLog.Enabled = true;
                _tbHiddenUnits.Enabled = true;
                _cmbActivationFunction.Enabled = true;
                _cmbActivationFunctionHidden.Enabled = true;
                _cmbActivationFunctionOutput.Enabled = true;
                try
                {
                    if (_cbLog.Checked)
                        logger.Close();
                    _tElapsedTime.Stop();
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.StackTrace);
                    MessageBox.Show(ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                MessageBox.Show(Resources.TrainingFinished, Resources.Finished, MessageBoxButtons.OK, MessageBoxIcon.Information);
                _buttonStart.Enabled = false;
                _buttonAbort.Enabled = false;
                _buttonPause.Enabled = false;
                return;
            }
            if (_cbLog.Enabled)
                logger.WriteLine(iteration + CommaSeparator + correctOutputs);
            Invoke(new Action(
                () =>
                {
                    _tbStatus.Text = status.ToString();
                    _textBoxIteration.Text = iteration.ToString();
                    _tbCorrectOutputs.Text = correctOutputs.ToString();
                    _textBoxError.Text = error.ToString();
                }));
        }

        /// <summary>
        ///   Abort training
        /// </summary>
        private void ButtonAbortClick(object sender, EventArgs e)
        {
            netTrainer.AbortTraining();
            stopped = true;
            _tElapsedTime.Stop();
            _buttonStart.Enabled = true;
            _buttonPause.Enabled = false;
            _buttonAbort.Enabled = false;
            _textBoxTimer.Text = duration.ToString();
        }

        /// <summary>
        ///   Timer tick callback
        /// </summary>
        private void Timer1Tick(object sender, EventArgs e)
        {
            if (!paused && !stopped)
            {
                duration = DateTime.Now - startTime;
                _textBoxTimer.Text = duration.ToString();
            }
        }

        /// <summary>
        ///   Pause training
        /// </summary>
        private void ButtonPauseClick(object sender, EventArgs e)
        {
            if (!paused)
            {
                paused = true;
                netTrainer.PauseTraining();
            }
            else
            {
                paused = false;
                netTrainer.ResumeTraining();
            }
        }

        /// <summary>
        ///   Selected index for input layer changed
        /// </summary>
        private void CmbActivationFunctionSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbActivationFunctionHidden.SelectedIndex == -1) return;
            _cmbActivationFunctionHidden.SelectedIndex = _cmbActivationFunction.SelectedIndex;
            _cmbActivationFunctionOutput.SelectedIndex = _cmbActivationFunction.SelectedIndex;
        }

        #endregion

        /// <summary>
        ///   Get activation function type based on string
        /// </summary>
        /// <param name = "type">Type of activation function</param>
        /// <returns>Type of activation function</returns>
        public static Type GetActivationFunctionType(string type)
        {
            if (type.Equals(ActivationTanh, StringComparison.InvariantCulture))
                return typeof (ActivationTANH);
            if (type.Equals(ActivationSigmoid, StringComparison.InvariantCulture))
                return typeof (ActivationSigmoid);
            if (type.Equals(ActivationLinear, StringComparison.InvariantCulture))
                return typeof (ActivationLinear);
            throw new ArgumentOutOfRangeException("No such type for activation function found: " + type);
        }
    }
}