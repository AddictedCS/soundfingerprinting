namespace SoundFingerprinting.Command;

using System.Linq;
using SoundFingerprinting.Query;

/// <summary>
///  Chained result entry filter.
/// </summary>
public class ChainedRealtimeEntryFilter : IRealtimeResultEntryFilter
{
    private readonly IRealtimeResultEntryFilter[] chain;

    /// <summary>
    ///  Initializes a new instance of the <see cref="ChainedRealtimeEntryFilter"/> class.
    /// </summary>
    /// <param name="chain">Chain of realtime result entry filters.</param>
    public ChainedRealtimeEntryFilter(IRealtimeResultEntryFilter[] chain)
    {
        this.chain = chain;
    }
    
    /// <inheritdoc cref="IRealtimeResultEntryFilter.Pass" />
    /// <remarks>
    ///  Example 1. <br />
    ///  minRelativeCoverage = 0.4 <see cref="TrackRelativeCoverageEntryFilter"/>, minTrackLength = 10.0 <see cref="TrackCoverageLengthEntryFilter"/><br/>
    ///  If <see cref="ResultEntry.TrackRelativeCoverage"/> is at least 0.4 or <see cref="ResultEntry.TrackCoverageWithPermittedGapsLength"/> is bigger than 10 seconds, the filter will pass.<br/>
    /// </remarks>
    public bool Pass(AVResultEntry entry, bool canContinueInTheNextQuery)
    {
        return chain.Any(filter => filter.Pass(entry, canContinueInTheNextQuery));
    }
}