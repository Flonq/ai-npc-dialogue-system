using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using AINPC.Dialogue;

namespace AINPC.AI
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChoiceDialogue choiceDialogue;

        [Header("LM Studio")]
        [SerializeField] private string endpoint = "http://localhost:1234/v1/chat/completions";
        [SerializeField] private string model = "qwen2.5-0.5b-instruct";
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private int maxTokens = 200;
        [SerializeField] private int requestTimeoutSeconds = 60;

        [Header("Prompts")]
        [TextArea(8, 20)]
        [SerializeField] private string systemPromptTemplate =
@"Sen birinci şahıs bir video oyununda yaşayan bir NPC'sin.
Karakterinden çıkma. Cevapların kısa olsun (1-2 cümle).
Tüm cevaplarını her zaman akıcı, doğal Türkçe ile ver.

API çağrısında bir JSON şeması alacaksın. Cevabını her zaman bu
şemaya uyan geçerli JSON olarak ver. ""oyuncu seçeneği 1"" gibi
yer tutucular ya da <...> gibi köşeli parantezler ASLA kullanma.
Her seçenek, oyuncunun gerçekten söyleyebileceği somut, doğal
bir Türkçe cümle olmalı.

'choices' kuralları:
- Tam olarak 3 öğe.
- Her öğe en fazla 8 kelimelik, doğal bir oyuncu cümlesi.
- Seçenekler birbirinden farklı olmalı.
- ""seçenek"" kelimesi ya da numaralandırma içermesin.
- Seçenekler içinde bulunulan konuşma bağlamına uygun olmalı.
- Tüm seçenekler Türkçe olmalı.

İyi örnek (sadece üslup için, harfiyen kopyalama):
{
  ""npc_reply"": ""Bu yüzü daha önce görmemiştim, yabancı."",
  ""choices"": [""Bugün buraya geldim."", ""Nerede uyuyabilirim?"", ""Hoşça kal, demirci.""]
}

NPC profili:
- İsim: {NAME}
- Rol: {ROLE}
- Ruh hali: {MOOD}
- Kişilik: {PERSONALITY}";

        private NpcProfile activeProfile;
        private readonly List<ChatMessage> history = new List<ChatMessage>();
        private string[] currentChoices = new string[0];
        private bool isWaiting;

        public void StartDialogue(NpcProfile profile)
        {
            if (isWaiting)
                return;

            if (profile == null)
            {
                Debug.LogError("[DialogueManager] StartDialogue called with null profile.");
                return;
            }

            activeProfile = profile;

            history.Clear();
            history.Add(new ChatMessage { role = "system", content = BuildSystemPrompt(profile) });
            history.Add(new ChatMessage { role = "user", content = profile.firstUserMessage });

            if (choiceDialogue != null)
                choiceDialogue.ShowLoading();

            StartCoroutine(RequestTurn());
        }

        private string BuildSystemPrompt(NpcProfile profile)
        {
            return systemPromptTemplate
                .Replace("{NAME}", profile.displayName ?? "")
                .Replace("{ROLE}", profile.role ?? "")
                .Replace("{MOOD}", profile.mood ?? "")
                .Replace("{PERSONALITY}", profile.personality ?? "");
        }

        public void OnPlayerChose(int index)
        {
            if (isWaiting)
                return;

            if (currentChoices == null || index < 0 || index >= currentChoices.Length)
                return;

            string playerLine = currentChoices[index];
            history.Add(new ChatMessage { role = "user", content = playerLine });

            if (choiceDialogue != null)
                choiceDialogue.ShowLoading();

            StartCoroutine(RequestTurn());
        }

        public void EndDialogue()
        {
            StopAllCoroutines();
            history.Clear();
            currentChoices = new string[0];
            activeProfile = null;
            isWaiting = false;
        }

        private IEnumerator RequestTurn()
        {
            isWaiting = true;

            var body = new
            {
                model,
                temperature,
                max_tokens = maxTokens,
                messages = history.ToArray(),
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "npc_turn",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                npc_reply = new { type = "string" },
                                choices = new
                                {
                                    type = "array",
                                    items = new { type = "string" },
                                    minItems = 3,
                                    maxItems = 3
                                }
                            },
                            required = new[] { "npc_reply", "choices" },
                            additionalProperties = false
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(body);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(bodyBytes);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = requestTimeoutSeconds;

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[DialogueManager] HTTP error: {req.error}\n{req.downloadHandler.text}");
                    ShowFallback();
                    isWaiting = false;
                    yield break;
                }

                string responseJson = req.downloadHandler.text;
                string assistantContent = ExtractAssistantContent(responseJson);

                if (string.IsNullOrEmpty(assistantContent))
                {
                    Debug.LogError("[DialogueManager] Empty assistant content.");
                    ShowFallback();
                    isWaiting = false;
                    yield break;
                }

                history.Add(new ChatMessage { role = "assistant", content = assistantContent });

                NpcTurn turn = TryParseNpcTurn(assistantContent);
                if (turn == null || string.IsNullOrEmpty(turn.npc_reply))
                {
                    Debug.LogWarning($"[DialogueManager] Could not parse JSON. Raw:\n{assistantContent}");
                    ShowFallback();
                    isWaiting = false;
                    yield break;
                }

                currentChoices = turn.choices ?? new string[0];

                if (choiceDialogue != null)
                    choiceDialogue.SetTurn(turn.npc_reply, currentChoices);

                isWaiting = false;
            }
        }

        private string ExtractAssistantContent(string responseJson)
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<ChatResponse>(responseJson);
                if (parsed != null && parsed.choices != null && parsed.choices.Length > 0
                    && parsed.choices[0].message != null)
                    return parsed.choices[0].message.content;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Outer parse error: {ex.Message}");
            }
            return null;
        }

        private NpcTurn TryParseNpcTurn(string content)
        {
            int start = content.IndexOf('{');
            int end = content.LastIndexOf('}');
            if (start < 0 || end < 0 || end <= start)
                return null;

            string jsonChunk = content.Substring(start, end - start + 1);

            try
            {
                return JsonConvert.DeserializeObject<NpcTurn>(jsonChunk);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueManager] Inner parse error: {ex.Message}\nChunk: {jsonChunk}");
                return null;
            }
        }

        private void ShowFallback()
        {
            currentChoices = new[] { "Tekrar dene", "Ayrıl" };
            if (choiceDialogue != null)
                choiceDialogue.SetTurn("...", currentChoices);
        }

        [Serializable]
        private class ChatResponse
        {
            public Choice[] choices;
        }

        [Serializable]
        private class Choice
        {
            public ChatMessage message;
        }

        [Serializable]
        private class NpcTurn
        {
            public string npc_reply;
            public string[] choices;
        }
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }
}