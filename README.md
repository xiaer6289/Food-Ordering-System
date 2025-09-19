# Food Ordering System

## 📌Overview 

This project is developed to streamline restaurant management and food ordering processes. This system provide a strcutured workflow for admin and staff only, with clear seperation of roles and access rights
The system requires initial setup by the first user, who creates the restaurant profile (company name, address, logo, and email). Once the restaurant is registered, the system moves to the login page where users can log in with the validated admin email.

## 👤 User Roles & Features
### Admin

* Authentication: Login with validated email (includes password reset and CAPTCHA for security).

* Restaurant Setup: Define company details (name, address, logo, email).

* Management Features (CRUD):

    * Seats

    * Food Categories

    * Foods

    * Staff accounts

    * Admin accounts

* Restocking Management: Monitor and manage ingredient stock.

* Order History: View and track all customer orders.

* Data Handling: Supports AJAX-based sorting, paging, and searching for ingredients, admins, staff, foods, and categories.

### Staff

* Food Ordering: Handle customer food orders.

* Food CRUD: Create, update, or remove food items.

* Category CRUD: Manage food categories.

### 🔐 API Usage

* The project was developed with API integration in mind.

* A demo API was provided during development and testing.

* Important: For security reasons, API keys and secrets are not included in this repository.

* When deploying this system, users must configure and use their own API credentials.

### ⚙️ Technical Features

* Login Security: Password reset functionality and CAPTCHA integration.

* AJAX Integration: Efficient sorting, paging, and searching across multiple management modules.

* Modular Structure: Flexible CRUD operations tailored to user roles.
