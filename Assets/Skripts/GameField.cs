using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    public int PlayerIndex;
    public GameObject Square;
    public int Rows;
    public int Columns;
    public GameObject[,] squares = new GameObject[15, 7];
    public int joker = 8;
    public int roundCount = 0;

    public int blueCount;
    public int redCount;
    public int greenCount;
    public int orangeCount;
    public int yellowCount;
    public int points = -30;
    public int colorsFinished = 0;
    private Controller ControllerScript;
    private GameObject Controller;

    public void Start() {
        this.name = "GameField " + PlayerIndex;
        pushSquaresintoArray();
        Controller = transform.parent.gameObject;
        ControllerScript = Controller.GetComponent<Controller>();
        ResetColorCount();
    }


    private void ResetColorCount() {
        List<GameObject> blueFields = new List<GameObject>();
        List<GameObject> greenFields = new List<GameObject>();
        List<GameObject> redFields = new List<GameObject>();
        List<GameObject> orangeFields = new List<GameObject>();
        List<GameObject> yellowFields = new List<GameObject>();

        foreach(GameObject field in squares){
            string color = field.GetComponent<FieldSquare>().color;
            switch (color) {
        case "blue":        blueFields.Add(field);      break;
        case "green":       greenFields.Add(field);     break;
        case "yellow":      yellowFields.Add(field);    break;
        case "orange":      orangeFields.Add(field);    break;
        case "red":         redFields.Add(field);       break;
            }
        }
        blueCount = blueFields.Count;
        redCount = redFields.Count;
        yellowCount = yellowFields.Count;
        greenCount = greenFields.Count;
        orangeCount = orangeFields.Count;
    }

    public void setNeighborsAvailable(GameObject square) {
        FieldSquare squareScript = square.GetComponent<FieldSquare>();
        int x = squareScript.getX();
        int y = squareScript.getY();
        List<Vector2> coordinateList = new List<Vector2>();
        coordinateList.Add(new Vector2(x-1, y));
        coordinateList.Add(new Vector2(x + 1, y));
        coordinateList.Add(new Vector2(x, y-1));
        coordinateList.Add(new Vector2(x, y+1));

        //remove Coords out of Bonds
        coordinateList.RemoveAll(coord => coord.x < 0 || coord.x >= Columns || coord.y < 0 || coord.y >= Rows);


        foreach (var coord in coordinateList) {
            GameObject neighborSquare = squares[(int)coord.x, (int)coord.y];
            FieldSquare neighborSquareScript = neighborSquare.GetComponent<FieldSquare>();
            neighborSquareScript.setAvailable();
        }
    }


    public void decreaseColorCount(string color) {
        switch (color) {
            case "red":         redCount --;      break;
            case "blue":        blueCount --;     break;
            case "green":       greenCount --;    break;
            case "yellow":      yellowCount --;   break;
            case "orange":      orangeCount --;   break;
        }
    }


    public int CheckNumberOfRemainingFields(int column) {
        List<GameObject> squaresofrow = new List<GameObject>();
        squaresofrow = getSquaresColumn(column);
        int remainingFields = 0;
        foreach (GameObject square in squaresofrow) {
            if (!square.GetComponent<FieldSquare>().crossed) {
                remainingFields++;
            }
        }
        return remainingFields;
    }


    public void pushSquaresintoArray() {
        FieldSquare[] childSquares = GetComponentsInChildren<FieldSquare>();
        foreach (var square in childSquares){
            int x = square.getX();
            int y = square.getY();
            this.squares[x, y] = square.gameObject;
        }
    }

    public void setPlayer(int playerIndex) {
        this.PlayerIndex = playerIndex;
    }


    private void instantiateSquares() {
        this.squares = new GameObject[Columns, Rows];
    }

    public void CrossFields(List<GameObject> fields){
        if (fields.Count==0){
            return;
        }
        foreach(GameObject field in fields){
            if (!field.GetComponent<FieldSquare>().crossed){
                field.GetComponent<FieldSquare>().CrossField();
                decreaseColorCount(field.GetComponent<FieldSquare>().color);
            }
        }
    }

    public void UpdateGroups(List<GameObject> fieldsToInspect){
        List<GameObject> groupUpdateList = GetGroupUpdateList(fieldsToInspect);
        foreach( GameObject field in groupUpdateList){
            List<GameObject> group = new List<GameObject>();
            group.Add(field);
            group = GetGroup(group);
            UpdateGroupCountOfEachElementInList(group);
        }
    }


