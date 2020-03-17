namespace SoundFingerprinting.Command
{
    using System.Threading.Tasks;
    using SoundFingerprinting.Data;

    public class ExecutedFingerprintCommand : IFingerprintCommand
    {
        private readonly Hashes result;
        
        public ExecutedFingerprintCommand(Hashes result)
        {
            this.result = result;
        }

        public Task<Hashes> Hash()
        {
            return Task.FromResult(result);
        }
    }
}