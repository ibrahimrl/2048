using Cairo;
using Gdk;
using Gtk;
using Color = Cairo.Color;
using Key = Gdk.Key;
using static Gdk.EventMask;

internal class Game
{
    public int[,] Board = new int [4, 4];
    Dictionary<string, (int, int, int, int, int, int)> directions;
    public int Score;
    public int BestScore;
    
    public Game(int BestScore)
    {
        GetNewNumber();
        GetNewNumber();

        directions = new Dictionary<string, (int, int, int, int, int, int)>();
        directions["w"] = (-1, 0, 0, 1, 0, 1);
        directions["a"] = (0, -1, 0, 1, 0, 1);
        directions["s"] = (1, 0, 3,-1, 0, 1);
        directions["d"] = (0, 1, 0, 1, 3, -1);
        this.BestScore = BestScore;
    }

    bool Check(int x, int y) => 
        x is >= 0 and < 4 && y is >= 0 and < 4;
    
    public bool Move(string s) =>
        ChangeSpot(s) | Add(s) | ChangeSpot(s);
    
    bool ChangeSpot(string s)
    {
        (int dx, int dy, int df, int dg, int dh, int dj) = directions[s];
        bool u = false;
        for (int i = 0; i < 4; ++i)
        {
            for (int c = 0; c < 4; ++c)
            {
                for (int r = 3; r >= 0; --r)
                {
                    if (Check(r + dx, c + dy) && Board[r + dx, c + dy] == 0 && Board[r, c] != 0)
                    {
                        Board[r + dx, c + dy] = Board[r, c];
                        Board[r, c] = 0;
                        u = true;
                    }
                }
            }
        }
        return u;
    }

    bool Add(string s)
    {
        (int dx, int dy, int df, int dg, int dh, int dj) = directions[s];
        bool u = false;
        for (int c = dh; c is < 4 and >= 0; c += dj)
        {
            for (int r = df; r is < 4 and >= 0; r += dg)
            {
                if (Check(r + dx, c + dy))
                {
                    if (Board[r, c] == Board[r + dx, c + dy] && Board[r, c] != 0)
                    {
                        Board[r, c] = 0;
                        Board[r + dx, c + dy] *= 2;
                        Score += Board[r + dx, c + dy];
                        BestScore = Math.Max(BestScore, Score);
                        u = true;
                    }
                }
            }
        }
        return u;
    }
    
    public bool FinishGame()
    {
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                foreach ((int dx, int dy, int df, int dg, int dh, int dj) in directions.Values)
                {
                    if (Check(j + dx, i + dy) && Board[j, i] == Board[j + dx, i + dy])
                        return false;
                }
                if (Board[j, i] == 0)
                    return false;
            }
        }
        return true;
    }

    bool RandomSpot() 
    {
        Random rd = new Random();
        int l = rd.Next(0, 4);
        int c = rd.Next(0, 4);
        int n = rd.Next(0, 101);

        // 90% of the time the random number that is added is 2, 10% it is 4.
        if (Board[l, c] != 0) return false;
        if (n < 90)
            Board[l, c] = 2;
        else
            Board[l, c] = 4;
        return true;

    }

    public void GetNewNumber()
    {
        while(!RandomSpot()){}
    }
}

class Area : DrawingArea
{
    Game? game;
    Dictionary<int , Color> colors = new Dictionary<int, Color>();
    (int, int) UpperLeft = (30, 40), TopMiddle = (220, 40),
        TopRight = (360, 40), MainSquare = (30, 250);
    
    Color darker_grey = new Color(0.73, 0.67, 0.62),
        other_colors = new Color(0.24, 0.23, 0.20),
        white = new Color(1,1,1),
        gold = new Color(0.92, 0.76, 0.01);

    public void UpdateBoard(Game g)
    {
        game = g;
    }
    
