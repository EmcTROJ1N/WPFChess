using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

                    List<ChessFigure> whiteFigures = new List<ChessFigure>()
                    {
                        new Rook(Colors.White, new Point(7, 0)),
                        new Knight(Colors.White, new Point(7, 0)),
                        new Bishop(Colors.White, new Point(7, 0)),
                        new Queen(Colors.White, new Point(7, 0)),
                        new King(Colors.White, new Point(7, 0)),
                        new Bishop(Colors.White, new Point(7, 0)),
                        new Knight(Colors.White, new Point(7, 0)),
                        new Rook(Colors.White, new Point(7, 0)),
                    };

                    List<ChessFigure> blackFigures = new List<ChessFigure>()
                    {
                        new Rook(Colors.Black, new Point(0, 0)),
                        new Knight(Colors.Black, new Point(0, 0)),
                        new Bishop(Colors.Black, new Point(0, 0)),
                        new Queen(Colors.Black, new Point(0, 0)),
                        new King(Colors.Black, new Point(0, 0)),
                        new Bishop(Colors.Black, new Point(0, 0)),
                        new Knight(Colors.Black, new Point(0, 0)),
                        new Rook(Colors.Black, new Point(0, 0)),
                    };

                    Random random = new Random();
                    void shuffle(ref List<ChessFigure> data)
                    {
                        for (int i = data.Count - 1; i >= 1; i--)
                        {
                            int j = random.Next(i + 1);
                            var temp = data[j];
                            data[j] = data[i];
                            data[i] = temp;
                        }
                    }

                    shuffle(ref whiteFigures);
                    shuffle(ref blackFigures);

                    foreach (var figure in whiteFigures) figure.IsWhiteTeam = true;

                    for (int i = 0; i < 8; i++)
                    {
                        if (IsWhiteBehind)
                        {
                            figures[7, i] = whiteFigures[i];
                            figures[0, i] = blackFigures[i];
                            figures[7, i].CurrentPos = new Point(7, i);
                            figures[0, i].CurrentPos = new Point(0, i);
                        }
                        else
                        {
                            figures[0, i] = whiteFigures[i];
                            figures[7, i] = blackFigures[i];
                            figures[0, i].CurrentPos = new Point(0, i);
                            figures[7, i].CurrentPos = new Point(7, i);
                        }

                    }
                    for (int i = 0; i < 8; i++)
                    {
                        if (IsWhiteBehind)
                        {
                            figures[6, i] = new Pawn(Colors.White, new Point(6, i));
                            figures[1, i] = new Pawn(Colors.Black, new Point(1, i));
                        }
                        else
                        {
                            figures[6, i] = new Pawn(Colors.Black, new Point(6, i));
                            figures[1, i] = new Pawn(Colors.White, new Point(1, i));
                        }
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
