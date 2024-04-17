namespace DinoGrr.Physics
{
    public class Background
    {
        int motion1 = 1;
        public int l1_X1 { get; set; }
        public int l1_X2 { get; set; }
        public Bitmap layer1 { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public Background(int width, int height)
        {
            layer1 = Resource.mountains;
            l1_X1 = 0;
            l1_X2 = width;
            this.width = width;
            this.height = height;
        }

        public void BackgroundMoveLeft()
        {
            if (l1_X1 < -width) { l1_X1 = width - motion1; }
            l1_X1 -= motion1; l1_X2 -= motion1;
            if (l1_X2 < -width) { l1_X2 = width - motion1; }
        }

        public void BackgroundMoveRight()
        {
            if (l1_X1 > width) { l1_X1 = -width + motion1; }
            l1_X1 += motion1; l1_X2 += motion1;
            if (l1_X2 > width) { l1_X2 = -width + motion1; }
        }
    }
}
