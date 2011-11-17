// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soundfingerprinting.AudioProxies.Strides;

namespace Soundfingerprinting.UnitTests
{
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
        protected const string PATH_TO_WAV = @"Kryptonite.wav";

        /// <summary>
        ///   Path to MP3 file
        /// </summary>
        protected const string PATH_TO_MP3 = @"Kryptonite.mp3";

        /// <summary>
        ///   Bits per sample
        /// </summary>
        protected const int BITS_PER_SAMPLE = 32;

        /// <summary>
        ///   Sample rate
        /// </summary>
        protected const int SAMPLE_RATE = 5512;

        /// <summary>
        ///   Wave header
        /// </summary>
        protected const int WAVE_HEADER = 58;

        /// <summary>
        ///   Samples to read
        /// </summary>
        protected const int SAMPLES_TO_READ = 128*64;

        protected const int MIN_YEAR = 1500;

        /// <summary>
        ///   Trace switch
        /// </summary>
        protected readonly BooleanSwitch Bswitch = new BooleanSwitch("trace", "Switch from config");

        protected readonly string ConnectionString = ConfigurationManager.ConnectionStrings["FingerprintConnectionString"].ConnectionString;

        protected readonly bool[] GENERIC_FINGERPRINT = new[]
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

        protected DbProviderFactory Dbf = SqlClientFactory.Instance;
    }
}