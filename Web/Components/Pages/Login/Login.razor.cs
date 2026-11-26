using AppModels.Login;
using Crypto;


namespace Web.Components.Pages.Login
{
    public partial class Login
    {

        private string Error = "";

        private string ImagePath { get; set; }
        private string NumberFormat { get; set; }




        public LoginModel UserLogin { get; set; } = new();

        protected override void OnInitialized()
        {
            ImagePath = Config.GetValue<string>("AssetImagePath") ?? "";
            NumberFormat = Global.Format.Number!;
        }

        public async Task OnLogin()
        {
            Error = string.Empty;

            // Validate Database, Create Db if not exist and add Admin User Account.
            if (!ValidateDB()) { NavManager.NavigateTo("/Error/DBError/2"); }


            if (string.IsNullOrWhiteSpace(UserLogin?.UserID))
            {
                Error = "Please enter your user ID.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UserLogin?.Password))
            {
                Error = "Please enter your password.";
                return;
            }

            if (Error == string.Empty)
            {
                var LoginService = new AppAuth.LoginClass(DbService.Connection, Config.MyModel);

                if (LoginService.IsConnected)
                {
                    var _LoginModel = new LoginModel()
                    {
                        UserID = UserLogin.UserID,
                        Password = string.Empty,
                        PasswordHash = Encrypt.EncryptString(UserLogin.Password, "Modern"),
                        RememberMe = false
                    };

                    UserProfile? _UserProfile = LoginService.GetUserProfile(_LoginModel);

                    if (_UserProfile.IsLogin)
                    {
                        await UserAuth.SignInAsync(_UserProfile);
                        NavManager.NavigateTo("/Dashboard");

                    }
                    else
                    {
                        Error = L[_UserProfile.LoginMessage];
                    }
                }
            }
        }

        private bool ValidateDB()
        {
            // Createing a new Database instance if not exist, create all data table and system and Admin User default Account

            bool _result = false;

            if (DbService.IsDBExist)
            {
                _result = DbService.CreateDataTables;
                if (_result) _result = DbService.UpdatePatches;
                if (_result) _result = DbService.CreateAdminAccount;
            }
            return _result;
        }
    }
}



