namespace Play.Egg
{
    public enum EggState
    {
        NULL,
        normal,
        perfect,
        bad
    }
    public class EggData
    {
        public float perfectSpeed;
        public float badSpeed;
        public EggState eggState = EggState.normal;

        public EggData(float perfectSpeed, float badSpeed)
        {
            this.perfectSpeed = perfectSpeed;
            this.badSpeed = badSpeed;
        }
    }
}
