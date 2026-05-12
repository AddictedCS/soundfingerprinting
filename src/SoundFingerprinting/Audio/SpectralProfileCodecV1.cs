namespace SoundFingerprinting.Audio
{
    using System;

    /// <summary>
    ///  Version 1 codec: byte-quantised per-second SFM and power, interleaved.
    /// </summary>
    /// <remarks>
    ///  Layout: <c>[version=1][flags=0][sfm0, pow0, sfm1, pow1, ...]</c>.
    ///  SFM is quantised as <c>(byte)(sfm * 255)</c>, power as <c>(byte)((power / maxPower) * 255)</c>.
    ///  Resolution ~0.004 — two orders of magnitude finer than the 0.05-0.15 tolerances used by strategies.
    /// </remarks>
    public sealed class SpectralProfileCodecV1 : ISpectralProfileCodec
    {
        /// <summary>
        ///  Gets the version byte handled by this codec.
        /// </summary>
        public byte Version => 1;

        /// <summary>
        ///  Singleton instance.
        /// </summary>
        public static SpectralProfileCodecV1 Instance { get; } = new ();

        /// <inheritdoc />
        public SpectralProfile Decode(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 2)
            {
                throw new ArgumentException("Payload must contain at least version and flags bytes.", nameof(payload));
            }

            if (payload[0] != Version)
            {
                throw new ArgumentException($"Payload version {payload[0]} does not match codec version {Version}.", nameof(payload));
            }

            int dataLength = payload.Length - 2;
            if ((dataLength & 1) != 0)
            {
                throw new ArgumentException("Payload data length must be even (interleaved SFM/power pairs).", nameof(payload));
            }

            int seconds = dataLength / 2;
            var perSecond = new SpectralSecond[seconds];
            for (int i = 0; i < seconds; ++i)
            {
                byte sfmByte = payload[2 + (i * 2)];
                byte powByte = payload[3 + (i * 2)];
                perSecond[i] = new SpectralSecond(sfmByte / 255.0, powByte / 255.0);
            }

            return new SpectralProfile(perSecond);
        }

        /// <inheritdoc />
        public byte[] Encode(SpectralProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            int seconds = profile.PerSecond.Count;
            var buffer = new byte[2 + (seconds * 2)];
            buffer[0] = Version;
            buffer[1] = 0;
            for (int i = 0; i < seconds; ++i)
            {
                var second = profile.PerSecond[i];
                buffer[2 + (i * 2)] = QuantiseUnitInterval(second.Sfm);
                buffer[3 + (i * 2)] = QuantiseUnitInterval(second.Power);
            }

            return buffer;
        }

        private static byte QuantiseUnitInterval(double value)
        {
            if (value <= 0)
            {
                return 0;
            }

            if (value >= 1)
            {
                return 255;
            }

            return (byte)(value * 255);
        }
    }
}
