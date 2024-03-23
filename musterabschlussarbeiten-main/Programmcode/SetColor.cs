    void SetColor(string color)  {
        GameObject square = Instantiate(squarePrefab, transform.position, Quaternion.identity);
        square.transform.parent = this.transform;
        SpriteRenderer squareRenderer = square.GetComponent<SpriteRenderer>();

        switch (color) {
            case "red":     squareRenderer.color = Color.red;                       break;
            case "blue":    squareRenderer.color = Color.blue;                      break;
            case "green":   squareRenderer.color = Color.green;                     break;
            case "yellow":  squareRenderer.color = Color.yellow;                    break;
            case "orange":  squareRenderer.color = new Color(1.0f, 0.5f, 0.0f);     break;
            case "joker":   squareRenderer.color = Color.black;                     break;
        }
    }
