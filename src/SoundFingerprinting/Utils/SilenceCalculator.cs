namespace SoundFingerprinting.Utils;

using System;
using System.Linq;
using SoundFingerprinting.Data;

/// <summary>
///  Silence calculator.
/// </summary>
/// <remarks>
///  A utility class allowing to calculate silence contained within audio and video hashes. <br />
///  The fingerprint extractor drops silent fingerprints, so hashes produced by this library report zero silence. <br />
///  The calculator applies to hashes the caller builds itself, or stored by releases before 15.0.0 with silence treated as signal.
/// </remarks>
public static class SilenceCalculator
{
    /// <summary>
    ///  Silence value, derived from the fact that an array of [255, 255, 255, 255] min hashes will map to -1 during hashes conversion.
    /// </summary>
    private const int Silence = -1;
    
    /// <summary>
    ///  Calculate silence contained within audio and video hashes.
    /// </summary>
    /// <param name="avHashes">AVHashes to analyze.</param>
    /// <returns>
    ///  Tuple containing audio and video silence. For video hashes silence means black frames.
    /// </returns>
    public static (TimeSpan, TimeSpan) Calculate(AVHashes avHashes)
    {
        var audioSilence = Calculate(avHashes.Audio);
        var videoSilence = Calculate(avHashes.Video);
        return (audioSilence, videoSilence);
    }
    
    /// <summary>
    ///  Calculate contained silence within hashes.
    /// </summary>
    /// <param name="hashes">Hashes to analyze.</param>
    /// <returns>Timespan.</returns>
    /// <remarks>
    ///  Hashes produced by this library contain no silent fingerprints and report zero silence.
    /// </remarks>
    public static TimeSpan Calculate(Hashes? hashes)
    {
        if (hashes == null || hashes.IsEmpty)
        {
            return TimeSpan.Zero;
        }

        var items = hashes.OrderBy(_ => _.SequenceNumber).ToArray();
        double silence = 0;
        for (int i = 1; i < items.Length; i++)
        {
            if (items[i - 1].HashBins.All(_ => _ == Silence))
            {
                silence += items[i].StartsAt - items[i - 1].StartsAt;
            }
        }

        return TimeSpan.FromSeconds(silence);
    }
}