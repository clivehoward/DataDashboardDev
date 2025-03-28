namespace DataDashboardWebLib
{
    class ChartColors
    {
        private static string[] colors;
        public static string[] Colors
        {
            get
            {
                InitialiseColorArray();
                return colors;
            }
        }

        private static void InitialiseColorArray()
        {
            // Array of colors
            colors = new string[]
            {
                "#EF5350",
                "#EC407A",
                "#AB47BC",
                "#7E57C2",
                "#5C6BC0",
                "#42A5F5",
                "#29B6F6",
                "#26C6DA",
                "#26A69A",
                "#66BB6A",
                "#9CCC65",
                "#D4E157",
                "#FFEE58",
                "#FFCA28",
                "#FFA726",
                "#FF7043",
                "#8D6E63",
                "#BDBDBD",
                "#78909C",
                "#b71c1c",
                "#880E4F",
                "#4A148C",
                "#311B92",
                "#1A237E",
                "#0D47A1",
                "#01579B",
                "#006064",
                "#004D40",
                "#1B5E20",
                "#33691E",
                "#827717",
                "#F57F17",
                "#FF6F00",
                "#E65100",
                "#BF360C",
                "#3E2723",
                "#212121",
                "#263238"
            };
        }
    }
}
