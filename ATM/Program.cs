using ATM.Models;
using System;
using System.Data.SqlClient;

namespace ATM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int amount = 0, deposit, withdraw, transfer, outStanding;
            string choice;
            MasterUser masterUser = new MasterUser();


            while (true)
            {
                choice = Console.ReadLine();

                var TempChoice = choice.Split(" ");

                switch (TempChoice[0].ToLower())
                {
                    case "login":
                        var Logins = Login(TempChoice[1]);
                        masterUser = Logins;

                        var dataOwed = GetOwedLogin(masterUser.UserId);
                      
                        Console.WriteLine("\n Hello, {0} ", masterUser.Username);
                        Console.WriteLine("\n Your balance is $ {0} ", masterUser.Balance);
                        if (dataOwed != null)
                        {
                            var userTarget = GetUserId(dataOwed.UserOrigin); 
                            Console.WriteLine("\n Owed $ {0} from {1}", dataOwed.Amount , userTarget.Username);
                        }
                        Console.WriteLine("\n");

                        break;

                    case "balance":
                        if (masterUser.Username != null && masterUser.Username != string.Empty)
                        {
                            Console.WriteLine("\n Your balance is $ {0} ", masterUser.Balance);
                            Console.WriteLine("\n");
                        }
                        else
                        {
                            Console.WriteLine("\n You need Login First");
                            Console.WriteLine("\n");
                        }

                        break;

                    case "deposit":
                        if (masterUser.Username != null && masterUser.Username != string.Empty)
                        {
                            deposit = int.Parse(TempChoice[1]);
                            amount = masterUser.Balance + deposit;
                            masterUser.Balance = amount;
                            DepositWithdraw(masterUser, deposit);
                            var DataOwed = GetOwed(masterUser.UserId);

                            if (DataOwed == null)
                            {
                                UpdateBalance(masterUser);
                                Console.WriteLine("\n Your balance is $ {0} ", amount);
                                Console.WriteLine("\n");
                            }
                            else
                            {

                                var userTransfer = GetUserId(DataOwed.UserTarget);
                                var owed = DataOwed.Amount - deposit;
                                if(owed <= 0)
                                {
                                    masterUser.Balance = deposit - DataOwed.Amount;
                                    UpdateBalance(masterUser);
                                    DeleteOwed(DataOwed);
                                    Console.WriteLine("\n Your balance is $ {0} ", masterUser.Balance);
                                    Console.WriteLine("\n");
                                }
                                else
                                {
                                    UpdateBalance(userTransfer);
                                    DataOwed.Amount = owed;
                                    UpdateOwed(DataOwed);
                                    Console.WriteLine("\n Your balance is $ {0} ", masterUser.Balance);
                                    Console.WriteLine("\n Owed $ {0} to {1} ", masterUser.Balance, userTransfer.Username);
                                    Console.WriteLine("\n");
                                }
                                
                            }
                        }
                        else
                        {
                            Console.WriteLine("\n You need Login First");
                            Console.WriteLine("\n");
                        }

                        break;
                    case "withdraw":
                        if (masterUser.Username != null && masterUser.Username != string.Empty)
                        {
                            withdraw = int.Parse(TempChoice[1]);
                            amount = masterUser.Balance - withdraw;
                            masterUser.Balance = amount;
                            if (amount >= 0)
                            {
                                DepositWithdraw(masterUser, withdraw);
                                UpdateBalance(masterUser);
                                Console.WriteLine("\n Your balance is $ {0} ", amount);
                                Console.WriteLine("\n");
                            }
                            else
                            {
                                Console.WriteLine("\n Your balance below 0");
                                Console.WriteLine("\n");
                            }

                        }
                        else
                        {
                            Console.WriteLine("\n You need Login First");
                            Console.WriteLine("\n");
                        }

                        break;
                    case "transfer":
                        if (masterUser.Username != null && masterUser.Username != string.Empty)
                        {
                            transfer = int.Parse(TempChoice[2]);
                            amount = masterUser.Balance - transfer;
                            outStanding = 0;
                            if (amount < 0)
                            {
                                outStanding = transfer - masterUser.Balance;
                                transfer -= outStanding;
                                amount = masterUser.Balance - transfer;
                            }

                            var TargetUser = GetUser(TempChoice[1]);
                            var tempTargetUser = TransferTarget(TargetUser, transfer);
                            TransferOrigin(masterUser, transfer);
                            masterUser.Balance = amount;
                            UpdateBalance(masterUser);
                            UpdateBalance(tempTargetUser);
                            Console.WriteLine("\n Transferred $ {0} to {1} ", transfer, TargetUser.Username);
                            Console.WriteLine("\n Your balance is $ {0} ", amount);
                            if (outStanding > 0)
                            {
                                Owed(masterUser.UserId, tempTargetUser.UserId, outStanding);
                                Console.WriteLine("\n Owed {0} to {1} ", outStanding, TargetUser.Username);
                                Console.WriteLine("\n");
                            }
                            Console.WriteLine("\n");
                        }
                        else
                        {
                            Console.WriteLine("\n You need Login First");
                            Console.WriteLine("\n");
                        }

                        break;
                    case "logout":
                        Console.WriteLine("\n Goodbye, {0}  ", masterUser.Username);
                        masterUser = new MasterUser();
                        break;
                }
            }

            Console.WriteLine("\n\n THANKS FOR USING YES ATM SERVICE");
        }

        #region Logical

        public static MasterUser Login(string userName)
        {
            MasterUser masterUser = new MasterUser();
            try
            {
                string displayQuery = "SELECT * FROM MasterUser WHERE Username='" + userName + "'";
                SqlDataReader dataReader = Helper.Select(displayQuery);
                while (dataReader.Read())
                {
                    var Id = Convert.ToInt32(dataReader.GetValue(0));
                    var Username = dataReader.GetValue(1);
                    var Balance = Convert.ToInt32(dataReader.GetValue(3));
                    masterUser.UserId = Id;
                    masterUser.Username = userName;
                    masterUser.Balance = Balance;

                    return masterUser;
                }

                if (!dataReader.Read())
                {
                    string insertQuery = "INSERT INTO MasterUser(Username,Pin,Balance) VALUES('" + userName + "', " + 1234 + "," + 0 + ")";
                    var resultId = Helper.InsertUpdate(displayQuery);

                    masterUser.UserId = resultId;
                    masterUser.Username = userName;
                    masterUser.Balance = 0;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return masterUser;
        }

        public static MasterUser DepositWithdraw(MasterUser user, int deposit)
        {
            MasterUser masterUser = new MasterUser();

            try
            {
                TransactionATM transactionATM = new TransactionATM()
                {
                    UserId = user.UserId,
                    Amount = deposit,
                    LastStatment = user.Balance,
                    PostStatment = user.Balance + deposit
                };

                string insertQuery = "INSERT INTO TransactionATM(UserId,Amount,LastStatment,PostStatment) VALUES(" + transactionATM.UserId + ", " + transactionATM.Amount + "," + transactionATM.LastStatment + "," + transactionATM.PostStatment + ")";
                var resultId = Helper.InsertUpdate(insertQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return masterUser;
        }

        public static MasterUser GetUser(string userName)
        {
            MasterUser masterUser = new MasterUser();
            try
            {
                string displayQuery = "SELECT * FROM MasterUser WHERE Username='" + userName + "'";
                SqlDataReader dataReader = Helper.Select(displayQuery);
                while (dataReader.Read())
                {
                    var Id = Convert.ToInt32(dataReader.GetValue(0));
                    var Username = dataReader.GetValue(1);
                    var Balance = Convert.ToInt32(dataReader.GetValue(3));
                    masterUser.UserId = Id;
                    masterUser.Username = userName;
                    masterUser.Balance = Balance;

                    return masterUser;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return masterUser;
        }

        public static MasterUser GetUserId(int userId)
        {
            MasterUser masterUser = new MasterUser();
            try
            {
                string displayQuery = "SELECT * FROM MasterUser WHERE UserId='" + userId + "'";
                SqlDataReader dataReader = Helper.Select(displayQuery);
                while (dataReader.Read())
                {
                    var Id = Convert.ToInt32(dataReader.GetValue(0));
                    var Username = dataReader.GetValue(1).ToString();
                    var Balance = Convert.ToInt32(dataReader.GetValue(3));
                    masterUser.UserId = Id;
                    masterUser.Username = Username;
                    masterUser.Balance = Balance;

                    return masterUser;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return masterUser;
        }

        public static MasterUser TransferTarget(MasterUser TargetUser, int amount)
        {
            MasterUser masterUser = new MasterUser();
            try
            {
                TransactionATM transactionATM = new TransactionATM()
                {
                    UserId = TargetUser.UserId,
                    Amount = amount,
                    LastStatment = TargetUser.Balance,
                    PostStatment = TargetUser.Balance + amount
                };

                masterUser = TargetUser;
                masterUser.Balance = transactionATM.PostStatment;

                string insertQuery = "INSERT INTO TransactionATM(UserId,Amount,LastStatment,PostStatment) VALUES(" + transactionATM.UserId + ", " + transactionATM.Amount + "," + transactionATM.LastStatment + "," + transactionATM.PostStatment + ")";
                var resultId = Helper.InsertUpdate(insertQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return masterUser;
        }

        public static MasterUser TransferOrigin(MasterUser User, int amount)
        {
            MasterUser masterUser = new MasterUser();
            try
            {
                TransactionATM transactionATM = new TransactionATM()
                {
                    UserId = User.UserId,
                    Amount = amount,
                    LastStatment = User.Balance,
                    PostStatment = User.Balance - amount
                };

                string insertQuery = "INSERT INTO TransactionATM(UserId,Amount,LastStatment,PostStatment) VALUES(" + transactionATM.UserId + ", " + transactionATM.Amount + "," + transactionATM.LastStatment + "," + transactionATM.PostStatment + ")";
                var resultId = Helper.InsertUpdate(insertQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return masterUser;
        }

        public static int UpdateBalance(MasterUser User)
        {
            int resultId = 0;
            try
            {
                string updateQuery = "UPDATE MasterUser SET BALANCE = " + User.Balance + " WHERE Username = '" + User.Username + "'";
                resultId = Helper.InsertUpdate(updateQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultId;
        }

        public static void Owed(int origin , int target ,int amount)
        {
            try
            {
                string insertQuery = "INSERT INTO Owed(UserOrigin,UserTarget,Amount) VALUES(" + origin + ", " + target + "," + amount + ")";
                var resultId = Helper.InsertUpdate(insertQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Owed GetOwed(int userId)
        {
            Owed Owed = new Owed();
            try
            {
                string displayQuery = "SELECT * FROM Owed WHERE UserOrigin=" + userId ;
                SqlDataReader dataReader = Helper.Select(displayQuery);
                while (dataReader.Read())
                {
                    var Id = Convert.ToInt32(dataReader.GetValue(0));
                    var UserOrigin = Convert.ToInt32(dataReader.GetValue(1));
                    var UserTarget = Convert.ToInt32(dataReader.GetValue(2));
                    var Amount = Convert.ToInt32(dataReader.GetValue(3));
                    Owed.OwedId = Id;
                    Owed.UserOrigin = UserOrigin;
                    Owed.UserTarget = UserTarget;
                    Owed.Amount = Amount;
                    return Owed;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Owed;

        }

        public static int UpdateOwed(Owed data)
        {
            int resultId = 0;
            try
            {
                string deleteQuery = "UPDATE Owed SET Amount = " + data.Amount +" WHERE OwedId = " + data.OwedId;
                resultId = Helper.InsertUpdate(deleteQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultId;
        }

        public static int DeleteOwed(Owed data)
        {
            int resultId = 0;
            try
            {
                string deleteQuery = "DELETE FROM Owed WHERE OwedId = "+ data.OwedId;
                resultId = Helper.InsertUpdate(deleteQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return resultId;
        }

        public static Owed GetOwedLogin(int userId)
        {
            Owed Owed = new Owed();
            try
            {
                string displayQuery = "SELECT * FROM Owed WHERE UserTarget=" + userId;
                SqlDataReader dataReader = Helper.Select(displayQuery);
                while (dataReader.Read())
                {
                    var Id = Convert.ToInt32(dataReader.GetValue(0));
                    var UserOrigin = Convert.ToInt32(dataReader.GetValue(1));
                    var UserTarget = Convert.ToInt32(dataReader.GetValue(2));
                    var Amount = Convert.ToInt32(dataReader.GetValue(3));
                    Owed.OwedId = Id;
                    Owed.UserOrigin = UserOrigin;
                    Owed.UserTarget = UserTarget;
                    Owed.Amount = Amount;
                    return Owed;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Owed;

        }
        #endregion
    }
}
