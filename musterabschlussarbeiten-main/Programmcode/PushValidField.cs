    private void PushValidField(Transform squareField, VectorSensor sensor) {
        sensor.AddObservation(GetColorIndexOneHotFromColor(squareField.GetComponent<FieldSquare>().color));
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().available);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().starField);
        sensor.AddObservation(squareField.GetComponent<FieldSquare>().crossed);
    }
