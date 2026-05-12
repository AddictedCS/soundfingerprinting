namespace SoundFingerprinting.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SoundFingerprinting.Data;
    using SoundFingerprinting.Utils;

    /// <summary>
    ///  Default <see cref="ISpectralProfileService"/> — computes per-Frame SFM and power from the raw spectrum, buckets the metrics into one-second windows, and encodes the result via the registry.
    /// </summary>
    /// <remarks>
    ///  SFM per row over the log-frequency bins, median across rows; power is the mean across the whole row-major image.
    /// </remarks>
    public sealed class SpectralProfileService : ISpectralProfileService
    {
        private const double Epsilon = 1e-12;

        // below this frame count the Parallel.For setup overhead outweighs the win; stay serial
        private const int ParallelThreshold = 8;

        private readonly ISpectralProfileCodecRegistry registry;

        /// <summary>
        ///  Initializes a new instance of the <see cref="SpectralProfileService"/> class with the supplied codec registry.
        /// </summary>
        /// <param name="registry">Codec registry used to encode the resulting profile.</param>
        public SpectralProfileService(ISpectralProfileCodecRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        ///  Gets the singleton bound to the default codec registry.
        /// </summary>
        public static SpectralProfileService Instance { get; } = new (SpectralProfileCodecRegistry.Default);

        /// <inheritdoc />
        public string? Encode(IReadOnlyList<Frame> frames)
        {
            if (frames.Count == 0)
            {
                return null;
            }

            var perFrame = new FrameSpectralMetric[frames.Count];
            int seedRows = frames[0].Rows;

            if (frames.Count <= ParallelThreshold)
            {
                double[] rowSfms = new double[seedRows];
                for (int i = 0; i < frames.Count; ++i)
                {
                    rowSfms = EnsureCapacity(rowSfms, frames[i].Rows);
                    perFrame[i] = ComputeFrameSpectralMetric(frames[i], rowSfms);
                }
            }
            else
            {
                Parallel.For(0, frames.Count, () => new double[seedRows], (i, _, buffer) =>
                {
                    buffer = EnsureCapacity(buffer, frames[i].Rows);
                    perFrame[i] = ComputeFrameSpectralMetric(frames[i], buffer);
                    return buffer;
                },
                _ => { });
            }

            return EncodeBucketed(perFrame);
        }

        // O(n) average median via quickselect — for odd n the kth-smallest at n/2 is the median;
        // for even n we quickselect the upper middle, then a single linear scan over the lower partition gives the lower middle.
        private static double ComputeMedian(Span<double> values)
        {
            int n = values.Length;
            if (n == 0)
            {
                return 0;
            }

            if (n == 1)
            {
                return values[0];
            }

            int upperMid = n / 2;
            double upper = QuickSelectAlgorithm.SelectNthSmallest(values, upperMid);
            if ((n & 1) == 1)
            {
                return upper;
            }

            // after SelectNthSmallest the partition invariant guarantees values[0 .. upperMid - 1] all <= upper, so the max of that prefix is the lower middle of the original distribution
            double lower = values[0];
            for (int i = 1; i < upperMid; ++i)
            {
                if (values[i] > lower)
                {
                    lower = values[i];
                }
            }

            return (lower + upper) / 2.0;
        }

        private static FrameSpectralMetric ComputeFrameSpectralMetric(Frame frame, double[] rowSfmsBuffer)
        {
            int rows = frame.Rows;
            int cols = frame.Cols;
            float[] data = frame.ImageRowCols;
            double powerSum = 0;
            Span<double> rowSfms = rowSfmsBuffer.AsSpan(0, rows);
            for (int r = 0; r < rows; ++r)
            {
                int rowOffset = r * cols;
                double logSum = 0;
                double arithSum = 0;
                for (int c = 0; c < cols; ++c)
                {
                    double value = data[rowOffset + c];
                    if (value < 0)
                    {
                        value = -value;
                    }

                    double floored = value < Epsilon ? Epsilon : value;
                    logSum += Math.Log(floored);
                    arithSum += floored;
                    powerSum += value;
                }

                double geoMean = Math.Exp(logSum / cols);
                double arithMean = arithSum / cols;

                // arithMean is mathematically >= Epsilon (floored values are all >= Epsilon), so the else branch is unreachable; kept as a defensive fallback against NaN propagation from a pathological input
                rowSfms[r] = arithMean > 0 ? Math.Min(1.0, geoMean / arithMean) : 1.0;
            }

            double median = ComputeMedian(rowSfms);
            double meanPower = powerSum / (rows * cols);
            return new FrameSpectralMetric(frame.StartsAt, median, meanPower);
        }

        private string EncodeBucketed(FrameSpectralMetric[] perFrame)
        {
            // Size by max(StartsAt) instead of last.StartsAt so Encode is order-independent. The O(n) scan over doubles is negligible next to the per-cell Math.Log work in pass 1.
            // Math.Floor (not a raw int cast) so a malformed frame with negative StartsAt rounds toward -infinity and is rejected by the bucket < 0 guard below — an int cast truncates toward zero and would silently fold a negative-time frame into second 0.
            double maxStartsAt = perFrame[0].StartsAt;
            for (int i = 1; i < perFrame.Length; ++i)
            {
                if (perFrame[i].StartsAt > maxStartsAt)
                {
                    maxStartsAt = perFrame[i].StartsAt;
                }
            }

            int seconds = (int)Math.Floor(maxStartsAt) + 1;
            if (seconds <= 0)
            {
                return registry.Encode(new SpectralProfile(Array.Empty<SpectralSecond>()));
            }

            var sfmSum = new double[seconds];
            var powSum = new double[seconds];
            var counts = new int[seconds];
            for (int i = 0; i < perFrame.Length; ++i)
            {
                ref readonly var metric = ref perFrame[i];
                int bucket = (int)Math.Floor(metric.StartsAt);
                if (bucket < 0 || bucket >= seconds)
                {
                    continue;
                }

                sfmSum[bucket] += metric.Sfm;
                powSum[bucket] += metric.Power;
                counts[bucket]++;
            }

            // fold means in place into the sum arrays so we don't allocate parallel means arrays
            double maxPower = 0;
            for (int i = 0; i < seconds; ++i)
            {
                if (counts[i] == 0)
                {
                    continue;
                }

                double inv = 1.0 / counts[i];
                sfmSum[i] *= inv;
                powSum[i] *= inv;
                if (powSum[i] > maxPower)
                {
                    maxPower = powSum[i];
                }
            }

            double invMaxPower = maxPower > 0 ? 1.0 / maxPower : 0;
            var perSecond = new SpectralSecond[seconds];
            for (int i = 0; i < seconds; ++i)
            {
                perSecond[i] = counts[i] == 0 ? default : new SpectralSecond(sfmSum[i], powSum[i] * invMaxPower);
            }

            return registry.Encode(new SpectralProfile(perSecond));
        }

        private static double[] EnsureCapacity(double[] buffer, int required)
        {
            return buffer.Length >= required ? buffer : new double[required];
        }

        private readonly struct FrameSpectralMetric(double startsAt, double sfm, double power)
        {
            public double StartsAt { get; } = startsAt;

            public double Sfm { get; } = sfm;

            public double Power { get; } = power;
        }
    }
}
