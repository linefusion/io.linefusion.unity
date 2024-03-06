using System.Collections;
using System.Collections.Generic;

using MyProject.Events;

using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public int health = 100;
    public int score = 0;

    [SerializeField]
    private KeyCode Left = KeyCode.A;

    [SerializeField]
    private KeyCode Right = KeyCode.D;

    public GameObject uiMenu;
    public GameObject uiPlaying;
    public GameObject uiEnd;

    public GameObject objects;

    public void Update()
    {
        var position = 0;

        position += Input.GetKey(Left) ? -1 : 0;
        position += Input.GetKey(Right) ? +1 : 0;

        if (position != 0)
        {
            Channel.PlayerMoveInput.Fire(new Vector3(position, 0, 0));
        }
    }

    private void OnEnable()
    {
        Channel.BulletMissed.Fired += OnBulletMissed;
        Channel.GameStarted.Fired += OnGameStarted;
        Channel.PlayerDied.Fired += OnPlayerDied;
        Channel.PlayerHit.Fired += OnPlayerHit;
    }

    private void OnDisable()
    {
        Channel.BulletMissed.Fired -= OnBulletMissed;
        Channel.GameStarted.Fired -= OnGameStarted;
        Channel.PlayerDied.Fired -= OnPlayerDied;
        Channel.PlayerHit.Fired -= OnPlayerHit;
    }

    private void OnGameStarted(Event<GameStarted>.Args ev)
    {
        objects?.SetActive(true);

        uiMenu.SetActive(false);
        uiPlaying.SetActive(true);
        uiEnd.SetActive(false);

        health = 100;
        Channel.PlayerHealth.Fire(health);

        score = 0;
        Channel.GameScore.Fire(score);
    }

    private void OnBulletMissed(Event<BulletMissed>.Args ev)
    {
        score += 100;
        Channel.GameScore.Fire(score);
    }

    private void OnPlayerHit(Event<PlayerHit>.Args ev)
    {
        this.health -= 10;
        if (this.health <= 0)
        {
            this.health = 0;
        }

        Channel.PlayerHealth.Fire(this.health);

        if (this.health <= 0)
        {
            Channel.PlayerDied.FireAt(transform.position);
        }
    }

    private void OnPlayerDied(Event<PlayerDied>.Args ev)
    {
        uiMenu.SetActive(false);
        uiPlaying.SetActive(false);
        uiEnd.SetActive(true);

        objects.SetActive(false);
    }
}
