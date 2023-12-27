using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    public GameObject NumberDicePrefab;
    public GameObject ColorDicePrefab;
    public GameObject[] numberDice;
    public GameObject[] colorDice;
    public GameObject field;
    public int playerNumber = 1;
    public float timeout = 1.0f;
    public GameObject Agent;
    private List<bool> finishedColumns = new List<bool>(new bool[15]);
    private Dictionary<string, bool> finishedColors = new Dictionary<string, bool>();


    void Start() {
        SetColorsUncompleted();
        SetColumnsUncompleted();
        CreateFields();
        InstantiateDices();
        InstantiateAgent();
    }

    private void SetColumnsUncompleted(){
        for (int i = 0; i<15; i++){
            finishedColumns.Add(false);
        }
    }

    private void SetColorsUncompleted(){
         foreach (string color in new string[] { "yellow", "green", "blue", "red", "orange" }) {
            finishedColors.Add(color, false);
        }
    }

    private void InstantiateAgent(){
        GameObject agent = Instantiate(Agent, transform.position + new Vector3(0, -2, 0), Quaternion.identity);
        agent.transform.parent = this.transform;
    }

    private void InstantiateDices() {
        for (int i = 0; i<3; i++){
            GameObject ColorDice = Instantiate(ColorDicePrefab, transform.position + new Vector3(-4, i*2, 0), Quaternion.identity);
            ColorDice.transform.parent = this.transform;
            GameObject NumberDice = Instantiate(NumberDicePrefab, transform.position + new Vector3(-2, i*2, 0), Quaternion.identity);
            NumberDice.transform.parent = this.transform;
        }
    }

    private void RerollDice() {
        for (int i = 0; i < 3; i++) {
            numberDice[i].GetComponent<NumberDice>().Reroll();
            colorDice[i].GetComponent<ColorDice>().Reroll();
        }
    }

    private void CreateFields() {
        GameObject GameFieldPrefab = Instantiate(field, transform.position, Quaternion.identity);
        
        GameFieldPrefab.GetComponent<GameField>().createField();
        GameFieldPrefab.GetComponent<GameField>().setPlayer(0);
       
        for (int i = 0; i < playerNumber-1; i++) {
            GameObject duplicatedField = Instantiate(GameFieldPrefab, transform.position, Quaternion.identity);
            duplicatedField.transform.position = new Vector3(0, (i + 1) * 15, 0);
            duplicatedField.GetComponent<GameField>().setPlayer(i + 1);
        }
    }

    public void RerollDices() {
         foreach (Transform child in transform) {
            if (child.CompareTag("NumberDice")) {
                child.GetComponent<NumberDice>().Reroll();
            }
            if (child.CompareTag("ColorDice")) {
                child.GetComponent<ColorDice>().Reroll();
            }
        }
    }

    public int GetColumnPoints(int column){
        int points;
        //TODO
        if (finishedColumns[column] == false) {
            finishedColumns[column] = true;
            //points = GetMaximumColorPoints -> switchcase shit
            return 5;
        } else {
            return 3;
            //points = GetMinimumColorPointsget not so maximum points
        }

    }

    public int GetColorPoints(string color){
        if (finishedColors[color] == false){
            finishedColors[color] = true;
            return 5;
            } else {
            return 3;
        }
    }


    public void Reset(){
        SetColumnsUncompleted();
        SetColorsUncompleted();
    }
}
