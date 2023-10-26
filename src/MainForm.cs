using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Filters
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Открытое изображение в <see cref="pictureBox"/>.
        /// </summary>
        private OpenFileDialog _ofd = new OpenFileDialog();

        /// <summary>
        /// Отслеживание нажатия кнопки <see cref="toolStripButtonContour"/>.
        /// </summary>
        private bool _isCountourFilter = false;

        /// <summary>
        /// Инициализация главной формы.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Преобразует цвет в соответствующий оттенок серого, где
        /// подразумевается, что 30% красного, 59% зеленого и 11% синего - 
        /// это взвешенное усреднение в процессе подсчета яркости.
        /// </summary>
        /// <param name="oldColor"></param>
        /// <returns></returns>
        private Color GrayscaleFilter(Color oldColor)
        {
            int luminance = (int)(oldColor.R * 0.3 + oldColor.G * 0.59 + oldColor.B * 0.11);

            return Color.FromArgb(0, luminance, luminance, luminance);
        }

        /// <summary>
        /// Преобразует переданный цвет в инвертированный.
        /// </summary>
        /// <param name="oldColor"></param>
        /// <returns></returns>
        private Color InversionFilter(Color oldColor)
        {
            int newRed = 0xFF - oldColor.R;
            int newGreen = 0xFF - oldColor.G;
            int newBlue = 0xFF - oldColor.B;

            return Color.FromArgb(0, newRed, newGreen, newBlue);
        }

        /// <summary>
        /// Применяет выбранный фильтр, используя один алгоритм преобразования.
        /// </summary>
        /// <param name="filterFunction"></param>
        private void ApplyFilter(Func<Color, Color> filterFunction)
        {
            try
            {
                Bitmap initialPicture = new Bitmap(_ofd.FileName);

                Bitmap resultPicture = new Bitmap(_ofd.FileName);

                progressBarLoading.Visible = true;

                int Height = initialPicture.Height;
                int Width = initialPicture.Width;

                for (int x = 0; x < Width - 1; x++)
                {
                    for (int y = 0; y < Height - 1; y++)
                    {
                        // Если была нажата кнопка контура, рассчитывает градиенты для получения контуров
                        // Иначе идёт по стандартному алгоритму, используя переданные цвета
                        if (_isCountourFilter == true)
                        {
                            int current = initialPicture.GetPixel(x, y).R;
                            int up = initialPicture.GetPixel(x + 1, y).R;
                            int right = initialPicture.GetPixel(x, y + 1).R;

                            int dx = Math.Abs(right - current);
                            int dy = Math.Abs(up - current);

                            int gradient = (int)Math.Sqrt(Math.Pow(dx, 2) - Math.Pow(dy, 2)) * trackBar.Value;


                            if (gradient > 255)
                            {
                                gradient = 255;
                            }

                            Color gradientColor;
                            gradientColor = Color.FromArgb(0, gradient, gradient, gradient);
                            resultPicture.SetPixel(x, y, gradientColor);
                        }
                        else
                        {
                            Color oldColor = initialPicture.GetPixel(x, y);
                            Color newColor = filterFunction(oldColor);
                            resultPicture.SetPixel(x, y, newColor);
                        }

                        double progress = (double)(x * Height + y) / (Width * Height);
                        progressBarLoading.Value = (int)(progress * 100);
                    }
                }

                progressBarLoading.Visible = false;

                _isCountourFilter = false;
                pictureBox.Image = resultPicture;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {   
            _ofd.Filter = "Image Files(*.PNG;*.JPG;*.BMP)|*.PNG;*.JPG;*.BMP";

            if (_ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox.Image = new Bitmap(_ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.OverwritePrompt = true;
                sfd.CheckPathExists = true;
                sfd.Filter = "JPG(*.JPG)|*.JPG|PNG(*.PNG)|*.PNG|BMP(*.BMP)|*.BMP";
                sfd.ShowHelp = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox.Image.Save(sfd.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void toolStripButtonContour_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                _isCountourFilter = true;
                ApplyFilter(GrayscaleFilter);
            }
        }

        private void toolStripButtonInversion_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                ApplyFilter(InversionFilter);
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            int value = trackBar.Value;
            if (value % 2 != 0)
            {
                // Округление значения до ближайшего четного числа
                int newValue = (value % 2 == 0) ? value : value + 1;
                trackBar.Value = newValue;
            }
        }
    }
}
