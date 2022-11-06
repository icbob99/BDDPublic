@pre_paid
Feature: PrePaid
/*
TODO : 
* create bonus logic
* make sure you can run test in STG - https://keestalkstech.com/2019/01/setup-multiple-setting-files-with-a-net-console-application/
*/
	
Background:
	Given I have a loyalty program with 5 new pre paid cards

Scenario: load card Basic
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	
	When I clear the card history
	And I load the card with 100.0$

	Then the balance of the card is 100.0$

Scenario: cant load if CanLoadMoney is off
	Given I'm the owner of Card "1000"
	And PrePaid Series is Configured to allow money load is "off"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 100.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "loadMoneyFailed"
	And In order to let other tests run, I reset the series to allow money load "on"


Scenario: Enforce minimum loading amount
	Given I'm the owner of Card "1000"
	And PrePaid Series is Configured to have minimum load of 10.0$
	And I'm at Site 1

	When I clear the card history
	And I load the card with 9$ And Exception Collect Is "on"
	Then I Get An Exception of type "MinimumLoadingAmountValidationFailed"
	And the balance of the card is 0$

	When I load the card with 11$
	Then the balance of the card is 11.0$
	


Scenario: Enforce maximum loading amount in a single load
	Given I'm the owner of Card "1000"
	And PrePaid Series is Configured to have a single charge limit of 10.0$
	And I'm at Site 1
	
	When I clear the card history
	And I load the card with 19.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "PrePaidSingleChargeLimitation"
	And the balance of the card is 0$

	When I load the card with 9$
	Then the balance of the card is 9.0$
	


Scenario: Enforce card balance maximum loading amount limit
	Given I'm the owner of Card "1000"
	And PrePaid Series is Configured to have a card balance limit of 10.0$
	And I'm at Site 1
	
	When I clear the card history
	And I load the card with 19.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "PrePaidBalanceLimitation"
	And the balance of the card is 0$

	When I load the card with 9$
	Then the balance of the card is 9.0$

	When I clear the card history
	And I load the card with 4$
	And I load the card with 10.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "PrePaidBalanceLimitation"
	And the balance of the card is 4.0$

	


Scenario: One can load money on a card more than once Recharge card only if the related flag is on.
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	
	When I clear the card history
	And I set PrePaid Series to be not rechargeable
	And I load the card with 19$
	And I load the card with 19.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "CardIsBlockedForLoad"
	And the balance of the card is 19$

	When I clear the card history
	And I set PrePaid Series to be rechargeable
	And I load the card with 19$
	And I load the card with 19$
	Then the balance of the card is 38.0$

	When I set PrePaid Series to be not rechargeable
	And I load the card with 1.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "CardIsBlockedForLoad"
	


Scenario: Cancel load of a Gift Card
	Given I'm the owner of Card "1000"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 19$
	And I cancel last load money
	Then the balance of the card is 0$


Scenario: After partial usage of a gift card, You cannot cancel initial load money operation.
	Given I'm the owner of Card "1000"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 19$
	And I Pay 10.0$
	And I cancel last load money

	Then I Get An Exception of type "CancelPayCardUsedAlready"
	And the balance of the card is 9$


Scenario: cancel load repeatedly and dont get an error
	Given I'm the owner of Card "1000"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 19$
	And I cancel last load money
	And I cancel last load money

	Then the balance of the card is 0$



Scenario: One cannot cancel a load money that never happend
	Given I'm the owner of Card "1000"
	And I'm at Site 1

	When I clear the card history
	And I cancel load money by reference "6023456"

	Then I Get An Exception of type "BadReferenceIdForCancelPayCardLoad"


Scenario: One cannot pay with empty card.
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	
	When I clear the card history
	And I Pay 10.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "EmptyBalance"

	When I load the card with 100.0$
	And I Pay 100.0$
	And I Pay 10.0$ And Exception Collect Is "on"
	Then I Get An Exception of type "EmptyBalance"


Scenario: One cannot pay more than the cards balance
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	
	When I clear the card history
	And I load the card with 20$
	And I Pay 30.0$ And Exception Collect Is "on"
	Then I Get an error with balance of 20.00$ and card rechargable is "on"

	When I set PrePaid Series to be not rechargeable
	And I Pay 30.0$ And Exception Collect Is "on"
	Then I Get an error with balance of 20.00$ and card rechargable is "off"
	

	

