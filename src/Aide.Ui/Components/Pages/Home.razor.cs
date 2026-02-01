using Aide.Ui.Models;
using Aide.Ui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

namespace Aide.Ui.Components.Pages;

public partial class Home : IDisposable
{
    [Inject]
    public required AideApiClient ApiClient { get; set; }

    [Inject]
    public required ILogger<Home> Logger { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required AppStateService AppStateService { get; set; }

    private List<ChatMessage> _currentMessages = [];
    private string _userInput = "";
    private bool _isLoading = false;
    private ElementReference _messagesContainer;

    protected override void OnInitialized()
    {
        AppStateService.OnChanged += HandleStateChanged;
        LoadCurrentChat();
    }

    private async void HandleStateChanged()
    {
        LoadCurrentChat();
        await InvokeAsync(StateHasChanged);
    }

    private void LoadCurrentChat()
    {
        var currentChat = AppStateService.GetCurrentChat();
        if (currentChat != null)
        {
            _currentMessages = currentChat.Messages;
        }
        else
        {
            // Create initial chat if none exists
            if (!AppStateService.ChatSessions.Any())
            {
                AppStateService.CreateNewChat();
                _currentMessages = AppStateService.GetCurrentChat()?.Messages ?? [];
            }
        }
    }

    protected string GetCurrentChatTitle()
    {
        var currentChat = AppStateService.GetCurrentChat();
        return currentChat?.Title ?? "New Chat";
    }

    protected async Task SendSuggestion(string suggestion)
    {
        _userInput = suggestion;
        await SendMessage();
    }

    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey && !string.IsNullOrWhiteSpace(_userInput) && !_isLoading)
        {
            await SendMessage();
        }
    }

    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_userInput) || _isLoading)
            return;

        var message = _userInput.Trim();
        _userInput = "";

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = message,
            Timestamp = DateTime.Now
        };

        // Add to current chat session
        if (AppStateService.CurrentChatId != null)
        {
            AppStateService.AddMessageToChat(AppStateService.CurrentChatId, userMessage);
        }

        _isLoading = true;
        StateHasChanged();

        try
        {
            Logger.LogInformation("Sending message to API: {Message}", message);

            var response = await ApiClient.SendChatMessageAsync(
                message: message,
                sessionId: AppStateService.CurrentChatId ?? ""
            );

            Logger.LogInformation("Received response from API");

            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = response.Message,
                Timestamp = response.Timestamp
            };

            // Add to current chat session
            if (AppStateService.CurrentChatId != null)
            {
                AppStateService.AddMessageToChat(AppStateService.CurrentChatId, assistantMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message");
            Snackbar.Add($"Failed to send message: {ex.Message}", Severity.Error);

            // Remove the user message if API call failed
            if (AppStateService.CurrentChatId != null)
            {
                var currentChat = AppStateService.GetCurrentChat();
                if (currentChat?.Messages.LastOrDefault() == userMessage)
                {
                    currentChat.Messages.Remove(userMessage);
                }
            }
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        AppStateService.OnChanged -= HandleStateChanged;
    }
}
