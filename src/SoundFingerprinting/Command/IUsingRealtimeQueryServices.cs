namespace SoundFingerprinting.Command
{
    /// <summary>
    ///  Contract for model service selection during realtime query building.
    /// </summary>
    public interface IUsingRealtimeQueryServices
    {
        /// <summary>
        ///  Sets the model service that will be used as the data source during query.
        /// </summary>
        /// <param name="modelService">ModelService to query.</param>
        /// <returns>Realtime command.</returns>
        IRealtimeQueryCommand UsingServices(IModelService modelService);
    }
}