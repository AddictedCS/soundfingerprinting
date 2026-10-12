namespace SoundFingerprinting.Video
{
    /// <summary>
    ///  Takes the crops of one frame region while a video source is read. The media service reads <see cref="Region"/> once at the start of every source.
    /// </summary>
    public interface IFrameCropSink
    {
        /// <summary>
        ///  Gets the region to crop, or null when nothing is cropped.
        /// </summary>
        FrameRegion? Region { get; }

        /// <summary>
        ///  Takes one crop.
        /// </summary>
        /// <param name="crop">The crop.</param>
        void Add(FrameCrop crop);
    }

    /// <summary>
    ///  A rectangle of the decoded picture in <see cref="FrameWidth"/> x <see cref="FrameHeight"/> coordinates and the rate at which it is cropped.
    /// </summary>
    public class FrameRegion
    {
        /// <summary>
        ///  Width of the frame the rectangle refers to.
        /// </summary>
        public const int FrameWidth = 640;

        /// <summary>
        ///  Height of the frame the rectangle refers to.
        /// </summary>
        public const int FrameHeight = 480;

        /// <summary>
        ///  Initializes a new instance of the <see cref="FrameRegion"/> class.
        /// </summary>
        /// <param name="x">Left edge of the rectangle.</param>
        /// <param name="y">Top edge of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="fps">Crops per second.</param>
        public FrameRegion(int x, int y, int width, int height, int fps)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Fps = fps;
        }

        /// <summary>
        ///  Gets the left edge of the rectangle.
        /// </summary>
        public int X { get; }

        /// <summary>
        ///  Gets the top edge of the rectangle.
        /// </summary>
        public int Y { get; }

        /// <summary>
        ///  Gets the width of the rectangle.
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///  Gets the height of the rectangle.
        /// </summary>
        public int Height { get; }

        /// <summary>
        ///  Gets the crops per second.
        /// </summary>
        public int Fps { get; }

        /// <summary>
        ///  Gets a value indicating whether the rectangle lies inside the frame and the rate is usable.
        /// </summary>
        public bool IsValid => X >= 0 && Y >= 0 && Width > 0 && Height > 0 && X + Width <= FrameWidth && Y + Height <= FrameHeight && Fps > 0;

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"[{X},{Y} {Width}x{Height}] @{Fps}fps";
        }
    }

    /// <summary>
    ///  One crop of a frame region, cut from one decoded frame.
    /// </summary>
    public sealed class FrameCrop
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="FrameCrop"/> class.
        /// </summary>
        /// <param name="region">Region the crop was cut with.</param>
        /// <param name="fileTime">Time of the frame in seconds from the first video frame of the source.</param>
        /// <param name="pixels">Gray8 pixels of the rectangle, row-major.</param>
        public FrameCrop(FrameRegion region, float fileTime, byte[] pixels)
        {
            Region = region;
            FileTime = fileTime;
            Pixels = pixels;
        }

        /// <summary>
        ///  Gets the region the crop was cut with.
        /// </summary>
        public FrameRegion Region { get; }

        /// <summary>
        ///  Gets the time of the frame in seconds from the first video frame of the source.
        /// </summary>
        public float FileTime { get; }

        /// <summary>
        ///  Gets the gray8 pixels of the rectangle, row-major, <c>Width x Height</c> bytes.
        /// </summary>
        public byte[] Pixels { get; }
    }
}
