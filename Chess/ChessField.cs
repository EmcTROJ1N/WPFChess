using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Stockfish.NET;

namespace Chess
{
    public class ChessField
    {
        public enum OpponentType { PC, Friend }
        public enum GameState { Continues, Check, Checkmate, Tie }
        delegate void SetPositionDelegate();

        public ChessFigure?[,] Figures { get; private set; }
        public VisualField Visual;
        public int Length { get; } = 8;
        public int Level { get; private set; } = 1;
        int CountStepsForTie = 0;
        Point SelectedFigureCords = new Point(-1, -1);
        Point? TaggedCords;
        public ChessFigure? SelectedFigure { get => SelectedFigureCords.X != -1 && SelectedFigureCords.Y != -1 ? Figures[(int)SelectedFigureCords.X, (int)SelectedFigureCords.Y] : null; }

        public bool CurrentCourse { get; set; } = true;

        public ObservableCollection<string> NotaionList { get; set; } = new ObservableCollection<string>();

        DispatcherTimer GameTimer = new DispatcherTimer();
        MediaPlayer MakeMovePlayer = new MediaPlayer();

        public DateTime TimeFirstTeam { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 10, 0);
        public DateTime TimeSecondTeam { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 10, 0);

        IStockfish? Stockfish;
        OpponentType Opponent;

        GameState _State;
        public GameState State
        {
            get
            {
                if (_State == GameState.Checkmate) return GameState.Checkmate;
                Point KingCords;
                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        if (Figures[i, j] is King && (Figures[i, j] as King)!.IsWhiteTeam == CurrentCourse)
                        {
                            KingCords = Figures[i, j]!.CurrentPos;
                            i = Length;
                            break;
                        }
                    }
                }

                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        if (Figures[i, j] == null) continue;
                        if (Figures[i, j]?.IsWhiteTeam == CurrentCourse) continue;

