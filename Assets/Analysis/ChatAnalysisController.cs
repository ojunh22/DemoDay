using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using OpenAI_API.Models;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Threading.Tasks;
public class ChatgptAPIController : MonoBehaviour
{
    public TMP_Text textfield;
    public TMP_InputField inputField;
    public Button okBtn;
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    private static readonly HttpClient httpClient = new HttpClient();  // HttpClient를 재사용 가능한 필드로 선언

    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI(new APIAuthentication(""));  // 기존 생성자 사용
        startConversation();
        okBtn.onClick.AddListener(() => Getresponse());
    }

    private void startConversation()
    {
        messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "너는 고등학교 학생들에게 진로에 대해 상담해주는 시스템이야. ...")
        };

        inputField.text = "";
        string startString = "안녕하세요! 진로와 생기부에 대해 고민이 있으시다면 도움을 드리겠습니다!";
        textfield.text = startString;
        Debug.Log(startString);
    }

    private async void Getresponse()
    {
        if (inputField.text.Length < 1)
        {
            return;
        }

        okBtn.enabled = false;

        ChatMessage userMessage = new ChatMessage
        {
            Role = ChatMessageRole.User,
            Content = inputField.text.Length > 100 ? inputField.text.Substring(0, 100) : inputField.text
        };

        Debug.Log(string.Format("{0} : {1}", userMessage.Role, userMessage.Content));

        messages.Add(userMessage);

        textfield.text = string.Format("You : {0}", userMessage.Content);
        inputField.text = "";

        var chatResult = await SendChatRequestWithRetry();

        if (chatResult != null)
        {
            ChatMessage responseMessage = new ChatMessage
            {
                Role = chatResult.Choices[0].Message.Role,
                Content = chatResult.Choices[0].Message.Content
            };
            Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
            messages.Add(responseMessage);

            textfield.text = string.Format("You : {0}\n\nChatGPT:\n{1}", userMessage.Content, responseMessage.Content);
        }

        okBtn.enabled = true;
    }

    private async Task<ChatResult> SendChatRequestWithRetry()
{
    const int maxRetries = 3;
    const int delayMilliseconds = 5000;  // 5초 대기

    for (int retry = 0; retry < maxRetries; retry++)
    {
        try
        {
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.9,
                MaxTokens = 2000,
                Messages = messages
            });

            if (chatResult != null)
            {
                Debug.Log("Successfully received response from ChatGPT.");
                return chatResult;
            }
            else
            {
                Debug.LogWarning("Received null response from ChatGPT.");
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.LogWarning($"HttpRequestException occurred: {ex.Message}");

            if (ex.Message.Contains("TooManyRequests"))
            {
                Debug.LogWarning("Too many requests. Waiting before retrying...");
                await Task.Delay(delayMilliseconds);  // 일정 시간 대기 후 재시도
            }
            else
            {
                Debug.LogError($"Failed to send request: {ex.Message}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
            return null;
        }
    }

    Debug.LogError("Failed to get response after multiple retries.");
    return null;
}
}
