namespace SoundFingerprinting.Image
{
    using System.Collections.Generic;

    public class Contour
    {
        public Contour(Coord topLeft, Coord bottomRight, int area)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
            Area = area;
        }

        public Coord TopLeft { get; }

        public Coord BottomRight { get; }

        public int Area { get; }

        public override string ToString()
        {
            return $"TopLeft:{TopLeft}, BottomRight:{BottomRight}, Area: {Area}";
        }

        public static IEnumerable<Contour> FindContours(byte[][] image, byte white, int areaThreshold)
        {
            int width = image[0].Length, height = image.Length;
            var union = new WeightedUnionFind(width, height);
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    byte current = image[i][j];
                    byte prevX = j > 0 ? image[i][j - 1] : byte.MinValue;
                    byte prevY = i > 0 ? image[i - 1][j] : byte.MinValue;

                    if (current == white)
                    {
                        if (current == prevX)
                        {
                            union.Union(i * width + j, i * width + j - 1);
                        }

                        if (current == prevY)
                        {
                            union.Union(i * width + j, (i - 1) * width + j);
                        }
                    }
                }
            }

            return union.FindDisjointContours(areaThreshold);
        }
    }
}