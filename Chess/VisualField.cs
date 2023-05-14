using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Chess
{
    public class VisualField
    {

        Grid ChessBoard;
        public Grid Field { get; set; }

        public Color LightCell;
        public Color DarkCell;

        public Color SecondTeam { get; set; } = Colors.Black;
        public Color FirstTeam { get; set; } = Colors.White;
        public Color SelectedStep { get; set; }
        public Color SelectedFigureColor { get; set; }
        public Color TagsColor { get; set; }

        public string FiguresSprite { get; set; }

        public TextBlock TimeLeftFirstTeam { get; set; }
        public TextBlock TimeLeftSecondTeam { get; set; }
        public ListView Notation { get; set; }
        public StackPanel EatenFirstTeam { get; set; }
        public StackPanel EatenSecondTeam { get; set; }

        public bool IsWhiteBehind;

        int LetterSize = 22;

        int SizeField = 8;

        public VisualField(Grid field, Grid chessBoard, StackPanel eatenFirstTeam, StackPanel eatenSecondTeam,
            ListView notation, TextBlock timeLeftTeamFirst, TextBlock timeLeftTeamSecond, bool isWhiteBehind)
        {
            TimeLeftFirstTeam = timeLeftTeamFirst;
            TimeLeftSecondTeam = timeLeftTeamSecond;
            Notation = notation;
            Field = field;
            EatenFirstTeam = eatenFirstTeam;
            EatenSecondTeam = eatenSecondTeam;

            StreamReader rdr = new StreamReader("save.dat", Encoding.Default);

            SelectedFigureColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
            SelectedStep = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
            TagsColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
            LightCell = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
            DarkCell = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
            FiguresSprite = rdr.ReadLine()!;

            rdr.Close();

            this.Clear();
            IsWhiteBehind = isWhiteBehind;

            Field = field;
            ChessBoard = chessBoard;

            EatenFirstTeam = eatenFirstTeam;
            EatenSecondTeam = eatenSecondTeam;

            this.MarkUp();
        }
        
        public void Clear()
        {
            EatenFirstTeam.Children.Clear();
            EatenSecondTeam.Children.Clear();
            Field.Children.Clear();
            Field.ColumnDefinitions.Clear();
            Field.RowDefinitions.Clear();
        }

        public void MarkUp()
        {
            for (int i = 0; i < SizeField; i++)
            {
                Field.ColumnDefinitions.Add(new ColumnDefinition());
                Field.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < ChessBoard.ColumnDefinitions.Count; i++)
            {
                for (int j = 0; j < ChessBoard.RowDefinitions.Count; j++)
                {

                    if (i == 0 && j == 0) continue;
                    if (i == 0)
                    {
                        TextBlock text = new TextBlock();
                        text.VerticalAlignment = VerticalAlignment.Center;
                        if (IsWhiteBehind) text.Text = (9 - j).ToString();
                        else text.Text = j.ToString();
                        text.FontSize = LetterSize;
                        text.HorizontalAlignment = HorizontalAlignment.Center;
                        Grid.SetColumn(text, i);
                        Grid.SetRow(text, j);
                        ChessBoard.Children.Add(text);
                        continue;
                    }
                    if (j == 0)
                    {
                        TextBlock text = new TextBlock();
                        
                        if (IsWhiteBehind)
                        {
                            switch (i)
                            {
                                case 1: text.Text = "A"; break;
                                case 2: text.Text = "B"; break;
                                case 3: text.Text = "C"; break;
                                case 4: text.Text = "D"; break;
                                case 5: text.Text = "E"; break;
                                case 6: text.Text = "F"; break;
                                case 7: text.Text = "G"; break;
                                case 8: text.Text = "H"; break;
                            }
                        }
                        else
                        {
                            switch (i)
                            {
                                case 1: text.Text = "H"; break;
                                case 2: text.Text = "G"; break;
                                case 3: text.Text = "F"; break;
                                case 4: text.Text = "E"; break;
                                case 5: text.Text = "D"; break;
                                case 6: text.Text = "C"; break;
                                case 7: text.Text = "B"; break;
                                case 8: text.Text = "A"; break;
                            }

                        }
                        text.FontSize = LetterSize;
                        text.HorizontalAlignment = HorizontalAlignment.Center;
                        Grid.SetColumn(text, i);
                        Grid.SetRow(text, j);
                        ChessBoard.Children.Add(text);
                        continue;
                    }
                }
            }

            SetColor(LightCell, DarkCell);

        }

        void SetColor(Color lightCell, Color darktCell)
        {
            int l = (int)lightCell.R + (int)lightCell.B + (int)lightCell.G;
            int d = (int)darktCell.R + (int)darktCell.B + (int)darktCell.G;

            if (l > d)
            {
                LightCell = lightCell;
                DarkCell = darktCell;
            }
            else
            {
                LightCell = darktCell;
                DarkCell = lightCell;
            }

            bool flag = true;
            for (int i = 0; i < Field.ColumnDefinitions.Count; i++)
            {
                for (int j = 0; j < Field.RowDefinitions.Count; j++)
                {
                    Rectangle box = new Rectangle();
                   // box.Stroke = new SolidColorBrush(Colors.Black);
                    if (flag)
                    {
                        box.Fill = new SolidColorBrush(LightCell);
                        flag = !flag;
                    }
                    else
                    {
                        box.Fill = new SolidColorBrush(DarkCell);
                        flag = !flag;
                    }
                    Grid.SetColumn(box, i);
                    Grid.SetRow(box, j);
                    Grid.SetZIndex(box, -1);
                    Field.Children.Add(box);
                }
                flag = !flag;
            }


        }
    }
}
