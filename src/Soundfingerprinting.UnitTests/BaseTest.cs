namespace Soundfingerprinting.UnitTests
{
    using System.Configuration;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Soundfingerprinting.AudioProxies.Strides;

    /// <summary>
    ///   Base class which has all of the required parameters
    /// </summary>
    [DeploymentItem(@"bass.dll")]
    [DeploymentItem(@"bass_fx.dll")]
    [DeploymentItem(@"bassmix.dll")]
    [DeploymentItem(@"bassflac.dll")]
    [DeploymentItem(@"Kryptonite.mp3")]
    [DeploymentItem(@"Kryptonite.wav")]
    public class BaseTest
    {
        /// <summary>
        ///   Path to already re sampled file .wav
        /// </summary>
        protected const string PathToWav = @"Kryptonite.wav";

        /// <summary>
        ///   Path to MP3 file
        /// </summary>
        protected const string PathToMp3 = @"Kryptonite.mp3";

        /// <summary>
        ///   Bits per sample
        /// </summary>
        protected const int BitsPerSample = 32;

        /// <summary>
        ///   Sample rate
        /// </summary>
        protected const int SampleRate = 5512;

        /// <summary>
        ///   Wave header
        /// </summary>
        protected const int WaveHeader = 58;

        /// <summary>
        ///   Samples to read
        /// </summary>
        protected const int SamplesToRead = 128 * 64;

        protected const int MinYear = 1500;

        protected readonly string ConnectionString = ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString;

        protected readonly bool[] GenericFingerprint = new[]
                                                        {
                                                            true, false, true, false, true, false, true, false, true, false, true, false,
                                                            false, true, false, true, false, true, false, true, false, true, false, true,
                                                            true, false, true, false, true, false, true, false, true, false, true, false,
                                                            false, true, false, true, false, true, false, true, false, true, false, true,
                                                            true, false, true, false, true, false, true, false, true, false, true, false,
                                                            false, true, false, true, false, true, false, true, false, true, false, true,
                                                            true, false, true, false, true, false, true, false, true, false, true, false,
                                                            false, true, false, true, false, true, false, true, false, true, false, true,
                                                            true, false, true, false, true, false, true, false, true, false, true, false,
                                                            false, true, false, true, false, true, false, true, false, true, false, true
                                                        };

        /// <summary>
        ///   Random stride used in gathering the fingerprints
        /// </summary>
        protected readonly IStride RandomStride = new RandomStride(0, 253); /*0-46 ms*/

        /// <summary>
        ///   Static stride used in gathering the fingerprints
        /// </summary>
        protected readonly IStride StaticStride = new StaticStride(5115); /*928 ms*/

        protected readonly int StrideSize = 5115;

        protected DbProviderFactory Dbf = SqlClientFactory.Instance;
    }
}