using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberDice : MonoBehaviour
{
    public GameObject[] numberSprites;
    public int number;

    void Start() {
        Roll();
    }

    void ShowNumber() {

        foreach (Transform child in transform)
    {
        Destroy(child.gameObject);
    }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject square = Instantiate(numberSprites[this.number-1], transform.position, Quaternion.identity);
        square.transform.parent = this.transform;
    }

    public int GetNumber() {
        return this.number;
    }

    public void Roll() {
        int numberResult = Random.Range(1, 7);
        this.number = numberResult;
//         ShowNumber();
    }

    public void Reroll() {
//         Destroy(this.transform.GetChild(0).gameObject);
        Roll();
    }
}
