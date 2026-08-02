using Microsoft.Agents.AI;

namespace _03_Mem0MemoryProvider
{
    public static class Extensions
    {
        private const string _stateKey = "ExecutionContext";

        public static async ValueTask<AgentSession> CreateSessionWithExecutionContext(this AIAgent aiAgent, SessionExecutionContext executionContext)
        {
            ArgumentNullException.ThrowIfNull(executionContext);

            var session = await aiAgent.CreateSessionAsync();
            session.StateBag.SetValue(_stateKey, executionContext);

            return session;
        }
        public static SessionExecutionContext? GetSessionExecutionContext(this AgentSession agentSession)
        {
            return agentSession.StateBag.TryGetValue<SessionExecutionContext>(_stateKey, out var context) ? context : null;
        }
    }
}
