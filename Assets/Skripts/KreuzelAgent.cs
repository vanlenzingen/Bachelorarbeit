using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class KreuzelAgent : Agent
{

    GameObject GameField;
    GameObject Controller;

    private GameObject controller;
    private Controller ControllerScript;
    private GameField GameFieldSkript;

    private string logFilePath;

    void LogData(string data){
        File.AppendAllText(logFilePath, data + "\n");
    }



    public override void OnEpisodeBegin() {
        controller = transform.parent.gameObject;
        ControllerScript = controller.GetComponent<Controller>();
        ControllerScript.RerollDices();
        foreach (Transform childTransform in controller.transform){
            GameObject child = childTransform.gameObject;
        if (child.CompareTag("GameField")){
            Transform GameFieldTransform = childTransform;
            GameField = GameFieldTransform.gameObject;
            }
        }
        GameFieldSkript = GameField.GetComponent<GameField>();
        GameFieldSkript.NextRound();
        ControllerScript.RerollDices();
    }

    public override void CollectObservations(VectorSensor sensor)    {
        // get Observation for amount of jokers 1
        sensor.AddObservation(GameFieldSkript.joker);

        // get Observation for current Round 1
        sensor.AddObservation(GameFieldSkript.roundCount);

        // get Observation for NumberDice 2x1
        foreach (Transform child in controller.transform) {
            if (child.CompareTag("NumberDice")) {
                sensor.AddObservation(child.GetComponent<NumberDice>().number);
            }
        }

        // get Observation for ColorDice 2x6
        foreach (Transform child in controller.transform) {
            if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
            }
        }
        // get Observation for fields 105* (6+3)
        foreach (Transform squareField in GameField.transform) {
            if (squareField.CompareTag("Square")) {
                PushValidField(squareField, sensor);
            }
        }
    }

    public void Start(){
        new WaitForSeconds(3.0f);
        logFilePath = Application.dataPath + "/log_kreuzel_points_3.txt";
    }


    //  choosenColor, choosenNumber, jokerNumber, field field field field field
    public override void OnActionReceived(ActionBuffers actionBuffers) {
            int colorDiceAction = actionBuffers.DiscreteActions[0];
            string choosenColor; // could be a joker
            choosenColor = GetColorOfChoosenDice(colorDiceAction);

            int numberDiceAction = actionBuffers.DiscreteActions[1];
            int choosenNumber; // could be a joker
            choosenNumber = GetNumberOfChoosenDice(numberDiceAction);

            if (choosenNumber == 6) {
                choosenNumber = actionBuffers.DiscreteActions[2] +1 ;
            }

            GetColorJokerPen(choosenColor);
            GetNumberJokerPen(choosenNumber);

            List<GameObject> availableFields = GameFieldSkript.GetAvailableFieldsForGroupAndColor(choosenColor, choosenNumber);

            if (availableFields.Count ==  0 || choosenNumber == 0){
                AddReward(-50.0f);
                if (GameFieldSkript.roundCount == 30){
                GameFieldSkript.AddToPoints(GameFieldSkript.joker);
                //Debug.Log("Added: Joker Points:" + GameFieldSkript.joker);
                AddReward(GameFieldSkript.joker);
                LogData(GameFieldSkript.points.ToString());
                GameFieldSkript.NewGame();

                }
                EndEpisode();
                return;
            }
            List<GameObject> pickedFields = new List<GameObject>();
            for(int i=0;i<choosenNumber; i++){
                if (availableFields.Count != 0){
                    GameObject pickedField = PickField(actionBuffers.ContinuousActions[i], availableFields);
                    pickedFields.Add(pickedField);
                    availableFields = GameFieldSkript.CalculateNeighbours(pickedFields);
                }
            }
            CrossOutFields(pickedFields);
            GameFieldSkript.UpdateGroups(pickedFields);
            CheckForRewards(pickedFields);
            if (GameFieldSkript.roundCount == 30){
                GameFieldSkript.AddToPoints(GameFieldSkript.joker);
                AddReward(GameFieldSkript.joker);
                LogData(GameFieldSkript.points.ToString());
                GameFieldSkript.NewGame();


            }
        EndEpisode();
    }


    private void CheckForRewards(List<GameObject> pickedFields){
        CheckForStarFields(pickedFields);
        CheckForColumnFinished(pickedFields);
        CheckForColorFinished(pickedFields);
    }


    private void CheckForStarFields(List<GameObject> pickedFields){
        foreach(GameObject field in pickedFields){
            if(field.GetComponent<FieldSquare>().starField){
                AddReward(2.0f);
                //Debug.Log("Added starField Points:" + 2);
                GameFieldSkript.AddToPoints(2);
            }
        }
    }

    private void CheckForColumnFinished(List<GameObject> pickedFields){
        List<int> columnList = GetColumnList(pickedFields);
        foreach (int column in columnList){
            int remainingFields = GameFieldSkript.CheckNumberOfRemainingFields(column);
            if (remainingFields == 0){
               float points = ControllerScript.GetColumnPoints(column);
               GameFieldSkript.AddToPoints((int)points);
               //Debug.Log("Added: COlumn FInished Points:" + points);
               AddReward(points);
            }
        }
    }

    private List<int> GetColumnList(List<GameObject> pickedFields){
         List<int> columnsToCheck = new List<int>();
        foreach (GameObject field in pickedFields){
            int column = field.GetComponent<FieldSquare>().getY();
            if (!columnsToCheck.Contains(column)){
                columnsToCheck.Add(column);
            }
        }
        return columnsToCheck;
    }

    private void CheckForColorFinished(List<GameObject> pickedFields){
        string refColor = pickedFields[0].GetComponent<FieldSquare>().color;
        if (GameFieldSkript.GetColorCount(refColor) == 0){
            GameFieldSkript.ColorFinished();
            float points = ControllerScript.GetColorPoints(refColor);
            //Debug.Log("Added Color Points:" + points);
            GameFieldSkript.AddToPoints((int)points);
            AddReward(points);
        }
    }

    private void CrossOutFields(List<GameObject> pickedFields){
        int numberOfFields = pickedFields.Count;
        float rewardMult = 0.02f;
        GameObject field = pickedFields[0];
        if (field.GetComponent<FieldSquare>().group == numberOfFields){
            rewardMult = 0.04f;
        }
        AddReward(rewardMult*numberOfFields);
       // Debug.Log("pickedFields" + pickedFields.Count);
        GameFieldSkript.CrossFields(pickedFields);
    }


    private GameObject PickField(float action, List<GameObject> AvailableFields) {
        action = Mathf.Clamp(action, 0f, 1f);

        // Subtract a small epsilon from 1.0 to avoid reaching the edge of the array
        float adjustedAction = Mathf.Clamp01(action - 0.0001f);

        int index = Mathf.FloorToInt(adjustedAction * AvailableFields.Count);

        // Ensure that the index is within a valid range
        index = Mathf.Clamp(index, 0, AvailableFields.Count - 1);

        return AvailableFields[index];
    }



    private void PushValidField(Transform squareField, VectorSensor sensor) {
        sensor.AddObservation(GetColorIndexOneHotFromColor(squareField.GetComponent<FieldSquare>().color));
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().available);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().starField);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().crossed);
    }


    private float[] GetColorIndexOneHotFromColor(string color){
        float[] colorOneHot = new float[6];
        switch (color)    {
            case "blue":    colorOneHot[0] = 1;     break;
            case "green":   colorOneHot[1] = 1;     break;
            case "yellow":  colorOneHot[2] = 1;     break;
            case "orange":  colorOneHot[3] = 1;     break;
            case "red":     colorOneHot[4] = 1;     break;
            case "joker":   colorOneHot[5] = 1;     break;
            case "":        break;
            }
        return colorOneHot;
    }

    private string GetColorFromActionBuffer(int number){
        switch (number) {
            case 1:     return  "blue";
            case 2:     return  "green";
            case 3:     return  "yellow";
            case 4:     return  "orange";
            case 5:     return  "red";
            default:    return  "";
        }
    }


    private string GetColorOfChoosenDice(int index){
        string[] diceArray = new string[2];
        int currentIndex = 0;
        foreach (Transform child in controller.transform) {
            if (child.CompareTag("ColorDice")) {
                diceArray[currentIndex] = child.GetComponent<ColorDice>().color;
                currentIndex ++;
            }
        }
        return diceArray[index];
    }

    private int GetNumberOfChoosenDice(int index){
        int[] diceArray = new int[2];
        int currentIndex = 0;
        foreach (Transform child in controller.transform) {
            if (child.CompareTag("NumberDice")) {
                diceArray[currentIndex] = child.GetComponent<NumberDice>().number;
                currentIndex ++;
            }
        }
        return diceArray[index];
    }


        private void GetJokerPenalty() {
        if (GameFieldSkript.joker > 1) {
            GameFieldSkript.ReduceJoker();
        } else {
            AddReward(-50.0f);
        }
    }

    private void GetColorJokerPen(string choosenColor){
        if (choosenColor == "joker"){
            GetJokerPenalty();
        }
    }

    private void GetNumberJokerPen(int choosenNumber){
         if (choosenNumber == 6){
            GetJokerPenalty();
        }
    }
}
