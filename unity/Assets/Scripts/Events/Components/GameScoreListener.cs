/// <auto-generated/>
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    public class GameScoreListener : MonoBehaviour
    {
        public UnityEvent<GameScore> onGameScore = new();
        public void OnEnable()
        {
            Channel.GameScore.Fired += OnTrigger;
        }

        public void OnDisable()
        {
            Channel.GameScore.Fired -= OnTrigger;
        }

        private void OnTrigger(Event<MyProject.Events.GameScore>.Args args)
        {
            onGameScore.Invoke(args.Data);
        }
    }
}