namespace CardGameServer.Model
{
    public class AccountModel
    {
        public int id;
        public string account;
        public string password;

        public AccountModel() {

        }

        public AccountModel(int id,string acc,string pwd) {
            this.id = id;
            this.account = acc;
            this.password = pwd;
        }
    }
}