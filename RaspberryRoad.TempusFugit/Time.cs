
namespace RaspberryRoad.TempusFugit
{
    public class Time
    {
        float globalTimeCoordinate;

        public float Current { get { return globalTimeCoordinate; } }
        public int GlobalTimeCoordinate { get { return (int)globalTimeCoordinate; } }

        public Time()
        {

        }

        public void UpdateGameTime(float deltaTime)
        {
            globalTimeCoordinate += deltaTime * 25f;
        }

        public void JumpTo(float targetTime)
        {
            globalTimeCoordinate = targetTime;
        }
    }
}
