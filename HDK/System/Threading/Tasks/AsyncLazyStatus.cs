using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public enum AsyncLazyStatus
    {
        //
        // Summary:
        //     The async m_Adaptee object has not been initialized yet.
        NotInitialized = -1,

        // Summary:
        //     The async m_Adaptee object has been initialized but has not yet been scheduled.
        Created = 0,

        //
        // Summary:
        //     The async m_Adaptee object is waiting to be activated and scheduled internally by the .NET
        //     Framework infrastructure.
        WaitingForActivation = 1,

        //
        // Summary:
        //     The async m_Adaptee object has been scheduled for execution but has not yet begun executing.
        WaitingToRun = 2,

        //
        // Summary:
        //     The async m_Adaptee object is running but has not yet completed.
        Running = 3,

        //
        // Summary:
        //     The async m_Adaptee object has finished executing and is implicitly waiting for attached child
        //     tasks to complete.
        WaitingForChildrenToComplete = 4,

        //
        // Summary:
        //     The async m_Adaptee object completed execution successfully.
        RanToCompletion = 5,

        //
        // Summary:
        //     The async m_Adaptee object acknowledged cancellation by throwing an OperationCanceledException
        //     with its own CancellationToken while the token was in signaled state, or
        //     the task's CancellationToken was already signaled before the task started
        //     executing. For more information, see m_adoptedTask Cancellation.
        Canceled = 6,

        //
        // Summary:
        //     The async m_Adaptee object completed due to an unhandled exception.
        Faulted = 7
    }

}
