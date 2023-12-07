using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentSkript : Agent
{
    GameObject GameField;
    GameObject Controller;

//GRIDSENSOR?


    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode
        GameField = this.transform.parent.gameObject;
        Controller = GameObject.FindWithTag("Controller");
    }


    public override void CollectObservations(VectorSensor sensor)    {

        //get Observation of NumberDices - number

        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                sensor.AddObservation(GetNumberOneHotFromNumber(child.GetComponent<NumberDice>().number));
                //Debug.Log(child.GetComponent<NumberDice>().number);        //this should be every time the number of dice which is getting
            }
        }

        //get Observation for ColorDice - colorindex
         foreach (Transform child in Controller.transform) {
         if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
                //Debug.Log(child.GetComponent<ColorDice>().color);
            }
         }

        //getObservation of Fields colorindex, available, star, crossed
        //this gamefields should maybe get sorted into a gridsensor
        foreach (Transform child in GameField.transform) {
            if (child.CompareTag("Square")) {
                //Debug.Log(child.GetComponent<FieldSquare>());
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<FieldSquare>().color));
                sensor.AddObservation(child.GetComponent<FieldSquare>().available);
                sensor.AddObservation(child.GetComponent<FieldSquare>().starField);
                sensor.AddObservation(child.GetComponent<FieldSquare>().crossed);
            }
        }
    }

    private float[] GetColorIndexOneHotFromColor(string color){
         float[] colorOneHot = new float[5]; //change for joker
        switch (color)    {
            case "blue":
                 colorOneHot[0] = 1;
                break;
            case "green":
                 colorOneHot[1] = 1;
                break;
            case "yellow":
                 colorOneHot[2] = 1;
                break;
            case "orange":
                 colorOneHot[3] = 1;
                break;
            case "red":
                 colorOneHot[4] = 1;
                break;
            /*case "joker":
                 colorOneHot[5] = 1;
                break;
                */
        }
        return colorOneHot;
    }

    private float[] GetNumberOneHotFromNumber(int number){
        float[] numberOneHot = new float[5]; //change for joker
        switch (number)    {
            case 1:
                 numberOneHot[0] = 1;
                break;
            case 2:
                 numberOneHot[1] = 1;
                break;
            case 3:
                 numberOneHot[2] = 1;
                break;
            case 4:
                 numberOneHot[3] = 1;
                break;
            case 5:
                 numberOneHot[4] = 1;
                break;
            /*case 6:
                 numberOneHot[5] = 1;
                break;
                */
        }
        return numberOneHot;
    }



//actionbuffers should be liek [diceindex, numberindex, field, field, field, field, field]
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        float reward = 0.0f;
        int colorDiceAction = actionBuffers.DiscreteActions[0];
        int numberDiceAction = actionBuffers.DiscreteActions[1];

        string choosenColor;
        choosenColor = GetColorOfChoosenDice(numberDiceAction);

        int choosenNumber;
        choosenNumber = GetNumberOfChoosenDice(numberDiceAction);


        reward += chooseColorDice(colorDiceAction);
        reward += chooseNumberDice(numberDiceAction);


        int[] squareIndices = new int[3];
        for (int i = 2; i < actionBuffers.DiscreteActions.Length; i++) {
            int squareIndex = actionBuffers.DiscreteActions[i];
            squareIndices[i-2]=i;
            if (squareIndex == 0) {
                // Skip index 0 (assuming it means no action for square)
                continue;
            } else {
                int x = squareIndex % 15;
                int y = squareIndex / 15;

                reward += CrossSquareField(x, y, GetColorOfChoosenDice(colorDiceAction)); //crossField (x, y) -> should be implemented in gameField
            }
        }
        reward += CheckNumberReward(choosenNumber, squareIndices);




    }

    private float  chooseColorDice(int colorDiceIndex){
     return 0.0f;
    }

    private float  chooseNumberDice(int numberDiceIndex){
    return 0.0f;

    }

    private float  CrossSquareField(int x, int y, string chosenColor){
        float reward = 0.0f;
        GameObject squareFieldGameObject = GameField.GetComponent<GameField>().GetSquareField(x,y);
        FieldSquare squareField = squareFieldGameObject.GetComponent<FieldSquare>();
        string fieldColor = squareField.color;

        reward += CheckForColorReward(fieldColor , chosenColor);

        /*
         * if available ++ crossfield
         * else --
         *

         * if not available --
         * else ++

        if color full ++

        if column full ++
            */
        return 0.0f;
    }

    /// Check For Rewards
    private float CheckForColorReward(string fieldColor, string chosenColor){
        if (fieldColor == chosenColor) {
            return 1.0f;
        } else {
            return -1.0f;
        }
    }



    private float CheckNumberReward(int number, int[] squareIndex){
        if (squareIndex.Length > number){ //numberPicked should be taken
            return -0.3f;
        } else if (squareIndex.Length < number) {
            return -0.1f;
        } else {
            return 1.0f;
        }
    }


    /// helper


        private int GetNumberOfChoosenDice(int index){
        int[] diceArray = new int[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                diceArray[diceArray.Length] = child.GetComponent<NumberDice>().number;
            }
        }
        return diceArray[index];
    }

        private string GetColorOfChoosenDice(int index){
        string[] diceArray = new string[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("ColorDice")) {
                diceArray[diceArray.Length] = child.GetComponent<ColorDice>().color;
            }
        }
        return diceArray[index];
    }

}

