// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Encog.Engine.Network.Activation;
using Encog.Neural.Networks.Layers;
using Encog.Util;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.NeuralHashing;
using Soundfingerprinting.NeuralHashing.NeuralTrainer;
using Soundfingerprinting.SoundTools.Properties;

namespace Soundfingerprinting.SoundTools.NetworkTrainer
{
    using Soundfingerprinting.Dao;

    /// <summary>
    ///   Network trainer GUI
    /// </summary>
    public partial class WinNetworkTrainer : Form
    {
        /// <summary>
        ///   Default input counts
        /// </summary>
        private const int INPUTS_COUNT = 4096;

        /// <summary>
        ///   Default output counts
        /// </summary>
        private const int OUTPUTS_COUNT = 10;

        /// <summary>
        ///   Default comma separator
        /// </summary>
        private const string COMMA_SEPARATOR = ",";

        /// <summary>
        ///   Activation bipolar function
        /// </summary>
        private const string ACTIVATION_TANH = "ActivationTANH";

        /// <summary>
        ///   Activation sigmoid function
        /// </summary>
        private const string ACTIVATION_SIGMOID = "ActivationSigmoid";

        private const string ACTIVATION_LINEAR = "ActivationLinear";

        /// <summary>
        ///   ModelService used to access the music storage
        /// </summary>
        private readonly ModelService _dalManager = new ModelService(ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString);


        /// <summary>
        ///   Types of Possible Activation Functions used in Training
        /// </summary>
        private readonly object[] _dataProvider =
            {
                ACTIVATION_TANH,
                ACTIVATION_SIGMOID,
                ACTIVATION_LINEAR
            };

        /// <summary>
        ///   Duration of the training
        /// </summary>
        private TimeSpan _duration = new TimeSpan(0);

        /// <summary>
        ///   Logger
        /// </summary>
        private StreamWriter _logger;

        /// <summary>
        ///   Network to train
        /// </summary>
        private Network _netToTrain;

        /// <summary>
        ///   Network trainer
        /// </summary>
        private NetTrainer _netTrainer;

        /// <summary>
        ///   Paused
        /// </summary>
        private bool _paused;

        /// <summary>
        ///   Start time
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        ///   Stop flag
        /// </summary>
        private bool _stopped;

        #region Constructors

        /// <summary>
        ///   Parameter less constructor
        /// </summary>
        public WinNetworkTrainer()
        {
            InitializeComponent();
            Icon = Resources.Sound;
            _cmbActivationFunction.DataSource = new List<Object>(_dataProvider);
            _cmbActivationFunction.SelectedIndex = 0;
            _cmbActivationFunctionHidden.DataSource = new List<Object>(_dataProvider);
            _cmbActivationFunctionHidden.SelectedIndex = 0;
            _cmbActivationFunctionOutput.DataSource = new List<Object>(_dataProvider);
            _cmbActivationFunctionOutput.SelectedIndex = 0;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///   Save the network
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            string hiddenUnits = _tbHiddenUnits.Text;
            string correctOutputs = Convert.ToDouble(_tbCorrectOutputs.Text).ToString();
            SaveFileDialog sfdSaveNetwork = new SaveFileDialog
                                            {
                                                FileName = hiddenUnits + "_hidden_" + correctOutputs + Resources.NetworkExtension,
                                                Filter = "(*" + Resources.NetworkExtension + ")|*" + Resources.NetworkExtension
                                            };
            if (sfdSaveNetwork.ShowDialog() == DialogResult.OK)
            {
                SerializeObject.Save(Path.GetFullPath(sfdSaveNetwork.FileName), _netToTrain);
            }
        }

        /// <summary>
        ///   Create network to train
        /// </summary>
        private void BtnCreateClick(object sender, EventArgs e)
        {
            // Create Network
            int hiddenNeuronsCount = Convert.ToInt32(_tbHiddenUnits.Text);
            _netToTrain = new Network();
            Type typeInput = GetActivationFunctionType(_cmbActivationFunction.SelectedItem.ToString());
            Type typeHidden = GetActivationFunctionType(_cmbActivationFunctionHidden.SelectedItem.ToString());
            Type typeOutput = GetActivationFunctionType(_cmbActivationFunctionOutput.SelectedItem.ToString());
            _netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeInput), true, INPUTS_COUNT)); /*4096*/
            _netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeHidden), true, hiddenNeuronsCount));
            _netToTrain.AddLayer(new BasicLayer((IActivationFunction) Activator.CreateInstance(typeOutput), false, OUTPUTS_COUNT)); /*10*/
            _netToTrain.Structure.FinalizeStructure();
            _netToTrain.Reset();
            _buttonStart.Enabled = true;
            _buttonSave.Enabled = true;
            if (_cbLog.Checked)
                _logger = new StreamWriter(hiddenNeuronsCount + "_hidden_log.csv", true);
        }

        /// <summary>
        ///   Start training, fade all controls
        /// </summary>
        private void ButtonStartClick(object sender, EventArgs e)
        {
            _netTrainer = new NetTrainer(_dalManager);
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
            _startTime = DateTime.Now;
            _tElapsedTime.Enabled = true;
            _tElapsedTime.Start();
            _netTrainer.StartTrainingAsync(_netToTrain, CallBack);
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
                        _logger.Close();
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
                _logger.WriteLine(iteration + COMMA_SEPARATOR + correctOutputs);
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
            _netTrainer.AbortTraining();
            _stopped = true;
            _tElapsedTime.Stop();
            _buttonStart.Enabled = true;
            _buttonPause.Enabled = false;
            _buttonAbort.Enabled = false;
            _textBoxTimer.Text = _duration.ToString();
        }

        /// <summary>
        ///   Timer tick callback
        /// </summary>
        private void Timer1Tick(object sender, EventArgs e)
        {
            if (!_paused && !_stopped)
            {
                _duration = DateTime.Now - _startTime;
                _textBoxTimer.Text = _duration.ToString();
            }
        }

        /// <summary>
        ///   Pause training
        /// </summary>
        private void ButtonPauseClick(object sender, EventArgs e)
        {
            if (!_paused)
            {
                _paused = true;
                _netTrainer.PauseTraining();
            }
            else
            {
                _paused = false;
                _netTrainer.ResumeTraining();
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
            if (type.Equals(ACTIVATION_TANH, StringComparison.InvariantCulture))
                return typeof (ActivationTANH);
            if (type.Equals(ACTIVATION_SIGMOID, StringComparison.InvariantCulture))
                return typeof (ActivationSigmoid);
            if (type.Equals(ACTIVATION_LINEAR, StringComparison.InvariantCulture))
                return typeof (ActivationLinear);
            throw new ArgumentOutOfRangeException("No such type for activation function found: " + type);
        }
    }
}