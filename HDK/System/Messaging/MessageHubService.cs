using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Services;
using System.Text;
using System.Threading.Tasks;

namespace System.Messaging
{
    interface IMessageHubService : IService
    {
        Task<Task[]> Publish<TEvent>(object sender, Task<TEvent> eventDataTask);
        Task Subscribe<TEvent>(object sender, Func<Task<TEvent>, Task> eventHandlerTaskFactory);
        Task Unsubscribe<TEvent>(object sender);
    }

    /// <summary>
    /// Original Source https://github.com/timothy-makarov/AsyncEventAggregator/blob/master/AsyncEventAggregatorExamples/Program.cs
    /// </summary>
    [Shared]
    [ExportService("Default MessageHub Service", "description", typeof(IMessageHubService))]
    public sealed class MessageHubService : IMessageHubService
    {
        private const string EventTypeNotFoundExceptionMessage = @"Event type not found!";
        private const string SubscribersNotFoundExceptionMessage = @"Subscribers not found!";
        private const string FailedToAddSubscribersExceptionMessage = @"Failed to add subscribers!";
        private const string FailedToGetEventHandlerTaskFactoriesExceptionMessage = @"Failed to get event handler task factories!";
        private const string FailedToAddEventHandlerTaskFactoriesExceptionMessage = @"Failed to add event handler task factories!";
        private const string FailedToGetSubscribersExceptionMessage = @"Failed to get subscribers!";
        private const string FailedToRemoveEventHandlerTaskFactories = @"Failed to remove event handler task factories!";

        private readonly TaskFactory _factory;

        /// <summary>
        ///     Dictionary(EventType, Dictionary(Sender, EventHandlerTaskFactories))
        /// </summary>
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentBag<object>>> _hub;

        public MessageHubService()
        {
            _factory = Task.Factory;

            _hub = new ConcurrentDictionary<Type, ConcurrentDictionary<object, ConcurrentBag<object>>>();
        }

        public Task<Task[]> Publish<TEvent>(object sender, Task<TEvent> eventDataTask)
        {
            var tcs = new TaskCompletionSource<Task[]>();

            _factory.StartNew(
                () =>
                {
                    Type eventType = typeof(TEvent);

                    if (_hub.ContainsKey(eventType))
                    {
                        ConcurrentDictionary<object, ConcurrentBag<object>> subscribers;

                        if (_hub.TryGetValue(eventType, out subscribers))
                        {
                            if (subscribers.Count > 0)
                            {
                                _factory.ContinueWhenAll(
                                    new ConcurrentBag<Task>(
                                        new ConcurrentBag<object>(subscribers.Keys)
                                            .Where(p => p != sender && subscribers.ContainsKey(p))
                                            .Select(p =>
                                            {
                                                ConcurrentBag<object> eventHandlerTaskFactories;

                                                bool isFailed = !subscribers.TryGetValue(p, out eventHandlerTaskFactories);

                                                return new
                                                {
                                                    IsFailed = isFailed,
                                                    EventHandlerTaskFactories = eventHandlerTaskFactories
                                                };
                                            })
                                            .SelectMany(
                                                p =>
                                                {
                                                    if (p.IsFailed)
                                                    {
                                                        var innerTaskCompletionSource = new TaskCompletionSource<Task>();
                                                        innerTaskCompletionSource.SetException(new Exception(FailedToGetEventHandlerTaskFactoriesExceptionMessage));
                                                        return new ConcurrentBag<Task>(new[] { innerTaskCompletionSource.Task });
                                                    }

                                                    return new ConcurrentBag<Task>(
                                                        p.EventHandlerTaskFactories
                                                         .Select(q =>
                                                         {
                                                             try
                                                             {
                                                                 return ((Func<Task<TEvent>, Task>)q)(eventDataTask);
                                                             }
                                                             catch (Exception ex)
                                                             {
                                                                 return _factory.FromException<object>(ex);
                                                             }
                                                         }));
                                                }))
                                        .ToArray(),
                                    tcs.SetResult);
                            }
                            else
                                tcs.SetException(new Exception(SubscribersNotFoundExceptionMessage));
                        }
                        else
                            tcs.SetException(new Exception(SubscribersNotFoundExceptionMessage));
                    }
                    else
                        tcs.SetException(new Exception(EventTypeNotFoundExceptionMessage));
                });

            return tcs.Task;
        }

