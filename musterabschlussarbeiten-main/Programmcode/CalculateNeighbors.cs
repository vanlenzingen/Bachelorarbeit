    public List<GameObject> CalculateNeighbours(List<GameObject> pickedFields){
        List<GameObject> validNeighbors = new List<GameObject>();
        foreach (GameObject field in pickedFields) {
            List<GameObject> possibleNeighbors= GetNeighborsOfTheSameColor(field);
            if (possibleNeighbors.Count != 0){
                foreach(GameObject  neighbor in possibleNeighbors) {
                    if (!pickedFields.Contains(neighbor)){
                        validNeighbors.Add(neighbor);
                        }
                    }
                }
            }
        return validNeighbors;
    }