Scenario: Can do partial payment if it is not allowed.
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	And I set PrePaid Series to forbid partial payment

	When I clear the card history
	And I load the card with 20$ And Exception Collect Is "off"
	And I Pay 10$ And Exception Collect Is "on"
	Then I Get An Exception of type "PartialRedemptionsIsNotPossible"
	And the balance of the card is 20$

	When I set PrePaid Series to allow partial payment
	And I Pay 10$
	Then the balance of the card is 10$
	


Scenario: Payment with rounding balance off
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "off"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 100$
	And I Pay 33.33$
	Then the balance of the card is 66.67$


Scenario: Payment with rounding balance on
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "on"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 100$
	And I Pay 30.00$
	Then the balance of the card is 70.00$



Scenario: Cancel payment of prepaid card

	Given I'm the owner of Card "1000"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 50$
	And I Pay 20$
	And I cancel last payment

	Then the balance of the card is 50$
	#second cancel
	When I cancel last payment
	Then I Get An Exception of type "loyaltyReferenceInvalid"
	#cancel imaginary refernce
	When I cancel payment by reference "6345267"
	Then I Get An Exception of type "loyaltyReferenceInvalid"


#****************************************************************************
# Reports testing : start
#****************************************************************************
Scenario: using prepaid on multiple sites
	Given I'm the owner of Card "1000"
	
	When I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 10$    | 10$     |
		| pay    | 2    | 5$     | 5$      |
	Then i am running the pre paid activity report and all actions are listed

Scenario: redeem money from several loads on same site
	Given I'm the owner of Card "1000"
	
	When I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 10$    | 10$     |
		| load   | 1    | 10$    | 20$     |
		| load   | 1    | 10$    | 30$     |
		| load   | 1    | 10$    | 40$     |
		| pay    | 1    | 33$    | 7$      |

	Then i am running the pre paid activity report and all actions are listed


Scenario: redeem money from several loads on different sites
	Given I'm the owner of Card "1000"
	When I clear the card history

	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 10$    | 10$     |
		| load   | 1    | 10$    | 20$     |
		| load   | 2    | 10$    | 30$     |
		| load   | 1    | 10$    | 40$     |
		| pay    | 2    | 33$    | 7$      |

	Then i am running the pre paid activity report and all actions are listed

@ignore @round_fail
Scenario: activity report with decimals when rounding is on
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "on"
	
	When I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 10.3$  | 10.3$   |
		| load   | 1    | 10.42$ | 20.72$  |
		| pay    | 1    | 5.25$  | 15.47$  |

	Then i am running the pre paid activity report and all actions are listed
	And the balance of the card is 15.00$
	

Scenario: activity report with decimals when rounding is off
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "off"
	
	When I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 10.3$  | 10.3$   |
		| load   | 1    | 10.42$ | 20.72$  |
		| pay    | 1    | 5.25$  | 15.47$  |

	Then i am running the pre paid activity report and all actions are listed
	And the balance of the card is 15.47$


#*********************************************************
# reports testing : finish
#**********************************************************


#*********************************************************
# CVV - load   : start
#**********************************************************
	
Scenario: load card using CVV from PAD
	Given I'm the owner of Card "1000"
	And I Dont have the CVV number
	And I'm at Site 1
	And transaction channel is "pad"
	And PrePaid Series is Configured to require CVV is "on"
	
	When I clear the card history
	And I load the card with 100.0$
	Then the balance of the card is 100.0$

Scenario: load card using CVV from Office
	Given I'm the owner of Card "1000"
	And I'm at Site 1
	And transaction channel is "office"
	And PrePaid Series is Configured to require CVV is "on"
	
	#no cvv
	When I clear the card history
	And I load the card with 100.0$ And Exception Collect Is "on"
	But I dont Type the Cvv Code for payment
	Then I Get An Exception of type "cvvRequired"

	#with Cvv
	When I Type the correct Cvv code for payment
	And I load the card with 100.0$
	Then the balance of the card is 100.0$
	

#*********************************************************
# CVV - load   : finish
#**********************************************************


#*********************************************************
# CVV - payment   : start
#**********************************************************
		

Scenario: pay with card using CVV from managed devices
	Given I'm the owner of Card "1000"
	And I have the correct CVV code
	And I'm at Site 1
	And transaction channel is <source>
	And PrePaid Series is Configured to require CVV is "on"
	
	When I clear the card history
	And I load the card with 100.0$
	#no cvv
	And I dont Type the Cvv Code for payment
	And I Pay 20$
	Then the balance of the card is 80.0$
	
	#correct cvv
	When I Type the correct Cvv code for payment
	And I Pay 20$
	Then the balance of the card is 60.0$
	
	#wrong cvv
	When I Type the wrong Cvv Code for payment
	And I Pay 20$
	Then the balance of the card is 40.0$
