using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    [Event]
    public struct GameScore
    {
        public int score;

        public GameScore(int score)
        {
            this.score = score;
        }
    }
}
