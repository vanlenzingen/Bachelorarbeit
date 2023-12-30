using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;

public class AgentSkript : Agent {

    GameObject GameField;
    GameObject Controller;
    float reward = 0.0f;

    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode

        GameField = GameObject.FindWithTag("GameField");
        GameField.GetComponent<GameField>().NextRound();
        Controller = GameObject.FindWithTag("Controller");
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
        reward = 0.0f;

        int colorDiceAction = actionBuffers.DiscreteActions[0];
        string choosenColor;
        choosenColor = GetColorOfChoosenDice(colorDiceAction);

        int numberDiceAction = actionBuffers.DiscreteActions[1];
        int choosenNumber;
        choosenNumber = GetNumberOfChoosenDice(numberDiceAction);

        List<int> squareIndices = new List<int>();
        squareIndices = GetSquareIndicesFromActionBuffer(actionBuffers);


        if (MoveLegal(squareIndices, choosenColor, choosenNumber)){
            //TODO remove jokers etc;
            CrossSquareFields(squareIndices);
            Controller.GetComponent<Controller>().RerollDices();
            reward += GetCompletionRewards(squareIndices, choosenColor);

            reward += 1000.0f;
        } else {
            reward -= 100.0f;
        }

        AddReward(reward);
        Debug.Log(reward);
        EndEpisode();
    }


    private float GetCompletionRewards(List<int> squareIndices, string color){
        float reward = 0.0f;
        if (CheckForColumnCompletionReward(squareIndices)){
            reward += 5.0f;
        }
        if (CheckForColorCompletionReward(squareIndices, color)){
            reward += 5.0f;
        }
        return reward;
    }

    private bool CheckForColumnCompletionReward(List<int> squareIndices){
        //TODO
        return true;
    }

    private bool CheckForColorCompletionReward(List<int> squareIndices, string choosenColor){
        int colorCount = GameField.GetComponent<GameField>().GetColorCount(choosenColor);
        if (colorCount == 0){
            //gdetting Points
            GameField.GetComponent<GameField>().ColorFinished(choosenColor);
        }
        if (GameField.GetComponent<GameField>().colorsFinished == 2){
                //Getting Points before
                GameField.GetComponent<GameField>().NewGame();
            }
            return true;
        }



