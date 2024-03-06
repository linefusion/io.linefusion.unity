/// <auto-generated/>
using System;
using UnityEngine;

public class BulletHitChannel : Event<BulletHit>
{
    public void Fire(Vector3.ctor)
    {
        this.Dispatch(new BulletHit(Vector3));
    }

    public void FireAt(Vector3.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new BulletHit( .ctor), __position);
    }
}

public class BulletMissedChannel : Event<BulletMissed>
{
    public void Fire()
    {
        this.Dispatch(new BulletMissed());
    }

    public void FireAt(UnityEngine.Vector3 position)
    {
        this.Dispatch(new BulletMissed(), position);
    }
}

public class BulletSpawnedChannel : Event<BulletSpawned>
{
    public void Fire(Vector3.ctor)
    {
        this.Dispatch(new BulletSpawned(Vector3));
    }

    public void FireAt(Vector3.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new BulletSpawned( .ctor), __position);
    }
}

public class ChatMessageReceivedChannel : Event<ChatMessageReceived>
{
    public void Fire(String.ctor, String.ctor, ChatMessageKind.ctor)
    {
        this.Dispatch(new ChatMessageReceived(String, String, ChatMessageKind));
    }

    public void FireAt(String.ctor, String.ctor, ChatMessageKind.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new ChatMessageReceived( .ctor, .ctor, .ctor), __position);
    }
}

public class GameScoreChannel : Event<GameScore>
{
    public void Fire(Int32.ctor)
    {
        this.Dispatch(new GameScore(Int32));
    }

    public void FireAt(Int32.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new GameScore( .ctor), __position);
    }
}

public class GameStartedChannel : Event<GameStarted>
{
    public void Fire()
    {
        this.Dispatch(new GameStarted());
    }

    public void FireAt(UnityEngine.Vector3 position)
    {
        this.Dispatch(new GameStarted(), position);
    }
}

public class PlayerDiedChannel : Event<PlayerDied>
{
    public void Fire()
    {
        this.Dispatch(new PlayerDied());
    }

    public void FireAt(UnityEngine.Vector3 position)
    {
        this.Dispatch(new PlayerDied(), position);
    }
}

public class PlayerHealthChannel : Event<PlayerHealth>
{
    public void Fire(Int32.ctor)
    {
        this.Dispatch(new PlayerHealth(Int32));
    }

    public void FireAt(Int32.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new PlayerHealth( .ctor), __position);
    }
}

public class PlayerHitChannel : Event<PlayerHit>
{
    public void Fire()
    {
        this.Dispatch(new PlayerHit());
    }

    public void FireAt(UnityEngine.Vector3 position)
    {
        this.Dispatch(new PlayerHit(), position);
    }
}

public class PlayerMoveInputChannel : Event<PlayerMoveInput>
{
    public void Fire(Vector3.ctor)
    {
        this.Dispatch(new PlayerMoveInput(Vector3));
    }

    public void FireAt(Vector3.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new PlayerMoveInput( .ctor), __position);
    }
}

public class PlayerSpawnedChannel : Event<PlayerSpawned>
{
    public void Fire(GameObject.ctor)
    {
        this.Dispatch(new PlayerSpawned(GameObject));
    }

    public void FireAt(GameObject.ctor, UnityEngine.Vector3 __position)
    {
        this.Dispatch(new PlayerSpawned( .ctor), __position);
    }
}

namespace MyProject.Events
{
    public static partial class Channel
    {
        public static BulletHitChannel BulletHit { get; private set; }

        public static BulletMissedChannel BulletMissed { get; private set; }

        public static BulletSpawnedChannel BulletSpawned { get; private set; }

        public static ChatMessageReceivedChannel ChatMessageReceived { get; private set; }

        public static GameScoreChannel GameScore { get; private set; }

        public static GameStartedChannel GameStarted { get; private set; }

        public static PlayerDiedChannel PlayerDied { get; private set; }

        public static PlayerHealthChannel PlayerHealth { get; private set; }

        public static PlayerHitChannel PlayerHit { get; private set; }

        public static PlayerMoveInputChannel PlayerMoveInput { get; private set; }

        public static PlayerSpawnedChannel PlayerSpawned { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            BulletHit = new BulletHitChannel();
            BulletMissed = new BulletMissedChannel();
            BulletSpawned = new BulletSpawnedChannel();
            ChatMessageReceived = new ChatMessageReceivedChannel();
            GameScore = new GameScoreChannel();
            GameStarted = new GameStartedChannel();
            PlayerDied = new PlayerDiedChannel();
            PlayerHealth = new PlayerHealthChannel();
            PlayerHit = new PlayerHitChannel();
            PlayerMoveInput = new PlayerMoveInputChannel();
            PlayerSpawned = new PlayerSpawnedChannel();
        }
    }
}

namespace MyProject.Events
{
    public static partial class Channel
    {
    }
}