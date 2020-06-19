namespace D2Evil.Peacock
{
    public enum LpkType
    {
        Standard,
        Standard2,
        Steam1_0,
        Legacy
    }

    public static class LpkUtils
    {
        public static LpkType GetLpkType(string type)
        {
            if (type.StartsWith("STD_"))
            {
                return LpkType.Standard;
            }
            if (type.StartsWith("STD2_"))
            {
                return LpkType.Standard2;
            }
            if (type == "STM_1_0")
            {
                return LpkType.Steam1_0;
            }

            return LpkType.Legacy;
        }
    }
}
