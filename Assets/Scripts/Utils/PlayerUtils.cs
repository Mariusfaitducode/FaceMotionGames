using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerUtils
{
    // Start is called before the first frame update
    public static IEnumerator PlayersSimpleTransition(GameObject[] players, Vector3 translation, AnimationCurve curve, float duration){
        float elapsed = 0;

        // StopAllPlayerAnimator();

        List<Vector3> playerInitialPositions = new List<Vector3>();

        foreach (var player in players)
        {
            playerInitialPositions.Add(player.transform.position);
            player.GetComponent<Player>().SetPlayerForTransition();
        }
        

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            int index = 0;
            foreach (var player in players)
            {
                player.transform.position = Vector3.Lerp(
                    playerInitialPositions[index],
                    playerInitialPositions[index] + translation,
                    curve.Evaluate(progress)
                );
                index++;
            }


            yield return null;
        }

        // foreach (var player in activePlayers)
        // {
        //     player.Value.GetComponent<Player>().ResetPlayerAfterTransition();
        // }
    }


    public static IEnumerator PlayersTransitionToPositions(GameObject[] players, List<Vector3> positions, AnimationCurve curve, float duration){

        float elapsed = 0;

        List<Vector3> playerInitialPositions = new List<Vector3>();

        foreach (var player in players)
        {
            playerInitialPositions.Add(player.transform.position);
            player.GetComponent<Player>().SetPlayerForTransition();
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            int index = 0;
            foreach (var player in players)
            {
                player.transform.position = Vector3.Lerp(
                    playerInitialPositions[index],
                    positions[index],
                    curve.Evaluate(progress)
                );
                index++;
            }
            yield return null;
        }
    }
}
