using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackGame : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource musicJetpackGame;

    [Header("Previous Scene")]
    [SerializeField] private GameObject waitingRoom;

    // [Header("Next Scene")]
    // [SerializeField] private GameObject endGame;

    [Header("Visual Components")]
    [SerializeField] private NightSkyGenerator nightSky;
    [SerializeField] private SpriteRenderer commandCenterRenderer;



    private PlayerManager playerManager;

    private Vector3 commandCenterFinalOffset = new Vector3(-6, 0, 0);

    void Start()
    {

        
        StartCoroutine(InitialTransition());

        
    }


    IEnumerator InitialTransition(){


        StartCoroutine(CommandCenterTransition(4f));

        musicJetpackGame.Play();
        StartCoroutine(MusicUtils.FadeMusicVolume(musicJetpackGame, 0f, 1f, 2f));

        yield return new WaitForSeconds(4f);

        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.PlayersStartJetpackGame();
    }






    // Command Center Transition
    IEnumerator CommandCenterTransition(float duration)
    {
        float elapsed = 0;
        // Vector3 startScale = commandCenterRenderer.transform.localScale;
        Vector3 startPosition = commandCenterRenderer.transform.position;

        // Démarrer la translation du ciel
        // if (nightSky != null)
        // {
        //     nightSky.TranslateSky(skyFinalOffset, duration, finalTransitionCurve);
        // }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            // float curvedProgress = finalTransitionCurve.Evaluate(progress);

            // Déplacer et redimensionner le centre de commande
            // commandCenterRenderer.transform.localScale = Vector3.Lerp(
            //     startScale,
            //     commandCenterBaseScale * commandCenterFinalScale,
            //     curvedProgress
            // );

            commandCenterRenderer.transform.position = Vector3.Lerp(
                startPosition,
                startPosition + commandCenterFinalOffset,
                progress
            );

            yield return null;
        }

        Debug.Log("Final transition completed");
    }
}
