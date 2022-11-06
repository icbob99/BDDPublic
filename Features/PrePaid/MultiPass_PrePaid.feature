@PrePaid @MultiPass
Feature: MultiPass_PrePaid

The feature contains base prepaid operations with 3rd party card (MultiPass) 

Background:
	Given i have a loyalty program
	And the site is configured for multipass
	And I'm the owner of Card "154340044563"
	And I'm at Site 2

@pay-money
Scenario: The card payment should decrease the card balance
	When I save on going balance of the card
	And I Pay 1 shekel
	Then the difference between current balance of the card and the saved one is -1.00
	
	# Just to be sure that balance of the card will not change because of test
	When I Pay -1.00 shekel
	Then the difference between current balance of the card and the saved one is 0.00

@cancel-pay-money
Scenario: Cancel of the last payment , should not change the card balance
	When I save on going balance of the card
	And I Pay 1.17 shekel
	And I cancel last payment
	Then the difference between current balance of the card and the saved one is 0.00

@cancel-pay-money
Scenario: Cancel of the last payment twice, should not change the card balance
	When I save on going balance of the card
	And I Pay 1.17 shekel
	And I cancel last payment
	And I cancel last payment

	Then the difference between current balance of the card and the saved one is 0.00

@cancel-pay-money
Scenario: Cancel not existed payment, should throw error
	When I cancel payment by reference "6345267"
	Then I Get An Exception of type "invoiceNotFound"

@cancel-load-money
Scenario: Cancel not existed load money, should throw error
	When I cancel load money by reference "6023456"
	Then I Get An Exception of type "DynamicException" with message 'כרטיס לא תקין או חסום לשמוש'

#@ignore
@load-money
Scenario: Load money by default should be multiples of 50
	Given I reset load from previous tests

	#When I clear the card history
	When I save on going balance of the card
	And I load the card with 1.00$ And Exception Collect Is "on"
	Then I Get An Exception of type "DynamicException" with message 'סכום טעינה נדרש להיות בכפולות של: 50'

@load-money
Scenario: Loaded money should increase the card balance
	Given I reset load from previous tests

	When I save on going balance of the card
	And I load the card with 50.00$

	Then the difference between current balance of the card and the saved one is 50.00

	# Just to be sure that balance of the card will not change because of the test
	When I Pay 50.00 shekel
	Then the difference between current balance of the card and the saved one is 0.00

@cancel-load-money
Scenario: Cancel Loaded money should not change balance
	Given I reset load from previous tests

	When I save on going balance of the card
	And I load the card with 50.00$

	Then the difference between current balance of the card and the saved one is 50.00

	When I cancel last load money
	Then the difference between current balance of the card and the saved one is 0.00
	#second cancel
	When I cancel last load money
	Then the difference between current balance of the card and the saved one is 0.00


@get-balance
Scenario: On a card load or payment by the card the card balance should be calculated correctly
	Given I reset load from previous tests

	When I save on going balance of the card
	And I load the card with 50.00$
	Then the balance calculation is correct

	When I Pay 50.00 shekel
	Then the balance calculation is correct

@ignore
@clean-scenario @run-after-debug
Scenario: Clean over amount that was added during debuging
When I save on going balance of the card
When I Pay 1000.00 shekel
Then the difference between current balance of the card and the saved one is -1000.00