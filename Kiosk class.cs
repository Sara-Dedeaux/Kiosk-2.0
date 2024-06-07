using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Kiosk_2{
    public class Kiosk {
        #region DECLARE CALSS FIELDS
        //DEBTS
        double _itemsTotal = 0;
        int _cashBack = 0;

        //CREDITS
        double _changeDue = 0;
        double _cardPaymentReceived = 0;
        double _cardTotal = 0;

        //KIOSK MONEY VALUE TRACKING
        double _bankValue = 0;
        double _balance = 0;
        double _holdingChamber = 0;
        double _paymentVault = 0;
        double _totalCardTransactions = 0;

        //KIOSK PHYSICAL MONEY/CARD STORAGE
        double[] arrCurrencyValues = { 100, 20, 10, 5, 1, .25, .10, .05, .01 };
        int[] arrCurrencyCounts = { 0, 0, 0, 0, 0, 0, 0, 0, 20 };
        string[] arrCurrencyNames = { "$Hundred$ Bill", "$Twenty$ Bill", "$Ten$ Bill", "$Five$ Bill", "$One$ Bill", "Quarter", "Dime", "Nickel", "Penny" };
        int[] arrChangeGiven = new int[9];
        string _creditCardNum = "";
        string _creditCardVendor = "N/A";


        #endregion

        #region PROPERTIES
        public string GetCardVendor { get { return _creditCardVendor; } }
        public double GetChangeDue { get { return _changeDue; } }//end property
        public string GetCreditCardNum { get { return _creditCardNum; } }//end property
        public int[] GetChangeArr { get { return arrChangeGiven; } }
        public string[] GetCurrencyNameArr { get { return arrCurrencyNames; } }//end property
        public double[] GetCurrencyValue { get { return arrCurrencyValues; } }//end property
        public double ItemTotals { get { return _itemsTotal; } set { _itemsTotal = value; } }//end property
        public double HoldingChamber { get { return Math.Round(_holdingChamber,2); } }//end property
        public int CashBack { get { return _cashBack; } set { _cashBack = value; } }//end property
        public double TotalCardPayments { get { return _cardPaymentReceived; } }

        #endregion

        #region METHODS
              
        public void ValidateDenominations() {
            double changeAvailable = 0;
            //LOOKS THROUGH ARRAY STARTING WITH THE SMALLEST DENOMINATION TO DETERMINE IF CHANGE CAN BE MADE
            for (int i = arrCurrencyCounts.Length-1; i >= 0; i--){

                //WHEN THERE ARE NO MORE DENOMINATIONS SMALLER THAN THE TOTAL OF CHANGE DUE- THE LOOP BREAKS
                if (arrCurrencyValues[i] > _changeDue && arrCurrencyValues[i]>_cashBack) { 
                   
                    break;
                }//end if

                //WHILE THE DENOMINATIONS ARE SMALLER THAN CHANGE DUE- IT IS TOTALED AND ADDED TO CHANGE COUNT
                else
                {
                    changeAvailable += arrCurrencyValues[i] * arrCurrencyCounts[i];
                }//end else

            }//end for loop

            //IF THE CHANGE AVAILABLE IS SMALLER THAN WHAT THE KIOSK NEEDS TO PAY- MESSAGE IS DISPAYED AND PAYMENT IS REFUNDED
            if (changeAvailable < _changeDue || changeAvailable<_cashBack) {
                if (changeAvailable < _cashBack) { 
                    ColorText("\n\t\t\t******* UNABLE TO MAKE CHANGE AT THIS TIME *******",ConsoleColor.DarkCyan);
                }//end if
                
                _holdingChamber = 0;
                _cashBack = 0;
            }//end if
        }//end method
        public double GetBalanceDue() {
            
           double debts = Math.Round(_itemsTotal + _cashBack,2);
            double credits = Math.Round (_cardPaymentReceived + _holdingChamber,2);
            _balance= Math.Round(debts-credits,2);
            return _balance;

        }//end method
        public double CheckBank()
        {
            //RESETS BANK TO 0 EVERY TIME IT IS CALLED SO THAT THE BANK VALUE DOES NOT INCREASE EACH TIME METHOD IS CALLED
            _bankValue = 0;
            //LOOP LOOKS THROUGH ARRAY AND CALCULATES BANK VALUE BASED ON THE NUMBER OF BILLS/COINS CURRENTLY IN BANK
            for (int i = 0; i < arrCurrencyCounts.Length; i++)
            {
                _bankValue += Math.Round(arrCurrencyCounts[i] * arrCurrencyValues[i], 2);
            }//end for loop

            return Math.Round(_bankValue,2);

        }//end 


        public bool InsertPayment(double payment)
        {
            //CHECKS IF USER HAS ENTER A VALID DENOMINATION OF CURRENCY
            bool validPayment = false;

            if (payment == .01 || payment == .05 || payment == .10 || payment == .25 || payment == .50 || payment == 1 ||
                payment == 2 || payment == 5 || payment == 10 || payment == 20 || payment == 50 || payment == 100)
            {

                //IF A VALID BILL OR COIN AMOUNT WAS GIVEN BOOL validPayment IS SWITCHED TO TRUE TO BREAK VALIDATION LOOP 
                validPayment = true;

                //SET KIOSK PAYMENTS RECEIVED
                _holdingChamber += Math.Round(payment,2);

                return validPayment;
            }//end if 

            else { return false; }//end else 
        }//end method
        public double CalculateChange()
        {
            _changeDue = Math.Round(_holdingChamber - (_itemsTotal-_cardPaymentReceived), 2);
            return Math.Round(_changeDue, 2);
        }//end method
        public void DispenseChange()
        {
            //WHILE CHANGE IS STILL DUE OWED: 
            while (_changeDue > 0)
            {
                //CLEAR ARRAY 
                for (int i=0;i<arrChangeGiven.Length;i++) {
                    arrChangeGiven[i] = 0;
                }//end for loop

                //LOOK THROUGH ARRAY 
                for (int i = 0; i < arrCurrencyCounts.Length; i++)
                {
                    //IF THERE ARE BILLS/COINS AVAILABLE IN THE TILL SLOT --- AND --- IF THE VALUE OF CHANGE DUE IS GREATER THAN THE BILL/COIN IN THAT TILL SLOT
                    while (arrCurrencyCounts[i] != 0 && _changeDue >= arrCurrencyValues[i])
                    {
                        //REMOVE ONE BILL/COIN FROM THE TILL SLOT
                        arrCurrencyCounts[i]--;
                        //ADD ONE BILL/COIN TO THE CHANGE POT
                        arrChangeGiven[i]++;
                        //SUBTRACT VALUE OF THE BILL/COIN PLACED IN CHANGE POT FROM CHANGE DUE
                        _changeDue -= arrCurrencyValues[i];
                        _changeDue = Math.Round(_changeDue, 2);
                    }//end while loop

                    //IF THE AMOUNT OF CHANGE DUE REACHES 0 BEFORE THE ENTIRE TILL SLOT HAS BEEN EVALUATED THEN LOOKING THROUGH TILL
                    if (_changeDue == 0)
                    {
                        break;
                    }//end if

                }//end while loop

            }//end while loop

        }//end method


        public void DetermineCashBack(double approvedAmount) {
            Thread.Sleep(1000);
            //IF THE AMOUNT CARD APPROVED IS EQUAL TO TOTAL AMOUNT OF ITEMS PLUS CASH BACK: 
            if (approvedAmount == _cardTotal) {

                ColorText($"\n\t\t\t---------------  TRANSACTION APPROVED FOR: {approvedAmount}  ---------------\n",ConsoleColor.DarkCyan);
           }//end if

            else 
            {
                if (_cashBack > 0) { 
                  ColorText("\n\t\t\t---------------  TRANSACTION FAILED TO PROCESS CASHBACK  ---------------\n ", ConsoleColor.Red);
                  _cashBack = 0;
                }//end if 


                if( approvedAmount > _itemsTotal ) {
                    approvedAmount = _itemsTotal;
                }//end if 

                ColorText($"\n\t\t\t---------------  TRANSACTION APPROVED FOR: {approvedAmount:c}  ---------------\n",ConsoleColor.DarkCyan);
            }//end else if 
            
            _cardTotal= approvedAmount;
            _cardPaymentReceived += _cardTotal;
            _changeDue+=_cashBack;
            _cashBack = 0;

        }//end method
        public void CardDeclined() {

            _cashBack = 0;
            _cardTotal = 0;
            _creditCardVendor = "N/A";
            
        
        }//end method
        public void RefundCreditCard() {

            _cardPaymentReceived -= _cardTotal;
            _cardTotal = 0;
            _creditCardVendor = "N/A";
        
        }//end method
        public double UpdateCardTotal()
        {

            _cardTotal = Math.Round((_itemsTotal + _cashBack)-_holdingChamber-_cardPaymentReceived,2);

            _balance = Math.Round(_cardTotal,2);
            return _cardTotal;

        }//end method
        public bool ValidateCreditCard(int[] cardNum)
        {
            #region TURN ARRAY INTO STRING 
            //TURNS INT ARRAY INTO STRING

                _creditCardNum = ""; 
            for (int i = 0; i < cardNum.Length; i++)
            {
                _creditCardNum += (char)cardNum[i];
            }//end for loop
            #endregion
            //DECLARE VARIABLES 
            int sum = 0;
            bool shouldApplyDouble = true;

            #region LUHN ALGR. CALCULATIONS
            //INDEX WILL BEGIN AT THE END OF THE STRING (SKIPPING CHECK SUM NUMBER) AND MOVE THROUGH THE STRING
            for (int index = _creditCardNum.Length - 2; index >= 0; index--)
            {
                //CURRENT DIGIT WILL HOLD THE VALUE AT THE INDEXER
                int currentDigit = (Int32)Char.GetNumericValue(_creditCardNum, index);

                //IF BOOL IS SET TO TRUE THE DIGIT WILL BE DOUBLED (THIS CONTROLS SKIPPING EVERY OTHER NUMBER TO DOUBLE)
                if (shouldApplyDouble)
                {
                    //IF THE CURRENT NUMBER IS GREATER THAN 4 - MEANING THAT THE DOUBLE OF THE NUMBER WOULD BE >= 10- 9 IS SUBTRACTED
                    if (currentDigit > 4)
                    {
                        sum += currentDigit * 2 - 9;
                    }//END IF

                    //IF NUMBER WHEN DOUBLED WILL NOT BE GREATER THAN 10 THE NUMBER IS JUST DOUBLED
                    else
                    {
                        sum += currentDigit * 2;
                    }//END ELSE
                }//END IF 

                //IF BOOL IS FALSE THE NUMBER IS ADDED WITHOUT MANIPULATION
                else
                {
                    sum += currentDigit;
                }//END ELSE

                //BOOL IS UPDATED TO STORE WHAT THE OPPOSITE OF WHAT THE CURRENT VALUE IS
                shouldApplyDouble = !shouldApplyDouble;
            }//END FOR LOOP 

            //CHECK DIGIT IS CREATED TO STORE THE NUMBER THAT MUST MATCH THE "CHECK SUM" NUMBER TO VERIFY THAT THE CARD NUM IS VALID 
            int checkDigit = 10 - (sum % 10);

            //RETURNS TRUE/FALSE BASED ON OUTCOME CHECKING CHECK SUM NUMBER AGAINST LUHN ALG. OUTCOME
            return Char.GetNumericValue(_creditCardNum[^1]) == checkDigit;
            #endregion
        }//end method
        public void SetCardVedor(char cardVendor) {

            if (cardVendor == '3')
            { _creditCardVendor = "AMERICAN-EXPRESS"; }

            if (cardVendor == '4') { _creditCardVendor = "VISA"; }
            if (cardVendor == '5') { _creditCardVendor = "MASTERCARD"; }
            if (cardVendor == '6') { _creditCardVendor = "DISCOVER"; }


        }//END METHOD


        public void DepositHoldingChamber()
        {
            _paymentVault += Math.Round(_holdingChamber, 2);
            _holdingChamber = 0;
        }//end method
        public void RefundHoldingChamber()
        {
            _holdingChamber = 0;
            _changeDue=_holdingChamber;
            _balance = _itemsTotal;
        }//end method
        public void ClearTransaction() {
            
            _changeDue = _holdingChamber;
            _itemsTotal = 0;
            _totalCardTransactions += _cardPaymentReceived;
            _cardPaymentReceived = 0;
            _balance = 0;
            _cardTotal = 0;
            _holdingChamber = 0;
            _creditCardVendor = "N/A";
            
        }//end method

        #endregion

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

    }//END KIOSK CLASS



}//end namespace



