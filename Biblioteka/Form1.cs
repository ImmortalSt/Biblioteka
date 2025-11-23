using System.Data;
using System.Data.SQLite;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace Biblioteka
{
    public partial class Form1 : Form
    {
        private PrivateFontCollection privateFonts = new PrivateFontCollection();

        //установка шрифтов
        private void LoadCustomFont()
        {
            try
            {
                // Способ 1: Загрузка из встроенных ресурсов проекта
                byte[] fontData = Properties.Resources.Comforter_Regular; // "MyCustomFont" - имя ресурса
                byte[] fontData2 = Properties.Resources.ofont_ru_LeoHand;

                // Загружаем шрифт в память
                IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                privateFonts.AddMemoryFont(fontPtr, fontData.Length);
                Marshal.FreeCoTaskMem(fontPtr);

                // Загружаем второй шрифт
                IntPtr fontPtr2 = Marshal.AllocCoTaskMem(fontData2.Length);
                Marshal.Copy(fontData2, 0, fontPtr2, fontData2.Length);
                privateFonts.AddMemoryFont(fontPtr2, fontData2.Length);
                Marshal.FreeCoTaskMem(fontPtr2);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шрифта: {ex.Message}");
                // Используем стандартный шрифт в случае ошибки
                //privateFonts.AddFontFamily("Arial");
            }
        }

        private bool isDragging = false;
        private Point startPoint;

        private int isForm = 1600;

        public Form1()
        {
            // подключение БД
            CreateDatabaseAndtable();

            InitializeComponent();
            LoadCustomFont();

            StartMain();
            AddBooks();
            Cataloge();

            OnOffADDBooks(false);
            OnOffCataloge(false);

            this.Size = new Size(isForm, 900);

            Panel panel = new Panel()
            {
                Width = this.Size.Width,
                Height = 30,
                //BackColor = Color.White,
            };

            this.Controls.Add(panel);

            // Обработчики перемещения
            panel.MouseDown += TitleBar_MouseDown;
            panel.MouseMove += TitleBar_MouseMove;
            panel.MouseUp += TitleBar_MouseUp;

            this.BackColor = Color.FromArgb(255, 250, 247); // установка цвета

        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)  // Если нажата ЛЕВАЯ кнопка мыши
            {
                isDragging = true;              // "ВКЛЮЧАЕМ РЕЖИМ ПЕРЕТАСКИВАНИЯ"
                startPoint = new Point(e.X, e.Y); // "ЗАПОМИНАЕМ, ГДЕ НАЧАЛИ"
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)  // "ЕСЛИ МЫ В РЕЖИМЕ ПЕРЕТАСКИВАНИЯ"
            {
                Point p = PointToScreen(e.Location);  // Узнаем где сейчас курсор на ЭКРАНЕ
                Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y); // Двигаем форму
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;  // "ВЫКЛЮЧАЕМ РЕЖИМ ПЕРЕТАСКИВАНИЯ"
        }


        //создание БД для пользователя
        private static void CreateDatabaseAndtable()
        {
            using (var connection = new SQLiteConnection("Data Source = Biblioteka.db"))
            {
                connection.Open();
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Cataloge (
                        ID_Cat	INTEGER NOT NULL,
                        Name_Cat	TEXT NOT NULL,
                        PRIMARY KEY(ID_Cat AUTOINCREMENT));
                    CREATE TABLE IF NOT EXISTS Books (
                        ID_Book	INTEGER NOT NULL,
                        Name_Book	TEXT NOT NULL,
                        Name_Autor	TEXT NOT NULL,
                        Num_Str INT NOT NULL,
                        ID_Class INT NOT NULL,
                        PRIMARY KEY(ID_Book AUTOINCREMENT)
                     FOREIGN KEY(ID_Class) REFERENCES Cataloge(ID_Cat))";
                using (var command = new SQLiteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        //получение ID раздела       
        private static int ValidateIdToCataloge(string cataloge)
        {
            int id = 0;
            using (var connection = new SQLiteConnection("Data Source = Biblioteka.db"))
            {
                connection.Open();
                string selectCataloge = @"SELECT ID_Cat FROM Cataloge WHERE Name_Cat = @cataloge";

                using (var command = new SQLiteCommand(selectCataloge, connection))
                {
                    command.Parameters.AddWithValue("@cataloge", cataloge);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = reader.GetInt32(0);
                        }
                    }
                }
            }
            return id;
        }

        // добавление данных в БД
        private static void ADDData(int id_Cat, string nameBook, string nameAutor, int numStr)
        {
            using (var connection = new SQLiteConnection("Data Source = Biblioteka.db"))
            {
                connection.Open();
                string insertSql = @"INSERT INTO Books (Name_Book, Name_Autor, Num_Str, ID_Class)
                                        VALUES(@nameBook, @nameAutor, @numStr, @id_Cat)";
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@nameBook", nameBook);
                    command.Parameters.AddWithValue("@nameAutor", nameAutor);
                    command.Parameters.AddWithValue("@numStr", numStr);
                    command.Parameters.AddWithValue("@id_Cat", id_Cat);

                    command.ExecuteNonQuery();
                }
            }
        }

        //получение данных о списке книг
        private static void LoadBooksIntoDataGridView(DataGridView dataGridView)
        {
            using (var connection = new SQLiteConnection("Data Source = Biblioteka.db"))
            {
                connection.Open();
                string selectSql = @" SELECT 
                                     Books.Name_Book as 'Название книги',   
                                     Books.Name_Autor as 'Автор',
                                        Books.Num_Str as 'Страниц',
                                        Cataloge.Name_Cat as 'Раздел'
                                    FROM Books, Cataloge
                                    WHERE Cataloge.ID_Cat = Books.ID_Class";

                using (var command = new SQLiteCommand(selectSql, connection))
                {
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }
            }
        }

        // стартовая страница
        PictureBox fonMain = new PictureBox();      //ФОН

        PictureBox But_ADD = new PictureBox();      //кнопка добавить
        PictureBox But_Del = new PictureBox();      //кнопка удалить
        PictureBox But_AllBook = new PictureBox();  //кнопка каталог
        PictureBox But_Search = new PictureBox();   //кнопка поиска
        PictureBox But_ExitMain = new PictureBox(); //кнопка выхода

        private void OnOffStartMain(bool isOn)
        {
            fonMain.Enabled = isOn;
            fonMain.Visible = isOn;

            But_ADD.Enabled = isOn;
            But_ADD.Visible = isOn;
            But_Del.Enabled = isOn;
            But_Del.Visible = isOn;
            But_AllBook.Enabled = isOn;
            But_AllBook.Visible = isOn;
            But_ExitMain.Enabled = isOn;
            But_ExitMain.Visible = isOn;
        }

        private void StartMain()
        {
            // фон
            fonMain.Image = Image.FromFile("img/fonMain.png");
            fonMain.Location = new Point(0, 30);
            fonMain.Name = "FonMain";
            fonMain.Size = new Size(1600, 875);
            fonMain.SizeMode = PictureBoxSizeMode.Zoom;
            fonMain.TabIndex = 0;
            fonMain.TabStop = false;
            fonMain.SendToBack();

            this.Controls.Add(fonMain);

            // Рисование текста поверх PictureBox
            using (Graphics g = Graphics.FromImage(fonMain.Image))
            {
                using (Font font = new Font(privateFonts.Families[0], 62, FontStyle.Bold))
                {
                    g.DrawString("Библиотека", font, Brushes.Black, new PointF(200, 30));
                }
            }
            fonMain.Invalidate(); // Обновляем PictureBox, чтобы изменения отобразились

            //кнопка добавить
            But_ADD.Location = new Point(355, 371);
            But_ADD.Name = "Add";
            But_ADD.Size = new Size(95, 121);
            But_ADD.TabIndex = 0;
            But_ADD.Image = Image.FromFile("button/ADD.png");
            But_ADD.BackColor = Color.Transparent;

            this.Controls.Add(But_ADD);
            this.Controls.SetChildIndex(But_ADD, 0);

            But_ADD.Click += (sender, e) =>
            {
                OnOffStartMain(false);
                OnOffADDBooks(true);
            };


            //кнопка удалить
            But_Del.Location = new Point(555, 371);
            But_Del.Name = "Delete";
            But_Del.Size = new Size(121, 121);
            But_Del.TabIndex = 0;
            But_Del.Image = Image.FromFile("button/Delete.png");
            But_Del.BackColor = Color.Transparent;

            this.Controls.Add(But_Del);
            this.Controls.SetChildIndex(But_Del, 0);

            //кнопка каталог книг
            But_AllBook.Location = new Point(755, 371);
            But_AllBook.Name = "AllBooks";
            But_AllBook.Size = new Size(121, 121);
            But_AllBook.TabIndex = 0;
            But_AllBook.Image = Image.FromFile("button/AllBooks.png");
            But_AllBook.BackColor = Color.Transparent;

            this.Controls.Add(But_AllBook);
            this.Controls.SetChildIndex(But_AllBook, 0);

            But_AllBook.Click += (sender, e) =>
            {
                OnOffStartMain(false);
                OnOffCataloge(true);
            };

            // кнопка ВЫХОД
            But_ExitMain.Location = new Point(1230, 60);
            But_ExitMain.Name = "ExitMain";
            But_ExitMain.Size = new Size(304, 155);
            But_ExitMain.TabIndex = 0;
            But_ExitMain.Image = Image.FromFile("button/ExitMain.png");

            this.Controls.Add(But_ExitMain);
            this.Controls.SetChildIndex(But_ExitMain, 0);

            But_ExitMain.Click += (sender, e) => { this.Close(); }; //обработчик события Click


        }

        //страница для добавления книги
        PictureBox fonADD = new PictureBox();

        //PictureBox addClass = new PictureBox();     //раздел: хобби, худ лит, наука и тд
        ComboBox addClass = new ComboBox();
        PictureBox addBook = new PictureBox();
        PictureBox addAutor = new PictureBox();
        PictureBox addNumStr = new PictureBox();

        PictureBox But_ExitADD = new PictureBox();
        PictureBox But_ReturnADD = new PictureBox();
        PictureBox But_ADDBook = new PictureBox();

        TextBox empty = new TextBox();
        TextBox nameClass = new TextBox();
        TextBox nameBook = new TextBox();
        TextBox nameAutor = new TextBox();
        TextBox numStr = new TextBox();

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        private void OnOffADDBooks(bool isOn)
        {
            fonADD.Enabled = isOn;
            fonADD.Visible = isOn;

            addClass.Enabled = isOn;
            addClass.Visible = isOn;
            addBook.Enabled = isOn;
            addBook.Visible = isOn;
            addAutor.Enabled = isOn;
            addAutor.Visible = isOn;
            addNumStr.Enabled = isOn;
            addNumStr.Visible = isOn;

            But_ExitADD.Enabled = isOn;
            But_ExitADD.Visible = isOn;
            But_ReturnADD.Enabled = isOn;
            But_ReturnADD.Visible = isOn;
            But_ADDBook.Enabled = isOn;
            But_ADDBook.Visible = isOn;

            empty.Enabled = isOn;
            empty.Visible = isOn;
            nameClass.Enabled = isOn;
            nameClass.Visible = isOn;
            nameBook.Enabled = isOn;
            nameBook.Visible = isOn;
            nameAutor.Enabled = isOn;
            nameAutor.Visible = isOn;
            numStr.Enabled = isOn;
            numStr.Visible = isOn;
        }

        private void AddBooks()
        {
            // фон
            fonADD.Image = Image.FromFile("img/fonAdd.png");
            fonADD.Location = new Point(0, 30);
            fonADD.Name = "FonAdd";
            fonADD.Size = new Size(1600, 875);
            fonADD.SizeMode = PictureBoxSizeMode.Zoom;
            fonADD.TabIndex = 0;
            fonADD.TabStop = false;
            fonADD.SendToBack();

            this.Controls.Add(fonADD);


            // Рисование текста поверх PictureBox
            using (Graphics g = Graphics.FromImage(fonADD.Image))
            {
                using (Font font = new Font(privateFonts.Families[0], 62, FontStyle.Bold))
                {
                    g.DrawString("Добавить книгу", font, Brushes.Black, new PointF(200, 30));
                }
            }
            fonADD.Invalidate(); // Обновляем PictureBox, чтобы изменения отобразились

            // пустой текст
            empty.Text = "";
            empty.PlaceholderText = "";
            empty.Location = new Point(110, 220);
            empty.Name = "empty";
            empty.Size = new Size(415, 40);
            empty.Font = new Font(privateFonts.Families[1], 20, FontStyle.Bold);
            empty.BackColor = Color.FromArgb(104, 201, 190); // установка цвета
            empty.BorderStyle = BorderStyle.None;   // Убираем рамку
            empty.ForeColor = Color.Gray;           // Цвет подсказки
            empty.BringToFront();

            this.Controls.Add(empty);
            this.Controls.SetChildIndex(empty, 100);

            // Очищаем текст при фокусировке
            nameClass.GotFocus += (sender, e) =>
            {
                if (empty.Text == "")
                {
                    empty.Text = "";
                    empty.ForeColor = Color.Black;
                    HideCaret(empty.Handle); // Скрыть курсор
                }
            };

            //добавление раздела
            //addClass.Image = Image.FromFile("object/AddText.png");
            //addClass.Location = new Point(70, 200);
            //addClass.Name = "nameClass";
            //addClass.Size = new Size(984, 76);
            //addClass.SizeMode = PictureBoxSizeMode.AutoSize;
            //addClass.TabIndex = 0;
            //addClass.TabStop = false;

            addClass.Location = new Point(70, 200);
            addClass.Name = "addClass";
            addClass.Size = new Size(1000, 76);
            addClass.Font = new Font(privateFonts.Families[1], 20, FontStyle.Bold);
            addClass.BackColor = Color.FromArgb(104, 201, 190); // установка цвета
            addClass.FlatStyle = FlatStyle.Flat;   // Убираем рамку           
            addClass.BringToFront();

            addClass.Items.AddRange(new object[] {
                "Хобби",
                "Наука",
                "Художественная литература",
                "Философия"
            });

            this.Controls.Add(addClass);
            this.Controls.SetChildIndex(addClass, 0);

            // добаить книгу
            addBook.Image = Image.FromFile("object/AddText.png");
            addBook.Location = new Point(70, 300);
            addBook.Name = "addBook";
            addBook.Size = new Size(984, 76);
            addBook.SizeMode = PictureBoxSizeMode.AutoSize;
            addBook.TabIndex = 0;
            addBook.TabStop = false;

            this.Controls.Add(addBook);
            this.Controls.SetChildIndex(addBook, 100);

            // ввод книги
            nameBook.Text = "Название книги";
            nameBook.PlaceholderText = "nameBook";
            nameBook.Location = new Point(70, 300);
            nameBook.Name = "nameStart";
            nameBook.Size = new Size(1000, 76);
            nameBook.Font = new Font(privateFonts.Families[1], 20, FontStyle.Bold);
            nameBook.BackColor = Color.FromArgb(104, 201, 190); // установка цвета
            nameBook.BorderStyle = BorderStyle.None;   // Убираем рамку
            nameBook.ForeColor = Color.Gray;           // Цвет подсказки
            nameBook.BringToFront();

            this.Controls.Add(nameBook);
            this.Controls.SetChildIndex(nameBook, 0);

            // Очищаем текст при фокусировке
            nameBook.GotFocus += (sender, e) =>
            {
                if (nameBook.Text == "Название книги")
                {
                    nameBook.Text = "";
                    nameBook.ForeColor = Color.Black;
                    HideCaret(nameBook.Handle); // Скрыть курсор
                }
            };

            // Возвращаем подсказку, если поле пустое
            nameBook.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBook.Text))
                {
                    nameBook.Text = "Название книги";
                    nameBook.ForeColor = Color.Gray;
                }
            };

            // ввод автора
            nameAutor.Text = "Автор книги";
            nameAutor.PlaceholderText = "nameAutor";
            nameAutor.Location = new Point(70, 400);
            nameAutor.Name = "nameAutor";
            nameAutor.Size = new Size(1000, 76);
            nameAutor.Font = new Font(privateFonts.Families[1], 20, FontStyle.Bold);
            nameAutor.BackColor = Color.FromArgb(104, 201, 190); // установка цвета
            nameAutor.BorderStyle = BorderStyle.None;   // Убираем рамку
            nameAutor.ForeColor = Color.Gray;           // Цвет подсказки
            nameAutor.BringToFront();

            this.Controls.Add(nameAutor);
            this.Controls.SetChildIndex(nameAutor, 0);

            // Очищаем текст при фокусировке
            nameAutor.GotFocus += (sender, e) =>
            {
                if (nameAutor.Text == "Автор книги")
                {
                    nameAutor.Text = "";
                    nameAutor.ForeColor = Color.Black;
                    HideCaret(nameAutor.Handle); // Скрыть курсор
                }
            };

            // Возвращаем подсказку, если поле пустое
            nameAutor.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameAutor.Text))
                {
                    nameAutor.Text = "Автор книги";
                    nameAutor.ForeColor = Color.Gray;
                }
            };

            // ввод страниц
            numStr.Text = "Страниц";
            numStr.PlaceholderText = "numStr";
            numStr.Location = new Point(70, 500);
            numStr.Name = "numStr";
            numStr.Size = new Size(270, 76);
            numStr.Font = new Font(privateFonts.Families[1], 20, FontStyle.Bold);
            numStr.BackColor = Color.FromArgb(104, 201, 190); // установка цвета
            numStr.BorderStyle = BorderStyle.None;   // Убираем рамку
            numStr.ForeColor = Color.Gray;           // Цвет подсказки
            numStr.BringToFront();

            this.Controls.Add(numStr);
            this.Controls.SetChildIndex(numStr, 0);

            // Очищаем текст при фокусировке
            numStr.GotFocus += (sender, e) =>
            {
                if (numStr.Text == "Страниц")
                {
                    numStr.Text = "";
                    numStr.ForeColor = Color.Black;
                    HideCaret(numStr.Handle); // Скрыть курсор
                }
            };

            // Возвращаем подсказку, если поле пустое
            numStr.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(numStr.Text))
                {
                    numStr.Text = "Страниц";
                    numStr.ForeColor = Color.Gray;
                }
            };

            // кнопка ДОБАВИТЬ
            But_ADDBook.Location = new Point(470, 600);
            But_ADDBook.Name = "But_ADDBook";
            But_ADDBook.Size = new Size(325, 166);
            But_ADDBook.TabIndex = 0;
            But_ADDBook.Image = Image.FromFile("button/But_ADD.png");

            this.Controls.Add(But_ADDBook);
            this.Controls.SetChildIndex(But_ADDBook, 0);

            But_ADDBook.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(addClass.Text) ||
                    string.IsNullOrWhiteSpace(nameBook.Text) ||
                    string.IsNullOrWhiteSpace(nameAutor.Text) ||
                    string.IsNullOrWhiteSpace(numStr.Text))
                {
                    MessageBox.Show("Все поля должны быть заполнены!");
                    return;
                }

                // Преобразуем количество страниц в число
                if (!int.TryParse(numStr.Text, out int str))
                {
                    MessageBox.Show("Количество страниц должно быть числом!");
                    return;
                }

                string cataloge = addClass.Text;
                int id_Cat = ValidateIdToCataloge(cataloge);

                string book = nameBook.Text;
                string autor = nameAutor.Text;

                ADDData(id_Cat, book, autor, str);

                // Опционально: сообщение об успешном добавлении
                MessageBox.Show("Книга успешно добавлена!");
            };

            // кнопка НАЗАД
            But_ReturnADD.Location = new Point(1230, 260);
            But_ReturnADD.Name = "ReturnADD";
            But_ReturnADD.Size = new Size(304, 155);
            But_ReturnADD.TabIndex = 0;
            But_ReturnADD.Image = Image.FromFile("button/Return.png");

            this.Controls.Add(But_ReturnADD);
            this.Controls.SetChildIndex(But_ReturnADD, 0);

            But_ReturnADD.Click += (sender, e) =>
            {
                OnOffStartMain(true);
                OnOffADDBooks(false);
            };

            // кнопка ВЫХОД
            But_ExitADD.Location = new Point(1230, 60);
            But_ExitADD.Name = "ExitMain";
            But_ExitADD.Size = new Size(304, 155);
            But_ExitADD.TabIndex = 0;
            But_ExitADD.Image = Image.FromFile("button/ExitMain.png");

            this.Controls.Add(But_ExitADD);
            this.Controls.SetChildIndex(But_ExitADD, 0);

            But_ExitADD.Click += (sender, e) => { this.Close(); }; //обработчик события Click


        }

        //страница списка книг
        PictureBox fonCataloge = new PictureBox();

        PictureBox listBooks = new PictureBox();
        DataGridView tableBooks = new DataGridView();

        PictureBox But_ReturnCat = new PictureBox();
        PictureBox But_ExitCat = new PictureBox();

        private void OnOffCataloge(bool isOn)
        {
            fonCataloge.Enabled = isOn;
            fonCataloge.Visible = isOn;

            listBooks.Enabled = isOn;
            listBooks.Visible = isOn;
            tableBooks.Enabled = isOn;
            tableBooks.Visible = isOn;

            But_ReturnCat.Enabled = isOn;
            But_ReturnCat.Visible = isOn;
            But_ExitCat.Enabled = isOn;
            But_ReturnCat.Visible = isOn;
        }

        private void Cataloge()
        {
            // фон
            fonCataloge.Image = Image.FromFile("img/fonCat.png");
            fonCataloge.Location = new Point(0, 30);
            fonCataloge.Name = "FonAdd";
            fonCataloge.Size = new Size(1600, 875);
            fonCataloge.SizeMode = PictureBoxSizeMode.Zoom;
            fonCataloge.TabIndex = 0;
            fonCataloge.TabStop = false;
            fonCataloge.SendToBack();

            this.Controls.Add(fonCataloge);

            // Рисование текста поверх PictureBox
            using (Graphics g = Graphics.FromImage(fonCataloge.Image))
            {
                using (Font font = new Font(privateFonts.Families[0], 62, FontStyle.Bold))
                {
                    g.DrawString("Полный список книг", font, Brushes.Black, new PointF(200, 30));
                }
            }
            fonCataloge.Invalidate(); // Обновляем PictureBox, чтобы изменения отобразились

            // список книг
            //listBooks.Image = Image.FromFile("object/AllBooksList.png");
            //listBooks.Location = new Point(50, 150);
            //listBooks.Name = "login";
            //listBooks.Size = new Size(1100, 630);
            //listBooks.SizeMode = PictureBoxSizeMode.AutoSize;
            //listBooks.TabIndex = 0;
            //listBooks.TabStop = false;

            //this.Controls.Add(listBooks);
            //this.Controls.SetChildIndex(listBooks, 0);

            tableBooks.Location = new Point(75, 180);
            tableBooks.Name = "tableBooks";
            tableBooks.Size = new Size(1050, 640);
            tableBooks.TabIndex = 0;
            tableBooks.TabStop = false;
            tableBooks.BackgroundColor = Color.FromArgb(233, 207, 171);
            tableBooks.BorderStyle = BorderStyle.None; // Убираем рамку

            this.Controls.Add(tableBooks);
            this.Controls.SetChildIndex(tableBooks, 0);
            LoadBooksIntoDataGridView(tableBooks);

            // кнопка НАЗАД
            But_ReturnCat.Location = new Point(1230, 260);
            But_ReturnCat.Name = "ReturnADD";
            But_ReturnCat.Size = new Size(304, 155);
            But_ReturnCat.TabIndex = 0;
            But_ReturnCat.Image = Image.FromFile("button/Return.png");

            this.Controls.Add(But_ReturnCat);
            this.Controls.SetChildIndex(But_ReturnCat, 0);

            But_ReturnCat.Click += (sender, e) =>
            {
                OnOffStartMain(true);
                OnOffCataloge(false);
            };

            // кнопка ВЫХОД
            But_ExitCat.Location = new Point(1230, 60);
            But_ExitCat.Name = "ExitMain";
            But_ExitCat.Size = new Size(304, 155);
            But_ExitCat.TabIndex = 0;
            But_ExitCat.Image = Image.FromFile("button/ExitMain.png");

            this.Controls.Add(But_ExitCat);
            this.Controls.SetChildIndex(But_ExitCat, 0);

            But_ExitCat.Click += (sender, e) => { this.Close(); }; //обработчик события Click


        }

    }


}
