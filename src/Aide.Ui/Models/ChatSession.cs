namespace Aide.Ui.Models;

public record ChatSession
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Chat";
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime LastMessageAt { get; set; } = DateTime.Now;
    public List<ChatMessage> Messages { get; init; } = [];
}

public record ChatMessage
{
    public required string Role { get; init; }
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
}
