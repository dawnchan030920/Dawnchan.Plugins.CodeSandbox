using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnchan.Plugins.CodeSandbox
{
    public class ExecutionRequestData
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Script { get; set; }
        public string? Stdin { get; set; }
        public string Language { get; set; }
    }
}
