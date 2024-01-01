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
        // get Observation for amount of jokers 1x11differnce
        sensor.AddObservation(GetOneHotFromInt(GameField.GetComponent<GameField>().joker,11));

        // get Observation for NumberDice 3x6
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                sensor.AddObservation(GetOneHotFromInt(child.GetComponent<NumberDice>().number-1,6)); // TODO overflow
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
        squareIndices = GetSquareIndicesFromActionBuffer(actionBuffers, choosenNumber);


        if (MoveLegal(squareIndices, choosenColor, choosenNumber)){
            ReduceJokerOfGameField(choosenColor, choosenNumber);
            CrossSquareFields(squareIndices);
            Controller.GetComponent<Controller>().RerollDices();
            reward += GetCompletionRewards(squareIndices, choosenColor);
            reward += 1000.0f;
        } else {
                reward -= 70.0f;
            }
        AddReward(reward);
        EndEpisode();
    }

    private void ReduceJokerOfGameField(string choosenColor, int choosenNumber){
        if (choosenNumber == 6){
            GameField.GetComponent<GameField>().ReduceJoker();
        }
        if (choosenColor == "joker"){
            GameField.GetComponent<GameField>().ReduceJoker();
        }

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



//TODO if no field choosen Skip Turn
    private bool MoveLegal(List<int> squareIndices, string choosenColor, int choosenNumber){
        int passedChecks = 0;
        if (AvailableCheck(squareIndices)){
            passedChecks++;
        }
        if (NotCrossedCheck(squareIndices)){
            passedChecks++;
        }

        if (ColorCheckFields(squareIndices, choosenColor)) {
             passedChecks++;
        }
        if (NeighborCheck(squareIndices)) {
            passedChecks++;
        }

        if (JokerPickedAndAvailable(choosenNumber, choosenColor)){
            passedChecks++;
        }
        if (passedChecks == 5) {
            Debug.Log("Yes! This is fine!");
            return true;
        }
        Debug.Log("Move Not Legal!(" + passedChecks + "/5)");
        return false;
    }


    private bool JokerPickedAndAvailable(int squareIndices, string choosenColor){
        int joker;
        joker = GameField.GetComponent<GameField>().joker;
        if (joker == 0 && (squareIndices == 6 || choosenColor == "joker")){
            AddBonusReward(-10.0f);
            Debug.Log("no remaining jokers");
            return false;
        }
        AddBonusReward(10.0f);
        return true;
    }

/*
    private bool NotTheSameFieldsCheck(List<int> squareIndices){
        squareIndices.RemoveAll(item => item == 125);
        List<int> checkList = new List<int>();
        foreach(int index in squareIndices){
            if (checkList.Contains(index)){
                Debug.Log("Picked one Field more than once");
                AddBonusReward(-10.0f);
                return false;
            } else {
                checkList.Add(index);
            }
        }
        AddBonusReward(10.0f);
        return true;
    }

        private bool NumberCheck(List<int>squareIndices, int choosenNumber){ // less difference should get a bit better reward
        squareIndices.RemoveAll(item => item == 125);
        if (choosenNumber == squareIndices.Count || choosenNumber == 6){
            AddBonusReward(10.0f);
            return true;
            }
        int difference = squareIndices.Count - choosenNumber;
        AddBonusReward(-10.0f-difference);
        Debug.Log("Number of Fields not matching ChoosenNumber");
        return false;
    }

    */

    private bool NeighborCheck(List<int>squareIndices){
        List<Vector2> coordinates = new List<Vector2>();
        if (squareIndices.Count > 1){
            foreach (int index in squareIndices){
                Vector2 coordinate = IndexToCoordinate(index);
                coordinates.Add(coordinate);
            }
            for (int i = 0; i < coordinates.Count; i++) {
                for (int j = i + 1; j < coordinates.Count; j++) {
                    if (!AreNeighbors(coordinates[i], coordinates[j])) {
                        AddBonusReward(-20.0f);
                        Debug.Log("Fields are not neighbors");
                        return false;
                        }
                    }
                }
        }
        AddBonusReward(20.0f);
        return true;
        }


    private bool AreNeighbors(Vector2 coordinate1, Vector2 coordinate2) {
        int x1 = (int)coordinate1.x;
        int y1 = (int)coordinate1.y;
        int x2 = (int)coordinate2.x;
        int y2 = (int)coordinate2.y;
        return Mathf.Abs(x1 - x2) <= 1 && Mathf.Abs(y1 - y2) <= 1;
    }


    private bool ColorCheckFields(List<int>squareIndices, string choosenColor){ //TODO The more colors together the better
        string oldColor = choosenColor;
        if (choosenColor == "joker"){
            GameObject squareField = GetGameFieldFromIndex(0);
            oldColor = squareField.GetComponent<FieldSquare>().color;
        }

        int difference = 0;
        foreach(int index in squareIndices){
           GameObject squareField = GetGameFieldFromIndex(index);
            if (choosenColor == "joker"){
                if (oldColor != squareField.GetComponent<FieldSquare>().color){
                    difference ++;
                }
            } else {
            if (choosenColor != squareField.GetComponent<FieldSquare>().color){
                difference++;
                }
            }
            oldColor = squareField.GetComponent<FieldSquare>().color;
        }
        if (difference == 0){
            AddBonusReward(30.0f);
            return true;
        }
        AddBonusReward(-5*difference);
        Debug.Log("Not matching colors");
        return false;
    }


    private GameObject GetGameFieldFromIndex(int index){
        Vector2 coordinate = IndexToCoordinate(index);
        GameObject squareField = GameField.GetComponent<GameField>().GetSquareField(coordinate);
        return squareField;
    }



    private bool AvailableCheck(List<int>squareIndices){
        foreach(int index in squareIndices){
            GameObject squareField = GetGameFieldFromIndex(index);
            if (squareField.GetComponent<FieldSquare>().available){
                AddBonusReward(10.0f);
                return true;
                }
            }
            AddBonusReward(-10.0f);
            Debug.Log("Fields not Available");
            return false;
        }

    private bool NotCrossedCheck(List<int>squareIndices){
        foreach(int index in squareIndices){
            GameObject squareField = GetGameFieldFromIndex(index);
            bool crossed = squareField.GetComponent<FieldSquare>().crossed;
            if (crossed){
                AddBonusReward(-10.0f);
                Debug.Log("Picked Crossed Fields");
                return false;
                }
            }
            AddBonusReward(10.0f);
            return true;
        }



    private void  CrossSquareFields(List<int> squareIndices){
        List<int> fieldsToCross = new List<int>();
        foreach (int squareFieldIndex in squareIndices){
            if (!fieldsToCross.Contains(squareFieldIndex)){
                fieldsToCross.Add(squareFieldIndex);
            }
        }
        foreach (int squareFieldIndex in fieldsToCross){
            GameObject squareField = GetGameFieldFromIndex(squareFieldIndex);
            Debug.Log("FieldIndex" + + squareFieldIndex + "crossed out!");
            Debug.Log("Available:" + squareField.GetComponent<FieldSquare>().available);
            Debug.Log("Crossed:" + squareField.GetComponent<FieldSquare>().crossed);
            Debug.Log("Color:" + squareField.GetComponent<FieldSquare>().color);
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
                //Controller.CalculateColorPoi.GetComponent<GameField>()(color);
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
        //Debug.Log("Number:" + number + " numberOfCases:"+ numberOfCases + "Array: "+ oneHotArray[0] +","+ oneHotArray[1] +","+ oneHotArray[2] +","+ oneHotArray[3] +","+ oneHotArray[4] +","+ oneHotArray[5]);
        return oneHotArray;
    }

        private List<int> GetSquareIndicesFromActionBuffer(ActionBuffers actionBuffers, int choosenNumber){
            //Debug.Log(actionBuffers.DiscreteActions[0] + " "+ actionBuffers.DiscreteActions[1] + " " + actionBuffers.DiscreteActions[2] + " "+ actionBuffers.DiscreteActions[3] + " "+actionBuffers.DiscreteActions[4] + " "+ actionBuffers.DiscreteActions[5] + " "+actionBuffers.DiscreteActions[6]);
        List<int> squareIndices = new List<int>();
         for (int i = 0; i < choosenNumber; i++) {
            if (i != 6) {
                int squareIndex = actionBuffers.DiscreteActions[i+1];
                    squareIndices.Add(squareIndex);
                }
            }
            return squareIndices;
        }


    private void AddBonusReward(float reward){
        this.reward += reward;
    }
}

