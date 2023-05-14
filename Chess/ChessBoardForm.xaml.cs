using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for ChessBoardForm.xaml
    /// </summary>
    partial class ChessBoardForm : Window
    {
        ChessField Chess;
        VisualField ChessVisual;
        ChessField.OpponentType Opponent;
        bool Rotated = false;
        int Level;
        DateTime TimeLeft;

        internal ChessBoardForm(ChessField.OpponentType type, int skillLevel, DateTime timeLeft, bool isWhiteBehind)
        {
            InitializeComponent();
            Opponent = type;
            Level = skillLevel;
            TimeLeft = timeLeft;

            ChessVisual = new VisualField(Field, ChessBoard, EatenFirstTeam, EatemSecondTeam, NotationView,
                TimeLeftLabelTeamFirst, TimeLeftLabelTeamSecond, isWhiteBehind);
            Chess = new ChessField(ChessVisual, Opponent, skillLevel, timeLeft, timeLeft);
        }

        internal ChessBoardForm(ChessFigure[,] chessFigures, ChessField.OpponentType type, bool currentCourse, int skillLevel, bool isWhiteBehind,
            DateTime? timeFirstTeam = null, DateTime? timeSecondTeam = null, List<string>? notation = null, List<string>? eatenList = null)
        {
            InitializeComponent();
            Opponent = type;
            if (timeFirstTeam != null)
                TimeLeft = (DateTime)timeFirstTeam;

            ChessVisual = new VisualField(Field, ChessBoard, EatenFirstTeam, EatemSecondTeam, NotationView,
                TimeLeftLabelTeamFirst, TimeLeftLabelTeamSecond, isWhiteBehind);

            Chess = new ChessField(chessFigures, ChessVisual, Opponent, currentCourse, skillLevel,
                timeFirstTeam, timeSecondTeam, notation, eatenList);
        }


        private void NewGame(object sender, RoutedEventArgs e)
        {
            ChessVisual.Clear();
            ChessVisual.MarkUp();
            Chess.Dispose();

            Chess = new ChessField(ChessVisual, Opponent, Level, TimeLeft, TimeLeft);
        }

        private void SaveGame(object sender, RoutedEventArgs e)
        {
            StreamWriter wrtr = new StreamWriter("data.log", false, Encoding.Default);
            wrtr.WriteLine(Opponent);

            if (Opponent == ChessField.OpponentType.PC) wrtr.WriteLine(Chess.Level);
            wrtr.WriteLine(ChessVisual.IsWhiteBehind);
            wrtr.WriteLine(Chess.CurrentCourse);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    wrtr.WriteLine(Chess[i, j]?.Team);
                    wrtr.WriteLine(Chess[i, j]?.Type);
                    wrtr.WriteLine(Chess[i, j]?.CurrentPos.X);
                    wrtr.WriteLine(Chess[i, j]?.CurrentPos.Y);
                    wrtr.WriteLine(Chess[i, j]?.CountStep);
                    if (Chess[i, j]?.Type == ChessFigure.FigureType.Pawn)
                    {
                        wrtr.WriteLine(((Pawn)Chess[i, j]!).Pass_takeover);
                        wrtr.WriteLine(((Pawn)Chess[i, j]!).OldCountSteps);
                    }
                }
            }

            wrtr.WriteLine(Chess.TimeFirstTeam);
            wrtr.WriteLine(Chess.TimeSecondTeam);

            foreach (string str in NotationView.ItemsSource)
                wrtr.WriteLine(str);
            wrtr.WriteLine("");

            foreach (FrameworkElement element in ChessVisual.EatenFirstTeam.Children)
                wrtr.WriteLine((element as Image)?.Source);
            wrtr.WriteLine("");
            foreach (FrameworkElement element in ChessVisual.EatenSecondTeam.Children)
                wrtr.WriteLine((element as Image)?.Source);
            
            wrtr.Close();
        }

        private void SaveSteps(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == true)
            {
                StreamWriter wrtr = new StreamWriter(sf.FileName, false, Encoding.Default);
                foreach (string step in NotationView.ItemsSource)
                    wrtr.WriteLine(step);
                wrtr.Close();
            }
        }

        private void Exit(object sender, RoutedEventArgs e) => this.Close();
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e) => TickPlayer.Position = TimeSpan.Zero;

        private void RotateField(object sender, RoutedEventArgs e)
        {
            RotateTransform rotate = new RotateTransform(Rotated ? 0 : 180);
            
            for (int i = 0; i < ChessBoard.Children.Count; i++)
            {
                ChessBoard.Children[i].RenderTransformOrigin = new Point(.5, .5);
                ChessBoard.Children[i].RenderTransform = rotate;
            }
            
            for (int i = 0; i < Field.Children.Count; i++)
            {
                Field.Children[i].RenderTransformOrigin = new Point(.5, .5);
                Field.Children[i].RenderTransform = rotate;
            }

            Rotated = !Rotated;
        }

        private void DeleteTags(object sender, RoutedEventArgs e)
        {
            List<int> idxs = new List<int>();

            for (int i = 0; i < ChessVisual.Field.Children.Count; i++)
            {
                if (((FrameworkElement)ChessVisual.Field.Children[i]).Tag is Point)
                    idxs.Add(i);
            }
            for (int i = idxs.Count - 1; i >= 0; i--)
                ChessVisual.Field.Children.RemoveAt(idxs[i]);
        }
    }
}
