using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.Attribute;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.Models;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.PluginsModels;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.Register;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.SDK.Basic;
using Sorux.Framework.Bot.Core.Kernel.Interface;
using Sorux.Framework.Bot.Core.Kernel.Utils;
using System.Text.Json;

using RestSharp;
using static System.String;

namespace Dawnchan.Plugins.CodeSandbox
{
    public class CodeSandboxController : BotController
    {
        private readonly string _settingsKey = "jdoodle-code-sandbox";

        private ILoggerService _loggerService;

        private IBasicAPI _bot;

        private IPluginsDataStorage _storage;

        private RestClient _client;

        public CodeSandboxController(ILoggerService loggerService, IBasicAPI bot, IPluginsDataStorage storage)
        {
            _loggerService = loggerService;
            _bot = bot;
            _storage = storage;

            _client = new RestClient("https://api.jdoodle.com/v1/execute");
        }

        [Event(EventType.SoloMessage)]
        [Command(CommandAttribute.Prefix.Single, "csb client [id] [secret]")]
        public PluginFucFlag SetClient(MessageContext context, string id, string secret)
        {
            if (IsNullOrEmpty(_storage.GetStringSettings(_settingsKey, context.TriggerId)))
            {
                _bot.SendPrivateMessage(context, "Resetting your code sandbox client.");
                _storage.RemoveStringSettings(_settingsKey, context.TriggerId);
            }

            _storage.AddStringSettings(_settingsKey, context.TriggerId,
                JsonSerializer.Serialize(new IdSecretPair { Id = id, Secret = secret }));

            return PluginFucFlag.MsgIntercepted;
        }

        [Event(EventType.GroupMessage)]
        [Command(CommandAttribute.Prefix.Single, "csb run [lang] <input>")]
        public PluginFucFlag RunCode(MessageContext context, string lang, string? input)
        {
            var raw = context.Message?.GetRawMessage();
            string code = Empty;
            if (raw is not null)
            {
                raw.Split('\n').ToList().RemoveAt(0);
                code = Concat(raw);
            }

            string clientId, clientSecret;
            var clientResult = GetClientInfoBySoruxId(context.TriggerId, out clientId, out clientSecret);
            if (!clientResult)
            {
                _bot.SendGroupMessage(context, "Your client info is broken. Please reset it.");
            }
            else
            {
                var response = RunCodeAsync(new ExecutionRequestData
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Language = lang,
                    Script = code,
                    Stdin = input
                });

                if (response.Result.Content is not null)
                {
                    if (response.Result.IsSuccessful)
                    {
                        var result = JsonSerializer.Deserialize<ExecutionSuccessfulResponseData>(response.Result.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = false
                        });
                        if (result is not null)
                        {
                            _bot.SendGroupMessage(context, result.Output);
                        }
                    }
                    else
                    {
                        var result = JsonSerializer.Deserialize<ExecutionResponseFailedData>(response.Result.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = false
                        });
                        if (result is not null)
                        {
                            _bot.SendGroupMessage(context, $"{result.StatusCode}: {result.Error}");
                        }
                    }
                }
            }

            return PluginFucFlag.MsgIntercepted;
        }

        private bool GetClientInfoBySoruxId(string soruxId, out string id, out string secret)
        {
            id = "";
            secret = "";
            var idSecretPair =
                JsonSerializer.Deserialize<IdSecretPair>(_storage.GetStringSettings(_settingsKey, soruxId));
            if (idSecretPair is null)
            {
                return false;
            }
            else
            {
                id = idSecretPair.Id;
                secret = idSecretPair.Secret;
                return true;
            }
        }

        private async Task<RestResponse> RunCodeAsync(ExecutionRequestData executionRequestData)
        {
            string json = JsonSerializer.Serialize(executionRequestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new RestRequest("/", Method.Post);
            request.AddStringBody(json, DataFormat.Json);

            return await _client.ExecuteAsync(request);
        }
    }
}