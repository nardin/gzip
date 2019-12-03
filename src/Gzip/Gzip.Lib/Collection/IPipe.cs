using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Gzip.Lib.Tests")]

namespace Gzip.Lib.Collection
{
    /// <summary>
    /// Pipe interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IPipe<T>
    {
        /// <summary>
        /// Attach to pipe. 
        /// </summary>
        /// <remarks>Need for know count producer.</remarks>
        void Attach();

        /// <summary>
        /// Deattach to pipe. 
        /// </summary>
        /// <remarks>Need for know count producer.</remarks>
        void Deattach();

        /// <summary>
        /// Pipe is completed.
        /// </summary>
        bool IsCompleted
        {
            get;
        }

        /// <summary>
        /// Add message to pipe
        /// </summary>
        /// <param name="message">Message</param>
        void Add(T message);

        /// <summary>
        /// Get message from pipe.
        /// </summary>
        /// <returns>Message</returns>
        T Take();

    }
}
