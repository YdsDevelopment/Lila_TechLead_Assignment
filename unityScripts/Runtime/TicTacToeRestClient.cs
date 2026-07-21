using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace TicTacToeMultiplayer
{
    /// <summary>Typed HTTP reads for the backend lobby and diagnostics endpoints.</summary>
    public sealed class TicTacToeRestClient
    {
        private readonly string baseUrl;

        public TicTacToeRestClient(string serverUrl)
        {
            baseUrl = serverUrl.TrimEnd('/');
        }

        public async Task<HealthResponse> GetHealthAsync()
        {
            return await GetAsync<HealthResponse>("/health");
        }

        /// <summary>Returns OPEN rooms only; active and finished rooms are not in this response.</summary>
        public async Task<RoomSummaryResponse[]> GetOpenRoomsAsync()
        {
            return await GetAsync<RoomSummaryResponse[]>("/rooms");
        }

        public async Task<RoomDetailsResponse> GetRoomAsync(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException("A room ID is required.", nameof(roomId));
            }

            return await GetAsync<RoomDetailsResponse>("/rooms/" + UnityWebRequest.EscapeURL(roomId));
        }

        private async Task<T> GetAsync<T>(string path)
        {
            using (var request = UnityWebRequest.Get(baseUrl + path))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                var body = request.downloadHandler == null ? string.Empty : request.downloadHandler.text;
                if (request.result != UnityWebRequest.Result.Success)
                {
                    var error = TryReadError(body);
                    throw new TicTacToeApiException(request.responseCode, error ?? request.error);
                }

                return JsonConvert.DeserializeObject<T>(body);
            }
        }

        private static string TryReadError(string body)
        {
            try
            {
                return JsonConvert.DeserializeObject<RestErrorResponse>(body)?.Error;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public sealed class TicTacToeApiException : Exception
    {
        public long StatusCode { get; }

        public TicTacToeApiException(long statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
