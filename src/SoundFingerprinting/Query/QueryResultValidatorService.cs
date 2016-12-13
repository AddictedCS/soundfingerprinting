namespace SoundFingerprinting.Query
{
    using SoundFingerprinting.Audio;
    using SoundFingerprinting.Builder;
    using SoundFingerprinting.Infrastructure;
    using SoundFingerprinting.Strides;

    public class QueryResultValidatorService
    {
        private readonly IQueryCommandBuilder queryCommandBuilder;

        public QueryResultValidatorService() : this(DependencyResolver.Current.Get<IQueryCommandBuilder>())
        {
        }

        internal QueryResultValidatorService(IQueryCommandBuilder queryCommandBuilder)
        {
            this.queryCommandBuilder = queryCommandBuilder;
        }

        public ResultEntry Validate(
            ResultEntry result,
            IStride validationStride,
            string pathToAudioFile,
            IModelService modelService,
            IAudioService audioService)
        {
            double startAt = result.TrackStartsAt, length = result.QueryLength - result.TrackStartsAt;

            if (startAt + result.Track.Length < result.QueryLength)
            {
                length = result.Track.Length;
            }

            var newResult = queryCommandBuilder.BuildQueryCommand()
                                               .From(pathToAudioFile, length, startAt)
                                               .WithFingerprintConfig(config => config.Stride = validationStride)
                                               .UsingServices(modelService, audioService)
                                               .Query()
                                               .Result;

            if (!newResult.ContainsMatches)
            {
                return result;
            }

            var newEntry = newResult.BestMatch;
            if (newEntry.Confidence > result.Confidence)
            {
                return new ResultEntry(
                    newEntry.Track,
                    newEntry.QueryMatchStartsAt,
                    newEntry.QueryMatchLength,
                    newEntry.TrackMatchStartsAt,
                    newEntry.TrackStartsAt + result.TrackStartsAt,
                    newEntry.Confidence,
                    newEntry.HammingSimilaritySum,
                    newEntry.QueryLength,
                    newEntry.BestMatch);
            }

            return result;
        }
    }
}
