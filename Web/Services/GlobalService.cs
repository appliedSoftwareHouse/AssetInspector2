using AppConfig;

namespace Web.Services
{
    public class GlobalService
    {
        public Format Format { get; set; }
        public Reports Reports { get; set; }
        public AppPaths Paths { get; set; }
        private ConfigKeys Keys { get; set; }


        public GlobalService(ConfigKeys _Keys)
        {
            Keys = _Keys;
            Format = GetFormats();
            Reports = GetReports();
            Paths = GetPaths();
        }

        private Format GetFormats()
        {
            return new()
            {
                Currency = Keys.Get<string>("Currency"),
                DateFormat = Keys.Get<string>("DefaultDate"),
                Number = Keys.Get<string>("Number"),
                Digit2 = Keys.Get<string>("Digit2"),
                Digit4 = Keys.Get<string>("Digit4"),
                Digit6 = Keys.Get<string>("Digit6")

            };
        }

        private Reports GetReports()
        {
            return new()
            {
                AssetReport = Keys.Get<string>("AssetReport"),
                FormC =       Keys.Get<string>("FormC"),
                Form11 =      Keys.Get<string>("Form11"),
            };
        }

        private AppPaths GetPaths()
        {
            return new()
            {
                SystemFolder = Directory.GetCurrentDirectory(),
                AssetImagePath = Keys.Get<string>("AssetImagePath"),
                ClassImagePath = Keys.Get<string>("ClassImagePath"),
                AppImagePath = Keys.Get<string>("AppImagePath")
            };
        }
    }


    public class Format
    {
        public string? Currency { get; set; }
        public string? DateFormat { get; set; }
        public string? Number { get; set; }
        public string? Digit2 { get; set; }
        public string? Digit4 { get; set; }
        public string? Digit6 { get; set; }
    }

    public class Reports
    {
        public string? AssetReport { get; set; }
        public string? FormC { get; set; }
        public string? Form11 { get; set; }
    }

    public class AppPaths
    {
        public string SystemFolder { get; set; } = string.Empty;
        public string AssetImagePath { get; set; } = string.Empty;
        public string ClassImagePath { get; set; } = string.Empty;
        public string AppImagePath { get; set; } = string.Empty;
            
    }
}
