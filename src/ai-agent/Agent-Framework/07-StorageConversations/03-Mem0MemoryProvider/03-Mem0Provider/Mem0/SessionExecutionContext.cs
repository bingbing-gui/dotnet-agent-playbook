using System;
using System.Collections.Generic;
using System.Text;

namespace _03_Mem0MemoryProvider
{
    public record SessionExecutionContext
    {
        public string? RunId { get; init; }
        public required string UserId { get; init; }
        public string? ApplicationId { get; init; }
        public string? AgentId { get; init; }
    }
}
