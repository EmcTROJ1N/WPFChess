using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Chess
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
            Player.Play();
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            ChooseSettingsBeforeGame form = new ChooseSettingsBeforeGame(this);
            try { form.ShowDialog(); }
            catch { return; }
        }

        private void LoadGame(object sender, RoutedEventArgs e)
        {
            try
            {
                ChessFigure[,] figures = new ChessFigure[8, 8];
                StreamReader rdr = new StreamReader("data.log", Encoding.Default);

                ChessField.OpponentType type = (ChessField.OpponentType)Enum.Parse(typeof(ChessField.OpponentType), rdr.ReadLine()!);
                int skillLevel = 0;
                bool isWhiteBehind = false;
                if (type == ChessField.OpponentType.PC)
                    skillLevel = Int32.Parse(rdr.ReadLine()!);
                switch (rdr.ReadLine())
                {
                    case "True": isWhiteBehind = true; break;
                    case "False": isWhiteBehind = false; break;
                }

                bool currentCourse = true;
                switch (rdr.ReadLine())
                {
                    case "True": currentCourse = true; break;
                    case "False": currentCourse = false; break;
                }

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        string? colHex = rdr.ReadLine();
                        if (colHex == "")
                        {
                            rdr.ReadLine();
                            rdr.ReadLine();
                            rdr.ReadLine();
                            rdr.ReadLine();
                            continue;
                        }
                        Color color = (Color)ColorConverter.ConvertFromString(colHex);
                        string? figureType = rdr.ReadLine();
                        Point pos = new Point(Int32.Parse(rdr.ReadLine()!), Int32.Parse(rdr.ReadLine()!));
                        Int32.TryParse(rdr.ReadLine(), out int countSteps);
                        switch (figureType)
                        {
                            case "Rook": figures[i, j] = new Rook(color, pos); break;
                            case "Knight": figures[i, j] = new Knight(color, pos); break;
                            case "Bishop": figures[i, j] = new Bishop(color, pos); break;
                            case "Queen": figures[i, j] = new Queen(color, pos); break;
                            case "King": figures[i, j] = new King(color, pos); break;
                            case "Pawn":
                                Pawn pawn = new Pawn(color, pos);
                                
                                switch (rdr.ReadLine())
                                {
                                    case "True": pawn.Pass_takeover = true; break;
                                    case "False": pawn.Pass_takeover = false; break;
                                }
                                Int32.TryParse(rdr.ReadLine(), out pawn.OldCountSteps);
                                
                                figures[i, j] = pawn;
                                break;
                        }

                        figures[i, j].CountStep = countSteps;
                    }
                }

                DateTime timeFirstTeam = DateTime.Parse(rdr.ReadLine()!);
                DateTime timeSecondTeam = DateTime.Parse(rdr.ReadLine()!);

                string? str = rdr.ReadLine();
                List<string> notaionList = new List<string>();
                while (str != "")
                {
                    notaionList.Add(str!);
                    str = rdr.ReadLine();
                }
                List<string> eatenList = new List<string>();
                str = rdr.ReadLine();
                while (str != null)
                {
                    eatenList.Add(str);
                    str = rdr.ReadLine();
                }

                rdr.Close();

                ChessBoardForm form = new ChessBoardForm(figures, type, currentCourse, skillLevel,
                    isWhiteBehind, timeFirstTeam, timeSecondTeam, notaionList, eatenList);
                form.Show();
                this.Close();
            }
            catch
            {
                MessageBox.Show("Поврежден конфиг, загрузка прервана");
            }
        }

        private void Chess960(object sender, RoutedEventArgs e)
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
                figures[7, i] = whiteFigures[i];
                figures[0, i] = blackFigures[i];

                figures[7, i].CurrentPos = new Point(7, i);
                figures[0, i].CurrentPos = new Point(0, i);
            }
            for (int i = 0; i < 8; i++)
            {
                figures[6, i] = new Pawn(Colors.White, new Point(6, i));
                figures[6, i].IsWhiteTeam = true;
                figures[1, i] = new Pawn(Colors.Black, new Point(1, i));
            }

            ChooseSettingsBeforeGame form = new ChooseSettingsBeforeGame(this);
            form.ShowDialog();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e) => Player.Position = TimeSpan.Zero;

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings form = new Settings();
            form.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e) =>
            Player.Stop();
    }
}
