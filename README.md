
This ensures business logic in `Core` never depends on how data is stored or how the API is exposed — the database or frontend could be swapped without touching domain logic.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | ASP.NET Core MVC, Razor Views, vanilla JS |
| Backend API | ASP.NET Core Web API |
| Database | SQL Server, Entity Framework Core |
| Authentication | Keycloak (OpenID Connect + JWT Bearer) |
| Background Jobs | Hangfire |
| Observability | OpenTelemetry, Prometheus, Grafana |
| Containerization | Docker Compose (Keycloak, Redis, Prometheus, Loki, Tempo, Grafana) |
| Charting | Chart.js |

---

## Key Features

- **Full ticket lifecycle** with a proper state machine (Open → Assigned → In Progress → Resolved → Closed), including validated transitions and re-opening
- **SLA engine** — automatic breach detection via a Hangfire recurring job that checks resolution deadlines every few minutes
- **Escalation** — manual escalation by staff, or automatic escalation on SLA breach; escalating a ticket raises its priority and flags it for review
- **Role-based dashboards** — Employee, Technician, Team Lead, and Admin each see a purpose-built dashboard with real data
- **Role-based data visibility** — internal notes on tickets are hidden from Employees but visible to staff, enforced at both the API and UI layer
- **Audit trail** — every status change, assignment, and escalation is recorded in an immutable history table
- **Real analytics** — Reports dashboard with live SQL aggregation (resolution rate, weekly/monthly trends, satisfaction score)
- **Full observability** — three Grafana dashboards: Executive (SQL-based business metrics), Technician (workload), and Operations (Prometheus-based system health)

---

## Project Structure