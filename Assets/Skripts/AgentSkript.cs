using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class AgentSkript : Agent
{
    GameObject GameField;
    GameObject Controller;


    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode
        GameField = this.transform.parent.gameObject;
        Controller = GameObject.FindWithTag("Controller");

        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                Debug.Log("Get the sensor Filled' for Numberdice");
            }
            if (child.CompareTag("ColorDice")) {
                Debug.Log("Get the sensor Filled' for'Colordice'");
            }
        }
    }


    public override void CollectObservations(VectorSensor sensor)    {
     //getField
    sensor.AddObservation(gameObject.transform.rotation.z);
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