    public Area() 
    {
        AddEvents((int) (ButtonPressMask));
        colors[0] = new Color(0.8, 0.75, 0.71);
        colors[2] = new Color(0.93, 0.89, 0.85);
        colors[4] = new Color(0.93, 0.87, 0.78);
        colors[8] = new Color(0.95, 0.69, 0.47);
        colors[16] = new Color(0.96, 0.58, 0.39);
        colors[32] = new Color(0.96, 0.49, 0.37);
        colors[64] = new Color(0.96, 0.37, 0.24);
        colors[128] = new Color(0.93, 0.81, 0.44);
        colors[256] = new Color(0.93, 0.80, 0.38);
        colors[512] = new Color(0.93, 0.78, 0.31);
        colors[1024] = new Color(0.93, 0.77, 0.25);
        colors[2048] = new Color(0.92, 0.76, 0.01);
    }

    protected override bool OnDrawn (Context c)
    {
        void DrawText(double x, double y, string s) {
            TextExtents te = c.TextExtents(s);
            c.SetSourceColor(white);
            c.MoveTo(x - (te.Width / 2 + te.XBearing), y - (te.Height / 2 + te.YBearing));
            c.ShowText(s);
        }

        void CreateSquare((int, int) s, int a, Color b)
        {
            (int dx, int dy) = s;
            c.Rectangle(x: dx, y: dy, width: a, height: a);
            c.SetSourceColor(b);
            c.Fill();
            c.Stroke();
        }

        void WriteW(string s, (int, int) q, int a, int x)
        {
            (int dx, int dy) = q;
            (double lx, int ly) = (dx + 55, dy + x);
            c.SetFontSize(a);
            DrawText(lx, ly, s);
        }

        void WriteNumbers(int a, int b)
        {
            string na = a.ToString();
            c.SetFontSize(40);
            DrawText(b, 105, na);
        }

        if (game == null)
            throw new Exception("game is null and it should not be, BUG!!!");
        
        // upper-left square
        CreateSquare(UpperLeft, 110, gold);
        WriteW("2048", UpperLeft , 40, 55);
        
        // top-middle square
        CreateSquare(TopMiddle, 110, darker_grey);
        WriteW("SCORE", TopMiddle, 20,20);
        WriteNumbers(game.Score, 275);
        
        //top-right square
        CreateSquare(TopRight, 110, darker_grey);
        WriteW("BEST", TopRight, 20, 20);
        WriteNumbers(game.BestScore, 415);
        
        // main square
        CreateSquare(MainSquare, 440, darker_grey);
        
        // small squares. padding between every square is 16.
        const int Padding = 106;
        int paddingVertically = 266;
        
        for (int x = 0; x < 4; x ++ )
        {
            int paddingHorizontally = 46;

            for (int i = 0; i < 4; i++)
            {
                c.Rectangle(x: paddingHorizontally, y: paddingVertically, width: 90, height: 90);
                if (game.Board[x, i] > 2048)
                    c.SetSourceColor(other_colors);
                else
                    c.SetSourceColor(colors[game.Board[x, i]]);
                c.Fill();
                c.Stroke();

                paddingHorizontally += Padding;
            }
            paddingVertically += Padding;
        }

        const int XPad = 91;
        const int YPad = 309;
        for (int i = 0; i < 4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                string num = game.Board[i, j].ToString();
                DrawText((j * Padding) + XPad, (i * Padding) + YPad, num != "0" ? num : " ");
            }
        }
        return true;
    }
}

class MyWindow : Gtk.Window
{
    Game g = new Game(0);
    Area area;

    public MyWindow() : base("2048") 
    {
        Resize(500, 750);
        area = new Area();
        Add(area);
        area.UpdateBoard(g);
        area.QueueDraw();
    }

    private void ProcessPress(string s)
    {
        if (g.Move(s))
            g.GetNewNumber();
    }
    
    protected override bool OnKeyPressEvent(EventKey e)
    {
        if (e.Key == Key.Up)
            ProcessPress("w");
        else if (e.Key == (Key.Down))
            ProcessPress("s");
        else if (e.Key == (Key.Right))
            ProcessPress("d");
        else if (e.Key == (Key.Left))
            ProcessPress("a");
        
        if (g.FinishGame())
        {
            g = new Game(g.BestScore);
        }
        area.UpdateBoard(g);
        area.QueueDraw();
        
        return true;
    }

    protected override bool OnDeleteEvent(Event e) 
    {
        Application.Quit();
        return true;
    }
}

class Hello 
{
    static void Main() 
    {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
        
    }
}