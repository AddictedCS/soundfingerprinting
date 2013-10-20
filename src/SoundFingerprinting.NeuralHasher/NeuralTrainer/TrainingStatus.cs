namespace SoundFingerprinting.NeuralHasher.NeuralTrainer
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