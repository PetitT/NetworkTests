using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AwaitTest : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("Begin Start");
        string bidule = await MyAsyncThing();
        Debug.Log("End Start");
        Debug.Log(bidule);
    }

    private async Task MyTask()
    {
        Debug.Log("Begin Task");
        bool isWaiting = true;

        StartCoroutine(ThingToWait(
            () =>
            {
                Debug.Log("Callback");
                isWaiting = false;
            }));

        while (isWaiting)
        {
            await Task.Yield();
        }

        Debug.Log("End of task");
    }

    private IEnumerator ThingToWait(Action onFinish)
    {
        Debug.Log("Begin coroutine");
        yield return new WaitForSeconds(2);
        Debug.Log("End coroutine");
        onFinish?.Invoke();
    }

    public Task<string> MyAsyncThing()
    {
        var tcs = new TaskCompletionSource<string>();

        StartCoroutine(ThingToWait(
            () =>
            {
                tcs.TrySetResult("Ja");
            }));

        return tcs.Task;
    }
}
