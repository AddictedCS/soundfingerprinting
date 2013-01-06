namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal
{
    internal class TargetOn : ITargetOn
    {
        public IWithConfiguration On(string pathToAudioFile)
        {
            return new WithConfiguration(new WorkUnitParameterObject { PathToAudioFile = pathToAudioFile });
        }

        public IWithConfiguration On(float[] audioSamples)
        {
            return new WithConfiguration(new WorkUnitParameterObject { AudioSamples = audioSamples });
        }

        public IWithConfiguration On(string pathToAudioFile, int millisecondsToProcess, int startAtMillisecond)
        {
            return
                new WithConfiguration(
                    new WorkUnitParameterObject()
                        {
                            PathToAudioFile = pathToAudioFile,
                            MillisecondsToProcess = millisecondsToProcess,
                            StartAtMilliseconds = startAtMillisecond
                        });
        }

        public IWithConfiguration On(float[] audioSamples, int millisecondsToProcess, int startAtMillisecond)
        {
            return
                new WithConfiguration(
                    new WorkUnitParameterObject()
                        {
                            AudioSamples = audioSamples,
                            MillisecondsToProcess = millisecondsToProcess,
                            StartAtMilliseconds = startAtMillisecond
                        });
        }
    }
}