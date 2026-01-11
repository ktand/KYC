using KYC.Service.ExternalClients;

namespace KYC.Service.Extensions;

public static class TaskExtensions
{
    /// <summary>
    /// Awaits the specified task and returns its result. If the task fails with an ApiException> with a status code
    /// of 404 (Not Found), it returns null instead.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the task. Must be a reference type.</typeparam>
    /// <param name="task">The task to be awaited.</param>
    /// <returns>
    /// The result of the task if successful. If an ApiException with a 404 status code is thrown, returns null.
    /// </returns>
    public static async Task<T?> GetOrReturnNullOnNotFound<T>(this Task<T> task) where T : class
    {
        try
        {
            return await task;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}