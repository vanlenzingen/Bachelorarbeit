using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class KreuzelAgentOnlyDice : Agent
{

    GameObject GameField;
    GameObject Controller;

    private GameObject controller;
    private Controller ControllerScript;
    private GameField GameFieldSkript;

    private string logFilePath;
    private string rewardLogPath;
    private float rewardSum;
    public int rewards = 0;

    void LogPoints(string data){
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

    public float GetJokersObservation(){
        return Normalize(GameFieldSkript.joker, 0, 10);
    }

    public float GetRoundCountObservation(){
        return Normalize(GameFieldSkript.roundCount, 1 , 30);
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

    }

    public void Start(){
        new WaitForSeconds(3.0f);
        logFilePath = Application.dataPath + "/Points/OnlyDice.txt";
    }


    //  choosenColor, choosenNumber, jokerNumber, field, field, field, field, field
    public override void OnActionReceived(ActionBuffers actionBuffers) {
            if (GameFieldSkript.roundCount > 30){
                GameFieldSkript.AddToPoints(GameFieldSkript.joker);
                AddReward(GameFieldSkript.joker);
                rewards += GameFieldSkript.joker;

                //string logRewards = rewards.ToString() + " " + GameFieldSkript.points.ToString();
                LogPoints(GameFieldSkript.points.ToString());
                GameFieldSkript.NewGame();
//                 logRewards = "";
                rewards = 0;
                EndEpisode();
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

             GetColorJokerPen(choosenColor);
             GetNumberJokerPen(choosenNumber);


            List<GameObject> availableFields = GameFieldSkript.GetAvailableFieldsForGroupAndColor(choosenColor, choosenNumber);
            if (choosenColor == "blue" && choosenNumber <= 4) {
                Debug.Log(availableFields.Count);
            }
            Debug.Log(availableFields.Count);
            if (availableFields.Count==0){
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
                GameFieldSkript.AddToPoints(2);
                AddReward(2.0f);
                rewards += 2;
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
               AddReward(points);
               rewards += (int)points;
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
            GameFieldSkript.AddToPoints((int)points);
            AddReward(points);
            rewards += (int)points;

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
//         sensor.AddObservation(GetCoordinateObservation(squareField.gameObject));
//         sensor.AddObservation(GetGroupObservation(squareField.gameObject));
    }

    private void PushEmptyField(VectorSensor sensor){
        sensor.AddObservation(GetColorIndexOneHotFromColor(""));
        sensor.AddObservation(0);
        sensor.AddObservation(0);
        sensor.AddObservation(0);
//         sensor.AddObservation(new Vector2(0,0));
//         sensor.AddObservation(0);
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
