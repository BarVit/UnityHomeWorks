public class Wave
{
    private readonly int _size;

    private int _retiredCount;

    public Wave(int size)
    {
        _size = size;
    }

    public int Size => _size;

    public bool IsCleared => _retiredCount >= _size;

    public void CountRetired()
    {
        _retiredCount++;
    }
}
