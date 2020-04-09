using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{

    /// <summary>
    ///     Represents media player actions
    /// </summary>
    public interface IMediaPlayer : IPlay<IMediaPlayerExecuteAsync>,
        IStop<IMediaPlayerExecuteAsync>, IPlayPause<IMediaPlayerExecuteAsync>,
        IPause<IMediaPlayerExecuteAsync>, ISpeak<IMediaPlayerExecuteAsync>
    {
    }

    /// <summary>
    ///     Excecutes media player actions async
    /// </summary>
    public interface IMediaPlayerExecuteAsync
    {
        /// <summary>
        ///     Excecutes media player actions async
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    ///     Implements the Fluent interface for MediaPlayers
    /// </summary>
    public class MediaPlayerManager : EntityBase, IMediaPlayer, IMediaPlayerExecuteAsync
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityIds">The unique ids of the entities managed</param>
        /// <param name="daemon">The Daemon that will handle API calls to Home Assistant</param>
        /// <param name="app">The Daemon App calling fluent API</param>
        public MediaPlayerManager(IEnumerable<string> entityIds, INetDaemon daemon, INetDaemonApp app) : base(entityIds, daemon, app)
        {
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Pause()
        {
            _currentAction = new FluentAction(FluentActionType.Pause);
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Play()
        {
            _currentAction = new FluentAction(FluentActionType.Play);
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync PlayPause()
        {
            _currentAction = new FluentAction(FluentActionType.PlayPause);
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Speak(string message)
        {
            _currentAction = new FluentAction(FluentActionType.Speak);
            _currentAction.MessageToSpeak = message;
            return this;
        }

        /// <inheritdoc/>
        public IMediaPlayerExecuteAsync Stop()
        {
            _currentAction = new FluentAction(FluentActionType.Stop);
            return this;
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync()
        {
            _ = _currentAction ?? throw new NullReferenceException("Missing fluent action type!");

            var executeTask = _currentAction.ActionType switch
            {
                FluentActionType.Play => CallServiceOnAllEntities("media_play"),
                FluentActionType.Pause => CallServiceOnAllEntities("media_pause"),
                FluentActionType.PlayPause => CallServiceOnAllEntities("media_play_pause"),
                FluentActionType.Stop => CallServiceOnAllEntities("media_stop"),
                FluentActionType.Speak => Speak(),
                _ => throw new NotSupportedException($"Action type not supported {_currentAction.ActionType}")
            };

            await executeTask.ConfigureAwait(false);

            // Use local function to get the nice switch statement above:)
            Task Speak()
            {
                foreach (var entityId in EntityIds)
                {
                    var message = _currentAction?.MessageToSpeak ??
                        throw new NullReferenceException("Message to speak is null or empty!");

                    Daemon.Speak(entityId, message);
                }
                return Task.CompletedTask;
            }
        }
    }

}