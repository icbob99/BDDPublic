@PrePaidBonus
Feature: BonusType_Load_PrePaid

The feature tests all related scenarios for loading money on a rechargeable card. 
When PrePaid Bonus Type is OnLoad

Background:
	Given I have a loyalty program with 5 new pre paid cards
	And I'm the owner of Card "1000"
	And I'm at Site 1
	#And I configure the account to give bonus 'OnLoad'
	

Scenario: Change of Pre Paid Bonus Type from 'OnLoad' to 'OnRedeem' is forbidden after 1st load on card
	Given I configure the Pre Paid Series to have Bonus of 10%
	And I clear the card history

	When I configure the account to give bonus 'OnRedeem'
	Then Pre Paid Bonus Type is 'OnRedeem'

	When I configure the account to give bonus 'OnLoad'
	Then Pre Paid Bonus Type is 'OnLoad'

	When I load the card with 90$
	Then the balance of the card is 99$

	When I configure the account to give bonus 'OnRedeem'
	Then I Get An Exception of type "changePrepaidBonusTypeForbidden"

Scenario: 'OnLoad' Bonus and constant bonus percentage
	Given I clear the card history
	
	When I do the following prepaid actions with Bonus
		| Action | Bonus | Site | Amount | Balance |
		| load   | 10    | 1    | 90$    | 99$     |
		| pay    |       | 1    | 15$    | 84$     |
		| load   | 10    | 1    | 30$    | 117$    |
		| pay    |       | 1    | 100$   | 17$     |

	And I Pay 20$ And Exception Collect Is "on"
	Then I Get An Exception of type "CustomInsufficientFunds"

Scenario: 'OnLoad' Bonus and change bonuns percentage
	Given I clear the card history

	When I do the following prepaid actions with Bonus
		| Action | Bonus | Site | Amount | Balance |
		| load   | 10    | 1    | 90$    | 99$     |
		| pay    |       | 1    | 15$    | 84$     |
		| load   | 20    | 1    | 30$    | 120$    |
		| pay    |       | 1    | 20$    | 100$    |
		| load   | 0     | 1    | 33$    | 133$    |
		| pay    |       | 1    | 10$    | 123$    |

