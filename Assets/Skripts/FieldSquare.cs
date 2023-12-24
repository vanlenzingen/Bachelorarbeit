using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldSquare : MonoBehaviour
{
    public GameObject Star;
    public string color;
    public bool starField = false;
    public bool crossed = false;
    public bool available;
    public int group;
    public int groupsize;
    public int xPos;
    public int yPos;

    // Start is called before the first frame update
    void Start() {
        SetColor(this.color);
        SetStarfield();
    }


    private void SetColor(string color)
    {
        SpriteRenderer squareRenderer = this.GetComponent<SpriteRenderer>();
        switch (color)
        {
            case "":
                squareRenderer.color = Color.white;
                break;
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
        }
    }

    public void ResetField(){
        this.crossed = false;
        SetColor(this.color);
        if (xPos != 7){
            this.available = false;
        }
        if (starField)            {
                Transform star = transform.GetChild(0);
                SpriteRenderer starRenderer = star.GetComponent<SpriteRenderer>();
                starRenderer.color = Color.white;
            }

    }



    public void SetStarfield() {
        if (this.starField) {
            GameObject starField = Instantiate(Star, new Vector3(transform.position.x, transform.position.y, transform.position.z -0.1f), Quaternion.identity);
            starField.transform.parent = this.transform;
        }
    }

    public void CrossField()
    {
        if (!crossed) { 
            SpriteRenderer squareRenderer = this.GetComponent<SpriteRenderer>();
            squareRenderer.color = Color.grey;

            if (starField)
            {
                Transform star = transform.GetChild(0);
                SpriteRenderer starRenderer = star.GetComponent<SpriteRenderer>();
                starRenderer.color = Color.black;
            }
            this.crossed = true;
            this.transform.parent.GetComponent<GameField>().decreaseColorCount(color);
            this.transform.parent.GetComponent<GameField>().setNeighborsAvailable(this.gameObject);
        }
    }

    public void setColor(string color){
    this.color = color;    
    }

    public void setToStarfield() {
        this.starField = true;
    }

    public void crossField() {
        this.crossed = true;
    }

    public void setGroup(int groupNumber){
        this.group = groupNumber;
    }


    public void setX(int xpos) {
        this.xPos = xpos;
    }

    public void setY(int ypos)
    {
        this.yPos = ypos;
    }

    public int getX() {
        return this.xPos;
    }

    public int getY() {
        return this.yPos;
    }

    public bool getAvailable(){
        return this.available;
    }

    public void setAvailable() {
        this.available = true;
    }
}
