    private void PushEmptyField(VectorSensor sensor){
        sensor.AddObservation(GetColorIndexOneHotFromColor(""));
        sensor.AddObservation(0);
        sensor.AddObservation(0);
        sensor.AddObservation(0);
    }
