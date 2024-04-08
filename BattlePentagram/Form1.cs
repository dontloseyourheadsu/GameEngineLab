namespace BattlePentagram;
public partial class Form1 : Form
{
    private Bitmap bitmap;
    private Graphics graphics;
    private PointF playerPosition = new PointF(200, 100);
    private int tileWidth = 40, tileHeight = 20;
    private int worldWidth = 14, worldHeight = 10;
    private int playerSize = 10;
    private float playerSpeed = 5f;
    private Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>();

    public Form1()
    {
        InitializeComponent();
        pictureBox1.Width = Width;
        pictureBox1.Height = Height;
        bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        graphics = Graphics.FromImage(bitmap);
        pictureBox1.Image = bitmap;

        // Initialize key states
        keyStates[Keys.W] = false;
        keyStates[Keys.A] = false;
        keyStates[Keys.S] = false;
        keyStates[Keys.D] = false;

        tileWidth = pictureBox1.Width / worldWidth;
        tileHeight = pictureBox1.Height / worldHeight;
        playerSize = Math.Min(tileWidth, tileHeight) / 2;

        this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        this.KeyUp += new KeyEventHandler(Form1_KeyUp);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        graphics.Clear(Color.Black);
        DrawWorld();

        // Calculate depths for comparison
        // Assuming a single bush for simplicity; you can generalize this for multiple objects
        float playerDepth = playerPosition.X + playerPosition.Y;
        // Adjust bushDepth based on its bottom center position, matching the DrawBushes adjustment
        float bushDepth = 3 * tileWidth + (3 + 0.5f) * tileHeight; // Example for bush at tile (3,3)

        UpdatePlayerPosition();

        // Determine drawing order
        if (playerDepth < bushDepth)
        {
            DrawPlayer();
            DrawBushes();
        }
        else
        {
            DrawBushes();
            DrawPlayer();
        }

        pictureBox1.Refresh();
    }


    private void DrawWorld()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                DrawTile(x, y);
            }
        }
    }

    private void DrawTile(int x, int y)
    {
        Point top = IsoPoint(x * tileWidth, y * tileHeight);
        Point[] points = new Point[] {
            new Point(top.X, top.Y),
            new Point(top.X + tileWidth / 2, top.Y + tileHeight / 2),
            new Point(top.X, top.Y + tileHeight),
            new Point(top.X - tileWidth / 2, top.Y + tileHeight / 2)
        };
        graphics.FillPolygon(Brushes.Green, points);
        graphics.FillPolygon(Brushes.SaddleBrown, new Point[] { points[0], points[1], new Point(points[1].X, points[1].Y + 5), new Point(points[0].X, points[0].Y + 5) });
    }

    private void DrawBushes()
    {
        // Example bush at tile (3,3). Modify or expand as needed.
        int bushX = 3 * tileWidth, bushY = 3 * tileHeight;
        Point bushTop = IsoPoint(bushX, bushY - tileHeight / 2); // Adjusting Y to draw the bush based on its bottom center
        graphics.FillRectangle(Brushes.LightGreen, bushTop.X - tileWidth / 4, bushTop.Y, tileWidth / 2, tileHeight / 2);
    }

    private void UpdatePlayerPosition()
    {
        if (keyStates[Keys.W]) playerPosition.Y -= playerSpeed;
        if (keyStates[Keys.S]) playerPosition.Y += playerSpeed;
        if (keyStates[Keys.A]) playerPosition.X -= playerSpeed;
        if (keyStates[Keys.D]) playerPosition.X += playerSpeed;
    }

    private void DrawPlayer()
    {
        Point screenPos = IsoPoint(playerPosition.X, playerPosition.Y);
        graphics.FillRectangle(Brushes.Black, screenPos.X - playerSize / 2, screenPos.Y - playerSize / 2, playerSize, playerSize);
    }

    private Point IsoPoint(float x, float y)
    {
        return new Point((int)(pictureBox1.Width / 2 + (x - y) / 2), (int)((x + y) / 4));
    }

    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        if (keyStates.ContainsKey(e.KeyCode))
            keyStates[e.KeyCode] = true;
    }

    private void Form1_KeyUp(object sender, KeyEventArgs e)
    {
        if (keyStates.ContainsKey(e.KeyCode))
            keyStates[e.KeyCode] = false;
    }

    private void Form1_SizeChanged(object sender, EventArgs e)
    {
        pictureBox1.Width = Width;
        pictureBox1.Height = Height;
        bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
        graphics = Graphics.FromImage(bitmap);
        pictureBox1.Image = bitmap;

        tileWidth = pictureBox1.Width / worldWidth;
        tileHeight = pictureBox1.Height / worldHeight;
        playerSize = Math.Min(tileWidth, tileHeight) / 2;
    }
}
