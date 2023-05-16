using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Chess
{
    public class DifficultyLevel
    {
        public string Title { get; set; }
        public Brush RectColor { get; set; }

        public DifficultyLevel(string title, Brush rectColor)
        {
            Title = title;
            RectColor = rectColor;
        }

        public override string ToString() => Title;
    }

    public partial class ChooseSettingsBeforeGame : Window
    {
        Menu _Menu;
        public bool IsWhiteBehind { get; set; }
        ObservableCollection<DifficultyLevel> Levels = new ObservableCollection<DifficultyLevel>();

        public ChooseSettingsBeforeGame(Menu menu)
        {
            InitializeComponent();
            _Menu = menu;
            this.Loaded += loadedCombobox;

            void loadedCombobox(object sender, EventArgs e)
            {
                opponentComboBox.SelectedIndex = 1;
                difficultyComboBox.IsEnabled = false;
                difficultyComboBox.SelectedIndex = 0;
            }

            difficultyComboBox.ItemsSource = Levels;
            
            Levels.Add(new DifficultyLevel("Легко", new SolidColorBrush(Colors.Green)));
            Levels.Add(new DifficultyLevel("Нормально", new SolidColorBrush(Colors.Yellow)));
            Levels.Add(new DifficultyLevel("Сложно", new SolidColorBrush(Colors.Red)));

            try
            {
                string sprite = File.ReadLines("save.dat").Last();

                BlackTeam.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}/Resource/Sprites/{sprite}/bK.png"));
                WhiteTeam.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}/Resource/Sprites/{sprite}/wK.png"));
            }
            catch
            {
                MessageBox.Show("Повреждена тема. Перейдите в настройки");
                this.Close();
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            _Menu.Close();

            ChessField.OpponentType type = (ChessField.OpponentType)Enum.Parse(typeof(ChessField.OpponentType), opponentComboBox.Text);
            int skillLevel = 0;

            switch (difficultyComboBox.Text)
            {
                case "Легко": skillLevel = 1; break;
                case "Нормально": skillLevel = 10; break;
                case "Сложно": skillLevel = 20; break;
            }
            try
            {
                ChessBoardForm form;
                if (CheckBoxPlay960.IsChecked == true)
                {
                    ChessFigure[,] figures = new ChessFigure[8, 8];

                    ChessFigure[] whiteFigures = new ChessFigure[8];
                    //{
                    //    new Rook(Colors.White, new Point(7, 0)),
                    //    new Knight(Colors.White, new Point(7, 0)),
                    //    new Bishop(Colors.White, new Point(7, 0)),
                    //    new Queen(Colors.White, new Point(7, 0)),
                    //    new King(Colors.White, new Point(7, 0)),
                    //    new Bishop(Colors.White, new Point(7, 0)),
                    //    new Knight(Colors.White, new Point(7, 0)),
                    //    new Rook(Colors.White, new Point(7, 0)),
                    //};

                    ChessFigure[] blackFigures = new ChessFigure[8];

                    Random random = new Random();

                    //void swap(IList<ChessFigure> lst, int idx1, int idx2)
                    //{
                    //    ChessFigure tmp = lst[idx1];
                    //    lst[idx1] = lst[idx2];
                    //    lst[idx2] = tmp;
                    //}

                    int getRandomEmptyPos(IList<int> exclude)
                    {
                        int result;
                        do
                        {
                            result = random.Next(0, 8);
                        } while (exclude.Contains(result));
                        exclude.Remove(result);
                        return result;
                    }

                    int kingIdx = random.Next(1, 6);
                    whiteFigures[kingIdx] = new King(Colors.White, new Point(7, 0));

                    int rookFirstIdx = random.Next(0, kingIdx);
                    int rookSecondIdx = random.Next(kingIdx + 1, 8);
                    whiteFigures[rookFirstIdx] = new Rook(Colors.White, new Point(7, 0));
                    whiteFigures[rookSecondIdx] = new Rook(Colors.White, new Point(7, 0));


                    List<int> exclude = new List<int> { kingIdx, rookFirstIdx, rookSecondIdx };
                    int bishopFirstIdx = getRandomEmptyPos(exclude);
                    exclude.Add(bishopFirstIdx);
                    whiteFigures[bishopFirstIdx] = new Bishop(Colors.White, new Point(7, 0));


                    List<int> secondBishupExclude = new List<int>();
                    secondBishupExclude.AddRange(exclude);
                    secondBishupExclude.AddRange(
                        (from numb in Enumerable.Range(0, 8)
                         where numb % 2 == bishopFirstIdx % 2
                         select numb));
                    int bishopSecondIdx = getRandomEmptyPos(secondBishupExclude);
                    exclude.Add(bishopSecondIdx);
                    whiteFigures[bishopSecondIdx] = new Bishop(Colors.White, new Point(7, 0));


                    int queenIdx = getRandomEmptyPos(exclude);
                    whiteFigures[queenIdx] = new Queen(Colors.White, new Point(7, 0));
                    exclude.Add(queenIdx);

                    for (int i = 0; i < 2; i++)
                    {
                        int idx = getRandomEmptyPos(exclude);
                        exclude.Add(idx);
                        whiteFigures[idx] = new Knight(Colors.White, new Point(7, 0));
                        blackFigures[idx] = new Knight(Colors.Black, new Point(7, 0));
                    }

                    blackFigures[rookFirstIdx] = new Rook(Colors.Black, new Point(7, 0));
                    blackFigures[rookSecondIdx] = new Rook(Colors.Black, new Point(7, 0));
                    blackFigures[kingIdx] = new King(Colors.Black, new Point(7, 0));
                    blackFigures[bishopFirstIdx] = new Bishop(Colors.Black, new Point(7, 0));
                    blackFigures[bishopSecondIdx] = new Bishop(Colors.Black, new Point(7, 0));
                    blackFigures[queenIdx] = new Queen(Colors.Black, new Point(7, 0));


                    for (int i = 0; i < 8; i++)
                    {
                        int whiteHorizontal = IsWhiteBehind ? 7 : 0;
                        int blackHorizontal = IsWhiteBehind ? 0 : 7;
                        int whitePawnsHorizontal = Math.Abs(whiteHorizontal - 1);
                        int blackPawnsHorizontal = Math.Abs(blackHorizontal - 1);
                        whiteFigures[i].IsWhiteTeam = true;
                        blackFigures[i].IsWhiteTeam = false;

                        figures[whiteHorizontal, i] = whiteFigures[i];
                        figures[blackHorizontal, i] = blackFigures[i];
                        figures[whiteHorizontal, i].CurrentPos = new Point(whiteHorizontal, i);
                        figures[blackHorizontal, i].CurrentPos = new Point(blackHorizontal, i);

                        figures[whitePawnsHorizontal, i] = new Pawn(Colors.White, new Point(whitePawnsHorizontal, i));
                        figures[blackPawnsHorizontal, i] = new Pawn(Colors.Black, new Point(blackPawnsHorizontal, i));
                    }

                    form = new ChessBoardForm(figures, ChessField.OpponentType.Friend, true, 0, IsWhiteBehind,
                        new DateTime(1970, 1, 1, 0, Int32.Parse(timeTextBox.Text), 0),
                        new DateTime(1970, 1, 1, 0, Int32.Parse(timeTextBox.Text), 0));
                }
                else
                    form = new ChessBoardForm(type, skillLevel, new DateTime(1970, 1, 1, 0, Int32.Parse(timeTextBox.Text), 0), IsWhiteBehind);

                form.Show();
                this.Close();
            }
            catch
            {
                MessageBox.Show("Не по плану что-то пошло... заполни все как надо");
            }

        }

        private void opponentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (opponentComboBox.SelectedIndex == 0)
                difficultyComboBox.IsEnabled = true;
            if (opponentComboBox.SelectedIndex == 1)
            {
                difficultyComboBox.IsEnabled = false;
                difficultyComboBox.Text = "None";
            }
        }

        private void WhiteBorder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WhiteBorder.BorderThickness = new Thickness(5);
            BlackBorder.BorderThickness = new Thickness(0);
            IsWhiteBehind = true;
        }

        private void BlackBorder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WhiteBorder.BorderThickness = new Thickness(0);
            BlackBorder.BorderThickness = new Thickness(5);
            IsWhiteBehind = false;
        }

        private void CheckBoxPlay960_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                opponentComboBox.SelectedIndex = 1;
                opponentComboBox.IsEnabled = false;
            }
            else
                opponentComboBox.IsEnabled = true;
        }
    }
}
