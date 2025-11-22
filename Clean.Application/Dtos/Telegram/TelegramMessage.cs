namespace Clean.Application.Dtos.Telegram;

public class TelegramMessage
    
{
    public long MessageId { get; set; }
    public TelegramChat Chat { get; set; }
    public string Text { get; set; }
}