namespace Biblioteka
{
    public partial class MsgBoxOk : Form
    {
        Label label_MSG = new Label();
        PictureBox fonMSG = new PictureBox();
        Button ok = new Button();

        public MsgBoxOk(string text, string caption)
        {
            InitializeComponent();

            label_MSG.Text = text;
            Text = caption;

            // фон
            fonMSG.Image = Image.FromFile("img/MsgOk.png");
            fonMSG.Location = new Point(0, 0);
            fonMSG.Name = "fonMSG";
            fonMSG.Size = new Size(500, 300);
            fonMSG.SizeMode = PictureBoxSizeMode.Zoom;
            fonMSG.TabIndex = 0;
            fonMSG.TabStop = false;
            fonMSG.SendToBack();

            this.Controls.Add(fonMSG);

            label_MSG.Location = new Point(30, 50);
            label_MSG.Font = new Font("Segoe Script", 18, FontStyle.Bold);
            label_MSG.Name = "labelMSG";
            label_MSG.BackColor = Color.FromArgb(201, 242, 204); // установка цвета
            label_MSG.MaximumSize = new Size(300, 250);
            label_MSG.AutoSize = true;
            label_MSG.TextAlign = ContentAlignment.MiddleCenter; // Выравнивание текста по центру


            this.Controls.Add(label_MSG);

            ok.Location = new Point(60, 200);
            ok.Name = "OK";
            ok.Size = new Size(150, 75);
            ok.TabIndex = 1;
            ok.Text = "Закрыть";
            ok.Font = new Font("Segoe Script", 18, FontStyle.Bold);
            ok.BackColor = Color.FromArgb(201, 242, 204); // установка цвета
            ok.FlatStyle = FlatStyle.Flat;
            ok.FlatAppearance.BorderSize = 0;   // Убираем рамку

            this.Controls.Add(ok);

            this.Controls.SetChildIndex(label_MSG, 0);
            this.Controls.SetChildIndex(ok, 0);

            ok.Click += (sender, e) => { this.Close(); }; //обработчик события Click

        }



    }
}
