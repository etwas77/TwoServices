this project will implement a simple microservice architecture, with 2 services communicating with each other.
- service backend:
    - offers REST API to manage customer object in mongodb instance
    - customer object has "isActive" field, which can be validated/set via second service
- service A:
    - offers REST API, that takes customerId as anput and validates/sets customer via backend service.

under development, look into develop branch
