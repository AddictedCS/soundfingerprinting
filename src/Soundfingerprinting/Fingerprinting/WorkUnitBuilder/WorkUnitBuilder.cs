namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
    using Soundfingerprinting.Fingerprinting.WorkUnitBuilder.Internal;

    public class WorkUnitBuilder : IWorkUnitBuilder
    {
        public ITargetOn BuildWorkUnit()
        {
            return new TargetOn();
        }
    }
}