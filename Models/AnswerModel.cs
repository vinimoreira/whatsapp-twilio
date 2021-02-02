namespace TwilioWhatsAppBot.Models
{
    public class Answer
    {
        public string Id { get; set; }
        public string FromId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public object ChannelId { get; set; }
        public int? QuestionId { get; set; }
    }
}
