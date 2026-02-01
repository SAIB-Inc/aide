using Aide.Ui.Models;

namespace Aide.Ui.Services;

public class AppStateService
{
    #region Properties

    private bool _isSidebarOpen = true;
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            if (_isSidebarOpen != value)
            {
                _isSidebarOpen = value;
                NotifyChanged();
            }
        }
    }

    private bool _isDarkMode = false;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                NotifyChanged();
            }
        }
    }

    private string? _currentChatId;
    public string? CurrentChatId
    {
        get => _currentChatId;
        set
        {
            if (_currentChatId != value)
            {
                _currentChatId = value;
                NotifyChanged();
            }
        }
    }

    private List<ChatSession> _chatSessions = [];
    public List<ChatSession> ChatSessions => _chatSessions;

    #endregion

    #region Events

    public event Action? OnChanged;
    private void NotifyChanged() => OnChanged?.Invoke();

    #endregion

    #region Methods

    public void CreateNewChat()
    {
        var newSession = new ChatSession
        {
            Id = Guid.NewGuid().ToString(),
            Title = "New Chat",
            CreatedAt = DateTime.Now,
            LastMessageAt = DateTime.Now,
            Messages = []
        };

        _chatSessions.Insert(0, newSession);
        CurrentChatId = newSession.Id;
        NotifyChanged();
    }

    public void DeleteChat(string chatId)
    {
        var session = _chatSessions.FirstOrDefault(s => s.Id == chatId);
        if (session != null)
        {
            _chatSessions.Remove(session);

            // If we deleted the current chat, select another one or create new
            if (CurrentChatId == chatId)
            {
                CurrentChatId = _chatSessions.FirstOrDefault()?.Id;
                if (CurrentChatId == null)
                {
                    CreateNewChat();
                }
            }

            NotifyChanged();
        }
    }

    public void SelectChat(string chatId)
    {
        CurrentChatId = chatId;
        NotifyChanged();
    }

    public ChatSession? GetCurrentChat()
    {
        return _chatSessions.FirstOrDefault(s => s.Id == CurrentChatId);
    }

    public void UpdateChatTitle(string chatId, string newTitle)
    {
        var session = _chatSessions.FirstOrDefault(s => s.Id == chatId);
        if (session != null)
        {
            session.Title = newTitle;
            NotifyChanged();
        }
    }

    public void AddMessageToChat(string chatId, ChatMessage message)
    {
        var session = _chatSessions.FirstOrDefault(s => s.Id == chatId);
        if (session != null)
        {
            session.Messages.Add(message);
            session.LastMessageAt = DateTime.Now;

            // Auto-generate title from first user message
            if (session.Messages.Count == 1 && message.Role == "user" && session.Title == "New Chat")
            {
                session.Title = message.Content.Length > 50
                    ? message.Content[..50] + "..."
                    : message.Content;
            }

            NotifyChanged();
        }
    }

    #endregion
}
