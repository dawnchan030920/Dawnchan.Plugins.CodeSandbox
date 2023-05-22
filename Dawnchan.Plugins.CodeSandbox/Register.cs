using Sorux.Framework.Bot.Core.Interface.PluginsSDK.Ability;
using Sorux.Framework.Bot.Core.Interface.PluginsSDK.Register;

namespace Dawnchan.Plugins.CodeSandbox
{
    public class Register : IBasicInformationRegister, ICommandPrefix
    {
        public string GetAuthor() => "Dawnchan";

        public string GetDescription() => "Sorux plugin for running and showcasing C# during conversation.";

        public string GetName() => "SoruxCodeSandbox";

        public string GetVersion() => "1.0.0";
        
        public string GetDLL() => "Dawnchan.Plugins.CodeSandbox.dll";

        public string GetCommandPrefix() => "#";
    }
}