private List<GameObject> GetGroup(List<GameObject> group){
    int refSize;
    do {
        refSize = group.Count;
        List<GameObject> newNeighbors = new List<GameObject>();

        foreach (GameObject field in group) {
            List<GameObject> neighbors = GetNeighborsOfTheSameColor(field);
            foreach (GameObject neighbor in neighbors) {
                if (!group.Contains(neighbor) && !newNeighbors.Contains(neighbor)) {
                    newNeighbors.Add(neighbor);
                }
            }
        }
        group.AddRange(newNeighbors);
    }
    while (group.Count != refSize);

    return group;
}


    private void UpdateGroupCountOfEachElementInList(List<GameObject> group){
        foreach (GameObject field in group){
            field.GetComponent<FieldSquare>().SetGroup(group.Count);
            }
        }


    public List<GameObject> GetGroupUpdateList(List<GameObject> fieldsToInspect){
        List<GameObject> fieldsToUpdate = new List<GameObject>();
        if (fieldsToInspect.Count > 0){
            foreach (GameObject field in fieldsToInspect){
                List<GameObject> neighborsToUpdate = GetNeighborsOfTheSameColor(field);
                foreach (GameObject neighbor in neighborsToUpdate){
                    if (!fieldsToInspect.Contains(neighbor)){
                        fieldsToUpdate.Add(neighbor);
                    }
                }
            }
        }
        return fieldsToUpdate;
    }


    private List<GameObject> GetNeighborsOfTheSameColor(GameObject field){
       List<GameObject> neighbors = new List<GameObject>();
        if (!field){
            return neighbors;
        }
        string refColor = field.GetComponent<FieldSquare>().color;
        Vector2 coords = field.GetComponent<FieldSquare>().GetCoords();
        int group = field.GetComponent<FieldSquare>().group;

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i=0;i<4;i++){
            int newX = (int)coords.x + dx[i];
            int newY = (int)coords.y + dy[i];

            Vector2 coordinates = new Vector2(newX, newY);

            if (CoordinatesInBond(coordinates)){
                GameObject neighbor = squares[newX, newY];
                if (neighbor.GetComponent<FieldSquare>().color == refColor && !neighbor.GetComponent<FieldSquare>().crossed && neighbor.GetComponent<FieldSquare>().group == group){
                    neighbors.Add(neighbor);
                }
            }
        }
        return neighbors;
    }


    public List<GameObject> GetAvailableFieldsForGroupAndColor(string color, int number){
        List<GameObject> availableFields = new List<GameObject>();
        int counter = 0;
        foreach(GameObject square in squares){
            counter ++;
//             Debug.Log(square.GetComponent<FieldSquare>().color + ":" + square.GetComponent<FieldSquare>().group);
            string squareColor = square.GetComponent<FieldSquare>().color;
            bool available = square.GetComponent<FieldSquare>().available;
            bool crossed = square.GetComponent<FieldSquare>().crossed;
            int group = square.GetComponent<FieldSquare>().group;
            if (color == "joker"){
                if (available && !crossed && group >= number){
                    availableFields.Add(square);
                }
            } else {
                if (available && !crossed && group >= number && squareColor == color){
                    availableFields.Add(square);
                }
            }
        }
        return availableFields;
    }

    public List<GameObject> CalculateNeighbours(List<GameObject> pickedFields){
        List<GameObject> validNeighbors = new List<GameObject>();
        foreach (GameObject field in pickedFields) {
            List<GameObject> possibleNeighbors= GetNeighborsOfTheSameColor(field);
            if (possibleNeighbors.Count != 0){
                foreach(GameObject  neighbor in possibleNeighbors) {
                    if (!pickedFields.Contains(neighbor)){
                        validNeighbors.Add(neighbor);
                        }
                    }
                }
            }
        return validNeighbors;
    }


    public List<GameObject> getSquaresColumn(int column)    {
        List<GameObject> squaresOfColumn = new List<GameObject>();
        if (column >= 0 && column < this.Columns)        {
            for (int j = 0; j < Rows; j++){
                GameObject square = this.squares[column, j];
                squaresOfColumn.Add(square);
            }
        }
        return squaresOfColumn;
    }


    public GameObject GetSquareField(Vector2 coordinates){
        return squares[(int)coordinates.x,(int)coordinates.y];
    }

    public bool CoordinatesInBond(Vector2 coordinates){
        int newX = (int)coordinates.x;
        int newY = (int)coordinates.y;
        bool inBounds = (newX >= 0 && newX < Columns) && (newY >= 0 && newY < Rows);
        return inBounds;
    }

    public void ReduceJoker(){
        if (joker>=1){
            this.joker --;
        }
    }


    public int GetColorCount(string color){
    switch (color)    {
        case "blue":    return blueCount;
        case "green":   return greenCount;
        case "yellow":  return yellowCount;
        case "orange":  return orangeCount;
        case "red":     return redCount;
        }
    return -1;
    }

    public void NewGame(){
        joker = 8;
        points = -30;
        roundCount = 0;
        colorsFinished = 0;
        ResetSquares();
        ResetColorCount();
        ResetController();
    }

    private void ResetController(){
        ControllerScript.Reset();
    }

    private void ResetSquares(){
         foreach(GameObject square in squares){
            square.GetComponent<FieldSquare>().ResetField();
        }
    }

    public void NextRound(){
        roundCount += 1;
    }



    public void AddToPoints(int number){
        points += number;
    }

    private void AddJokerPoints(){
        AddToPoints(joker);
    }


    private void AddRemainingStarFieldPoints(){
        int stars = 0;
        foreach (GameObject field in squares){
            if (!field.GetComponent<FieldSquare>().crossed && field.GetComponent<FieldSquare>().starField){
                stars ++;
            }
        }
        AddToPoints(stars*-2);
    }

    public void ColorFinished(){
        colorsFinished ++;
    }
}
