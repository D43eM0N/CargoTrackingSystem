# Cargo Tracking System

- **Cargo Tracking System** API built using Clean Architecture principles and .NET 10. It covers creating and updating shipments, courier assignments, real-time status tracking.

---

## Technologies Used

*   .NET 10.0 
*   PostgreSQL 
*   Entity Framework Core
*   MediatR (CQRS)
*   FluentValidation 
*   Scalar.AspNetCore (GUI)
*   Docker & Docker Compose (Containerization)

---

## Running the Project

### Prerequisites
* **Git**: To clone the project from GitHub.
*   Ensure **Docker Desktop** is installed and running on your machine.

### Step-by-Step Execution
1.  Open your terminal/command prompt and navigate to the project's root directory (where the `.sln` and `docker-compose.yml` files are located).
2.  Run the following command to build and launch all containers: `docker compose up --build`

---

## Scalar GUI

Once the application is up and running, you can explore and test the endpoints directly from your browser:

*   **Scalar Interactive UI:** [http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)

---

##  Usage Notes

*   **Toggling Courier Active Status:** To change a courier's `IsActive` status, you do not need to provide any JSON body. Simply send an empty request to the toggle endpoint; it will automatically flip the status (from Active to Passive, or Passive to Active).
*   **Updating Shipments:** When using `UpdateShipment`, you don't need equalize entire JSON body with exist Shipment. You can only specify variables you want to update and leave rest of them null.
*   **Background Status Progression:** Once a shipment is manually moved to the `"In Transit"` status, the background worker will automatically change it first to `"Out for Delivery"` and then to `"Delivered"` in 1-minute intervals.
*   **Automated Courier Unassignment:** If no active courier exists for a postal code, the shipment's courier is automatically set to null. A courier will be reassigned if the postal code is updated to a matching one, or if a new courier is added for that postal code.
*   **Filtering:** Shipments can be filtered by Status, Create Date, Courier ID and Tracking Number with Query Parameters.
*   **Time Filtering:** CreatedAt value in shipment only works for days. If day not specified or hour will specify it won't work. It will show the shipments that made in that day. Example Usage `CreatedAt : 2026-06-18`. 
*   **Page:** Page can be changed via writing Query Parameter, Page and It's number. Application will create new page and write the 4th shipment to the new page automatically. Page size can also can be changed from query parameters temporarily.

## Stopping the Project

To stop the running application safely, press `Ctrl + C` in your terminal, or run the following command to completely down and clean up the containers:

```bash
docker compose down
```
