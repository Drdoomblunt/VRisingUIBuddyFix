namespace UIBuddy.UI.ScrollView;

public struct DataViewInfo
{
    // static
    public static DataViewInfo None => s_default;
    private static DataViewInfo s_default = default;

    public static implicit operator float(DataViewInfo it) => it.height;

    public DataViewInfo(int index, float height, float startPos, int spread)
    {
        dataIndex = index;
        this.height = height;
        startPosition = startPos;
        normalizedSpread = spread;
    }

    // instance
    public int dataIndex, normalizedSpread;
    public float height, startPosition;

    public override bool Equals(object obj)
    {
        DataViewInfo other = (DataViewInfo)obj;

        return dataIndex == other.dataIndex
               && height == other.height
               && startPosition == other.startPosition
               && normalizedSpread == other.normalizedSpread;
    }

    public override int GetHashCode() => base.GetHashCode();
}