//TODO if joker = 0 dont pick a joker or a joker
    private bool MoveLegal(List<int> squareIndices, string choosenColor, int choosenNumber){
        if (AvailableCheck(squareIndices) && NotTheSameFieldsCheck(squareIndices)  && NumberCheck(squareIndices, choosenNumber) && ColorCheckFields(squareIndices, choosenColor)  && NeighborCheck(squareIndices)  && NotCrossedCheck(squareIndices)) {
            Debug.Log("Yes! This is fine!");
            return true;
        }
        Debug.Log("Move Not Legal!");
        return false;
    }


    private bool NotTheSameFieldsCheck(List<int> squareIndices){
        squareIndices.RemoveAll(item => item == 125);
        List<int> checkList = new List<int>();
        foreach(int index in squareIndices){
            if (checkList.Contains(index)){
                AddBonusReward(-1000.0f);
                return false;
            } else {
                checkList.Add(index);
            }
        }
        AddBonusReward(1000.0f);
        return true;
    }

    private bool NeighborCheck(List<int>squareIndices){
        List<Vector2> coordinates = new List<Vector2>();
        foreach (int index in squareIndices){
            Vector2 coordinate = IndexToCoordinate(index);
            coordinates.Add(coordinate);
        }
        for (int i = 0; i < coordinates.Count; i++) {
            for (int j = i + 1; j < coordinates.Count; j++) {
                if (!AreNeighbors(coordinates[i], coordinates[j])) {
                    return false;
                    }
                }
            }
        return true;
        }


    private bool AreNeighbors(Vector2 coordinate1, Vector2 coordinate2) {
        int x1 = (int)coordinate1.x;
        int y1 = (int)coordinate1.y;
        int x2 = (int)coordinate2.x;
        int y2 = (int)coordinate2.y;
        return Mathf.Abs(x1 - x2) <= 1 && Mathf.Abs(y1 - y2) <= 1;
    }


    private bool ColorCheckFields(List<int>squareIndices, string choosenColor){
        string oldColor = choosenColor;
        foreach(int index in squareIndices){
            Vector2 coordinate = IndexToCoordinate(index);
            GameObject squareField = GameField.GetComponent<GameField>().GetSquareField(coordinate);
            if (choosenColor == "joker"){
                if (oldColor != squareField.GetComponent<FieldSquare>().color){
                AddBonusReward(-1000.0f);
                return false;
            }
            oldColor = squareField.GetComponent<FieldSquare>().color;
            } else {
            if (choosenColor!= squareField.GetComponent<FieldSquare>().color){
                AddBonusReward(-1000.0f);
                return false;
                }
            }
        }
        AddBonusReward(10000.0f);
        return true;
    }

    private bool NumberCheck(List<int>squareIndices, int choosenNumber){
        squareIndices.RemoveAll(item => item == 125);
        if (choosenNumber == squareIndices.Count || choosenNumber == 6){
            AddBonusReward(10000.0f);
            return true;
        }
        AddBonusReward(-1000.0f);
        return false;
    }

    private bool AvailableCheck(List<int>squareIndices){
        foreach(int index in squareIndices){
            Vector2 coordinate = IndexToCoordinate(index);
            GameObject squareField = GameField.GetComponent<GameField>().GetSquareField(coordinate);
            if (squareField.GetComponent<FieldSquare>().available){
                AddBonusReward(10000.0f);
                return true;
            }
        }
        AddBonusReward(-1000.0f);
        return false;
    }

    private bool NotCrossedCheck(List<int>squareIndices){
        foreach(int index in squareIndices){
            Vector2 coordinate = IndexToCoordinate(index);
            GameObject squareField = GameField.GetComponent<GameField>().GetSquareField(coordinate);
            bool crossed = squareField.GetComponent<FieldSquare>().crossed;
            if (crossed){
                AddBonusReward(-1000.0f);
                return false;
                }
            }
        AddBonusReward(10000.0f);
        return true;
    }



    private void  CrossSquareFields(List<int> squareIndices){
        foreach (int squareFieldIndex in squareIndices){
            Vector2 koordinates = IndexToCoordinate(squareFieldIndex);
            GameField.GetComponent<GameField>().CrossField(koordinates);
        }
    }

    /// Rewards
    private float CheckForColorCompletionReward(string color){
        int colorCount;
        colorCount = GameField.GetComponent<GameField>().GetColorCount(color);
        if (colorCount == 0){
            GameField.GetComponent<GameField>().ColorFinished(color);
            Debug.Log("Colors Finished:" + GameField.GetComponent<GameField>().colorsFinished);
            if (GameField.GetComponent<GameField>().colorsFinished == 2){
                GameField.GetComponent<GameField>().NewGame();
                //Controller.CalculateColorPoints(color);
                return 3.0f;
            } else {
                //Controller.CalculateColorPoi.GetComponent<GameField>()nts(color);
            return 1.0f;
            }
        }
    return 0.0f;
    }



    
    private float CheckForStarFieldReward(bool starField){
        if (starField){
            return 2.0f;
        } 
        return 0.0f;
    }


    /// helper

    private Vector2 IndexToCoordinate(int index) {
        int x = index % 15;
        int y = index / 15;
        return new Vector2(x,y);
    }

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

        private List<int> GetSquareIndicesFromActionBuffer(ActionBuffers actionBuffers){
        List<int> squareIndices = new List<int>();
         for (int i = 2; i < actionBuffers.DiscreteActions.Length; i++) {
            int squareIndex = actionBuffers.DiscreteActions[i];
            if (squareIndex == 105) {  // normally fields should not picked doubled but 105 should be picked more often
            } else {
                squareIndices.Add(squareIndex);
            }
        }
        return squareIndices;
    }


    private void AddBonusReward(float reward){
        this.reward += reward;
    }
}

