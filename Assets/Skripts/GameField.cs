using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    public int PlayerIndex;
    public GameObject Square;
    private int Rows = 7;
    private int Columns = 15;
    public GameObject[,] squares = new GameObject[15, 7];
    public int joker = 10;
    public int roundCount = 0;

    private int blueCount = 21;
    private int redCount = 21;
    private int greenCount = 21;
    private int orangeCount = 21;
    private int yellowCount = 21;
    public int points = 0;
    public int colorsFinished = 0;
    

    public void Start() {
        this.name = "GameField " + PlayerIndex;
        pushSquaresintoArray(); // delte this later
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
        coordinateList.RemoveAll(coord => coord.x < 0 || coord.x >= 15 || coord.y < 0 || coord.y >= 7);


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


    public void GetColumnPoints(int column) {
        //TODO only one time per row
            int columnPoints;
            GameObject Controller = GameObject.FindWithTag("Controller");
            columnPoints = Controller.GetComponent<Controller>().GetColumnPoints(column);
            AddToPoints(columnPoints);
    }

    public void pushSquaresintoArray() {
        FieldSquare[] childSquares = GetComponentsInChildren<FieldSquare>();

        foreach (var square in childSquares){
            int x = square.getY();
            int y = square.getX();
            this.squares[y, x] = square.gameObject;
        }
    }

    public void setPlayer(int playerIndex) {
        this.PlayerIndex = playerIndex;
    }


    private void instantiateSquares() {
        this.squares = new GameObject[Columns, Rows];
    }


    public void CrossField(Vector2 coordinates){
        GameObject field = GetSquareField(coordinates);
        field.GetComponent<FieldSquare>().CrossField();
    }


    public List<GameObject> getSquaresColumn(int column)
    {
        List<GameObject> squaresOfColumn = new List<GameObject>();

        if (column >= 0 && column < this.Columns)
        {
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

        AddRemainingStarFieldPoints();
        AddJokerPoints();
        Debug.Log(points);
        Debug.Log(roundCount);
        Debug.Log("Game Finished points("+ points+") per Turn("+roundCount+"):" + points / roundCount);
        joker = 10;
        points = 0;
        roundCount = 0;
        colorsFinished = 0;
        ResetSquares();
        ResetColorCount();
        ResetController();
    }

    private void ResetController(){
        GameObject Controller = GameObject.FindWithTag("Controller");
        Controller.GetComponent<Controller>().Reset();
        Debug.Log("Resetting Controller");
    }

    private void ResetSquares(){
         foreach(GameObject square in squares){
            square.GetComponent<FieldSquare>().ResetField();
        }
    }

    public void NextRound(){
        roundCount += 1;
    }

    private void ResetColorCount(){
        Debug.Log("ResetColorCount");
        blueCount = 21;
        redCount = 21;
        greenCount = 21;
        orangeCount = 21;
        yellowCount = 21;
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

    public void ColorFinished(string color){
        GameObject Controller = GameObject.FindWithTag("Controller");
        int points = Controller.GetComponent<Controller>().GetColorPoints(color);
        AddToPoints(points);
        colorsFinished ++;
    }
}
