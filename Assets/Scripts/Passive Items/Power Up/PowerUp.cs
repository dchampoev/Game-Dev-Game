using UnityEngine;

public class PowerUp : Passive
{
    [System.Serializable]
    public class Data
    {
        public string name;
        public int level;
        public Data(string name, int level)
        {
            this.name = name;
            this.level = level;
        }
    }
}
