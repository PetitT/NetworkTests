using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace FishingCactus.Util
{
    public static class TaskExtensions
    {
        public static IEnumerator AsEnumerator( this Task task )
        {
            while ( !task.IsCompleted )
            {
                yield return null;
            }
            
            if ( task.IsFaulted )
            {
                // When an exception is thrown again ("throw ex"), its stack will be
                // overwritten. Instead, it should be rethrown with its original context.

                // More info here (Figure 2):
                // https://msdn.microsoft.com/en-us/magazine/mt620018.aspx
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }
        }

        public static IEnumerator AsEnumerator<T>( this Task<T> task, Ref<T> result) where T : struct
        {
            yield return AsEnumerator(task);

            result.Value = task.Result;
        }

        public static IEnumerator AsEnumerator<T>( this Task<T> task, T result) where T : class
        {
            yield return AsEnumerator(task);

            result = task.Result;
        }
    }

    /// <summary>
    /// A simple boxing utility to allow us to pass data by reference in coroutines
    /// </summary>
    public class Ref<T> where T : struct
    {
        public T Value { get; set; }
        public Ref(T reference)
        {
            Value = reference;
        }

        public static implicit operator Ref<T>(T value)
        {
            return new Ref<T>(value);
        }

        public static implicit operator T(Ref<T> value)
        {
            return value.Value;
        }
    }
}