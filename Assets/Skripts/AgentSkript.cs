using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;

public class AgentSkript : Agent {

    GameObject GameField;
    GameObject Controller;

    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode

        GameField = GameObject.FindWithTag("GameField");
        GameField.GetComponent<GameField>().NextRound();
        Controller = GameObject.FindWithTag("Controller");
        Controller.GetComponent<Controller>().RerollDices();
       // Debug.Log("Agents new episode");
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
    }


    //actionbuffers should be like [diceindex, numberindex, field, field, field, field, field]
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        float reward = 0.0f;

        int colorDiceAction = actionBuffers.DiscreteActions[0];
        int numberDiceAction = actionBuffers.DiscreteActions[1];

        string choosenColor;
        choosenColor = GetColorOfChoosenDice(numberDiceAction);

        int choosenNumber;
        choosenNumber = GetNumberOfChoosenDice(numberDiceAction);



        reward += GetColorDiceReward(colorDiceAction);
        reward += GetNumberDiceReward(numberDiceAction);

        List<int> squareIndices = new List<int>();
        List<Vector2> fieldKoordinates = new List<Vector2>();
        for (int i = 2; i < actionBuffers.DiscreteActions.Length; i++) {
            int squareIndex = actionBuffers.DiscreteActions[i];
            if (squareIndex == 105) {  // normally fields should not picked doubled but 105 should be picked more often
                Debug.Log("No Field picked");
            } else {
                squareIndices.Add(squareIndex);
                fieldKoordinates.Add(new Vector2(squareIndex % 15, squareIndex / 15));
                reward += CrossSquareField(
                    squareIndex % 15,
                    squareIndex / 15,
                    GetColorOfChoosenDice(colorDiceAction)
                ); 
                CheckForNeighborReward(fieldKoordinates);
            }
        }
         //Debug.Log("number of Picked Fields: " + squareIndices.Count);
        reward += CheckNumberReward(choosenNumber, squareIndices);
        AddReward(reward);
        //Debug.Log("Reward: " + reward);
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



        if (squareField.getAvailable()){ // TODO will this work with multiple fields?
            Debug.Log(squareField.getAvailable());
            Debug.Log(x +" "+ y + "should be available");
            reward += CheckForColorReward(squareField.color , chosenColor);
            reward += CheckForAvailableReward(squareField.available);
            reward += CheckForCrossedReward(squareField.crossed);
            reward += CheckForStarFieldReward(squareField.starField);
            reward += CheckForColorAvailableReward(squareField.color);
            squareField.CrossField();
            int remainingFields = GameField.GetComponent<GameField>().CheckNumberOfRemainingFields(squareField.xPos);
            if (remainingFields == 0) {
                GameField.GetComponent<GameField>().GetColumnReward(squareField.xPos);
                reward += 3.0f; //reward += Controller.CalculateColumnReward(squareField.x) // TODO implement in Controller
            } else {
                reward -= 1000.0f;
            }
        }
        reward += CheckForColorCompletionReward(squareField.color);
        return reward;
    }


    /// Rewards
    private float CheckForColorCompletionReward(string color){
        int colorCount;
        colorCount = GameField.GetComponent<GameField>().GetColorCount(color);
        if (colorCount == 0){
            GameField.GetComponent<GameField>().ColorFinished();
            Debug.Log("Colors Finished:" + GameField.GetComponent<GameField>().colorsFinished);
            if (GameField.GetComponent<GameField>().colorsFinished == 2){
                GameField.GetComponent<GameField>().NewGame();
                //Controller.CalculateColorPoints(color);
                return 3.0f;
            } else {
                //Controller.CalculateColorPoints(color);
            return 1.0f;
            }
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
                    //Debug.Log("Choosen Fields arent Neighbors");
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
            //Debug.Log("Tried to cross out crossed Field");
            return -10.0f;
        }
        return 1.0f;
    }

    private float CheckForColorReward(string fieldColor, string chosenColor){
        if (fieldColor == chosenColor || chosenColor == "joker") {
            return 1.0f;
        } else {
          //  Debug.Log("Not the Same Color Penalty");
            return -10.0f;
        }
    }

    private float CheckForAvailableReward(bool available) {
        if (available){
            return 100.0f;
        }
        //Debug.Log("Penalty aplied -not available Field");
        return -1000.0f;
    }


    private float CheckNumberReward(int number, List<int> squareIndex){
        if (squareIndex.Count == number || number == 6 ){
            return 100.0f;
        }
        //Debug.Log("Number Of Fields("+squareIndex.Count + ") not matching picked number("+number+")");
        return -1000.0f;
    }


    private float CheckForColorAvailableReward(string color){
        //TODO
        // getColorCountOfGameField
        // if colorCOunt = 0
        //penalty
        // sonst gut
        return 0.0f;
    }

    /// helper

    private int GetNumberOfChoosenDice(int index){
        int[] diceArray = new int[3];
        int currentIndex = 0;
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                diceArray[currentIndex] = child.GetComponent<NumberDice>().number;
                currentIndex ++;
            }
        }
        if (diceArray[index] == 6) {
            GameField.GetComponent<GameField>().ReduceJoker();
        }
        return diceArray[index];
    }

    private string GetColorOfChoosenDice(int index){
        string[] diceArray = new string[3];
        int currentIndex = 0;
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("ColorDice")) {
                diceArray[currentIndex] = child.GetComponent<ColorDice>().color;
                currentIndex ++;
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

