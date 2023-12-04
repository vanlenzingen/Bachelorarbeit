using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject NumberDicePrefab;
    public GameObject ColorDicePrefab;
    public GameObject[] numberDice;
    public GameObject[] colorDice;
    public GameObject field;
    public int playerNumber = 1;
    public float timeout = 1.0f;

    void Start() {
        CreateFields();
        InstantiateDices();
        //Invoke("RerollDices",1f);
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
}
