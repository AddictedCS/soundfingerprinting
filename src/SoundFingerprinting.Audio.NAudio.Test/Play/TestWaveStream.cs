namespace SoundFingerprinting.Audio.NAudio.Test.Play
{
    using global::NAudio.Wave;

    internal class TestWaveStream : WaveStream
    {
        public override WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(5512, 1);

        public override long Length => 0;

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }
    }
}