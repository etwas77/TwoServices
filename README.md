project (.Net Web API and mongodb as database) will implement a simple microservice architecture, with 2 services communicating with each other.
- service backend:
    - offers REST API to manage customer object in mongodb instance
    - customer object has "isActive" field, which can be validated/set via second service
    - global error handling (not all objects covered, just example)
    - generic repository for typical CRUD operations
    - logger for error handling
    - startup validator for db and resilience policy
    - model validation
    - retry policy (using polly nuget)
    - REST API for user credentials, is used in conjunction with JWT authorisation via serviceA.
    - order and items objects to demonstrate RabbitMQ functionality
- service A:
    - functions as a gateway to backend
    - offers REST API, that takes customerId as anput and validates/sets customer via backend service
    - endpoint uses Backend REST API, no direct connection to db
    - no logging or error handling, to save effort. (can be done analog to backend)
    - JWT authentification in force. login and register endpoints implemented, they use backend to gain access to user credentials.
    - CustomerController is protected by [Authorize]
    - extra endpoint to fill in RabbitMQ queue.
- common lib Contracts, with DTO for both Backend and ServiceA
- RabbitMQ is used to swap objects between both microservices, retry policy, validation etc
- swagger module or HTTP Request file (Backend.http, ServiceA.http) in projects for fast testing.

under development, look into develop branch
