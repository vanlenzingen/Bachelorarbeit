    public void Roll() {
        List<string> colors = new List<string> {"red", "blue", "green", "yellow", "orange", "joker"};
        if(colorShift != ""){
            for (int i=0;i<10;i++){
                colors.Add(colorShift);
            }
        }
        int randomIndex = Random.Range(0, colors.Count);
        string colorResult = colors[randomIndex];
        this.color = colorResult;
        SetColor(color);  // show dice
    }
