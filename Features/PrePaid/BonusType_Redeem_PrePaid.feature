@PrePaidBonus
Feature: BonusType_Redeem_PrePaid

The feature tests all related scenarios for loading money on a rechargeable card. 
When PrePaid Bonus Type is OnRedeem


Background:
	Given I have a loyalty program with 5 new pre paid cards
	And I'm the owner of Card "1000"
	And I'm at Site 1
	And I configure the account to give bonus 'OnRedeem'

Scenario: Change of Pre Paid Bonus Type from 'OnRedeem' to 'OnLoad' is forbidden after 1st load on card
	Given I configure the Pre Paid Series to have Bonus of 10%
	And I clear the card history

	When I configure the account to give bonus 'OnLoad'
	Then Pre Paid Bonus Type is 'OnLoad'

	When I configure the account to give bonus 'OnRedeem'
	Then Pre Paid Bonus Type is 'OnRedeem'

	When I load the card with 90$
	Then the balance of the card is 99$

	When I configure the account to give bonus 'OnLoad'
	Then I Get An Exception of type "changePrepaidBonusTypeForbidden"

@bonusdiscount_calculation
Scenario: 'OnRedeem' Bonus and constant bonus percentage
	Given I clear the card history

	When I do the following prepaid actions with on Redeem Bonus
		| Action | Bonus | Site | Amount | Balance | Paid Amount | Bonus Discount |
		| load   | 10    | 1    | 90$    | 99$     |             |                |
		| pay    |       | 1    | 15$    | 84$     | 13.64       | 1.36$          |
		| load   | 10    | 1    | 30$    | 117$    |             |                |
		| pay    |       | 1    | 100$   | 17$     | 90.91$      | 9.09$          |

	And I Pay 20$ And Exception Collect Is "on"
	Then I Get An Exception of type "CustomInsufficientFunds"
	
@bonusdiscount_calculation
Scenario: 'OnRedeem' Bonus use only 1st load
	Given I clear the card history

	When I do the following prepaid actions with on Redeem Bonus
		| Action | Bonus | Site | Amount | Balance | Paid Amount | Bonus Discount |
		| load   | 10    | 1    | 90$    | 99$     |             |                |
		| pay    |       | 1    | 15$    | 84$     | 13.64$      | 1.36$          |
		| load   | 20    | 1    | 30$    | 120$    |             |                |
		| pay    |       | 1    | 20$    | 100$    | 18.18$      | 1.82$          |
		| load   | 0     | 1    | 33$    | 133$    |             |                |
		| pay    |       | 1    | 21$    | 112$    | 19.09$      | 1.91$          |

@bonusdiscount_calculation
Scenario: 'OnRedeem' Bonus use 2nd load money after 1st was finished
	Given I clear the card history

	When I do the following prepaid actions with on Redeem Bonus
		| Action | Bonus | Site | Amount | Balance | Paid Amount | Bonus Discount |
		| load   | 10    | 1    | 90$    | 99$     |             |                |
		| pay    |       | 1    | 15$    | 84$     | 13.64$      | 1.36$          |
		| load   | 20    | 1    | 30$    | 120$    |             |                |
		| pay    |       | 1    | 84$    | 36$     | 76.36$      | 7.64$          |
		| pay    |       | 1    | 24$    | 12$     | 20$         | 4$             |

@bonusdiscount_calculation
Scenario: 'OnRedeem' Bonus use mixed (from 1st load and from 2nd)
	Given I clear the card history

	When I do the following prepaid actions with on Redeem Bonus
		| Action | Bonus | Site | Amount | Balance | Paid Amount | Bonus Discount |
		| load   | 10    | 1    | 90$    | 99$     |             |                |
		| pay    |       | 1    | 90$    | 9$      | 81.82$      | 8.18$          |
		| load   | 20    | 1    | 30$    | 45$     |             |                |
		| pay    |       | 1    | 10$    | 35$     | 9.01$       | 0.99$          |
		| pay    |       | 1    | 24$    | 11$     | 20$         | 4$             |


@bonusdiscount_calculation
Scenario: 'OnRedeem' Bonus use 1st load, 2nd & 3rd
	Given I clear the card history

	When I do the following prepaid actions with on Redeem Bonus
		| Action | Bonus | Site | Amount | Balance | Paid Amount | Bonus Discount |
		| load   | 10    | 1    | 90$    | 99$     |             |                |
		| pay    |       | 1    | 15$    | 84$     | 13.64$      | 1.36$          |
		| pay    |       | 1    | 84$    | 0$      | 76.36$      | 7.64$          |
		| load   | 20    | 1    | 30$    | 36$     |             |                |
		| pay    |       | 1    | 20$    | 16$     | 16.67$      | 3.33$          |
		| load   | 0     | 1    | 33$    | 49$     |             |                |
		| pay    |       | 1    | 21$    | 28$     | 18.33$      | 2.67$          |
		| pay    |       | 1    | 28$    | 0$      | 28$         | 0$             |