// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
namespace Soundfingerprinting.NeuralHashing.NeuralTrainer
{
    public enum TrainingStatus
    {
        FillingStandardInputs,
        RunningDynamicEpoch,
        OutputReordering,
        FixedTraining,
        Finished,
        Paused,
        Aborted,
        Exception
    }
}