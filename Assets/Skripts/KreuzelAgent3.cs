using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class KreuzelAgent3 : Agent
{

    GameObject GameField;
    GameObject Controller;

    private GameObject controller;
    private Controller ControllerScript;
    private GameField GameFieldSkript;

    private string logFilePath;
    private string rewardLogPath;
    private float rewardSum;

    void LogPoints(string data){
        if (controller.name == "Controller" || controller.name == "Controller_1" || controller.name == "Controller_2"){
            File.AppendAllText(logFilePath, data + "\n");
        }
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

    public float GetJokersObservation(){
        return Normalize(GameFieldSkript.joker, 0, 10);
    }

    public float GetRoundCountObservation(){
        return Normalize(GameFieldSkript.roundCount, 1 , 600);
    }

    public float GetNumberDiceObservation(GameObject child){
        return Normalize(child.GetComponent<NumberDice>().number, 1, 6);
    }

    public float GetGroupObservation(GameObject squareField){
        return Normalize(squareField.GetComponent<FieldSquare>().group, 1, 6);
    }

    public Vector2 GetCoordinateObservation(GameObject squareField){
        float x = Normalize(squareField.GetComponent<FieldSquare>().xPos, 0, GameFieldSkript.Columns-1);
        float y = Normalize(squareField.GetComponent<FieldSquare>().yPos, 0, GameFieldSkript.Rows-1);
        return new Vector2(x,y);
    }

    public float Normalize(int current, int min, int max){
        return ((current-min)/(max-min));
    }



    public override void CollectObservations(VectorSensor sensor)    {
        // get Observation for amount of jokers 1
        sensor.AddObservation(GetJokersObservation());

        // get Observation for current Round 1
        sensor.AddObservation(GetRoundCountObservation());

        // get Observation for NumberDice 2x1
        foreach (Transform childTransform in controller.transform) {
            if (childTransform.CompareTag("NumberDice")) {
                GameObject child = childTransform.gameObject;
                sensor.AddObservation(GetNumberDiceObservation(child));
            }
        }

        // get Observation for ColorDice 2x6
        foreach (Transform child in controller.transform) {
            if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
            }
        }
        // get Observation for fields 105* (6+2+4)
        int counter = 0;
        foreach (Transform squareField in GameField.transform) {
            if (squareField.CompareTag("Square")) {
                PushValidField(squareField, sensor);
                counter += 1;
            }
        }
        do {
            if (counter < 105){
            PushEmptyField(sensor);
            counter += 1;
            }
        }
            while (counter < 105);
    }

    public void Start(){
        new WaitForSeconds(3.0f);

        logFilePath = Application.dataPath + "/LogPoints_coordinate_picker.txt";

    }


    //  choosenColor, choosenNumber, jokerNumber, choosenFieldX, choosenFieldY, neighbor, neighbor, neighbor, neighbor
    public override void OnActionReceived(ActionBuffers actionBuffers) {
            if (GameFieldSkript.points >= 10){
                //GameFieldSkript.AddToPoints(GameFieldSkript.joker);
                //Debug.Log("Added: Joker Points:" + GameFieldSkript.joker);
                //AddReward(GameFieldSkript.joker);
                //rewardSum += GameFieldSkript.joker;
                //LogPoints(GameFieldSkript.roundCount.ToString());
                //Debug.Log(rewardSum-30);
                rewardSum = 0;
//                 LogRewards(rewardSum.ToString());
                GameFieldSkript.NewGame();
                }


            int colorDiceAction = actionBuffers.DiscreteActions[0];
            string choosenColor; // could be a joker
            choosenColor = GetColorOfChoosenDice(colorDiceAction);

            int numberDiceAction = actionBuffers.DiscreteActions[1];
            int choosenNumber; // could be a joker
            choosenNumber = GetNumberOfChoosenDice(numberDiceAction);

            if (choosenNumber == 6) {
                choosenNumber = actionBuffers.DiscreteActions[2] +1 ;
            }

//             GetColorJokerPen(choosenColor);
//             GetNumberJokerPen(choosenNumber);

             Vector2 coordinates = new Vector2(actionBuffers.DiscreteActions[3], actionBuffers.DiscreteActions[4]);
             if (!GameFieldSkript.CoordinatesInBond(coordinates)) {
//                  Debug.Log("Out Of Bounds");
                 EndEpisode();
                 return;
             }
             GameObject initialField = GameFieldSkript.GetSquareField(coordinates);
             if (initialField.GetComponent<FieldSquare>().crossed){
//                  Debug.Log("Field Already Crossed");
                 //AddReward(-0.2f);

             }
            List<GameObject> availableFields = GameFieldSkript.GetAvailableFieldsForGroupAndColor(choosenColor, choosenNumber);
            if (!availableFields.Contains(initialField) || initialField.GetComponent<FieldSquare>().available == false){
                 Debug.Log("Not Available");
                AddReward(-0.3f);
                EndEpisode();
            } else {
                 Debug.Log("Available");
                AddReward(0.1f);
            List<GameObject> pickedFields = new List<GameObject>();
            pickedFields.Add(initialField);
            for(int i=0;i<choosenNumber-1; i++){
                availableFields = GameFieldSkript.CalculateNeighbours(pickedFields);
                if (availableFields.Count != 0){
                GameObject pickedField = PickField(actionBuffers.ContinuousActions[i], availableFields);
                pickedFields.Add(pickedField);
                }
            }
        CrossOutFields(pickedFields);
        GameFieldSkript.UpdateGroups(pickedFields);
        CheckForRewards(pickedFields);
        EndEpisode();
        }
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
                //rewardSum += 2.0f;
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
               AddReward(points*10);
               rewardSum += points;
            }
        }
    }

    private List<int> GetColumnList(List<GameObject> pickedFields){
         List<int> columnsToCheck = new List<int>();
        foreach (GameObject field in pickedFields){
            int column = field.GetComponent<FieldSquare>().getX();
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
            AddReward(points*10);
            rewardSum += points;
        }
    }

    private void CrossOutFields(List<GameObject> pickedFields){
        GameFieldSkript.CrossFields(pickedFields);
    }


    private GameObject PickField(float action, List<GameObject> AvailableFields) {
        action = Mathf.Clamp01(Mathf.Abs(action)*0.9999f);

        int index = Mathf.FloorToInt(action * AvailableFields.Count);
        // Ensure that the index is within a valid range
        index = Mathf.Clamp(index, 0, AvailableFields.Count-1);
        return AvailableFields[index];
    }



    private void PushValidField(Transform squareField, VectorSensor sensor) {
        sensor.AddObservation(GetColorIndexOneHotFromColor(squareField.GetComponent<FieldSquare>().color));
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().available);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().starField);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().crossed);
        sensor.AddObservation(GetCoordinateObservation(squareField.gameObject));
        sensor.AddObservation(GetGroupObservation(squareField.gameObject));
    }

    private void PushEmptyField(VectorSensor sensor){
        sensor.AddObservation(GetColorIndexOneHotFromColor(""));
        sensor.AddObservation(0);
        sensor.AddObservation(0);
        sensor.AddObservation(0);
        sensor.AddObservation(new Vector2(0,0));
        sensor.AddObservation(0);
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
        if (GameFieldSkript.joker >= 1) {
            GameFieldSkript.ReduceJoker();
        } else {
            EndEpisode();
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
