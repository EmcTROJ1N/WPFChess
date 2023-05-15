using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{
    public abstract class ChessFigure
    {
        public enum FigureType { Pawn, Rook, Knight, Bishop, Queen, King }

        public Color Team;
        public FigureType Type;
        public Image Sprite = new Image();
        public Point CurrentPos;
        public ChessField? Parent;
        public bool IsWhiteTeam;
        
        public abstract List<Point> MoveWithoutCheck();

        public bool IsFirstStep { get => CountStep == 0; }

        public int CountStep = 0;
        public virtual Point[] Move()
        {
            if (Parent == null) return new Point[0];
            List<Point> points = MoveWithoutCheck();
            List<Point> result = new List<Point>();

            foreach (Point point in points)
            {
                if (Math.Abs(CurrentPos.Y - point.Y) > 1 && Parent.State == ChessField.GameState.Check && Type == FigureType.King) continue;

                Point tmpPoint = new Point(CurrentPos.X, CurrentPos.Y);

                ChessFigure? tmpFig = Parent.Figures[(int)point.X, (int)point.Y];
                Parent.Figures[(int)point.X, (int)point.Y] = this;
                CurrentPos = new Point((int)point.X, (int)point.Y);
                Parent.Figures[(int)tmpPoint.X,(int)tmpPoint.Y] = null;

                if (Parent.State == ChessField.GameState.Continues) result.Add(point); 

                Parent.Figures[(int)tmpPoint.X,(int)tmpPoint.Y] = this;
                Parent.Figures[(int)point.X, (int)point.Y] = tmpFig;
                CurrentPos = new Point(tmpPoint.X, tmpPoint.Y);
            }

            return result.ToArray();
        }

        public ChessFigure(Point currentPos, ChessField? field, Color color)
        {
            Sprite.Tag = this;
            CurrentPos = currentPos;
            Grid.SetZIndex(Sprite, 10);
            Team = color;
            IsWhiteTeam = color == Colors.Black ? false : true;
            Parent = field;

            string pathToSprite = $"{Environment.CurrentDirectory}/Resource/Sprites/";
            pathToSprite +=  Parent != null ? Parent.Visual.FiguresSprite : File.ReadLines("save.dat").Last();
            pathToSprite += IsWhiteTeam ? "/w" : "/b";
            
            string letter = this.ToString().Substring(0, 1);

            if (new string[] { "a", "b", "c", "d", "e", "f", "g", "h" }.Contains(letter)) pathToSprite += "P";
            else pathToSprite += letter;

            pathToSprite += ".png";
            Sprite.Source = new BitmapImage(new Uri(pathToSprite));

        }

        protected bool InBorder(int i, int j, int border = 8)
        {
            if (i < 0) return false;
            if (j < 0) return false;
            if (i > border - 1) return false;
            if (j > border - 1) return false;

            return true;
        }

        public override string ToString()
        {
            if (Parent?.Visual.IsWhiteBehind == true || Parent == null)
            {
                switch (CurrentPos.Y)
                {
                    case 0: return $"a{Math.Abs(CurrentPos.X - 8)}";
                    case 1: return $"b{Math.Abs(CurrentPos.X - 8)}";
                    case 2: return $"c{Math.Abs(CurrentPos.X - 8)}";
                    case 3: return $"d{Math.Abs(CurrentPos.X - 8)}";
                    case 4: return $"e{Math.Abs(CurrentPos.X - 8)}";
                    case 5: return $"f{Math.Abs(CurrentPos.X - 8)}";
                    case 6: return $"g{Math.Abs(CurrentPos.X - 8)}";
                    case 7: return $"h{Math.Abs(CurrentPos.X - 8)}";
                }
            }
            else
            {
                switch (CurrentPos.Y)
                {
                    case 0: return $"h{CurrentPos.X + 1}";
                    case 1: return $"g{CurrentPos.X + 1}";
                    case 2: return $"f{CurrentPos.X + 1}";
                    case 3: return $"e{CurrentPos.X + 1}";
                    case 4: return $"d{CurrentPos.X + 1}";
                    case 5: return $"c{CurrentPos.X + 1}";
                    case 6: return $"b{CurrentPos.X + 1}";
                    case 7: return $"a{CurrentPos.X + 1}";
                }
            }
            return "none";
        }
    }

    class Pawn : ChessFigure
    {
        public int Step;
        public bool Pass_takeover = false;
        public int OldCountSteps = 0;

        public Pawn(Color team, Point currentPos, ChessField? field = null) : base(currentPos, field, team)
        {
            Type = FigureType.Pawn;

            if (Parent != null && Parent.Visual.IsWhiteBehind) Step = IsWhiteTeam ? -1 : 1;
            else Step = IsWhiteTeam ? 1 : -1;
        }

        public override string ToString() => base.ToString();

        public override List<Point> MoveWithoutCheck()
        {
           List<Point> points = new List<Point>();
           if (Parent == null || Parent.Figures == null) return points;
         
           if (Pass_takeover)
           {
               if ((CurrentPos.X!=4 && CurrentPos.X != 3) || OldCountSteps < Parent.NotaionList.Count)
                   Pass_takeover = false;
           }  
        

           if (InBorder((int)CurrentPos.X + Step, (int)CurrentPos.Y))
            if (Parent.Figures?[(int)CurrentPos.X + Step, (int)CurrentPos.Y] == null)
            {
                points.Add(new Point(CurrentPos.X + Step, CurrentPos.Y));

                if (InBorder((int)CurrentPos.X + Step + Step, (int)CurrentPos.Y))
                {
                    if ((int)CurrentPos.X + Step + Step < Parent.Figures?.Length
                        && Parent.Figures?[(int)CurrentPos.X + Step + Step, (int)CurrentPos.Y] == null && (new double[] { 1, 6 }.Contains(CurrentPos.X)))
                    {
                        points.Add(new Point(CurrentPos.X + Step + Step, CurrentPos.Y));
                        //Pass_takeover = true;
                    }
                }
            }

            move(1);
            move(-1);


            void move(int y)
            {
                if (InBorder((int)CurrentPos.X + Step, (int)CurrentPos.Y + y))
                {
                    if (Parent!.Figures![(int)CurrentPos.X + Step, (int)CurrentPos.Y + y] != null)
                        if (Parent!.Figures![(int)CurrentPos.X + Step, (int)CurrentPos.Y + y]!.IsWhiteTeam != IsWhiteTeam)
                        points.Add(new Point(CurrentPos.X + Step, CurrentPos.Y + y));

                   if ((CurrentPos.X == 3 || CurrentPos.X == 4) && Parent.Figures?[(int)CurrentPos.X + Step, (int)CurrentPos.Y + y] == null)
                        if (Parent.Figures?[(int)CurrentPos.X, (int)CurrentPos.Y + y] != null)
                            if (Parent.Figures[(int)CurrentPos.X, (int)CurrentPos.Y + y]!.IsWhiteTeam != IsWhiteTeam &&
                                (Parent.Figures?[(int)CurrentPos.X, (int)CurrentPos.Y + y] as Pawn)?.Pass_takeover == true &&
                                Parent.Figures?[(int)CurrentPos.X, (int)CurrentPos.Y + y]!.CountStep == 1)
                                points.Add(new Point(CurrentPos.X + Step, CurrentPos.Y + y));


                }
            }
            return points;
        }
    }

    class Rook : ChessFigure
    {
        public Rook(Color team, Point currentPos, ChessField? field = null) :base(currentPos, field, team)
        {
            Type = FigureType.Rook;
            IsWhiteTeam = Team == Colors.Black ? false : true;
        }

        public override string ToString() => $"R{base.ToString()}";

        public override List<Point> MoveWithoutCheck()
        {
            List<Point> points = new List<Point>();
            int i = 0;

            for (i = (int)CurrentPos.X + 1; InBorder(i, (int)CurrentPos.Y) && Parent.Figures?[i, (int)CurrentPos.Y] == null; i++)
                points.Add(new Point(i, CurrentPos.Y));

            if (InBorder(i, (int)CurrentPos.Y))
            {
                if (Parent.Figures?[i, (int)CurrentPos.Y] != null && Parent.Figures?[i, (int)CurrentPos.Y].IsWhiteTeam!=IsWhiteTeam)
                    points.Add(new Point(i, CurrentPos.Y));
            }  

            for (i = (int)CurrentPos.X - 1; InBorder(i, (int)CurrentPos.Y) && Parent.Figures?[i, (int)CurrentPos.Y] == null; i--)
                points.Add(new Point(i, CurrentPos.Y));
            if (InBorder(i, (int)CurrentPos.Y))
            {
                if (Parent.Figures?[i, (int)CurrentPos.Y] != null && Parent.Figures?[i, (int)CurrentPos.Y].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point(i, CurrentPos.Y));
            }

            for (i = (int)CurrentPos.Y + 1; InBorder((int)CurrentPos.X, i) && Parent.Figures?[(int)CurrentPos.X, i] == null; i++)
                points.Add(new Point(CurrentPos.X, i));
            if (InBorder((int)CurrentPos.X, i))
            {
                if (Parent.Figures?[(int)CurrentPos.X, i] != null && Parent.Figures?[(int)CurrentPos.X, i].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X, i));
            }

            for (i = (int)CurrentPos.Y - 1; InBorder((int)CurrentPos.X, i) && Parent.Figures?[(int)CurrentPos.X, i] == null; i--)
                points.Add(new Point(CurrentPos.X, i));

            if (InBorder((int)CurrentPos.X, i))
            {
                if (Parent.Figures?[(int)CurrentPos.X, i] != null && Parent.Figures?[(int)CurrentPos.X, i].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X, i));
            }

            return points;
        }
    }

    class Knight : ChessFigure
    {
        public Knight(Color team, Point currentPos, ChessField? field = null) :base(currentPos, field, team)
        {
            IsWhiteTeam = Team == Colors.Black ? false : true;
            Type = FigureType.Knight;
        }
        public override string ToString() => $"N{base.ToString()}";

        public override List<Point> MoveWithoutCheck()
        {
            List<Point> points = new List<Point>();

            if (InBorder((int)CurrentPos.X + 2, (int)CurrentPos.Y + 1))
            {
                if (Parent?.Figures?[(int)CurrentPos.X + 2, (int)CurrentPos.Y + 1] ==null)
                    points.Add(new Point((int)CurrentPos.X + 2, (int)CurrentPos.Y + 1));
                else if (Parent.Figures?[(int)CurrentPos.X + 2, (int)CurrentPos.Y + 1]!.IsWhiteTeam!=IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X + 2, (int)CurrentPos.Y + 1));
            }

            if (InBorder((int)CurrentPos.X + 2, (int)CurrentPos.Y - 1))
            {
                if (Parent?.Figures?[(int)CurrentPos.X + 2, (int)CurrentPos.Y - 1] == null)
                    points.Add(new Point((int)CurrentPos.X + 2, (int)CurrentPos.Y - 1));
                else if (Parent.Figures?[(int)CurrentPos.X + 2, (int)CurrentPos.Y - 1]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X + 2, (int)CurrentPos.Y - 1));
            }

            if (InBorder((int)CurrentPos.X - 2, (int)CurrentPos.Y + 1))
            {
                if (Parent.Figures?[(int)CurrentPos.X - 2, (int)CurrentPos.Y + 1] == null)
                    points.Add(new Point((int)CurrentPos.X - 2, (int)CurrentPos.Y + 1));
                else if (Parent.Figures?[(int)CurrentPos.X - 2, (int)CurrentPos.Y + 1]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X - 2, (int)CurrentPos.Y + 1));
            }

            if (InBorder((int)CurrentPos.X - 2, (int)CurrentPos.Y - 1))
            {
                if (Parent.Figures?[(int)CurrentPos.X - 2, (int)CurrentPos.Y - 1] == null)
                    points.Add(new Point((int)CurrentPos.X - 2, (int)CurrentPos.Y - 1));
                else if (Parent.Figures?[(int)CurrentPos.X - 2, (int)CurrentPos.Y - 1]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X - 2, (int)CurrentPos.Y - 1));
            }


            if (InBorder((int)CurrentPos.X + 1, (int)CurrentPos.Y + 2))
            {
                if (Parent.Figures?[(int)CurrentPos.X + 1, (int)CurrentPos.Y + 2] == null)
                    points.Add(new Point((int)CurrentPos.X + 1, (int)CurrentPos.Y + 2));
                else if (Parent.Figures?[(int)CurrentPos.X + 1, (int)CurrentPos.Y + 2]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X + 1, (int)CurrentPos.Y + 2));
            }

            if (InBorder((int)CurrentPos.X - 1, (int)CurrentPos.Y + 2))
            {
                if (Parent.Figures?[(int)CurrentPos.X - 1, (int)CurrentPos.Y + 2] == null)
                    points.Add(new Point((int)CurrentPos.X - 1, (int)CurrentPos.Y + 2));
                else if (Parent.Figures?[(int)CurrentPos.X - 1, (int)CurrentPos.Y + 2]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X - 1, (int)CurrentPos.Y + 2));
            }

            if (InBorder((int)CurrentPos.X + 1, (int)CurrentPos.Y - 2))
            {
                if (Parent.Figures?[(int)CurrentPos.X + 1, (int)CurrentPos.Y - 2] == null)
                    points.Add(new Point((int)CurrentPos.X + 1, (int)CurrentPos.Y - 2));
                else if (Parent.Figures?[(int)CurrentPos.X + 1, (int)CurrentPos.Y - 2]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X + 1, (int)CurrentPos.Y - 2));
            }

            if (InBorder((int)CurrentPos.X - 1, (int)CurrentPos.Y - 2))
            {
                if (Parent.Figures?[(int)CurrentPos.X - 1, (int)CurrentPos.Y - 2] == null)
                    points.Add(new Point((int)CurrentPos.X - 1, (int)CurrentPos.Y - 2));
                else if (Parent.Figures?[(int)CurrentPos.X - 1, (int)CurrentPos.Y - 2]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X - 1, (int)CurrentPos.Y - 2));
            }

            return points;
        }
    }

    class Bishop : ChessFigure
    {
        public Bishop(Color team, Point currentPos, ChessField? field = null) :base(currentPos, field, team)
        {
            IsWhiteTeam = Team == Colors.Black ? false : true;
            Type = FigureType.Bishop;
        }
        public override string ToString() => $"B{base.ToString()}";

        public override List<Point> MoveWithoutCheck()
        {
            List<Point> points = new List<Point>();

            int i = 0, j = 0;

            for (i = (int)CurrentPos.X + 1, j = (int)CurrentPos.Y + 1; InBorder(i, j) && Parent.Figures?[i, j] == null; i++, j++)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent.Figures?[i, j] != null)
                    if (Parent.Figures?[i, j].IsWhiteTeam!=IsWhiteTeam) points.Add(new Point(i, j));
            

            for (i = (int)CurrentPos.X - 1, j = (int)CurrentPos.Y - 1; InBorder(i, j) && Parent.Figures?[i, j] == null; i--, j--)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent.Figures?[i, j] != null)
                    if (Parent.Figures?[i, j].IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            for (i = (int)CurrentPos.X + 1, j = (int)CurrentPos.Y - 1; InBorder(i, j) && Parent.Figures?[i, j] == null; i++, j--)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent.Figures?[i, j] != null)
                    if (Parent.Figures?[i, j].IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            for (i = (int)CurrentPos.X - 1, j = (int)CurrentPos.Y + 1; InBorder(i, j) && Parent.Figures?[i, j] == null; i--, j++)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent.Figures?[i, j] != null)
                    if (Parent.Figures?[i, j].IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            return points;
        }
    }

    class Queen : ChessFigure
    {
        public Queen(Color team, Point currentPos, ChessField? field = null) :base(currentPos, field, team)
        {
            IsWhiteTeam = Team == Colors.Black ? false : true;
            Type = FigureType.Queen;
        }
        public override string ToString() => $"Q{base.ToString()}";

        public override List<Point> MoveWithoutCheck()
        {
            List<Point> points = new List<Point>();


            int i = 0, j = 0;

            for (i = (int)CurrentPos.X + 1, j = (int)CurrentPos.Y + 1; InBorder(i, j) && Parent.Figures[i, j] == null; i++, j++)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent!.Figures![i, j] != null)
                    if (Parent.Figures[i, j]!.IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));


            for (i = (int)CurrentPos.X - 1, j = (int)CurrentPos.Y - 1; InBorder(i, j) && Parent.Figures[i, j] == null; i--, j--)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent!.Figures[i, j] != null)
                    if (Parent.Figures[i, j]!.IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            for (i = (int)CurrentPos.X + 1, j = (int)CurrentPos.Y - 1; InBorder(i, j) && Parent!.Figures[i, j] == null; i++, j--)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent!.Figures!?[i, j] != null)
                    if (Parent.Figures?[i, j]!.IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            for (i = (int)CurrentPos.X - 1, j = (int)CurrentPos.Y + 1; InBorder(i, j) && Parent!.Figures!?[i, j] == null; i--, j++)
                points.Add(new Point(i, j));
            if (InBorder(i, j))
                if (Parent!.Figures!?[i, j] != null)
                    if (Parent.Figures?[i, j]!.IsWhiteTeam != IsWhiteTeam) points.Add(new Point(i, j));

            for (i = (int)CurrentPos.X + 1; InBorder(i, (int)CurrentPos.Y) && Parent!.Figures!?[i, (int)CurrentPos.Y] == null; i++)
                points.Add(new Point(i, CurrentPos.Y));

            if (InBorder(i, (int)CurrentPos.Y))
            {
                if (Parent!.Figures![i, (int)CurrentPos.Y] != null && Parent.Figures[i, (int)CurrentPos.Y]!.IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point(i, CurrentPos.Y));
            }

            for (i = (int)CurrentPos.X - 1; InBorder(i, (int)CurrentPos.Y) && Parent?.Figures![i, (int)CurrentPos.Y] == null; i--)
                points.Add(new Point(i, CurrentPos.Y));
            if (InBorder(i, (int)CurrentPos.Y))
            {
                if (Parent.Figures?[i, (int)CurrentPos.Y] != null && Parent.Figures?[i, (int)CurrentPos.Y].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point(i, CurrentPos.Y));
            }

            for (i = (int)CurrentPos.Y + 1; InBorder((int)CurrentPos.X, i) && Parent.Figures?[(int)CurrentPos.X, i] == null; i++)
                points.Add(new Point(CurrentPos.X, i));
            if (InBorder((int)CurrentPos.X, i))
            {
                if (Parent.Figures?[(int)CurrentPos.X, i] != null && Parent.Figures?[(int)CurrentPos.X, i].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X, i));
            }

            for (i = (int)CurrentPos.Y - 1; InBorder((int)CurrentPos.X, i) && Parent.Figures?[(int)CurrentPos.X, i] == null; i--)
                points.Add(new Point(CurrentPos.X, i));

            if (InBorder((int)CurrentPos.X, i))
            {
                if (Parent.Figures?[(int)CurrentPos.X, i] != null && Parent.Figures?[(int)CurrentPos.X, i].IsWhiteTeam != IsWhiteTeam)
                    points.Add(new Point((int)CurrentPos.X, i));
            }



            return points;
        }
    }

    class King : ChessFigure
    {
        public King(Color team, Point currentPos, ChessField? field = null) :base(currentPos, field, team)
        {
            IsWhiteTeam = Team == Colors.Black ? false : true;
            Type = FigureType.King;
        }
        public override string ToString() => $"K{base.ToString()}";

        public override List<Point> MoveWithoutCheck()
        {
            List<Point> points = new List<Point>();
            if (Parent == null || Parent.Figures == null) return points;

            move(1, 0);
            move(0, 1);
            move(-1, 0);
            move(0, -1);
            move(1, 1);
            move(-1, -1);
            move(-1, 1);
            move(1, -1);

            void move(int x, int y)
            {
                if (InBorder((int)CurrentPos.X + x, (int)CurrentPos.Y + y))
                {
                    if (Parent.Figures[(int)CurrentPos.X + x, (int)CurrentPos.Y + y] != null)
                    {
                        if (Parent.Figures[(int)CurrentPos.X + x, (int)CurrentPos.Y + y]?.IsWhiteTeam != IsWhiteTeam)
                            points.Add(new Point((int)CurrentPos.X + x, (int)CurrentPos.Y + y));
                    }
                    else
                        points.Add(new Point((int)CurrentPos.X + x, (int)CurrentPos.Y + y));
                }
            }

            bool isFirstCastle = true;
            bool isSecondCastle = true;
            
            if (IsFirstStep)
            {
                for (int i = (int)CurrentPos.Y + 1, j = (int)CurrentPos.Y - 1; i < 7; i++, j--)
                {
                    if (Parent.Figures![(int)CurrentPos.X, i] != null) isFirstCastle = false;
                    if (j > 0 && Parent.Figures![(int)CurrentPos.X, j] != null) isSecondCastle = false;
                    for (int x = 0; x < 8; x++)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            if ((Parent.Figures[x, k] == null || Parent.Figures[x, k]?.Type == FigureType.King) ||
                                Parent.Figures?[x, k]?.IsWhiteTeam == this.IsWhiteTeam) continue;
                            List<Point> steps = Parent.Figures![x, k]!.MoveWithoutCheck();
                            isFirstCastle = steps.Contains(new Point(CurrentPos.X, i)) ? false : isFirstCastle;
                            isSecondCastle = steps.Contains(new Point(CurrentPos.X, j)) ? false : isSecondCastle;
                        }
                    }
                }

                isFirstCastle = Parent.Figures[(int)CurrentPos.X, 7] == null ? false : isFirstCastle;
                isSecondCastle = Parent.Figures[(int)CurrentPos.X, 0] == null ? false : isSecondCastle;

                isFirstCastle = Parent.Figures[(int)CurrentPos.X, 7]?.Type == FigureType.Rook ? isFirstCastle : false;
                isSecondCastle = Parent.Figures[(int)CurrentPos.X, 0]?.Type == FigureType.Rook ? isSecondCastle : false;

                if (isFirstCastle) isFirstCastle = Parent.Figures[(int)CurrentPos.X, 7]!.IsFirstStep ? isFirstCastle : false;
                if (isSecondCastle) isSecondCastle = Parent.Figures[(int)CurrentPos.X, 0]!.IsFirstStep ? isSecondCastle : false;

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if ((Parent.Figures[i, j] == null || Parent.Figures[i, j]!.Type == FigureType.King) ||
                             Parent.Figures[i, j]?.IsWhiteTeam == this.IsWhiteTeam) continue;
                        List<Point> steps = Parent.Figures![i, j]!.MoveWithoutCheck();
                        isSecondCastle = steps.Contains(new Point(CurrentPos.X, 0)) ? false : isSecondCastle;
                        isFirstCastle = steps.Contains(new Point(CurrentPos.X, 7)) ? false : isFirstCastle;
                    }
                }

                if (isFirstCastle) points.Add(new Point((int)CurrentPos.X, (int)CurrentPos.Y + 2));
                if (isSecondCastle) points.Add(new Point((int)CurrentPos.X, (int)CurrentPos.Y - 2));
            }
            return points;
        }
    }
}
