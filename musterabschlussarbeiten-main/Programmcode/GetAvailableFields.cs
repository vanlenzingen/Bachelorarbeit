     public List<GameObject> GetAvailableFieldsForGroupAndColor(string color, int number){
        List<GameObject> availableFields = new List<GameObject>();
        int counter = 0;
        foreach(GameObject square in squares){
            counter ++;
            string squareColor = square.GetComponent<FieldSquare>().color;
            bool available = square.GetComponent<FieldSquare>().available;
            bool crossed = square.GetComponent<FieldSquare>().crossed;
            int group = square.GetComponent<FieldSquare>().group;
            if (color == "joker"){
                if (available && !crossed && group >= number){
                    availableFields.Add(square);
                }
            } else {
                if (available && !crossed && group >= number && squareColor == color){
                    availableFields.Add(square);
                }
            }
        }
        return availableFields;
    }
