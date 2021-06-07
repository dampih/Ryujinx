namespace Ryujinx.Graphics.GAL
{
    public struct ScreenShotImageInfo
    {
        public ScreenShotImageInfo(int width, int height, bool isBgra, byte[] data)
        {
            Width = width;
            Height = height;
            IsBgra = isBgra;
            Data = data;
        }

        public int Width { get; }
        public int Height { get; }
        public bool IsBgra{ get; }
        public byte[] Data { get; }
    }
}