namespace Web.Services
{
    public class GlobalService
    {
        public IConfigurationService Config;
        public Format Format { get; set; }
        public Reports Reports { get; set; }

        public GlobalService(IConfigurationService config)
        {
            Config = config;
            Format = GetFormats();
            Reports = GetReports();
        }

        private Format GetFormats()
        {
            return new()
            {
                Currency = Config.GetValue<string>("Currency"),
                DateFormat = Config.GetValue<string>("DefaultDate"),
                Number = Config.GetValue<string>("Number"),
                Digit2 = Config.GetValue<string>("Digit2"),
                Digit4 = Config.GetValue<string>("Digit4"),
                Digit6 = Config.GetValue<string>("Digit6")

            };
        }

        private Reports GetReports()
        {
            return new()
            {
                AssetReport = Config.GetValue<string>("AssetReport"),
                FormC = Config.GetValue<string>("FormC"),
                Form11 = Config.GetValue<string>("Form11"),
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
}