        public Task Subscribe<TEvent>(object sender, Func<Task<TEvent>, Task> eventHandlerTaskFactory)
        {
            var tcs = new TaskCompletionSource<object>();

            _factory.StartNew(
                () =>
                {
                    ConcurrentDictionary<object, ConcurrentBag<object>> subscribers;
                    ConcurrentBag<object> eventHandlerTaskFactories;

                    Type eventType = typeof(TEvent);

                    if (_hub.ContainsKey(eventType))
                    {
                        if (_hub.TryGetValue(eventType, out subscribers))
                        {
                            if (subscribers.ContainsKey(sender))
                            {
                                if (subscribers.TryGetValue(sender, out eventHandlerTaskFactories))
                                {
                                    eventHandlerTaskFactories.Add(eventHandlerTaskFactory);
                                    tcs.SetResult(null);
                                }
                                else
                                    tcs.SetException(new Exception(FailedToGetEventHandlerTaskFactoriesExceptionMessage));
                            }
                            else
                            {
                                eventHandlerTaskFactories = new ConcurrentBag<object>();

                                if (subscribers.TryAdd(sender, eventHandlerTaskFactories))
                                {
                                    eventHandlerTaskFactories.Add(eventHandlerTaskFactory);
                                    tcs.SetResult(null);
                                }
                                else
                                    tcs.SetException(new Exception(FailedToAddEventHandlerTaskFactoriesExceptionMessage));
                            }
                        }
                        else
                            tcs.SetException(new Exception(FailedToGetSubscribersExceptionMessage));
                    }
                    else
                    {
                        subscribers = new ConcurrentDictionary<object, ConcurrentBag<object>>();

                        if (_hub.TryAdd(eventType, subscribers))
                        {
                            eventHandlerTaskFactories = new ConcurrentBag<object>();

                            if (subscribers.TryAdd(sender, eventHandlerTaskFactories))
                            {
                                eventHandlerTaskFactories.Add(eventHandlerTaskFactory);
                                tcs.SetResult(null);
                            }
                            else
                                tcs.SetException(new Exception(FailedToAddEventHandlerTaskFactoriesExceptionMessage));
                        }
                        else
                            tcs.SetException(new Exception(FailedToAddSubscribersExceptionMessage));
                    }
                });

            return tcs.Task;
        }

        public Task Unsubscribe<TEvent>(object sender)
        {
            var tcs = new TaskCompletionSource<object>();

            _factory.StartNew(
                () =>
                {
                    Type eventType = typeof(TEvent);

                    if (_hub.ContainsKey(eventType))
                    {
                        ConcurrentDictionary<object, ConcurrentBag<object>> subscribers;

                        if (_hub.TryGetValue(eventType, out subscribers))
                        {
                            if (subscribers == null)
                                tcs.SetException(new Exception(FailedToGetSubscribersExceptionMessage));
                            else
                            {
                                if (subscribers.ContainsKey(sender))
                                {
                                    ConcurrentBag<object> eventHandlerTaskFactories;

                                    if (subscribers.TryRemove(sender, out eventHandlerTaskFactories))
                                        tcs.SetResult(null);
                                    else
                                        tcs.SetException(new Exception(FailedToRemoveEventHandlerTaskFactories));
                                }
                                else
                                    tcs.SetException(new Exception(FailedToGetEventHandlerTaskFactoriesExceptionMessage));
                            }
                        }
                        else
                            tcs.SetException(new Exception(FailedToGetSubscribersExceptionMessage));
                    }
                    else
                        tcs.SetException(new Exception(EventTypeNotFoundExceptionMessage));
                });

            return tcs.Task;
        }
    }
}
