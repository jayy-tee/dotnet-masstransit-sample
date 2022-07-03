using System.Diagnostics;

namespace JayyTee.MassTransitSample.Worker.Tests.Infrastructure;

public static class RetryHelper
{
    public static void Eventually(Action callback, int timeoutInSeconds, int waitBetweenRetriesInMillis)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var timeout = timeoutInSeconds * 1000;
        bool successIndicator = false;
        int retries = 0;

        Exception? lastException = null;

        do
        {
            try
            {
                callback.Invoke();
                successIndicator = true;
                break;
            }
            catch (Exception e)
            {
                lastException = e;

                retries++;
                Console.WriteLine($"Retry Attempt {retries}: Waiting {waitBetweenRetriesInMillis}ms");
                Thread.Sleep(waitBetweenRetriesInMillis);
            }
        } while (stopwatch.ElapsedMilliseconds <= timeout);

        if (successIndicator is false)
        {
            Console.WriteLine($"Retry Timeout expired after {timeoutInSeconds}s. Failing test.");
            Assert.Fail(lastException!.Message);
        }

        stopwatch.Stop();
    }

    public static async Task EventuallyAsync(Func<Task> callback, int timeoutInSeconds, int waitBetweenRetriesInMillis)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var timeout = timeoutInSeconds * 1000;
        bool successIndicator = false;
        int retries = 0;

        Exception? lastException = null;

        do
        {
            try
            {
                await callback();
                successIndicator = true;
                break;
            }
            catch (Exception e)
            {
                lastException = e;

                retries++;
                Console.WriteLine($"Retry Attempt {retries}: Waiting {waitBetweenRetriesInMillis}ms");
                Thread.Sleep(waitBetweenRetriesInMillis);
            }
        } while (stopwatch.ElapsedMilliseconds <= timeout);

        if (successIndicator is false)
        {
            Console.WriteLine($"Retry Timeout expired after {timeoutInSeconds}s. Failing test.");
            Assert.Fail(lastException!.Message);
        }

        stopwatch.Stop();
    }

    public static async Task<T?> EventuallyReturnAsync<T>(Func<Task<T?>> callback, int timeoutInSeconds, int waitBetweenRetriesInMillis)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var timeout = timeoutInSeconds * 1000;
        bool successIndicator = false;
        int retries = 0;

        Exception? lastException = null;
        T? response = default;

        do
        {
            try
            {
                response = await callback();
                successIndicator = true;
                break;
            }
            catch (Exception e)
            {
                lastException = e;

                retries++;
                Console.WriteLine($"Retry Attempt {retries}: Waiting {waitBetweenRetriesInMillis}ms");
                Thread.Sleep(waitBetweenRetriesInMillis);
            }
        } while (stopwatch.ElapsedMilliseconds <= timeout);

        if (successIndicator is false)
        {
            Console.WriteLine($"Retry Timeout expired after {timeoutInSeconds}s. Failing test.");
            Assert.Fail(lastException!.Message);
        }

        stopwatch.Stop();
        return response;
    }
}
