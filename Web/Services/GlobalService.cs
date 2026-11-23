namespace Web.Services
{
    public class GlobalService
    {
        public IConfigurationService Config;
        public Format Format { get; set; }

        public GlobalService(IConfigurationService config)
        {
            Config = config;
            Format = GetFormats();
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
    }


    public class Format
    {
        public string Currency { get; set; }
        public string DateFormat { get; set; }
        public string Number { get; set; }
        public string Digit2 { get; set; }
        public string Digit4 { get; set; }
        public string Digit6 { get; set; }


    }
}
