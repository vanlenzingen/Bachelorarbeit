using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberDice : MonoBehaviour
{
    public GameObject[] numberSprites;
    public int number;


    // Start is called before the first frame update
    void Start()
    {
        Roll();
    }

    void ShowNumber() {
        if (number >= 1 && number <= 6)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            GameObject square = Instantiate(numberSprites[this.number-1], transform.position, Quaternion.identity);
            square.transform.parent = this.transform;
        }
    }

    public int GetNumber() {
        return this.number;
    }

    public void Roll() {
        int numberResult = Random.Range(1, 7);
        //Debug.Log("Number Dice Result: " + numberResult);
        this.number = numberResult;
        ShowNumber();
    }

    public void Reroll() {
        Destroy(this.transform.GetChild(0).gameObject);
        Roll();
    }
}
