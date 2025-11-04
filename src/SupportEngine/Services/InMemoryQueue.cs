using System.Threading.Channels;
using SupportEngine.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SupportEngine.Services
{
    public interface IQuestionQueue
    {
        ValueTask EnqueueAsync(QuestionMessage message);
        ValueTask<QuestionMessage?> DequeueAsync(CancellationToken cancellationToken);
    }

    public class InMemoryQueue : IQuestionQueue
    {
        private readonly Channel<QuestionMessage> _channel;

        public InMemoryQueue()
        {
            _channel = Channel.CreateUnbounded<QuestionMessage>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
        }

        public ValueTask EnqueueAsync(QuestionMessage message)
        {
            return _channel.Writer.WriteAsync(message);
        }

        public async ValueTask<QuestionMessage?> DequeueAsync(CancellationToken cancellationToken)
        {
            try
            {
                var item = await _channel.Reader.ReadAsync(cancellationToken);
                return item;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}