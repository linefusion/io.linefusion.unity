using System;
using UnityEngine;

namespace MyProject.Events
{
    [Serializable]
    public enum ChatMessageKind
    {
        Local,
        Global
    }

    [Event]
    [Serializable]
    public struct ChatMessageReceived
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Message;

        [SerializeField]
        public ChatMessageKind Kind;

        public ChatMessageReceived(string name = "", string message = "", ChatMessageKind kind = ChatMessageKind.Local)
        {
            this.Name = name;
            this.Message = message;
            this.Kind = kind;
        }
    }
}
