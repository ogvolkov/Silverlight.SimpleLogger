using System;
using System.Collections.Generic;
using System.Threading;

namespace Silverlight.SimpleLogger
{
    /// <summary>
    /// Queue of background actions to be executed in a separate thread
    /// </summary>
    public class BackgroundActionsProcessor
    {
        /// <summary>
        /// Thread on which actions will be executed
        /// </summary>
        private readonly Thread _thread;

        /// <summary>
        /// Actions queue
        /// </summary>
        private readonly Queue<Action> _actions = new Queue<Action>();

        /// <summary>
        /// Initializes background action processor and starts its thread
        /// </summary>
        public BackgroundActionsProcessor()
        {
            _thread = new Thread(ProcessRequests);
            _thread.Start();
        }

        /// <summary>
        /// Enqueues new action in execution queue
        /// </summary>
        /// <param name="action">Action to be executed</param>
        public void Enqueue(Action action)
        {
            lock (_actions)
            {
                _actions.Enqueue(action);
                if (_actions.Count == 1)
                {
                    Monitor.Pulse(_actions); // inform waiting Remove that new action is added
                }
            }
        }

        /// <summary>
        /// Removes action from a queue
        /// Wait for it if queue is empty        
        /// </summary>
        /// <returns>Action that was added first to the queue</returns>
        private Action Remove()
        {
            lock (_actions)
            {
                if (_actions.Count == 0)
                {
                    Monitor.Wait(_actions);
                }

                return _actions.Dequeue();
            }
        }

        /// <summary>
        /// Processes actions queue
        /// </summary>
        private void ProcessRequests()
        {
            for (;;)
            {
                var action = Remove();
                if (action != null)
                {
                    action();
                }
            }
        }
    }
}
