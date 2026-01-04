Feature: Get Order By ID
    As a system user (admin or customer)
    I want to retrieve an order by its ID
    So that I can view order details

    Scenario: Admin obtains order by ID successfully
        Given I am an admin user
        And there is an order with ID "123e4567-e89b-12d3-a456-426614174000" and code "ORD-001"
        When I request to get the order with ID "123e4567-e89b-12d3-a456-426614174000"
        Then the order should be returned successfully
        And the order code should be "ORD-001"

    Scenario: Customer obtains their own order by ID successfully
        Given I am a customer with ID "123e4567-e89b-12d3-a456-426614174000"
        And there is an order with ID "223e4567-e89b-12d3-a456-426614174000" belonging to customer "123e4567-e89b-12d3-a456-426614174000"
        When I request to get the order with ID "223e4567-e89b-12d3-a456-426614174000"
        Then the order should be returned successfully
        And the order customer ID should be "123e4567-e89b-12d3-a456-426614174000"

    Scenario: Customer tries to get another customer's order and receives error
        Given I am a customer with ID "123e4567-e89b-12d3-a456-426614174000"
        And there is an order with ID "223e4567-e89b-12d3-a456-426614174000" belonging to customer "999e4567-e89b-12d3-a456-426614174000"
        When I request to get the order with ID "223e4567-e89b-12d3-a456-426614174000"
        Then I should receive an error that the order was not found

    Scenario: Order not found returns error
        Given I am an admin user
        And there is no order with ID "999e4567-e89b-12d3-a456-426614174000"
        When I request to get the order with ID "999e4567-e89b-12d3-a456-426614174000"
        Then I should receive an error that the order was not found
