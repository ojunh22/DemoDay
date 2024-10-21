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
public class ChatgptAPIController : MonoBehaviour
{
    public TMP_Text textfield;
    public TMP_InputField inputField;
    public Button okBtn;
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    // Start is called before the first frame update
    void Start()
    {

        api = new OpenAIAPI("");
        startConversation();
        okBtn.onClick.AddListener(() => Getresponse());
    }

    private void startConversation()
    {
        messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, "너는 고등학교 학생들에게 진로에 대해 상담해주는 시스템이야. 만약 자신의 진로에 대해 알아보고 싶다고 하면, 너는 현재 대화상대가 관심이 있는 직업이 무엇인지 물어보고, 그 직업과 관련된 학과와, 어떤 활동을 하면 그 학과에 들어가는데 도움이 되는 생활기록부를 구성할 수 있는지를 설명해 주어야해. 만약 아직 진로를 정하지 못하였다고 한다면, 자신의 진로를 정할 수 있는 방법을 추천해줘.")

        } ;
        
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

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        if (userMessage.Content.Length > 100)
        {
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }
        Debug.Log(string.Format("{0} : {1}", userMessage.Role, userMessage.Content));

        messages.Add(userMessage);

        textfield.text = string.Format("You : {0}", userMessage.Content);
        inputField.text = "";

        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 2000,
            Messages = messages
        });

        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
        messages.Add(responseMessage);

        textfield.text = string.Format("You : {0}\n\nChatGPT:\n{1}", userMessage.Content, responseMessage.Content);

        okBtn.enabled = true;
    }
    

    // Update is called once per frame

}
