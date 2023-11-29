using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class DiceGameAgent : Agent
{
    public override void OnEpisodeBegin()
    {
        // Reset the environment for a new episode
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Getting Stuff");

     // Collect observations from the environment (e.g., current state of the dice)
     //getField
     //getDice

    }
    /*
    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.Log("Do Something");
        // Take actions based on the received vectorAction
        // Apply rewards and penalties
        // Check if the episode is done
    }
    */
}
