using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Kiosk_2._0
{
    internal class Program
    {
        static void Main(string[] args)
        {


            //DECLARE VARIABLES NEEDED OUTSIDE OF LOOPS
            bool newTransaction=true;
                Random transactionNum = new Random();
                Kiosk selfCheckOut = new Kiosk();

            do
            {
                //DISPLAY MESSAGES TO USER
                MainHeader();
                Header("Please enter the price of each item seperately. \n\t\t\t    Press enter when all prices have been submitted. ");
    
                //DECLARE VARIABLES
                int paymentMethod = 0;
                double itemsTotal = 0;

                //CALLS FUNCTION TO ADD ITEM PRICES
                itemsTotal=AddItems(selfCheckOut);
                 
                //WHILE LOOP WILL RUN UNTIL USER HAS PAID THE TOTAL BALANCE DUE
                while (selfCheckOut.GetBalanceDue() > 0) { 
                
                    //CALLS FUNCTION FOR USER TO CHOOSE FORM OF PAYMENT
                    paymentMethod= ChoosePaymentMethod(selfCheckOut) ;
                    
                    //CALLS FUNCTION FOR CASH TO BE ENTERED
                    if (paymentMethod == 1) {
    
                        PayCash(selfCheckOut);
                        selfCheckOut.GetBalanceDue();
                    }//end cash if 
    
                    //CARD PAYMENT IS SELECTED
                    else if (paymentMethod == 2) { 
                        
                       //CALLS FUNCTION FOR CARD INFORMATION TO BE ENTERED
                       int[] creditCardNum= PayCard(selfCheckOut); 

                        //CALLS KIOSK METHOD TO VALIDATE CREDIT CARD NUMBER
                         bool validNumber= selfCheckOut.ValidateCreditCard(creditCardNum);

                        //IF CARD IS VALID-- FUNCTION IS CALLED TO SEE IF CARD IS APPROVED FOR FUNDS
                        if (validNumber == true)
                        {
                            string[] cardOutCome = MoneyRequest(selfCheckOut.GetCreditCardNum, selfCheckOut.GetBalanceDue());

                            //IF CARD IS DECLINED MESSAGE IS DISPLAYED AND KIOSK METHOD RUNS TO CLEAR INFORMATION FOR NEXT TRANSACTION ATTEMPT
                            if (cardOutCome[1] == "declined")
                            {
                                ColorText("\t\t\t--------------- CARD DECLINED ---------------", ConsoleColor.Red);
                                selfCheckOut.CardDeclined();
                            }//end if

                            //IF CARD IS APPROVED METHOD IS CALLED TO DETERMINE IF IT WAS ENOUGH TO PAY CASHBACK AND/OR TOTAL
                            else
                            {
                                double amountApproved = 0;
                                double.TryParse(cardOutCome[1], out amountApproved);
                                selfCheckOut.DetermineCashBack(Math.Round(amountApproved, 2));
                            }//end else

                        }//end if

                        //IF CREDIT CARD ENTERED WAS NOT A VALID NUMBER MESSAGE IS DISPLAYED AND CASHBACK IS SET TO 0
                        else {
                            Header("\tCREDIT CARD COULD NOT BE VALIDATED");
                            selfCheckOut.CashBack = 0;                       
                        }//end else

                    }//end card if 
    
                    //IF USER CHOSE TO CANCEL TRANSACTION- CONFIRMATION IS REQUESTED- ALL PAYMENTS MADE ARE REFUNDED 
                    else if (paymentMethod == 3) {
                        int userConfirmCancel=PromptTryInt("\tTO CONFIRM CANCEL:  ENTER 3\t\t OTHERWISE ENTER ANY NUMBER TO CONTINUE: ");
                        if (userConfirmCancel == 3) {
                            selfCheckOut.RefundCreditCard();
                            selfCheckOut.ClearTransaction();
                        }//END IF - confirm cancel
                    }// end else if- abandon
                }//end while loop

                //DISPLAYS TO USER TRANSACTION DETAILS AND METHOD CALLED TO LOG THEN CLEAR TRANSACTION
                Thread.Sleep(1000);
                Header("\t\t  GENERATING RECEIPT");
                int tranNum= transactionNum.Next(1000, 100000);
                Console.WriteLine($"\n\t\t\t---------------  TRANSACTION NUMBER: {tranNum}  ---------------\n");
                Thread.Sleep(1000);
                FinalizeTransaction (selfCheckOut,tranNum);

                //TRANSACTION NUMBER ITERATES FOR NEXT TICKET
                //transactionNum++;
                Thread.Sleep (1000);
            } while (newTransaction);

        }//END MAIN


        static double AddItems(Kiosk selfCheckOut) {
            //DECLARE VARIABLES
            bool lastItem=false;
            int itemNum = 0;
            double itemPrice = 0;
            double itemsTotal = 0;
            string userResponse = "";
            double userInput = 0;
            bool isValid = false;
           
            //WHILE LOOP RUNS UNTIL USER HAS FINISHED ADDING ITEMS
            while (lastItem == false)
            {
                //COUNT INCREASES FOR SCREEN DISPLAY
                itemNum++;

                //ITEMS ARE ADDED TO TOTAL
                itemsTotal += itemPrice;

                //SET KIOSK FIELD ITEM TOTALS
                selfCheckOut.ItemTotals = itemsTotal;

                //HERE TO CLEAN UP DISPLAY SCREEN 
                if (selfCheckOut.ItemTotals > 0) { 
                    Console.WriteLine($"\t\t\t-------------------------------------{selfCheckOut.ItemTotals:C}\n");
                }//end if
                Console.Write($"\t\t\tITEM {itemNum}:\t\t\t\t$");
               
                //EVALUATES WHAT USER HAS ENTERED AS A PRICE
                userResponse = Console.ReadLine();

                //DO WHILE- VALIDATION LOOP-
                do
                {
                    //IF USER ENTERS EMPTY STRING
                    if (userResponse == "")
                    {
                        //BREAKS OUTTER WHILE LOOP
                        lastItem = true;
                        //BREAKS INNER DO WHILE LOOP 
                        isValid = true;

                    }//end if

                    //IF USER ENTERS ANYTHING EXCEPT EMPTY STRING
                    else
                    {
                        //IF WHAT USER ENTERED IS A REAL NUMBER INNER DO/WHILE LOOP WILL BREAK
                        isValid = double.TryParse(userResponse, out userInput);
                        
                        //IF THE NUMBER ENTERED IS LESS THAN ONE CENT MESSAGE DISPLAYED- INNER DO/WHILE LOOP REMAINS
                        if (userInput < .01)
                        {
                            MinorHeader("\t\t\t\t\t  Invalid Entry");
                            isValid = false;
                        }//end if

                        //IF USER HAS ENTERED ANYTHING GREATER THAN ONE CENT PRICE IS UPDATED
                        else
                        {
                            itemPrice = userInput;
                        }//end else

                        //IF A VALID NUMBER HAS NOT BEEN ENTERED- USER WILL BE PROMPTED FOR A NEW RESPONSE BEFORE LOOPING AGAIN
                        if (isValid == false)
                        {
                            Console.Write($"\t\t\tITEM {itemNum}:\t\t\t\t$");
                            userResponse = Console.ReadLine();
                        }//end if

                    }//end else
                } while (isValid == false);

            }//end while

            return itemsTotal;
            
        }//end function


        static int ChoosePaymentMethod(Kiosk selfCheckOut)
        {

            int payMethod = 0;
           
            // WHILE LOOP IS ENTER TO DETERMINE USER'S PAYMENT CHOICE- THEY MAY CHOOSE TO PAY WITH CASH, CARD, OR ABANDON ITEMS
            while (payMethod != 1 && payMethod != 2 && payMethod != 3)
            {

                //PAYMETHOD IS UPDATED TO HOLD USER'S SELECTION PROMPT TRY INT WILL ENSURE ONLY AN INT IS ACCEPTED
                MinorHeader($"\t\t\t\t\t  TOTAL DUE: ${selfCheckOut.GetBalanceDue()}");
                Header("\tPLEASE CHOOSE YOUR PAYMENT METHOD");
                Console.WriteLine();
                payMethod = PromptTryInt("  TO PAY WITH CASH: ENTER 1\tTO PAY WITH CARD: ENTER 2\tTO CANCEL TRANSACTION: ENTER 3\n");
               
            }//END WHILE

            return payMethod;

        }//END FUNCTION
        static double PayCash(Kiosk selfCheckOut)
        {
            int paymentNumber = 0;
            double payment = 0;
            double remainingBalance = 0;
            bool validPayment = false;
            bool paymentComplete = false;
            int paymentType = 0;

           
            #region TOTAL DUE AND CASH INSTRUCTIONS DISPLAY MESSAGES

            //THIS IS WHERE OPTION FOR PROMPTING USER FOR CASH CARD OR CANCEL SHOULD TAKE PLACE 
                Header("\t\t\t PAY CASH");
                Thread.Sleep(1000);
                Console.WriteLine("\n\t\t\t\t*** TO CHANGE FORM OF PAYMENT ENTER: 3 ***");
                Thread.Sleep(1000);
                MinorHeader("\t\t\t\t Enter bills and coins individually");
                #endregion

             //WHILE LOOP TO RUN WHILE THE BALANCE DUE HAS NOT BEEN PAID 
             while (paymentComplete == false)
             {
                paymentNumber++;
                    //PROMPTS USER TO ENTER A PAYMENT
                    payment = PromptTryDouble($"\t\t\tPAYMENT {paymentNumber}:\t\t\t$");

                      if (payment == 3) {
                          return payment;
                      }//end if 

                else { 
                    //DO WHILE LOOP ESTABLISHED TO ENSURE A VALID PAYMENT IS ENTERED- IF A VALID RESPONSE IS NOT GIVEN PAYMENT CANNOT BE UPDATED
                    do
                    {
                        //CHECKS IF USER HAS ENTER A VALID DENOMINATION OF CURRENCY

                        validPayment = selfCheckOut.InsertPayment(payment);

                        if (validPayment == true)
                        {
                            if (selfCheckOut.GetBalanceDue()<=0 )
                            {
                                paymentComplete = true;

                            }//end if

                        }//end if 

                        //IF ANYTHING OTHER THAN AN ACCEPTED VALUE WAS ENTERED A MESSAGE IS DISPLAYED TO THE USER & BOOL validPayment IS SET FALSE TO 
                        //REMAIN IN DO WHILE LOOP 
                        else
                        {
                        
                            MinorHeader("\t\t\t\tThat is not a recognized bill or coin");
                            validPayment = false;
                            payment = PromptTryDouble($"\t\t\tPAYMENT {paymentNumber}:\t\t\t$");

                        }//end else

                    } while (validPayment == false);

                }//end else   
                    //IF STATEMENT HERE JUST FOR DISPLAY PURPOSES- ONLY SHOWS IF A BALANCE IS STILL DUE
                    if (selfCheckOut.GetBalanceDue() > 0)
                    {
                        Console.WriteLine($"\t\t\t-------------------------------------{selfCheckOut.GetBalanceDue():C}\n");
                    }//end if 

             }//end while

             //IF KIOSK IS UNABLE TO PROVIDE CHANGE FOR THE PAYMENT ENTERED PAYMENT IS REFUNDED AND MESSAGE DISPLAYED
                if (selfCheckOut.CalculateChange() > selfCheckOut.CheckBank() )
                {

                    Thread.Sleep(1000);
                    MinorHeader($"\t\t\t\t\t UNABLE TO MAKE CHANGE.");
                    Thread.Sleep(1000);

                    MinorHeader($"\t\t\t\tYOUR PAYMENT OF: {selfCheckOut.HoldingChamber:C} HAS BEEN REFUNDED.");
                    Thread.Sleep(1000);

                   MinorHeader($"\t\t\t\tENTER EXACT CHANGE OR PAY WITH A CARD.");  
                   Thread.Sleep(1000);
                   selfCheckOut.RefundHoldingChamber();

                }//end if

                selfCheckOut.ValidateDenominations();

            return payment;
     
        }//end function
        static int[] PayCard(Kiosk selfCheckOut)
        {
            //DECLARE VARIABLES
            string userInput = "";
            bool intEntered = true;
            bool cardChecked = false;
            bool validCard = false;
            int[] cardNum = new int[userInput.Length];


            //PROMPTS USER TO ENTER CASHBACK AMOUNT AND UPDATES KIOSK FIELD
            selfCheckOut.CashBack = PromptTryIntCashBack("\n\t\t\tENTER CASH BACK AMOUNT: $0-$500: $");

            //KIOSK METHOD CALLED TO CLEAR FIELD AND GET ACCURATE COUNT OF MONEY IN THE BANK 
            selfCheckOut.CheckBank();

            //KIOSK METHOD CALLED TO MAKE SURE CHANGE CAN BE MADE WITH THE DENOMINATIONS AVAILABLE IN BANK 
            selfCheckOut.ValidateDenominations();

            //IF THE CASHBACK NEEDED IS GREATER THAN THE MONEY IN THE BANK MESSAGE IS DISPLAYED AND CASHBACK IS CLEARED
            if ((double)selfCheckOut.CashBack > selfCheckOut.CheckBank())
            {
                Console.WriteLine($"---------------We are unable to process cash back request at this time.--------------- \r---------------BALANCE DUE:  {selfCheckOut.GetBalanceDue():c}---------------");
                selfCheckOut.CashBack = 0;
            }//end if

            //IF USER HAS CHOSEN CASHBACK - FIELDS ARE UPDATED AND MESSAGE IS DISPLAYED TO SHOW UPDATED TOTAL
            if (selfCheckOut.CashBack>0)
            {
                selfCheckOut.GetBalanceDue();
                ColorText($"\n\t\t--------------- UPDATED TOTAL TO INCLUDE CASH BACK: {selfCheckOut.UpdateCardTotal():c} ---------------",ConsoleColor.DarkCyan);
                Thread.Sleep(1000);
            }//end else



            //ONCE IT IS DECIDED IF KIOSK IS ABLE TO PROVIDE CASH BACK - CARD NUMBER IS CAPTURED AND VALIDATED
            //WHILE LOOP WILL CONTINUE TO RUN UNTIL USER HAS ONLY INPUTTED NUMBERS AND THE LENGTH OF THE NUMBERS IS EITHER 15 OR 16
            while (cardChecked == false)
            {
                //DO WHILE LOOP WILL RUN AS LONG AS USER HAS FAILED TO ENTER ONLY NUMBERS- 
                do
                {
                    //BOOL INT ENTER IS RE-SET TO TRUE
                    intEntered = true;
                    //USER IS PROMPTED TO ENTER A CARD NUMBER
                    userInput = Prompt("\n\t\t\tENTER CARD NUMBER WITHOUT ANY SPACES: ");
                    Console.WriteLine();
                    Thread.Sleep(1000);

                    //FOR LOOP CHECKS THROUGH THE STRING CHARS
                    for (int i = 0; i < userInput.Length; i++)
                    {

                        //IF A CHARACTER IN THE STRING ENTERED IS NOT A NUMBER
                        if (userInput[i] < 47 && userInput[i] > 58)
                        {
                            //INT ENTER IS SET TO FALSE
                            intEntered = false;

                            //USER IS TOLD THAT ENTERY IS NOT VALID
                            Header("\t\t\t\tThat is not a valid entry");

                        }//end if 

                    }//end for loop

                    //LOOP CHECKS TO SEE IF ANY CHAR FOUND IN THE STRING HAS FLIPPED THE BOOL TO FALSE
                } while (intEntered == false);

                cardNum = new int[userInput.Length];
                //ONCE A STRING WITH ONLY INTS HAVE BEEN ENTERED FOR LOOP WILL FILL AN INT ARR WITH THE STRING 
                for (int i = 0; i < cardNum.Length; i++)
                {
                    cardNum[i] = userInput[i];
                }//end for loop

                //CHECKS TO SEE IF THE FIRST NUMBER IN ARR IS 3- INDICATING IT IS AN AMERICAN EXPRESS CARD AND MUST HAVE 15 DIGITS TO BE VALID
                if (cardNum[0] == 51)
                {
                    if (cardNum.Length == 15)
                    {
                        cardChecked = true;
                        Console.WriteLine($"\t\t\tAMERICAN EXPRESS: {userInput}");
                        selfCheckOut.SetCardVedor('3');
                    }//end if
                    else { Console.WriteLine("Invalid number of digits entered"); }//end else
                }//end if 

                //CHECKS TO SEE IF CARD NUMBER IS 4,5, OR 6- INDICATING THE CARD ISSUER REQUIRES 16 DIGITS TO BE VALID
                else if (cardNum[0] == 52 || cardNum[0] == 53 || cardNum[0] == 54)
                {
                    if (cardNum.Length == 16)
                    {
                        cardChecked = true;
                        if (cardNum[0] == '4')
                        {
                            Console.WriteLine($"\t\t\tVISA: {userInput}");
                            selfCheckOut.SetCardVedor('4');

                        }//end if

                        else if (cardNum[0] == '5')
                        {
                            Console.WriteLine($"\t\t\tMASTERCARD: {userInput}");
                            selfCheckOut.SetCardVedor('5');

                        }//end if

                        else if (cardNum[0] == '6')
                        {
                            Console.WriteLine($"\t\t\tDISCOVER: {userInput}");
                            selfCheckOut.SetCardVedor('6');
                        }//end if
                    }//end if 

                    else
                    {
                        ColorText("\t\t\tInvalid number of digits entered", ConsoleColor.DarkCyan);
                    }//end else

                }//end else if 

                //CHECKS IF USER HAS CHOSEN TO CHANGE THEIR FORM OF PAYMENT 
                else if (cardNum[0] == 48)
                {
                    if (cardNum.Length == 1)
                    {
                        cardChecked = true;
                    }//end if

                }//end if

                //ALL OTHER ENTRIES WILLL RESULT IN AN ERROR MESSAGE AND LOOP WILL NOT BREAK
                else
                {
                    MinorHeader("\t\t\t\tThat is not a valid card number. \n\t\t\tRe-enter number or ENTER 0 to change form of payment.");
                }//end else

            }//end while
            return cardNum;
            //ONCE USER ENTRY HAS BEEN SCRUBBED IT WILL BE RETURNED TO BE CHECKED FURTHER FOR NUMBER VALIDITY
          
        }//end function
        static string[] MoneyRequest(string account_number, double amount)
        {
            Random rnd = new Random();
            //50% CHANCE TRANSACTION PASSES OR FAILS
            bool pass = rnd.Next(100) < 50;
            //50% CHANCE THAT A FAILED TRANSACTION IS DECLINED
            bool declined = rnd.Next(100) < 50;

            if (pass)
            {
                return new string[] { account_number, amount.ToString() };
            }//end if 

            else
            {
                if (!declined)
                {
                    return new string[] { account_number, (amount / rnd.Next(2, 6)).ToString() };
                }//end if

                else
                {
                    return new string[] { account_number, "declined" };
                }//end else

            }//end else

        }//end function


        static void DisplayChangeDispensed(Kiosk selfCheckOut)
        {
            //METHOD RUNS TO HANDLE MAKING CHANGE AND UPDATING KIOSK INFORMATION 
            selfCheckOut.DispenseChange();

            #region DECLARE ARRAY VARIABLES
            //VARIABLES ARE CREATED TO GET INFORMATION FROM KIOSK CLASS TO BE USED TO DISPLAY CHANGE GIVEN
            string[] arrCurrencyName = selfCheckOut.GetCurrencyNameArr;
            int[] arrChangeGiven = selfCheckOut.GetChangeArr;
            double[] arrMoneyValues = selfCheckOut.GetCurrencyValue;
            #endregion


            #region DISPLAY CHANGE DISPENSING TO USER
            //USES LOOP TO LOOK THROUGH ARRAY 
            for (int i = 0; i < arrChangeGiven.Length; i++)
            {

                //IF A LOCATION IS FOUND THAT CONTAINS AT LEAST ONE BILL/COIN IN THE CHANGE POT THAT NUMBER IS STORED IN VARIABLE NUM
                //THIS IS PURELY FOR DISPLAY PURPOSES SO THAT EACH VALUE IS DISPLAYED ONE AT A TIME
                if (arrChangeGiven[i] > 0)
                {
                    int num = arrChangeGiven[i];
                    Console.WriteLine();
                    //FOR LOOP WILL PRINT THE VALUE OF THE BILL/COIN BEING DISPENSED AND WILL PRINT THAT VALUE FOR HOWEVER MANY BILLS/COINS WERE GIVEN
                    for (int j = 0; j < num; j++)
                    {
                        ColorText($"\t\t\t\t\t\t{arrCurrencyName[i]} Dispensed",ConsoleColor.DarkGreen);
                    }//end for loop 

                    
                    if (arrMoneyValues[i] < 1) { 
                        Console.WriteLine($"\n\t\t\t\t\t{arrChangeGiven[i]} COIN(S) of {arrMoneyValues[i]:C} DISPENSED\n");
                    
                    }//end if

                    else {
                        Console.WriteLine($"\n\t\t\t\t\t{arrChangeGiven[i]} BILL(S) of {arrMoneyValues[i]:C} DISPENSED. \n");
                    }//end else
                    
                }//end if

            }//end for loop
            #endregion

        }//end function
        static void FinalizeTransaction(Kiosk selfCheckOut,int transactionNum) {
            //DECLARE VARIABLES 
            DateTime transactionTime = DateTime.Now;
            string tranDate = transactionTime.ToString("d");
            string tranTime= transactionTime.ToString("t");
            char[] buffer= tranTime.ToCharArray();
            string tranTimeBuff = "";

            //FOR LOOP USED TO REPLACE SPACE WITH DASH TO AVOID PROBLEMS IN PASSING ARGS TO KIOSK LOG PROGRAM
            for (int i = 0; i < buffer.Length; i++) { 
                char c = buffer[i];
                if (c == 32) {
                    c = '-';
                }//end if

                tranTimeBuff += c;
            }//end for loop

            //CREATES VARIABLES TO BE DISPLAYED
            tranTime = tranTimeBuff;
            string tranNumber = transactionNum.ToString();
            string cardPaid = selfCheckOut.TotalCardPayments.ToString();
            string cardVendor = selfCheckOut.GetCardVendor;
            string cashPaid = selfCheckOut.HoldingChamber.ToString();
            string changeGiven = selfCheckOut.GetChangeDue.ToString();

            //DISPALY TO SCREEN
            Console.WriteLine($"\t\t\t\t\tTRANSACTIN DATE:        {tranDate}");
            Console.WriteLine($"\t\t\t\t\tTRANSACTION TIME:       {tranTime}");
            Console.WriteLine($"\t\t\t\t\tITEMS TOTAL:            {selfCheckOut.ItemTotals:c}");
            Console.WriteLine($"\t\t\t\t\tCARD VENDOR:            {cardVendor}");
            Console.WriteLine($"\t\t\t\t\tTOTAL CHARGED TO CARD:  ${cardPaid:c}");
            Console.WriteLine($"\t\t\t\t\tTOTAL CASH PAID:        ${cashPaid:c}");

            //IF CHANGE IS DUE DISPLAY AMOUNT AND CALL FUNCTIONS TO DISPENSE CHANGE AND DISPLAY CHANGE GIVEN
            if (selfCheckOut.GetChangeDue > 0) {
                Console.WriteLine($"\t\t\t\t\tCHANGE DUE:\t        {selfCheckOut.GetChangeDue:c}");
                selfCheckOut.DispenseChange();
                DisplayChangeDispensed(selfCheckOut);
            }//end if
            Console.WriteLine($"\t\t\t\t\tCHANGE DUE:             {selfCheckOut.GetChangeDue:c}\n\n\n");
           
            //RUN KIOSK METHODS TO CLEAN UP/RESET FIELDS FOR THE NEXT TRANSACTION
            selfCheckOut.DepositHoldingChamber();

            //CALL FUNCTION TO OPEN KIOSK LOG PROGRAM AND LOG TRANSACTION INFORMATION THEN CLEAR KIOSK FIELDS
            LogTransaction(tranNumber,tranDate,tranTime,cashPaid,cardVendor,cardPaid,changeGiven);
            selfCheckOut.ClearTransaction();
            
            Thread.Sleep(1000);

        }//end function
        static void LogTransaction(string tranNumber, string tranDate,string tranTime, string cashPaid,string cardVendor, string cardPaid, string changeGiven) {

            //CALLS KIOSK LOG PROGRAM
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"C:\Users\MCA-30\source\repos\Kiosk Log\bin\Debug\net8.0\Kiosk Log.exe";
            startInfo.Arguments = $"{tranNumber} {tranDate} {tranTime} {cashPaid} {cardVendor} {cardPaid} {changeGiven}";
            Process.Start(startInfo);

        }//end function



        #region Deco Headers

        static void Header(string displayText) {

        
            ColorText("\t\t------------------------------------------------------------------------",ConsoleColor.DarkYellow);
            ColorText($"                            {displayText}",ConsoleColor.Gray);
            ColorText("\t\t------------------------------------------------------------------------\n", ConsoleColor.DarkYellow);

        }//end function
        static void MainHeader() {

            ColorText("\t------------------------------------------------------------------------------------", ConsoleColor.DarkYellow);
            ColorText("\t------------------------------------------------------------------------------------", ConsoleColor.DarkCyan   );
            ColorText($"\t***************** THANK YOU FOR CHOOSING TO SHOP WITH US TODAY! ****************", ConsoleColor.Gray);
            ColorText("\t------------------------------------------------------------------------------------", ConsoleColor.DarkCyan);
            ColorText("\t------------------------------------------------------------------------------------\n", ConsoleColor.DarkYellow);

        }//end function}
        static void MinorHeader(string displayText) {

            ColorText("\n\t\t\t\t *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*",ConsoleColor.DarkCyan);
            ColorText($"{displayText}", ConsoleColor.Gray);
            ColorText($"\t\t\t\t *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*\n",ConsoleColor.DarkCyan);

        }//end function
        static void ColorText(string displayText, ConsoleColor color, bool isWriteLine = true)
        {
            Console.ForegroundColor = color;
            if (isWriteLine)
            {
                Console.WriteLine(displayText);
            }

            else
            {
                Console.Write(displayText);
            }
            Console.ResetColor();
        }//end function
       
        #endregion




        #region PROMPT FUNCTIONS

        static string Prompt(string dataRequest)
        {

            //VARIABLES
            string userInput = "";
            //REQUEST INFORMATION FROM USER
            Console.Write(dataRequest);
            //RECEIVE RESPONSE
            userInput = Console.ReadLine();
            //OUTPUT
            return userInput;
        }//END Prompt FUNCTION 

        static int PromptTryInt(string dataRequest)
        {
            //VARIABLES
            int userInput = 0;
            bool isValid = false;

            //INPUT VALIDATION LOOP
            do
            {
                Console.Write(dataRequest);
                isValid = int.TryParse(Console.ReadLine(), out userInput);


            } while (isValid == false);

            return userInput;
        }//END PromptInt FUNCTION

        static int PromptTryIntCashBack(string dataRequest)
        {
            //VARIABLES
            int userInput = 0;
            bool isValid = false;

            //INPUT VALIDATION LOOP
            do
            {
                Console.Write(dataRequest);
                isValid = int.TryParse(Console.ReadLine(), out userInput);

                if (userInput < 0 || userInput > 500)
                {
                    isValid = false;
                }//end if

            } while (isValid == false);

            return userInput;
        }//END PromptInt FUNCTION

        static double PromptTryDouble(string dataRequest)
        {
            //VARIABLES
            double userInput = 0;
            bool isValid = false;

            //INPUT VALIDATION LOOP
            do
            {
                Console.Write(dataRequest);

                isValid = double.TryParse(Console.ReadLine(), out userInput);

            } while (isValid == false);

            return userInput;
        }//END PromptInt FUNCTION
        #endregion



    }//END CLASS

}//END NAMESPACE
