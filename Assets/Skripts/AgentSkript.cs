using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class AgentSkript : Agent {

    GameObject GameField;
    GameObject Controller;

    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode

        GameField = this.transform.parent.gameObject;
        Controller = GameObject.FindWithTag("Controller");
        Controller.GetComponent<Controller>().RerollDices();
        Debug.Log("Agents new episode");
    }

    public override void CollectObservations(VectorSensor sensor)    {
        // get Observation for amount of jokers 1x11
        sensor.AddObservation(GetOneHotFromInt(GameField.GetComponent<GameField>().joker,11));

        // get Observation for NumberDice 3x6
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                sensor.AddObservation(GetOneHotFromInt(child.GetComponent<NumberDice>().number-1,6));
            }
        }

        // get Observation for ColorDice 3x6
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
            }
        }

        // get Observation for fields 105* (6+3)
        foreach (Transform child in GameField.transform) {
            if (child.CompareTag("Square")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<FieldSquare>().color));
                sensor.AddObservation(child.GetComponent<FieldSquare>().available);
                sensor.AddObservation(child.GetComponent<FieldSquare>().starField);
                sensor.AddObservation(child.GetComponent<FieldSquare>().crossed);
            }
        }
        Debug.Log("Observations Collected");
    }

    //actionbuffers should be like [diceindex, numberindex, field, field, field, field, field]
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        float reward = 0.0f;
        //validateRewards
        /*
        if (actionBuffers.DiscreteActions[0] < 0 && actionBuffers.DiscreteActions[0] > 2){
        reward += -2.0f;
        }
        if (actionBuffers.DiscreteActions[1] < 0 && actionBuffers.DiscreteActions[1] > 2){
            reward += -2.0f;
        }
        for (int i=2; i<=6;i++) {
            for (int j = 2; j<=6;j++){
                if (actionBuffers.DiscreteActions[i] == actionBuffers.DiscreteActions[j] && i != j)
                    reward += -2.0f;
                }
            if  (actionBuffers.DiscreteActions[i] < -1 || actionBuffers.DiscreteActions[i] > 104) {
               reward += -2.0f;
            }
        }
        */
        int colorDiceAction = actionBuffers.DiscreteActions[0];
        int numberDiceAction = actionBuffers.DiscreteActions[1];

        string choosenColor;
        choosenColor = GetColorOfChoosenDice(numberDiceAction);

        int choosenNumber;
        choosenNumber = GetNumberOfChoosenDice(numberDiceAction);

        reward += GetColorDiceReward(colorDiceAction);
        reward += GetNumberDiceReward(numberDiceAction);

        int[] squareIndices = new int[5]; 
        List<Vector2> fieldKoordinates = new List<Vector2>();
        for (int i = 2; i < actionBuffers.DiscreteActions.Length; i++) {
            int squareIndex = actionBuffers.DiscreteActions[i];
            Debug.Log(squareIndex);
            Debug.Log(squareIndex % 15);
            Debug.Log(squareIndex / 15);
            squareIndices[i-2]=i;
            if (squareIndex == 105) {
                continue;
            } else {
                fieldKoordinates.Add(new Vector2(squareIndex % 15, squareIndex / 15));
                // but why he did not learn
                // when i should start for the next round @fail? || @ fertig???
                reward += CrossSquareField(
                    squareIndex % 15,
                    squareIndex / 15,
                    GetColorOfChoosenDice(colorDiceAction)
                ); 
                GameField.GetComponent<GameField>().CrossField(squareIndex % 15, squareIndex / 15);
                CheckForNeighborReward(fieldKoordinates);
            }
        }
        reward += CheckNumberReward(choosenNumber, squareIndices);
        AddReward(reward);
        Debug.Log("Reward: " + reward);
        EndEpisode();
    }


    private float  GetColorDiceReward(int colorDiceIndex){
        if (GetColorOfChoosenDice(colorDiceIndex) == "joker" && GameField.GetComponent<GameField>().joker == 0){
            return -1.0f;
        }
    return 0.0f;
    }


    private float  GetNumberDiceReward(int numberDiceIndex){
        if (GetNumberOfChoosenDice(numberDiceIndex)==6 && GameField.GetComponent<GameField>().joker == 0){
            return -1.0f;
        }
    return 0.0f;
    }

    private float  CrossSquareField(int x, int y, string chosenColor){
        float reward = 0.0f;
        GameObject squareFieldGameObject = GameField.GetComponent<GameField>().GetSquareField(x,y);
        FieldSquare squareField = squareFieldGameObject.GetComponent<FieldSquare>();
        squareField.CrossField();

        reward += CheckForColorReward(squareField.color , chosenColor);
        reward += CheckForAvailableReward(squareField.available);
        reward += CheckForCrossedReward(squareField.crossed);
        reward += CheckForStarFieldReward(squareField.starField);

        squareField.crossField();
        int remainingFields = GameField.GetComponent<GameField>().CheckNumberOfRemainingFields(squareField.xPos);
        if (remainingFields == 0) {
            reward += 3.0f; //reward += Controller.CalculateColumnReward(squareField.x) // TODO implement in Controller
        }         
        reward += CheckForColorCompletionReward(squareField.color);
        return reward;
    }


    /// Rewards
    private float CheckForColorCompletionReward(string color){
        int colorCount;
        colorCount = GameField.GetComponent<GameField>().GetColorCount(color);
        if (colorCount == 0){
            Controller.GetComponent<Controller>().NewGame();
            return 3.0f; // check reward depending on First or second //TODO Controller.CalculateColorReward()
        }
        return 0.0f;
    }

    
    private float CheckForNeighborReward(List<Vector2> koordinates){
        foreach (Vector2 koord in koordinates){
            int x1 = (int)koord.x;
            int y1 = (int)koord.y;
            foreach (Vector2 field2 in koordinates){
                int x2 = (int)field2.x;
                int y2 = (int)field2.y;
                if (x1 == x2+1 && y1 == y2 || x1 == x2-1 && y1 == y2 ||  y1 == y2-1 && x1 == x2||y1 == y2-1 && x1 == x2 || x1==x2 && y1==y2){
                    continue;
                    } else {
                    return -50.0f;
                }
            }
        }
    return 1.0f;
    }
    

    
    private float CheckForStarFieldReward(bool starField){
        if (starField){
            return 2.0f;
        } 
        return 0.0f;
    }
    
    
    private float CheckForCrossedReward(bool crossed){
        if (crossed){
            return -10.0f;
        }
        return 1.0f;
    }

    private float CheckForColorReward(string fieldColor, string chosenColor){
        if (fieldColor == chosenColor || chosenColor == "joker") {
            return 1.0f;
        } else {
            return -10.0f;
        }
    }

    private float CheckForAvailableReward(bool available) {
        if (available){
            return 1.0f;
        }
        return -10.0f;
    }


    private float CheckNumberReward(int number, int[] squareIndex){
        if (squareIndex.Length == number ){
            return 5.0f;
        }
        return -10.0f;
    }

    /// helper

    private int GetNumberOfChoosenDice(int index){
        int[] diceArray = new int[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                diceArray[diceArray.Length-1] = child.GetComponent<NumberDice>().number;
            }
        }
        if (diceArray[index] == 6) {
            GameField.GetComponent<GameField>().ReduceJoker();
        }
        return diceArray[index];
    }

    private string GetColorOfChoosenDice(int index){
        string[] diceArray = new string[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("ColorDice")) {
                diceArray[diceArray.Length-1] = child.GetComponent<ColorDice>().color;
            }
        }
        if (diceArray[index] == "joker") {
            GameField.GetComponent<GameField>().ReduceJoker();
            }
        return diceArray[index];
    }


    private float[] GetColorIndexOneHotFromColor(string color){
        float[] colorOneHot = new float[6];
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
            case "joker":
                 colorOneHot[5] = 1;
                break;
            }
        return colorOneHot;
    }


    private float[] GetOneHotFromInt(int number, int numberOfCases){
        float[] oneHotArray = new float[numberOfCases];
        oneHotArray[number] = 1;
        return oneHotArray;
    }
}

