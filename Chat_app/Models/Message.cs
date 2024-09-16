namespace Chat_app.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
        public DateTime Timestamp { get; set; }

        public string TestMethod()
        {
            return "Test";
        }
    }
}
