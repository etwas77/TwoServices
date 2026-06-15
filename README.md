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
- service A:
    - offers REST API, that takes customerId as anput and validates/sets customer via backend service.
- swagger module or HTTP Request file (Backend.http) in projects for fast testing.

under development, look into develop branch
