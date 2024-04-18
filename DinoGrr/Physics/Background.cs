namespace DinoGrr.Physics
{
    public class Background
    {
        float motion1 = 0.5f;
        float motion2 = 1f;
        public float l1_X1 { get; set; }
        public float l2_X1 { get; set; }
        public float l1_X2 { get; set; }
        public float l2_X2 { get; set; }
        public Bitmap layer1 { get; set; }
        public Bitmap layer2 { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public Background(int width, int height)
        {
            layer1 = Resource.mountains;
            layer2 = Resource.plants_background;
            l1_X1 = 0;
            l2_X1 = 0;
            l1_X2 = width;
            l2_X2 = width;
            this.width = width;
            this.height = height;
        }

        public void BackgroundMoveLeft()
        {
            if (l1_X1 < -width) { l1_X1 = width - motion1; }
            l1_X1 -= motion1; l1_X2 -= motion1;
            if (l1_X2 < -width) { l1_X2 = width - motion1; }

            if (l2_X1 < -width) { l2_X1 = width - motion2; }
            l2_X1 -= motion2; l2_X2 -= motion2;
            if (l2_X2 < -width) { l2_X2 = width - motion2; }
        }

        public void BackgroundMoveRight()
        {
            if (l1_X1 > width) { l1_X1 = -width + motion1; }
            l1_X1 += motion1; l1_X2 += motion1;
            if (l1_X2 > width) { l1_X2 = -width + motion1; }

            if (l2_X1 > width) { l2_X1 = -width + motion2; }
            l2_X1 += motion2; l2_X2 += motion2;
            if (l2_X2 > width) { l2_X2 = -width + motion2; }
        }
    }
}
