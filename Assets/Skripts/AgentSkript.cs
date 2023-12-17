using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentSkript : Agent {

    GameObject GameField;
    GameObject Controller;

    public override void OnEpisodeBegin()    {
        // Reset the environment for a new episode
        GameField = this.transform.parent.gameObject;
        Controller = GameObject.FindWithTag("Controller");
    }


    public override void CollectObservations(VectorSensor sensor)    {

        // get Observation for amount of jokers
        sensor.AddObservation(GetOneHotFromInt(GameField.GetComponent<GameField>().joker,11));

        //get Observation of NumberDices - number
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                sensor.AddObservation(GetOneHotFromInt(child.GetComponent<NumberDice>().number,6));
            }
        }

        //get Observation for ColorDice - colorindex
         foreach (Transform child in Controller.transform) {
         if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
            }
         }

        //getObservation of Fields colorindex, available, star, crossed
        foreach (Transform child in GameField.transform) {
            if (child.CompareTag("Square")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<FieldSquare>().color));
                sensor.AddObservation(child.GetComponent<FieldSquare>().available);
                sensor.AddObservation(child.GetComponent<FieldSquare>().starField);
                sensor.AddObservation(child.GetComponent<FieldSquare>().crossed);
            }
        }
    }

    //TODO Validation of ints e.g. dont pick a field above 120 and not und 0 etc etc etc
    //TODO dont pick a dice > 6


    //actionbuffers should be like [diceindex, numberindex, field, field, field, field, field]
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        float reward = 0.0f;
        //validateRewards
        /*
        if (actionBuffers.DiscreteActions[0] < 0 && actionBuffers.DiscreteActions[0] > 2){
        reward += -2.0f;
        }
        if (actionBuffers.DiscreteActions[1] < 0 && actionBuffers.DiscreteActions[1] > 2){
            reward += -2.0f;
        }
        for (int i=2; i<6;i++) {
           if  (actionBuffers.DiscreteActions[i] < 0 || actionBuffers.DiscreteActions[i] > 104) {
               reward += -2.0f;
           }
        }        
        */
        
        int colorDiceAction = actionBuffers.DiscreteActions[0];
        int numberDiceAction = actionBuffers.DiscreteActions[1];

        string choosenColor;
        choosenColor = GetColorOfChoosenDice(numberDiceAction);

        int choosenNumber;
        choosenNumber = GetNumberOfChoosenDice(numberDiceAction);


        reward += GetColorDiceReward(colorDiceAction);
        reward += GetNumberDiceReward(numberDiceAction);


        int[] squareIndices = new int[5]; // shouldnt this be 5?
        for (int i = 2; i < actionBuffers.DiscreteActions.Length; i++) {
            int squareIndex = actionBuffers.DiscreteActions[i];
            squareIndices[i-2]=i;
            if (squareIndex == -1) {
                continue;
            } else {
                reward += CrossSquareField(
                    squareIndex % 15,
                    squareIndex / 15,
                    GetColorOfChoosenDice(colorDiceAction)
                ); 
                //GameField.GetComponent<GameField>().CrossField (x, y); -> should be implemented in gameField
                // Check For Neighbor Reward
            }
        }
        reward += CheckNumberReward(choosenNumber, squareIndices);
    }


    private float  GetColorDiceReward(int colorDiceIndex){
        if (GetColorOfChoosenDice(colorDiceIndex)== "joker" && GameField.GetComponent<GameField>().joker == 0){
            return -1.0f;
        }
    return 0.0f;
    }


    private float  GetNumberDiceReward(int numberDiceIndex){
        if (GetNumberOfChoosenDice(numberDiceIndex)==6 && GameField.GetComponent<GameField>().joker == 0){
            return -1.0f;
        }
    return 0.0f;
    }


    private float  CrossSquareField(int x, int y, string chosenColor){
        float reward = 0.0f;
        GameObject squareFieldGameObject = GameField.GetComponent<GameField>().GetSquareField(x,y);
        FieldSquare squareField = squareFieldGameObject.GetComponent<FieldSquare>();

        reward += CheckForColorReward(squareField.color , chosenColor);
        reward += CheckForAvailableReward(squareField.available);
        reward += CheckForCrossedReward(squareField.crossed);

      /*  if color full ++

        if column full ++
            */

        return reward;
    }
    /// Rewards
    /*
    private float CheckForNeighborReward(int[] FieldIndicies){
        // translate FieldIndicies into Vektor2D[] koords 
        koordinates = new List<Vector2D>;
        for (field in FieldIndicies){
            koordinates.push(new Vektor2D(field.GetComponent<FieldSquare>().x,field.GetComponent<FieldSquare>().y)
        }

        for (koord in koordinates){
        int x1 = koord.GetComponent<FieldSquare>().x;
        int y1 = koord.GetComponent<FieldSquare>().y;
            for (int i= 0 ; i<koordinates.Length; i++){
            int x2 =
            int y2 =  
            if (x1 == x2+1 && y1 == y2 || x1 == x2-1 && y1 == y2 ||  y1 == y2-1 && x1 == x2||y1 == y2-1 && x1 == x2){
            continue;
        } else {
        return -1.0f;
    }
    return 1.0f;
    } 
    
    
    */
    
    private float CheckForCrossedReward(bool crossed){
        if (crossed){
            return -1.0f;
        }
        return 1.0f;
    }

    private float CheckForColorReward(string fieldColor, string chosenColor){
        if (fieldColor == chosenColor) {
            return 1.0f;
        } else {
            return -1.0f;
        }
    }

    private float CheckForAvailableReward(bool available) {
        if (available){
            return 1.0f;
        }
        return -1.0f;
    }


    private float CheckNumberReward(int number, int[] squareIndex){
        if (squareIndex.Length > number){ //numberPicked should be taken
            return -0.3f;
        } else if (squareIndex.Length < number) {
            return -0.1f;
        } else {
            return 1.0f;
        }
    }


    /// helper


    private int GetNumberOfChoosenDice(int index){
        int[] diceArray = new int[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("NumberDice")) {
                diceArray[diceArray.Length] = child.GetComponent<NumberDice>().number;
            }
        }
        if (diceArray[index] == 6) {
            GameField.GetComponent<GameField>().ReduceJoker();
        }
        return diceArray[index];
    }

    private string GetColorOfChoosenDice(int index){
        string[] diceArray = new string[3];
        foreach (Transform child in Controller.transform) {
            if (child.CompareTag("ColorDice")) {
                diceArray[diceArray.Length] = child.GetComponent<ColorDice>().color;
            }
        }
        if (diceArray[index] == "joker") {
            GameField.GetComponent<GameField>().ReduceJoker();
            }
        return diceArray[index];
    }


    private float[] GetColorIndexOneHotFromColor(string color){
        float[] colorOneHot = new float[6];
        switch (color)    {
            case "blue":
                 colorOneHot[0] = 1;
                break;
            case "green":
                 colorOneHot[1] = 1;
                break;
            case "yellow":
                 colorOneHot[2] = 1;
                break;
            case "orange":
                 colorOneHot[3] = 1;
                break;
            case "red":
                 colorOneHot[4] = 1;
                break;
            case "joker":
                 colorOneHot[5] = 1;
                break;
            }
        return colorOneHot;
    }


    private float[] GetOneHotFromInt(int number, int numberOfCases){
        float[] oneHotArray = new float[numberOfCases];
        oneHotArray[number] = 1;
        return oneHotArray;
    }
}

