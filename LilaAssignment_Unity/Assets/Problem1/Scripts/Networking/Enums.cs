using System;
using Newtonsoft.Json.Converters;

namespace TicTacToe
{
    [Serializable]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerSymbol
    {
        X,
        O
    }

    [Serializable]
    public enum RoomStatus
    {
        OPEN,
        ACTIVE,
        FINISHED
    }
}
