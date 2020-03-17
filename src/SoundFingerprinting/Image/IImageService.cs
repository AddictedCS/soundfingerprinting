namespace SoundFingerprinting.Image
{
    public interface IImageService
    {
        float[] Image2RowCols(float[][] image);

        float[][] RowCols2Image(float[] image, int rows, int cols);
        
        byte[][] FloatsToByteImage(float[][] image, int max);
        
        float[][] BytesToFloatImage(byte[][] array, int max);
    }
}