                        if (Figures[i, j] != null)
                            if (Figures[i, j]?.MoveWithoutCheck().Contains(KingCords) == true)
                                return GameState.Check;
                    }
                }
                return GameState.Continues;
            }
            set => _State = value;
        }

        public ChessField(VisualField visual, OpponentType type, int? skillLevel = null, DateTime? timeFirstTeam = null, DateTime? timeSecondTeam = null)
        {
            Initialize(visual, type, skillLevel, timeFirstTeam, timeSecondTeam);

            Figures = new ChessFigure[8, 8];

            Color frontTeam =  visual.IsWhiteBehind ? visual.SecondTeam : visual.FirstTeam;
            Color backTeam =  visual.IsWhiteBehind ? visual.FirstTeam : visual.SecondTeam;
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Figures[i, j] = null;
                    if (i == 1) Figures[i, j] = new Pawn(frontTeam, new Point(i, j), this);
                    if (i == 6) Figures[i, j] = new Pawn(backTeam, new Point(i, j), this);
                    if (i == 0)
                    {
                        if (j == 7 || j == 0) Figures[i, j] = new Rook(frontTeam, new Point(i, j), this);
                        if (j == 6 || j == 1) Figures[i, j] = new Knight(frontTeam, new Point(i, j), this);
                        if (j == 5 || j == 2) Figures[i, j] = new Bishop(frontTeam, new Point(i, j), this);
                        if (visual.IsWhiteBehind)
                        {
                            if (j == 3) Figures[i, j] = new Queen(frontTeam, new Point(i, j), this);
                            if (j == 4) Figures[i, j] = new King(frontTeam, new Point(i, j), this);
                        }
                        else
                        {
                            if (j == 4) Figures[i, j] = new Queen(frontTeam, new Point(i, j), this);
                            if (j == 3) Figures[i, j] = new King(frontTeam, new Point(i, j), this);
                        }
                    }
                    if (i == 7)
                    {
                        if (j == 7 || j == 0) Figures[i, j] = new Rook(backTeam, new Point(i, j), this);
                        if (j == 6 || j == 1) Figures[i, j] = new Knight(backTeam, new Point(i, j), this);
                        if (j == 5 || j == 2) Figures[i, j] = new Bishop(backTeam, new Point(i, j), this);

                        if (visual.IsWhiteBehind)
                        {
                            if (j == 3) Figures[i, j] = new Queen(backTeam, new Point(i, j), this);
                            if (j == 4) Figures[i, j] = new King(backTeam, new Point(i, j), this);
                        }
                        else
                        {
                            if (j == 4) Figures[i, j] = new Queen(backTeam, new Point(i, j), this);
                            if (j == 3) Figures[i, j] = new King(backTeam, new Point(i, j), this);
                        }
                    }
                    if (Figures[i, j] != null)
                    {
                        if (Opponent == OpponentType.Friend && Figures[i, j]!.IsWhiteTeam)
                            Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                        if (Opponent == OpponentType.PC && Figures[i, j]!.IsWhiteTeam == visual.IsWhiteBehind)
                           Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                    }
                }
            }
            Display();

            if (Opponent == OpponentType.PC && visual.IsWhiteBehind == false)
                PCMove();
        }

        public ChessField(ChessFigure?[,] figures, VisualField visual, OpponentType type, bool currentCourse, int skillLevel, 
            DateTime? timeFirstTeam = null, DateTime? timeSecondTeam = null, List<string>? notation = null, List<string>? eatenFigs = null)
        {
            Initialize(visual, type, skillLevel, timeFirstTeam, timeSecondTeam);

            CurrentCourse = currentCourse;
            Figures = figures;
            Display();
            
            if (notation != null)
            {
                foreach (string step in notation)
                    NotaionList.Add(step);
            }

            if (eatenFigs != null)
            {
                bool flag = false;
                foreach(string path in eatenFigs)
                {
                    if (path == "")
                    {
                        flag = true;
                        continue;
                    }
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(path));
                    if (flag) visual.EatenFirstTeam.Children.Add(img);
                    else visual.EatenSecondTeam.Children.Add(img);
                }
            }

            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (Figures[i, j] != null)
                    {
                        Figures[i, j]!.Parent = this;
                        
                        if (Opponent == OpponentType.Friend && Figures[i, j]!.IsWhiteTeam)
                            Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                        if (Opponent == OpponentType.PC && Figures[i, j]!.IsWhiteTeam == visual.IsWhiteBehind)
                           Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                        
                        if (Figures[i, j]!.Type == ChessFigure.FigureType.Pawn)
                        {
                            if (Figures[i, j]!.Parent!.Visual.IsWhiteBehind)
                                ((Pawn)Figures[i, j]!).Step = Figures[i, j]!.IsWhiteTeam ? -1 : 1;
                            else
                                ((Pawn)Figures[i, j]!).Step = Figures[i, j]!.IsWhiteTeam ? 1 : -1;
                        }
                    }
                }
            }
        }
            
        void Initialize(VisualField visual, OpponentType type, int? skillLevel = null, DateTime? timeFirstTeam = null, DateTime? timeSecondTeam = null)
        {
            if (timeFirstTeam != null) TimeFirstTeam = (DateTime)timeFirstTeam;
            if (timeSecondTeam != null) TimeSecondTeam = (DateTime)timeSecondTeam;
            if (skillLevel != null) Level = (int)skillLevel;
            
            visual.Notation.ItemsSource = NotaionList;
            visual.TimeLeftFirstTeam.Text = $"First team time left: {TimeSecondTeam.ToLongTimeString()}";
            visual.TimeLeftSecondTeam.Text = $"Second team time left: {TimeSecondTeam.ToLongTimeString()}";

            GameTimer.Interval = TimeSpan.FromSeconds(1);
            GameTimer.Tick += TimerTick;
            MakeMovePlayer.Open(new Uri("Resource/FigureStepSound.mp3", UriKind.Relative));
            MakeMovePlayer.MediaEnded += (sender, e) => { MakeMovePlayer.Position = TimeSpan.Zero; MakeMovePlayer.Stop(); };

            Opponent = type;

            Visual = visual;
            Visual.Field.MouseLeftButtonDown += MakeMove;
            Visual.Field.MouseRightButtonDown += Field_MouseRightButtonDown;
            Visual.Field.MouseRightButtonUp += Field_MouseRightButtonUp;
            
            if (Opponent == OpponentType.PC)
            {
                Stockfish = new Stockfish.NET.Stockfish(Environment.CurrentDirectory + "\\Resource\\stockfish.exe", new Random().Next(15),
                    new Stockfish.NET.Models.Settings(skillLevel:Level));
                CurrentCourse = Visual.IsWhiteBehind;
            }
        }

        void MoveFigures(Point startPos, Point endPos)
        {
            ChessFigure? currentFigure = Figures[(int)startPos.X, (int)startPos.Y];
            if (currentFigure == null) return;

            string step = $"{currentFigure}";
            currentFigure.CountStep++;

            int n = Grid.GetRow(currentFigure?.Sprite);

            bool castling = false;
            bool transform = false;

            if (Figures[(int)endPos.X, (int)endPos.Y] != null)
            {
                Visual.Field.Children.Remove(Figures[(int)endPos.X, (int)endPos.Y]?.Sprite);
                Figures[(int)endPos.X, (int)endPos.Y]!.Sprite.RenderTransform = new RotateTransform(0);
                
                if (Figures[(int)endPos.X, (int)endPos.Y]!.IsWhiteTeam)
                    Visual.EatenFirstTeam.Children.Add(Figures[(int)endPos.X, (int)endPos.Y]?.Sprite);
                else
                    Visual.EatenSecondTeam.Children.Add(Figures[(int)endPos.X, (int)endPos.Y]?.Sprite);

                step += "x";

                if (currentFigure?.Type == ChessFigure.FigureType.Pawn)
                { transform = endPos.X == 0 || endPos.X == 7 ? true : false;  }
               
            }
            else if (currentFigure?.Type == ChessFigure.FigureType.Pawn)
            {
                if (currentFigure.CountStep > 1)
                    (currentFigure as Pawn)!.Pass_takeover = false;
                if (currentFigure.CountStep == 1 && Math.Abs(endPos.X - startPos.X) == 2)
                {
                    (currentFigure as Pawn)!.Pass_takeover = true;
                    (currentFigure as Pawn)!.OldCountSteps = NotaionList.Count + 1;
                }

                if (new int[] { 3, 4 }.Contains(Grid.GetRow(currentFigure.Sprite)))
                {
                    if (new int[] { 2, 5 }.Contains((int)endPos.X) && (int)endPos.Y != Grid.GetColumn(currentFigure.Sprite))
                    {
                        Visual.Field.Children.Remove(Figures[(int)endPos.X - ((Pawn)currentFigure).Step, (int)endPos.Y]?.Sprite);
                        Figures[(int)endPos.X - ((Pawn)currentFigure).Step, (int)endPos.Y] = null;
                        step += "x";
                    }
                }
                else if (Grid.GetRow(currentFigure.Sprite) == 4)
                {
                    if ((int)endPos.X == 5 && (int)endPos.Y != Grid.GetColumn(currentFigure.Sprite))
                    {
                        Visual.Field.Children.Remove(Figures[(int)endPos.X - 1, (int)endPos.Y]?.Sprite);
                        step += "x";
                    }
                }
                transform = endPos.X == 0 || endPos.X == 7 ? true : false;
            }
            else if (currentFigure?.Type == ChessFigure.FigureType.King)
            {
                if (Math.Abs(Grid.GetColumn(currentFigure.Sprite) - endPos.Y) > 1)
                    castling = true;
            }

            void completed(object? sender, EventArgs e)
            {
                currentFigure.Sprite.Margin = new Thickness(0);
                currentFigure!.Sprite.Width = double.NaN;
                currentFigure!.Sprite.Height = double.NaN;
                Grid.SetColumnSpan(currentFigure?.Sprite, 1);
                Grid.SetRowSpan(currentFigure?.Sprite, 1);
                Grid.SetColumn(currentFigure?.Sprite, (int)endPos.Y);
                Grid.SetRow(currentFigure?.Sprite, (int)endPos.X);

            }

            if (currentFigure?.Sprite.ActualHeight == 0)
            {
                currentFigure.Sprite.Width = 69;
                currentFigure.Sprite.Height = 69;
            }
            else
            {
                currentFigure!.Sprite.Width = currentFigure.Sprite.ActualWidth;
                currentFigure.Sprite.Height = currentFigure.Sprite.ActualHeight;
            }

            Grid.SetColumnSpan(currentFigure.Sprite, 8);
            Grid.SetRowSpan(currentFigure.Sprite, 8);
            Grid.SetColumn(currentFigure.Sprite, 0);
            Grid.SetRow(currentFigure.Sprite, 0);

            ThicknessAnimation figureAnimMargin = new ThicknessAnimation(
               new Thickness(Visual.Field.ActualWidth * -.875 + Visual.Field.ActualWidth / 4 * startPos.Y,
                             Visual.Field.ActualHeight * -.875 + Visual.Field.ActualHeight / 4 * startPos.X, 0, 0),
               new Thickness(Visual.Field.ActualWidth * -.875 + Visual.Field.ActualWidth / 4 * endPos.Y,
                             Visual.Field.ActualHeight * -.875 + Visual.Field.ActualHeight / 4 * endPos.X, 0, 0),
                             TimeSpan.FromSeconds(.5));
            figureAnimMargin.FillBehavior = FillBehavior.Stop;
            PowerEase powerEase = new PowerEase();
            powerEase.EasingMode = EasingMode.EaseOut;
            powerEase.Power = 5;
            figureAnimMargin.EasingFunction = powerEase;

            figureAnimMargin.Completed += completed;

            currentFigure.Sprite.BeginAnimation(Image.MarginProperty, figureAnimMargin);

            if (castling)
            {
                step += "-";
                int end_c = 0;
                int begin_c = 0;
                if (Visual.IsWhiteBehind)
                {
                    end_c = (int)endPos.Y > 4 ? 5 : 3;
                    begin_c = (int)endPos.Y > 5 ? 7 : 0;
                }
                else
                {
                    end_c = (int)endPos.Y <= 4 ? 2 : 4;
                    begin_c = (int)endPos.Y < 5 ? 0 : 7;
                }

                if (Figures[(int)endPos.X, begin_c] != null)
                {
                    Figures[(int)endPos.X, begin_c]!.Sprite.Width = currentFigure.Sprite.ActualWidth;
                    Figures[(int)endPos.X, begin_c]!.Sprite.Height = currentFigure.Sprite.ActualHeight;

                    Grid.SetColumnSpan(Figures[(int)endPos.X, begin_c]!.Sprite, 8);
                    Grid.SetRowSpan(Figures[(int)endPos.X, begin_c]!.Sprite, 8);
                    Grid.SetColumn(Figures[(int)endPos.X, begin_c]!.Sprite, 0);
                    Grid.SetRow(Figures[(int)endPos.X, begin_c]!.Sprite, 0);

                    ThicknessAnimation castleRookAnim = new ThicknessAnimation(
                       new Thickness(Visual.Field.ActualWidth * -.875 + Visual.Field.ActualWidth / 4 * begin_c,
                                     Visual.Field.ActualHeight * -.875 + Visual.Field.ActualHeight / 4 * startPos.X, 0, 0),
                       new Thickness(Visual.Field.ActualWidth * -.875 + Visual.Field.ActualWidth / 4 * end_c,
                                     Visual.Field.ActualHeight * -.875 + Visual.Field.ActualHeight / 4 * startPos.X, 0, 0),
                                     TimeSpan.FromSeconds(.5));
                    castleRookAnim.FillBehavior = FillBehavior.Stop;
                    castleRookAnim.EasingFunction = powerEase;
                    castleRookAnim.Completed += completedCastle;
                    void completedCastle(object? sender, EventArgs e)
                    {
                        Figures[(int)endPos.X, end_c]!.Sprite.Margin = new Thickness(0);
                        Grid.SetColumnSpan(Figures[(int)endPos.X, end_c]!.Sprite, 1);
                        Grid.SetRowSpan(Figures[(int)endPos.X, end_c]!.Sprite, 1);
                        Grid.SetRow(Figures[(int)endPos.X, end_c]!.Sprite, (int)endPos.X);
                        Grid.SetColumn(Figures[(int)endPos.X, end_c]!.Sprite, end_c);
                    }

                    Figures[(int)endPos.X, begin_c]!.Sprite.BeginAnimation(Image.MarginProperty, castleRookAnim);
                    
                    Figures[(int)endPos.X, end_c] = Figures[(int)endPos.X, begin_c];
                    Figures[(int)endPos.X, end_c]!.CurrentPos = new Point((int)endPos.X, end_c);
                    Figures[(int)endPos.X, begin_c] = null;
                }
            }

            if (transform)
            {
                Transformation trans = new Transformation(this);

                trans.ShowDialog();

                string r = trans.r;

                Color cur = CurrentCourse == true ? Colors.White : Colors.Black;

                Visual.Field.Children.Remove(Figures[(int)startPos.X, (int)startPos.Y]?.Sprite);

                Figures[(int)startPos.X, (int)startPos.Y]!.Sprite.RenderTransform = new RotateTransform(0);
                
                if (Figures[(int)startPos.X, (int)startPos.Y]!.IsWhiteTeam)
                    Visual.EatenFirstTeam.Children.Add(Figures[(int)startPos.X, (int)startPos.Y]?.Sprite);
                else
                    Visual.EatenSecondTeam.Children.Add(Figures[(int)startPos.X, (int)startPos.Y]?.Sprite);

                Figures[(int)startPos.X, (int)startPos.Y] = null;

                switch (r)
                {
                    case "Q":
                        {
                            Figures[(int)endPos.X, (int)endPos.Y] = new Queen(cur, new Point((int)endPos.X, (int)endPos.Y), this);
                            break;
                        }
                    case "B":
                        {
                            Figures[(int)endPos.X, (int)endPos.Y] = new Bishop(cur, new Point((int)endPos.X, (int)endPos.Y), this);
                            break;
                        }
                    case "K":
                        {
                            Figures[(int)endPos.X, (int)endPos.Y] = new Knight(cur, new Point((int)endPos.X, (int)endPos.Y), this);
                            break;
                        }
                    case "R":
                        {
                            Figures[(int)endPos.X, (int)endPos.Y] = new Rook(cur, new Point((int)endPos.X, (int)endPos.Y), this);
                            break;
                        }
                    default:
                        break;
                }

                Grid.SetColumn(Figures[(int)endPos.X, (int)endPos.Y]!.Sprite, (int)endPos.Y);
                Grid.SetRow(Figures[(int)endPos.X, (int)endPos.Y]!.Sprite, (int)endPos.X);
                Visual.Field.Children.Add(Figures[(int)endPos.X, (int)endPos.Y]?.Sprite);

            }
            else
            { 
                Figures[(int)endPos.X, (int)endPos.Y] = currentFigure;
                Figures[(int)currentFigure.CurrentPos.X,(int)currentFigure.CurrentPos.Y] = null;
                currentFigure.CurrentPos = new Point((int)endPos.X, (int)endPos.Y);
            }

            step += $"{Figures[(int)endPos.X, (int)endPos.Y]}";

            CurrentCourse = !CurrentCourse;
            step += State == GameState.Check ? "+" : null;
            NotaionList.Add(step);

            if (State == GameState.Check)
            {
                bool flag = true;
                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        if (Figures[i, j] == null || Figures[i, j]?.IsWhiteTeam != CurrentCourse) continue;
                        if (Figures[i, j]?.Move().Length != 0)
                            flag = false;
                    }
                }
                if (flag && GameTimer.IsEnabled) FinalGame(GameState.Checkmate);

                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        if (Figures[i, j] is King && (Figures[i, j] as King)!.IsWhiteTeam == CurrentCourse)
                        {
                            Rectangle rect = new Rectangle();

                            rect.Fill = new SolidColorBrush(Colors.Red);
                            rect.Opacity = 1;
                            Grid.SetColumn(rect, j);
                            Grid.SetRow(rect, i);
                            Visual.Field.Children.Add(rect);

                            DoubleAnimation opacityAnim = new DoubleAnimation(0, .8, TimeSpan.FromSeconds(1));
                            opacityAnim.AutoReverse = true;

                            rect.BeginAnimation(Rectangle.OpacityProperty, opacityAnim);
                            opacityAnim.Completed += (sender, e) => Visual.Field.Children.Remove(rect);

                            i = Length;
                            break;
                        }
                    }
                }
            }
        
            CurrentCourse = !CurrentCourse;
        }

        private void MakeMove(object sender, MouseButtonEventArgs e)
        {
            if (SelectedFigure == null) return;

            Point[] boxes = SelectedFigure.Move();

            int cN = 0;
            int rN = 0;
            

            getTabBoardCords(e, out cN, out rN);

            if (boxes.Contains(new Point(rN, cN)) == false) return;
            
            GameTimer.Start();
            MoveFigures(SelectedFigureCords, new Point(rN, cN));

            ClearSelected();
            
            MakeMovePlayer.Stop();
            MakeMovePlayer.Play();

            if (State == GameState.Checkmate || State == GameState.Tie) return;
            
            if (Opponent == OpponentType.Friend) ChangeCurrent();
            if (Opponent == OpponentType.PC)     PCMove();


            if (NotaionList.Count > 0)
            {
                if (NotaionList[^1].Length > 4 && NotaionList[^1].Contains("x") == false) CountStepsForTie++;
                else CountStepsForTie = 0;
            }

            if (CountStepsForTie >= 50)
                FinalGame(GameState.Tie);

            if (NotaionList.Count >= 5)
            {
                bool isTie = true;
                string removeUnnecessary(string move)
                {
                    move = move.Replace("-", "");
                    move = move.Replace("x", "");
                    move = move.Replace("+", "");
                    move = move.Replace("N", "");
                    move = move.Replace("K", "");
                    move = move.Replace("Q", "");
                    move = move.Replace("B", "");
                    move = move.Replace("R", "");

                    return move;
                }

                if (removeUnnecessary(NotaionList[^1]) != removeUnnecessary(NotaionList[^5])) isTie = false;
                if (removeUnnecessary(NotaionList[^2]).Substring(2, 2) != removeUnnecessary(NotaionList[^4]).Substring(0, 2) ||
                    removeUnnecessary(NotaionList[^2]).Substring(0, 2) != removeUnnecessary(NotaionList[^4]).Substring(2, 2)) isTie = false;
                if (removeUnnecessary(NotaionList[^1]).Substring(2, 2) != removeUnnecessary(NotaionList[^3]).Substring(0, 2) ||
                    removeUnnecessary(NotaionList[^1]).Substring(0, 2) != removeUnnecessary(NotaionList[^3]).Substring(2, 2)) isTie = false;
                if (removeUnnecessary(NotaionList[^1]).Substring(2, 2) != removeUnnecessary(NotaionList[^3]).Substring(0, 2) ||
                    removeUnnecessary(NotaionList[^1]).Substring(0, 2) != removeUnnecessary(NotaionList[^3]).Substring(2, 2)) isTie = false;
                if (isTie) FinalGame(GameState.Tie);
            }

            int countSteps = 0;
            List<ChessFigure> lstFigures = new List<ChessFigure>();

            for (int i = 0; i < Length; i++)
                for (int j = 0; j < Length; j++)
                    if (Figures[i, j] != null)
                    {
                        if (Figures[i, j]?.IsWhiteTeam == CurrentCourse)
                            countSteps += Figures[i, j]!.Move().Length;
                        lstFigures.Add(Figures[i, j]!);
                    }
            if (countSteps == 0) FinalGame(GameState.Tie);
            
            List<ChessFigure.FigureType> lst = new List<ChessFigure.FigureType>();
            switch (lstFigures.Count)
            {
                case 2: FinalGame(GameState.Tie); break;
                case 3:
                    for (int i = 0; i < lstFigures.Count; i++)
                    {
                        if (lstFigures[i].Type == ChessFigure.FigureType.King) continue;
                        lst.Add(lstFigures[i].Type);
                    }
                    if (lst[0] == ChessFigure.FigureType.Knight || lst[0] == ChessFigure.FigureType.Bishop)
                        FinalGame(GameState.Tie);
                    break;
                case 4:
                    int[] idx = new int[2];
                    for (int i = 0, j = 0; i < lstFigures.Count; i++)
                    {

                        if (lstFigures[i].Type == ChessFigure.FigureType.King) continue;
                        lst.Add(lstFigures[i].Type);
                        idx[j] = i;
                        j++;

                    }
                    if (lst[0] == ChessFigure.FigureType.Bishop && lst[1] == ChessFigure.FigureType.Bishop)
                    {
                        if ((lstFigures[idx[0]].CurrentPos.X + lstFigures[idx[0]].CurrentPos.Y) % 2 ==
                            (lstFigures[idx[1]].CurrentPos.X + lstFigures[idx[1]].CurrentPos.Y) % 2) FinalGame(GameState.Tie);
                    }
                    break;

            }
        }

        private void Field_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (TaggedCords == null) return;

            int x, y;
            getTabBoardCords(e, out x, out y);
            if (new Point(x, y) == TaggedCords)
            {
                Ellipse? ellipse = null;
                foreach (FrameworkElement element in Visual.Field.Children)
                {
                    if (((element as Ellipse)?.Tag as Point?) == TaggedCords)
                        ellipse = (Ellipse)element;
                }
                if (ellipse != null)
                {
                    Visual.Field.Children.Remove(ellipse);
                    return;
                }

                ellipse = new Ellipse();
                ellipse.Tag = TaggedCords;
                ellipse.Stroke = new SolidColorBrush(Visual.TagsColor);
                ellipse.StrokeThickness = 5;
                Grid.SetColumn(ellipse, (int)((Point)TaggedCords).X);
                Grid.SetRow(ellipse, (int)((Point)TaggedCords).Y);

                Visual.Field.Children.Add(ellipse);
            }
            else
            {
                Line? line = null;
                foreach (FrameworkElement element in Visual.Field.Children)
                {
                    if (element is Line)
                    {
                        Line ln = (Line)element;
                        Point endPos = (Point)ln.Tag;
                        if (ln.X1 == Visual.Field.ActualWidth / 8 * ((Point)TaggedCords).X + Visual.Field.ActualWidth / 16 &&
                            ln.Y1 == Visual.Field.ActualHeight / 8 * ((Point)TaggedCords).Y + Visual.Field.ActualHeight / 16 &&
                            ln.X2 == Visual.Field.ActualWidth / 8 * endPos.X + Visual.Field.ActualWidth / 16 &&
                            ln.Y2 == Visual.Field.ActualHeight / 8 * endPos.Y + Visual.Field.ActualHeight / 16)
                            line = ln;
                    }
                }
                if (line != null)
                {
                    Visual.Field.Children.Remove(line);
                    return;
                }

                line = new Line();
                line.Tag = new Point(x, y);
                line.Stroke = new SolidColorBrush(Visual.TagsColor);
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Triangle;
                line.StrokeThickness = 25;
                Grid.SetRowSpan(line, 8);
                Grid.SetColumnSpan(line, 8);
                Grid.SetColumn(line, 0);
                Grid.SetRow(line, 0);

                line.X1 = Visual.Field.ActualWidth / 8 * ((Point)TaggedCords).X + Visual.Field.ActualWidth / 16;
                line.Y1 = Visual.Field.ActualHeight / 8 * ((Point)TaggedCords).Y + Visual.Field.ActualHeight / 16;

                line.X2 = Visual.Field.ActualWidth / 8 * x + Visual.Field.ActualWidth / 16;
                line.Y2 = Visual.Field.ActualHeight / 8 * y + Visual.Field.ActualHeight / 16;

                Visual.Field.Children.Add(line);
            }
        }

        private void Field_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int x, y;
            getTabBoardCords(e, out x, out y);
            TaggedCords = new Point(x, y);
        }

        void getTabBoardCords(MouseButtonEventArgs e, out int cN, out int rN)
        {
            cN = 0;
            rN = 0;
            Point mousePos = e.GetPosition(Visual.Field);

            double width = 0;
            for (int i = 0; i < Visual.Field.ColumnDefinitions.Count; i++)
            {
                width += Visual.Field.ColumnDefinitions[i].ActualWidth;
                if (width > mousePos.X)
                {
                    cN = i;
                    break;
                }
            }

            double height = 0;
            for (int i = 0; i < Visual.Field.RowDefinitions.Count; i++)
            {
                height += Visual.Field.RowDefinitions[i].ActualHeight;
                if (height > mousePos.Y)
                {
                    rN = i;
                    break;
                }
            }
        }

        void FinalGame(GameState state)
        {
            switch (state)
            {
                case GameState.Checkmate:
                    NotaionList[^1] = NotaionList[^1].Substring(0, NotaionList[^1].Length - 1) + "#";
                    string str = CurrentCourse ? "Черные" : "Белые";
                    MessageBox.Show($"{str} победили!");
                    State = GameState.Checkmate;
                    break;
                case GameState.Tie:
                    MessageBox.Show("Ничья");
                    State = GameState.Tie;
                    break;
            }
            Dispose();
        }

        void ChangeCurrent()
        {
            CurrentCourse = !CurrentCourse;
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (Figures[i, j] != null)
                    {
                        if (Figures[i, j]!.IsWhiteTeam == CurrentCourse)
                            Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                        else Figures[i, j]!.Sprite.MouseDown -= ChessFocusSelect;
                    }
                }
            }
        }

        async void PCMove()
        {
            if (Opponent == OpponentType.PC)
            {
                string[] posArr = new string[NotaionList.Count];

                for (int i = 0; i < NotaionList.Count; i++)
                {
                    posArr[i] = NotaionList[i].Replace("-", "");
                    posArr[i] = posArr[i].Replace("x", "");
                    posArr[i] = posArr[i].Replace("+", "");
                    posArr[i] = posArr[i].Replace("N", "");
                    posArr[i] = posArr[i].Replace("K", "");
                    posArr[i] = posArr[i].Replace("Q", "");
                    posArr[i] = posArr[i].Replace("B", "");
                    posArr[i] = posArr[i].Replace("R", "");
                }

                string? str = "";
                void setPositionMethod()
                {
                    for (int i = 0; i < Length; i++)
                    {
                        for (int j = 0; j < Length; j++)
                        {
                            if (Figures[i, j] != null)
                                Figures[i, j]!.Sprite.MouseDown -= ChessFocusSelect;
                        }
                    }
                    Stockfish?.SetPosition(posArr);
                    str = Stockfish?.GetBestMove();
                }
                
                SetPositionDelegate setPositionDel = setPositionMethod;
                await Task.Run(() => setPositionDel());
                
                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        if (Figures[i, j] != null && Figures[i, j]?.IsWhiteTeam == Visual.IsWhiteBehind)
                            Figures[i, j]!.Sprite.MouseDown += ChessFocusSelect;
                    }
                }
                MoveFigures(convertIdx(str?.Substring(0, 2)), convertIdx(str?.Substring(2, 2)));
                
                Point convertIdx(string? str)
                {
                    if (str == null) return new Point(-1, -1);

                    Point res = new Point(-1, -1);

                    if (Visual.IsWhiteBehind)
                    {
                        switch (str[0])
                        {
                            case 'a': res.Y = 0; break;
                            case 'b': res.Y = 1; break;
                            case 'c': res.Y = 2; break;
                            case 'd': res.Y = 3; break;
                            case 'e': res.Y = 4; break;
                            case 'f': res.Y = 5; break;
                            case 'g': res.Y = 6; break;
                            case 'h': res.Y = 7; break;
                        }
                        res.X = Math.Abs(Int32.Parse(new string(new char[] { str[^1] })) - 8);
                    }
                    else
                    {
                        switch (str[0])
                        {
                            case 'h': res.Y = 0; break;
                            case 'g': res.Y = 1; break;
                            case 'f': res.Y = 2; break;
                            case 'e': res.Y = 3; break;
                            case 'd': res.Y = 4; break;
                            case 'c': res.Y = 5; break;
                            case 'b': res.Y = 6; break;
                            case 'a': res.Y = 7; break;
                        }
                        res.X = Int32.Parse(new string(new char[] { str[str.Length - 1] })) - 1;
                    }
                    return res;
                }
            }
        }
        
        private void ChessFocusSelect(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;

            ChessFigure? currentFigure = (ChessFigure?)((Image?)sender)?.Tag;
            if (currentFigure == null) return;

            if (SelectedFigure != null) ClearSelected();

            SelectedFigureCords = new Point(currentFigure.CurrentPos.X, currentFigure.CurrentPos.Y);

            Rectangle box = new Rectangle();
            RadialGradientBrush brush = new RadialGradientBrush(Visual.SelectedFigureColor, Color.FromArgb(0, 0, 0, 0));
            brush.RadiusX = 2;
            brush.RadiusY = 2;
            box.Fill = brush;
            box.Opacity = .5;
            box.Tag = "selected";
            Grid.SetRow(box, (int)SelectedFigure!.CurrentPos.X);
            Grid.SetColumn(box, (int)SelectedFigure.CurrentPos.Y);
            Visual.Field.Children.Add(box);


            Point[] points = currentFigure.Move();

            for (int i = 0; i < points.Length; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = new SolidColorBrush(Visual.SelectedStep);
                ellipse.Stroke = new SolidColorBrush(Visual.SelectedStep);
                ellipse.Width = (Visual.Field.ActualHeight / 8) / 2;
                ellipse.Height = ellipse.Width;
                ellipse.Opacity = .5;
                ellipse.Tag = "selected";
                Grid.SetRow(ellipse, (int)points[i].X);
                Grid.SetColumn(ellipse, (int)points[i].Y);
                Visual.Field.Children.Add(ellipse);
            }

            e.Handled = true;
        }

        public void Display()
        {
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (Figures[i, j] != null)
                    {
                        Grid.SetRow(Figures[i, j]?.Sprite, i);
                        Grid.SetColumn(Figures[i, j]?.Sprite, j);
                        Visual.Field.Children.Add(Figures[i, j]?.Sprite);
                    }
                }
            }
        }

        void ClearSelected()
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            foreach (FrameworkElement item in Visual.Field.Children)
            {
                if ((item.Tag as string) == "selected")
                    elements.Add(item);
            }

            foreach (FrameworkElement item in elements)
                Visual.Field.Children.Remove(item);

            SelectedFigureCords = new Point(-1, -1);
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (CurrentCourse)
            {
                TimeFirstTeam = TimeFirstTeam.AddSeconds(-1);
                Visual.TimeLeftFirstTeam.Text = $"First team time left: {TimeFirstTeam.ToLongTimeString()}";
                if (TimeFirstTeam.ToLongTimeString() == "0:00:00")
                    FinalGame(GameState.Checkmate);
            }
            else
            {
                TimeSecondTeam = TimeSecondTeam.AddSeconds(-1);
                Visual.TimeLeftSecondTeam.Text = $"Second team time left: {TimeSecondTeam.ToLongTimeString()}";
                if (TimeSecondTeam.ToLongTimeString() == "0:00:00")
                    FinalGame(GameState.Checkmate);
            }
        }

        public ChessFigure? this[char letter, int idx]
        {
            get
            {
                int i = 0;
                switch (letter)
                {
                    case 'A': i = 1; break;
                    case 'B': i = 2; break;
                    case 'C': i = 3; break;
                    case 'D': i = 4; break;
                    case 'E': i = 5; break;
                    case 'F': i = 6; break;
                    case 'G': i = 7; break;
                    case 'H': i = 8; break;
                }
                return Figures[i, idx];
            }
        }

        public ChessFigure? this[int i, int j] { get => Figures[i, j]; }

        public void Dispose()
        {
            MakeMovePlayer.Stop();
            GameTimer.Stop();
            Visual.Field.MouseLeftButtonDown -= MakeMove;
            Visual.Field.MouseRightButtonDown -= Field_MouseRightButtonDown;
            Visual.Field.MouseRightButtonUp -= Field_MouseRightButtonUp;
        }
    }
}
