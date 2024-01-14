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
        InstantiateField();
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
            finishedColors[color] = false;
        }
    }

    private void InstantiateAgent(){
        GameObject agent = Instantiate(Agent, transform.position + new Vector3(0, -2, 0), Quaternion.identity);
        agent.transform.parent = this.transform;
    }

    private void InstantiateDices() {
        for (int i = 0; i<2; i++){
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

    private void InstantiateField() {
        GameObject GameFieldPrefab = Instantiate(field, transform.position, Quaternion.identity);
        GameFieldPrefab.GetComponent<GameField>().setPlayer(0);
        GameFieldPrefab.transform.parent = this.transform;

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

    public float GetColumnPoints(int column){
        if (finishedColumns[column] == false) {
            finishedColumns[column] = true;
            return GetMaximumColumnPoints(column);
        } else {
            return GetMinimumColumnPoints(column);
            }
        }

    public float GetColorPoints(string color){
        if (finishedColors[color] == false){
            finishedColors[color] = true;
            return 5.0f;
            } else {
            return 3.0f;
        }
    }


    public void Reset(){
        SetColumnsUncompleted();
        SetColorsUncompleted();
    }

    private float GetMaximumColumnPoints(int column) {
    switch (column) {
        case 0:
        case 14:
            return 5.0f;
        case 1:
        case 2:
        case 3:
        case 11:
        case 12:
        case 13:
            return 3.0f;
        case 4:
        case 5:
        case 6:
        case 8:
        case 9:
        case 10:
            return 2.0f;
        case 7:
            return 1.0f;
        default:
            return 0.0f;
        }
    }

    private float GetMinimumColumnPoints(int column) {
        switch (column) {
            case 0:
            case 14:
                return 3.0f;
            case 1:
            case 2:
            case 3:
            case 11:
            case 12:
            case 13:
                return 2.0f;
            case 4:
            case 5:
            case 6:
            case 8:
            case 9:
            case 10:
                return 1.0f;
            case 7:
                return 0.0f;
            default:
                return 0.0f;
        }
    }

}
