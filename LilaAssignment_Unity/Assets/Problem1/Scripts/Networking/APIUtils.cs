using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace TicTacToe
{
    public static class APIUtils
    {
        private static string BaseUrl => NetworkManager.Instance != null
            ? NetworkManager.Instance.ServerUrl.TrimEnd('/')
            : "http://localhost:3000";

        public static async Task<HealthResponse> GetHealthAsync()
        {
            return await GetAsync<HealthResponse>("/health");
        }

        public static async Task<RoomSummary[]> GetOpenRoomsAsync()
        {
            return await GetAsync<RoomSummary[]>("/rooms");
        }

        public static async Task<RoomDetailsResponse> GetRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentException("Room ID is required", nameof(roomId));

            return await GetAsync<RoomDetailsResponse>("/rooms/" + UnityWebRequest.EscapeURL(roomId));
        }

        private static async Task<T> GetAsync<T>(string path)
        {
            using (var request = UnityWebRequest.Get(BaseUrl + path))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                var body = request.downloadHandler?.text ?? string.Empty;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    var error = TryReadError(body);
                    throw new ApiException((int)request.responseCode, error ?? request.error);
                }

                return JsonHelper.Deserialize<T>(body);
            }
        }

        private static string TryReadError(string body)
        {
            try
            {
                var error = JsonHelper.Deserialize<ErrorResponse>(body);
                return error?.message ?? error?.error;
            }
            catch
            {
                return null;
            }
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
