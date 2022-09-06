using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace FishingCactus.Util
{
    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        private UnityWebRequestAsyncOperation asyncOp;
        private Action continuation;

        public UnityWebRequestAwaiter( UnityWebRequestAsyncOperation async_operation )
        {
            this.asyncOp = async_operation;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted { get { return asyncOp.isDone; } }

        public void GetResult() { }

        public void OnCompleted( Action continuation )
        {
            this.continuation = continuation;
        }

        private void OnRequestCompleted( AsyncOperation obj )
        {
            continuation();
        }
    }

    public static class ExtensionMethods
    {
        public static UnityWebRequestAwaiter GetAwaiter( this UnityWebRequestAsyncOperation async_operation )
        {
            return new UnityWebRequestAwaiter( async_operation );
        }
    }
}