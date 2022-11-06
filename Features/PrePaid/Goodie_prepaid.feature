@PrePaid @Goodie
Feature: Goodie_prepaid


The feature contains base prepaid operations with 3rd party card (Goodie) 

Background:
	Given i have a loyalty program
	And the site is configured for Goodie
	And I'm the owner of Card "150191007"
	And I'm at Site 2
	And I topup Goodie amount

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

@pay-money
Scenario: I get exception when the payment exceed its limit
	Given I save on going balance of the card

	When I Pay more 1.00$ than the card balance
	Then I Get An Exception of type "DynamicException"


