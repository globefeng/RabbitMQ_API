```
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: this.tbxQueue.Text,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var body = Encoding.UTF8.GetBytes(this.tbxMessage.Text);

                    channel.BasicPublish(exchange: "",
                                         routingKey: this.tbxQueue.Text,
                                         basicProperties: null,
                                         body: body);
                }
            }
        }

        public delegate void MyDelegate(string myString);

        private async void button1_Click(object sender, EventArgs e)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: this.tbxQueue.Text,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            int messageCount = Convert.ToInt16(channel.MessageCount(this.tbxQueue.Text));

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                this.tbxMessage.BeginInvoke(new MyDelegate(DelegateMethod), new object[] { message });
            };
            channel.BasicConsume(queue: this.tbxQueue.Text,
                                 autoAck: false,
                                 consumer: consumer);

            await Task.Run(() => { Thread.Sleep(2000); });
        }

        public void DelegateMethod(string myString)
        {
            if (string.IsNullOrEmpty(this.tbxMessage.Text))
            {
                this.tbxMessage.Text = myString;
            }
            else
            {
                this.tbxMessage.Text += "\r\n" + myString;
            }
        }
    }
```
