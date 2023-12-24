using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDice : MonoBehaviour
{
    public GameObject squarePrefab;
    public string color;


    void Start()
    {
        Roll();
    }


    void SetColor(string color)
    {
        GameObject square = Instantiate(squarePrefab, transform.position, Quaternion.identity);
        square.transform.parent = this.transform;

        SpriteRenderer squareRenderer = square.GetComponent<SpriteRenderer>();


        switch (color)
        {
            case "red":
                squareRenderer.color = Color.red;
                break;
            case "blue":
                squareRenderer.color = Color.blue;
                break;
            case "green":
                squareRenderer.color = Color.green;
                break;
            case "yellow":
                squareRenderer.color = Color.yellow;
                break;
            case "orange":
                squareRenderer.color = new Color(1.0f, 0.5f, 0.0f);
                break;
            case "joker":
                squareRenderer.color = Color.black;
                break;
        }
    }

    public void Roll() {
        string[] colors = { "red", "blue", "green", "yellow", "orange", "joker" };
        int randomIndex = Random.Range(0, colors.Length);
        string colorResult = colors[randomIndex];
        this.color = colorResult;
        SetColor(color);
    }


    public void Reroll() {
        Destroy(this.transform.GetChild(0).gameObject);
        Roll();
    }

    public string GetColor() {
        return this.color;
    }
}
