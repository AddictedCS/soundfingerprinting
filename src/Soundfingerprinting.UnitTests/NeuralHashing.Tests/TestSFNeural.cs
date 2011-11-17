// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Soundfingerprinting.UnitTests.NeuralHashing.Tests
{
    [TestClass]
    public class TestSFNeural
    {
    }

    //{
    //    /*
    //     * Neuron test from SFNeural
    //     */

    //    [TestMethod]
    //    public void NeuronTest()
    //    {
    //        int inputsCount = 4096;
    //        Neuron neuron0 = new Neuron(inputsCount, new SigmoidFunction());
    //        neuron0.Randomize();
    //        Neuron neuron1 = new Neuron(inputsCount, new SigmoidFunction());
    //        neuron1.Randomize();
    //        Assert.AreNotEqual(neuron0.Bias, neuron1.Bias);
    //        Assert.AreEqual(inputsCount, neuron0.InputsCount);
    //        Assert.AreEqual(inputsCount, neuron1.InputsCount);
    //        float[] inputArray = TestUtilities.GenerateRandomFloatArray(inputsCount);
    //        float result = neuron0.Compute(inputArray);
    //        Assert.AreEqual(true, result >= 0 && result <= 1);
    //        float firedValue = inputArray.Sum() - neuron0.Bias;
    //        float output = neuron0.ActivationFunction.Output(firedValue);
    //        Assert.AreEqual(true, Math.Abs(result - output) < 0.001);
    //        Assert.AreEqual(result, neuron0.Output);
    //        BinaryFormatter formatter = new BinaryFormatter();

    //        string path = Path.GetTempPath() + @"\serialization.xml";
    //        using (FileStream stream = new FileStream(path, FileMode.Create))
    //        {
    //            formatter.Serialize(stream, neuron0);
    //        }

    //        Assert.AreEqual(true, File.Exists(path));
    //        Assert.AreEqual(true, (new FileInfo(path)).Length > 0);

    //        using (FileStream stream = new FileStream(path, FileMode.Open))
    //        {
    //            Neuron deserialized = (Neuron)formatter.Deserialize(stream);
    //            Assert.AreEqual(neuron0.ActivationFunction.GetType(), deserialized.ActivationFunction.GetType());
    //            Assert.AreEqual(neuron0.InputsCount, deserialized.InputsCount);
    //            Assert.AreEqual(neuron0.Output, deserialized.Output);
    //            Assert.AreEqual(neuron0.Bias, deserialized.Bias);
    //            for (int i = 0; i < deserialized.InputsCount; i++)
    //            {
    //                Assert.AreEqual(true, neuron0[i] - deserialized[i] < 0.00001);
    //            }
    //        }
    //        File.Delete(path);
    //    }

    //    /*
    //     * Layer test from SFNeural
    //     */

    //    [TestMethod]
    //    public void LayerTest()
    //    {
    //        Layer layer = new Layer(10, 100, new BipolarSigmoidFunction());
    //        Assert.AreEqual(10, layer.NeuronsCount);
    //        layer.Randomize();
    //        Assert.AreEqual(100, layer.InputsCount);

    //        BinaryFormatter formatter = new BinaryFormatter();

    //        string path = Path.GetTempPath() + @"\serializationLayer.xml";
    //        using (FileStream stream = new FileStream(path, FileMode.Create))
    //        {
    //            formatter.Serialize(stream, layer);
    //        }

    //        Assert.AreEqual(true, File.Exists(path));
    //        Assert.AreEqual(true, (new FileInfo(path)).Length > 0);

    //        using (FileStream stream = new FileStream(path, FileMode.Open))
    //        {
    //            Layer deserialized = (Layer)formatter.Deserialize(stream);
    //            Assert.AreEqual(layer.ActivationFunction.GetType(), deserialized.ActivationFunction.GetType());
    //            Assert.AreEqual(layer.InputsCount, deserialized.InputsCount);
    //            Assert.AreEqual(layer.Output, deserialized.Output);

    //            for (int i = 0; i < deserialized.NeuronsCount; i++)
    //            {
    //                for (int j = 0; j < deserialized[i].InputsCount; j++)
    //                {
    //                    Assert.AreEqual(true, layer[i][j] - deserialized[i][j] < 0.00001);
    //                    Assert.AreEqual(layer[i].ActivationFunction.GetType().FullName, deserialized[i].ActivationFunction.GetType().FullName);
    //                    Assert.AreEqual(layer[i].InputsCount, deserialized[i].InputsCount);
    //                    Assert.AreEqual(layer[i].Output, deserialized[i].Output);
    //                    Assert.AreEqual(true, (layer[i].Bias - deserialized[i].Bias) < 0.00001);
    //                }
    //            }
    //        }
    //        File.Delete(path);
    //    }

    //    /*
    //     * Network test from SFNeural
    //     */

    //    [TestMethod]
    //    public void NetworkTest()
    //    {
    //        Network network = new Network(100, 10, 5, 10);
    //        Assert.AreEqual(3, network.LayersCount);
    //        network.Randomize();

    //        BinaryFormatter formatter = new BinaryFormatter();

    //        string path = Path.GetTempPath() + @"\serializationNetwork.xml";
    //        using (FileStream stream = new FileStream(path, FileMode.Create))
    //        {
    //            formatter.Serialize(stream, network);
    //        }

    //        Assert.AreEqual(true, File.Exists(path));
    //        Assert.AreEqual(true, (new FileInfo(path)).Length > 0);

    //        using (FileStream stream = new FileStream(path, FileMode.Open))
    //        {
    //            Network deserialized = (Network)formatter.Deserialize(stream);
    //            Assert.AreEqual(network.LayersCount, deserialized.LayersCount);

    //            for (int i = 0; i < deserialized.LayersCount; i++)
    //            {
    //                for (int j = 0; j < deserialized[i].NeuronsCount; j++)
    //                {
    //                    for (int k = 0; k < deserialized[i][j].InputsCount; k++)
    //                        Assert.AreEqual(true, network[i][j][k] - deserialized[i][j][k] < 0.00001);
    //                    Assert.AreEqual(network[i][j].ActivationFunction.GetType().FullName, deserialized[i][j].ActivationFunction.GetType().FullName);
    //                    Assert.AreEqual(network[i][j].InputsCount, deserialized[i][j].InputsCount);
    //                    Assert.AreEqual(network[i][j].Output, deserialized[i][j].Output);
    //                    Assert.AreEqual(true, (network[i][j].Bias - deserialized[i][j].Bias) < 0.00001);
    //                }
    //            }
    //        }
    //        File.Delete(path);
    //    }

    //    /*
    //     * Neuron general test
    //     */

    //    [TestMethod]
    //    public void NeuronGeneralTest()
    //    {
    //        int inputsCount = 1;
    //        Neuron n = new Neuron(inputsCount);
    //        Assert.AreEqual(typeof(SigmoidFunction), n.ActivationFunction.GetType());
    //        Assert.AreEqual(inputsCount, n.InputsCount);
    //        n.InputsCount = inputsCount * 2;
    //        Assert.AreEqual(inputsCount * 2, n.InputsCount);
    //        n.Randomize();
    //        Assert.AreNotEqual(0, n.Bias);
    //        n.Bias = inputsCount;
    //        Assert.AreEqual(inputsCount, n.Bias);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(ArgumentException))]
    //    public void NeuronComputeExceptionTest()
    //    {
    //        int inputsCount = 1;
    //        Neuron n = new Neuron(inputsCount);
    //        n.Compute(new float[inputsCount * 2]);
    //    }

    //    [TestMethod]
    //    public void NetworkGeneralTest()
    //    {
    //        int inputsCount = 1;
    //        int neuronFirstLayer = 4096;
    //        int neuronSecondLayer = 5;
    //        int neuronThirdLayer = 10;
    //        int numberOfLayers = 3;
    //        Network n = new Network(inputsCount, neuronFirstLayer, neuronSecondLayer, neuronThirdLayer);
    //        Assert.AreEqual(numberOfLayers, n.LayersCount);
    //        Assert.AreEqual(neuronFirstLayer, n[0].NeuronsCount);
    //        Assert.AreEqual(inputsCount, n[0].InputsCount);

    //        Assert.AreEqual(neuronSecondLayer, n[1].NeuronsCount);
    //        Assert.AreEqual(neuronFirstLayer, n[1].InputsCount);

    //        Assert.AreEqual(neuronThirdLayer, n[2].NeuronsCount);
    //        Assert.AreEqual(neuronSecondLayer, n[2].InputsCount);

    //        Assert.AreEqual(typeof(SigmoidFunction), n[0].ActivationFunction.GetType());
    //        Assert.AreEqual(typeof(SigmoidFunction), n[1].ActivationFunction.GetType());
    //        Assert.AreEqual(typeof(SigmoidFunction), n[2].ActivationFunction.GetType());

    //        n.SetActivationFunction(new StepFunction(0.5f));
    //        for (int i = 0; i < numberOfLayers; i++)
    //        {
    //            Assert.AreEqual(typeof(StepFunction), n[i].ActivationFunction.GetType());
    //            for (int j = 0; j < n[i].NeuronsCount; j++)
    //            {
    //                Assert.AreEqual(typeof(StepFunction), n[i][j].ActivationFunction.GetType());
    //                for (int k = 0; k < n[i][j].InputsCount; k++)
    //                {
    //                    Assert.AreNotEqual(0f, n[i][j][k]);
    //                }
    //            }
    //        }
    //    }

    //    [TestMethod]
    //    public void NetworkSerializationTest()
    //    {
    //        int inputsCount = 1;
    //        int neuronFirstLayer = 4096;
    //        int neuronSecondLayer = 5;
    //        int neuronThirdLayer = 10;
    //        int numberOfLayers = 3;
    //        Network n = new Network(inputsCount, neuronFirstLayer, neuronSecondLayer, neuronThirdLayer);
    //        Assert.AreEqual(numberOfLayers, n.LayersCount);
    //        Assert.AreEqual(neuronFirstLayer, n[0].NeuronsCount);
    //        Assert.AreEqual(inputsCount, n[0].InputsCount);
    //        string path = Path.GetTempPath() + "\\" + @"serializednetwork.data";
    //        n.Randomize();
    //        n.Save(path);
    //        Assert.AreNotEqual(0, (new FileInfo(path)).Length);
    //        Network n2 = Network.Load(path);
    //        Assert.AreEqual(numberOfLayers, n2.LayersCount);
    //        Assert.AreEqual(neuronFirstLayer, n2[0].NeuronsCount);
    //        Assert.AreEqual(inputsCount, n2[0].InputsCount);


    //        for (int i = 0; i < numberOfLayers; i++)
    //        {
    //            Assert.AreEqual(n[i].ActivationFunction.GetType(), n2[i].ActivationFunction.GetType());
    //            Assert.AreEqual(n[i].NeuronsCount, n2[i].NeuronsCount);

    //            for (int j = 0; j < n[i].NeuronsCount; j++)
    //            {
    //                Assert.AreEqual(n[i][j].ActivationFunction.GetType(), n2[i][j].ActivationFunction.GetType());
    //                Assert.AreEqual(n[i][j].InputsCount, n2[i][j].InputsCount);
    //                for (int k = 0; k < n[i][j].InputsCount; k++)
    //                {
    //                    Assert.AreEqual(true, n[i][j][k] - n2[i][j][k] < 0.00001);
    //                }
    //            }
    //        }
    //        File.Delete(path);
    //    }


    //    [TestMethod]
    //    public void TestXOR()
    //    {
    //        float[][] inputs = {
    //                               new float[] {0, 0},
    //                               new float[] {0, 1},
    //                               new float[] {1, 0},
    //                               new float[] {1, 1}
    //                           };
    //        float[][] outputs = {
    //                                new float[] {0},
    //                                new float[] {1},
    //                                new float[] {1},
    //                                new float[] {0}
    //                            };


    //        Network network = new Network(new SigmoidFunction(), 2, 5, 1);

    //        BackPropagationLearning learner = new BackPropagationLearning(network) { LearningRate = 0.4f, Momentum = 0.7f };
    //        float error = 0.0f;
    //        DateTime time = DateTime.Now;
    //        for (int i = 0; i < 10000; i++)
    //        {
    //            error = learner.LearnEpoch(inputs, outputs);
    //            Trace.WriteLine("Error: " + error + "\nIteration: " + i + "\n");
    //        }
    //        TimeSpan totalTime = DateTime.Now - time;

    //        Trace.WriteLine("Input: 0.0 0.0  Output: " + network.Compute(new float[] { 0, 0 })[0]);
    //        Trace.WriteLine("Input: 0.0 1.0  Output: " + network.Compute(new float[] { 0, 1 })[0]);
    //        Trace.WriteLine("Input: 1.0 0.0  Output: " + network.Compute(new float[] { 1, 0 })[0]);
    //        Trace.WriteLine("Input: 1.0 1.0  Output: " + network.Compute(new float[] { 1, 1 })[0]);
    //        Trace.WriteLine("Time elapsed: " + totalTime);
    //        Assert.IsTrue(error <= 0.3f);
    //    }

    //    [TestMethod]
    //    public void TestSaveAndLoad()
    //    {
    //        float[][] inputs = {
    //                               new float[] {0, 0},
    //                               new float[] {0, 1},
    //                               new float[] {1, 0},
    //                               new float[] {1, 1}
    //                           };
    //        float[][] outputs = {
    //                                new float[] {0},
    //                                new float[] {1},
    //                                new float[] {1},
    //                                new float[] {0}
    //                            };


    //        Network network = new Network(new SigmoidFunction(), 2, 5, 1);

    //        BackPropagationLearning learner = new BackPropagationLearning(network) { LearningRate = 0.4f, Momentum = 0.7f };
    //        float error = 0.0f;

    //        for (int i = 0; i < 10000; i++)
    //        {
    //            error = learner.LearnEpoch(inputs, outputs);
    //        }
    //        Assert.IsTrue(error <= 0.3f);
    //        network.Save("testNetwork.ntwrk");

    //        Network loadedNetwork = Network.Load("testNetwork.ntwrk");

    //        Assert.IsTrue(network.Compute(new float[] { 0, 0 })[0] <= 0.05);
    //        Assert.IsTrue(network.Compute(new float[] { 1, 1 })[0] <= 0.05);
    //        Assert.IsTrue(network.Compute(new float[] { 0, 1 })[0] >= 0.95);
    //        Assert.IsTrue(network.Compute(new float[] { 1, 0 })[0] >= 0.95);
    //    }
}