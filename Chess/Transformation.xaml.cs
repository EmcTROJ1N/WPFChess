using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{

    public partial class Transformation : Window
    {
        ChessField ChessField;


        Image Queen = new Image();
        Image Rook = new Image();
        Image Knight = new Image();
        Image Bishop = new Image();

        //ChessFigure res;
        public string r;
        
        public Transformation(ChessField field)
        {
            InitializeComponent();
            ChessField = field;
            Grid_trans.Background = new SolidColorBrush(ChessField.Visual.LightCell);

            Color team = ChessField.SelectedFigure.Team;
          //  Color team = Colors.Black;

            string pathToSprite = $"{Environment.CurrentDirectory}/Sprites/";
            pathToSprite +=  Parent != null ? ChessField.Visual.FiguresSprite : File.ReadLines("save.dat").Last();

            if (team==Colors.Black)
            {
                Queen.Source = new BitmapImage(new Uri(@$"{pathToSprite}\bQ.png"));
                Rook.Source = new BitmapImage(new Uri(@$"{pathToSprite}\bR.png"));
                Knight.Source = new BitmapImage(new Uri(@$"{pathToSprite}\bK.png"));
                Bishop.Source = new BitmapImage(new Uri(@$"{pathToSprite}\bB.png"));
            }
            else
            {
                Queen.Source = new BitmapImage(new Uri(@$"{pathToSprite}\wQ.png"));
                Rook.Source = new BitmapImage(new Uri(@$"{pathToSprite}\wR.png"));
                Knight.Source = new BitmapImage(new Uri(@$"{pathToSprite}\wK.png"));
                Bishop.Source = new BitmapImage(new Uri(@$"{pathToSprite}\wB.png"));
            }

            Grid.SetColumn(Knight, 0);
            Grid.SetColumn(Queen, 1);
            Grid.SetColumn(Bishop, 3);
            Grid.SetColumn(Rook, 2);

            Grid.SetRow(Queen, 2);
            Grid.SetRow(Knight, 2);
            Grid.SetRow(Rook, 2);
            Grid.SetRow(Bishop, 2);

            Queen.MouseDown += Select_figure;
            Knight.MouseDown += Select_figure;
            Rook.MouseDown += Select_figure;
            Bishop.MouseDown += Select_figure;

            Grid_trans.Children.Add(Queen);
            Grid_trans.Children.Add(Knight);
            Grid_trans.Children.Add(Rook);
            Grid_trans.Children.Add(Bishop);
            ///Queen = new Queen(ChessField.SelectedFigure.Team, new Point(ChessField.SelectedFigure, j), )
        }

        public string Select()
        {
            return r;
        }

        private void Select_figure(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image Select = (Image)sender;

            if (Select==Queen)
            {
                r = "Q";
                this.Close();
                return;
            }

            if (Select == Bishop)
            {
                r = "B";
                this.Close();
                return;
            }

            if (Select == Rook)
            {
                r = "R";
                this.Close();
                return;
            }

            if (Select == Knight)
            {
                r = "K";
                this.Close();
                return;
            }
          //  e.Handled = true;
        }

    }
}
