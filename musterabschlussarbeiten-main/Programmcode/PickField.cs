    private GameObject PickField(float action, List<GameObject> AvailableFields) {
        action = Mathf.Clamp01(Mathf.Abs(action)*0.9999f);

        int index = Mathf.FloorToInt(action * AvailableFields.Count);

        index = Mathf.Clamp(index, 0, AvailableFields.Count-1);
        return AvailableFields[index];
    }