Examples:
	| source        |
	| "call-center" |
	| "pad"         |
	

Scenario: pay with card using CVV from customer facing channels
	Given I'm the owner of Card "1000"
	And I have the correct CVV code
	And I'm at Site 1
	And transaction channel is <source>
	And PrePaid Series is Configured to require CVV is "on"
	
	When I clear the card history
	And I load the card with 100.0$
	
	#no cvv
	And I dont Type the Cvv Code for payment
	And I Pay 20$ And Exception Collect Is "on"
	Then I Get An Exception of type "cvvRequired"
	
	#wrong cvv
	When I Type the wrong Cvv Code for payment
	And I Pay 20$ And Exception Collect Is "on"
	Then I Get An Exception of type "incorrectCvv"

	#correct cvv
	When I Type the correct Cvv code for payment
	And I Pay 20$
	Then the balance of the card is 80.0$
	
Examples:
	| source        |
	| "office"      |
	| "tabit-order" |
	| "tabit-app"   |
	
#*********************************************************
# CVV - payment   : finish
#**********************************************************



#*********************************************************
# rounding   : start
#**********************************************************

@ignore
Scenario: rounding balance off with decmial values
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "off"
	And I'm at Site 1
	When I clear the card history
	And I load the card with 99.8$
	Then the balance of the card is 99.8$
@ignore
Scenario: rounding balance on
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "off"
	And I'm at Site 1
	When I clear the card history
	And I load the card with 99.8$
	Then the balance of the card is 99$
@ignore
Scenario: rounding balance near zero
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "on"
	And I'm at Site 1
	When I clear the card history
	And I load the card with 0.8$
	Then the balance of the card is 0$
@ignore
Scenario: rounding balance get to a value of over 1
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to "on"
	And I'm at Site 1
	When I clear the card history
	And I load the card with 0.2$
	Then the balance of the card is 0$
	
	When I load the card with 0.9$
	Then the balance of the card is 1$
@ignore
Scenario: loading decimals but balance is always correct
	Given I'm the owner of Card "1000"
	And Account Configuration of PrePaid rounding is set to <Round>
	And I'm at Site 1

	#decimal
	When I clear the card history
	And I load the card with 1.5$
	And I load the card with 1.5$

	Then the balance of the card is 3$


Examples:
	| Round |
	| "on"  |
	| "off" |


#*********************************************************
# rounding   : finish
#**********************************************************

Scenario: Blocked card cannot pay and cannot load money
	Given I'm the owner of Card "1001"
	And I'm at Site 1
	
	When I clear the card history
	And I load the card with 10$
	And I block my card
	And I Pay 1$ And Exception Collect Is "on"
	Then I Get An Exception of type "CardIsNotActive"

	When I load the card with 10$ And Exception Collect Is "on"
	Then I Get An Exception of type "CardIsNotActive"

	When I get customer
	Then I Get An Exception of type "CardNotActive"

	When I unblock my card
	And I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 1$     | 1$      |
		| pay    | 1    | 1$     | 0$      |
	Then i am running the pre paid activity report and all actions are listed

Scenario: Blocked Account prevents pay or load money for cards
	Given I'm the owner of Card "1001"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 10$
	And I block the Account
	And I Pay 1$ And Exception Collect Is "on"
	Then I Get An Exception of type "AccountNotActive"

	When I load the card with 10$ And Exception Collect Is "on"
	Then I Get An Exception of type "AccountNotActive"

	When I get customer
	Then I Get An Exception of type "AccountNotActive"

	When I unblock the Account
	And I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 1$     | 1$      |
		| pay    | 1    | 1$     | 0$      |
	Then i am running the pre paid activity report and all actions are listed

@ignore
Scenario: Blocked Business prevents pay or load money for cards
	Given I'm the owner of Card "1001"
	And I'm at Site 1

	When I clear the card history
	And I load the card with 10$
	And I block the Site
	And I Pay 1$ And Exception Collect Is "on"
	#Then I Get An Exception of type "Boris"

	When I load the card with 10$ And Exception Collect Is "on"
	#Then I Get An Exception of type "Denis"

	When I get customer
	#Then I Get An Exception of type "Almog"

	When I unblock the Site
	And I clear the card history
	And I do the following prepaid actions
		| Action | Site | Amount | Balance |
		| load   | 1    | 1$     | 1$      |
		| pay    | 1    | 1$     | 0$      |
	Then i am running the pre paid activity report and all actions are listed