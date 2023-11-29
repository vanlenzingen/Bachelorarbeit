using System.Collections;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject[] numberDice;
    public GameObject[] colorDice;
    public GameObject field;
    public int playerNumber = 2;
    public float timeout = 1.0f;

    void Start()
    {
        CreateFields();
        StartCoroutine(RollDiceCoroutine());
    }

    IEnumerator RollDiceCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            //Debug.Log("Iteration: " + i);
            yield return new WaitForSeconds(timeout);
            RerollDice();
        }
    }

    private void RerollDice()
    {
        for (int i = 0; i < 3; i++)
        {
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
}