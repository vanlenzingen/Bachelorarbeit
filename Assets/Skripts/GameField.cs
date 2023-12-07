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

    private int blueCount = 21;
    private int redCount = 21;
    private int greenCount = 21;
    private int orangeCount = 21;
    private int yellowCount = 21;
    public GameObject Agent;
    

    public void Start() {
        InstantiateAgent();
        this.name = "GameField " + PlayerIndex;
        pushSquaresintoArray(); // delte this later
    }

    private void InstantiateAgent(){
        GameObject agent = Instantiate(Agent, transform.position + new Vector3(0, -2, 0), Quaternion.identity);
        agent.transform.parent = this.transform;
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
        switch (color)
        {
            case "red":
                redCount--;
                if (redCount == 0) {
                    GetBonusPoints(color);
                }
                break;
            case "blue":
                blueCount-=1;
                if (blueCount == 0) {
                    GetBonusPoints(color);
                }
                break;
            case "green":
                greenCount -= 1;
                if (greenCount == 0) {
                    GetBonusPoints(color);
                }
                break;
            case "yellow":
                yellowCount -= 1;
                if (yellowCount == 0) {
                    GetBonusPoints(color);
                }
                break;
            case "orange":
                orangeCount -= 1;
                if (orangeCount == 0) {
                    GetBonusPoints(color);
                }
                break;
        }

    }

    private void GetBonusPoints(string color) {
        Debug.Log("Get Points for: " + color);
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
        if (remainingFields == 0) {
            Debug.Log("Give some Points column " + column + " is full");
        }
        return remainingFields;
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

    public void createField() {
        //instantiateSquares();
        //colorizeAndGroupSquares();
        //setStars();
    }

    private void instantiateSquares() {
        this.squares = new GameObject[Columns, Rows];
        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Columns; j++) {
                GameObject squareField = Instantiate(Square, new Vector2(i, j), Quaternion.identity);
                squareField.transform.parent = this.transform;
                squareField.GetComponent<FieldSquare>().setX(i);
                squareField.GetComponent<FieldSquare>().setY(j);
                this.squares[i, j] = squareField;
            }
        }
    }

    private void colorizeAndGroupSquares() {
        string[] colors = { "red", "blue", "green", "yellow", "orange"}; ;
        for (int i = 0; i<colors.Length;i++){
            colorizeFields(colors[i]);
        }
    }

    private void colorizeFields(string color) {
        // 6, 5, 4, 3, 2, 1, splitting not neccessary to be like that, import is to have 21 fields
        
  
        Debug.Log(color);
    }

    private void setStars() {
        Debug.Log("Shuffle Up Stars");
    }


    public List<GameObject> getSquaresColumn(int column)
    {
        List<GameObject> squaresOfColumn = new List<GameObject>();

        if (column >= 0 && column < this.Columns)
        {
            for (int j = 0; j < Rows; j++)
            {

                GameObject square = this.squares[column, j];
                squaresOfColumn.Add(square);
            }
        }
        return squaresOfColumn;
    }

    public GameObject GetSquareField(int x, int y){
        return squares[x,y];
    }
}
