 
    public override void CollectObservations(VectorSensor sensor)    {
        sensor.AddObservation(GetJokersObservation());
        sensor.AddObservation(GetRoundCountObservation());

        foreach (Transform childTransform in controller.transform) {
            if (childTransform.CompareTag("NumberDice")) {
                GameObject child = childTransform.gameObject;
                sensor.AddObservation(GetNumberDiceObservation(child));
            }
        }

        foreach (Transform child in controller.transform) {
            if (child.CompareTag("ColorDice")) {
                sensor.AddObservation(GetColorIndexOneHotFromColor(child.GetComponent<ColorDice>().color));
            }
        }

        int counter = 0;
        foreach (Transform squareField in GameField.transform) {
            if (squareField.CompareTag("Square")) {
                PushValidField(squareField, sensor);
                counter += 1;
            }
        }
        do {
            if (counter < 105){
            PushEmptyField(sensor);
            counter += 1;
            }
        }
            while (counter < 105);
    }
