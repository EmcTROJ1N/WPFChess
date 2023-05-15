using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chess
{
    public class SpritesTitle
    {
        public string Title { get; set; }
        public BitmapImage Image { get; set; }

        public SpritesTitle(string title, BitmapImage image)
        {
            Title = title;
            Image = image;
        }

        public override string ToString() => Title;
    }

    public partial class Settings : Window
    {
        ObservableCollection<SpritesTitle> Sprites = new ObservableCollection<SpritesTitle>();

        public Settings()
        {
            InitializeComponent();

            ComboboxSprites.ItemsSource = Sprites;
            foreach (string item in Directory.GetDirectories($"{Environment.CurrentDirectory}/Resource/Sprites"))
                Sprites.Add(new SpritesTitle(Path.GetFileName(item), new BitmapImage(new Uri($"{item}/wK.png"))));


            bool flag = true;
            try
            {
                StreamReader test = new StreamReader("save.dat", Encoding.Default);
                test.Close();
            }
            catch { Init(); flag = false; }

            if (flag)
            {
                StreamReader rdr = new StreamReader("save.dat", Encoding.Default);
                try
                {
                    SelectedFigureColor.SelectedColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
                    SelectedStepColor.SelectedColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
                    TagsColor.SelectedColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
                    DarkCellColor.SelectedColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
                    LightCellColor.SelectedColor = (Color)ColorConverter.ConvertFromString(rdr.ReadLine());
                    string? str = rdr.ReadLine();
                    foreach (SpritesTitle item in ComboboxSprites.Items)
                        if (item.Title == str) ComboboxSprites.SelectedItem = item;
                    if (ComboboxSprites.SelectedIndex == -1) throw new Exception();
                    rdr.Close();
                }
                catch { rdr.Close(); Init(); }
            }

            void Init()
            {
                MessageBox.Show("Конфиг поврежден, тема сброшена");

                SelectedFigureColor.SelectedColor = Colors.AliceBlue;
                SelectedStepColor.SelectedColor = Colors.Gray;
                TagsColor.SelectedColor = Color.FromArgb(100, 85, 20, 255);
                LightCellColor.SelectedColor = Color.FromArgb(150, 94, 255, 255);
                DarkCellColor.SelectedColor = Color.FromArgb(255, 0, 74, 83);
                ComboboxSprites.SelectedIndex = 8;

                StreamWriter wrtr = new StreamWriter("save.dat", false, Encoding.Default);

                wrtr.WriteLine(SelectedFigureColor.SelectedColor);
                wrtr.WriteLine(SelectedStepColor.SelectedColor);
                wrtr.WriteLine(TagsColor.SelectedColor);
                wrtr.WriteLine(LightCellColor.SelectedColor);
                wrtr.WriteLine(DarkCellColor.SelectedColor);
                wrtr.WriteLine(ComboboxSprites.SelectedItem);

                wrtr.Close();
            }
        }

        private void ApplyClick(object sender, RoutedEventArgs e)
        {
            StreamWriter wrtr = new StreamWriter("save.dat", false, Encoding.Default);

            wrtr.WriteLine(SelectedFigureColor.SelectedColor);
            wrtr.WriteLine(SelectedStepColor.SelectedColor);
            wrtr.WriteLine(TagsColor.SelectedColor);
            wrtr.WriteLine(LightCellColor.SelectedColor);
            wrtr.WriteLine(DarkCellColor.SelectedColor);
            wrtr.WriteLine(ComboboxSprites.SelectedItem);

            wrtr.Close();

            this.Close();
        }

        private void UndoClick(object sender, RoutedEventArgs e) => this.Close();

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            SelectedFigureColor.SelectedColor = Colors.AliceBlue;
            SelectedStepColor.SelectedColor = Colors.Gray;
            TagsColor.SelectedColor = Color.FromArgb(100, 85, 20, 255);
            LightCellColor.SelectedColor = Color.FromArgb(150, 94, 255, 255);
            DarkCellColor.SelectedColor = Color.FromArgb(255, 0, 74, 83);
            ComboboxSprites.SelectedIndex = 8;

            StreamWriter wrtr = new StreamWriter("save.dat", false, Encoding.Default);

            wrtr.WriteLine(SelectedFigureColor.SelectedColor);
            wrtr.WriteLine(SelectedStepColor.SelectedColor);
            wrtr.WriteLine(TagsColor.SelectedColor);
            wrtr.WriteLine(LightCellColor.SelectedColor);
            wrtr.WriteLine(DarkCellColor.SelectedColor);
            wrtr.WriteLine(ComboboxSprites.SelectedItem);

            wrtr.Close();

            MessageBox.Show("Тема сброшена");
        }
    }
}
