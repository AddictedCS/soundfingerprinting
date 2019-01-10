namespace SoundFingerprinting.Command
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SoundFingerprinting.Data;

    public class ExecutedFingerprintCommand : IFingerprintCommand
    {
        private readonly List<HashedFingerprint> result;
        
        public ExecutedFingerprintCommand(List<HashedFingerprint> result)
        {
            this.result = result;
        }

        public Task<List<HashedFingerprint>> Hash()
        {
            return Task.FromResult(result);
        }
    }
}