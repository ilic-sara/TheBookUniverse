# TheBookUniverse

The Book Universe is a full-stack online bookstore built with ASP.NET Core MVC and MongoDB, designed to offer a seamless shopping experience with robust administrative tools. The platform supports user registration, login, role-based access control, and includes personalized features like favorites and order history.

The application follows clean architecture principles — separating concerns across data access, business logic, and presentation layers — and uses MongoDB replica sets with multi-document transactions to ensure data consistency, especially during order processing and admin updates. The admin panel provides full CRUD capabilities for managing books, authors, promotional banners, filter options, and customer orders.

Authentication and authorization are handled via ASP.NET Identity, supporting secure login, role management, and cookie-based sessions.

# Features

- Browse books and authors with various filters

- Add books to cart, place orders, and manage favorites

- Admin dashboard for managing books, authors, orders, and UI banners

- Full user authentication and role-based authorization

- MongoDB replica sets and transactions for consistent order management

- Responsive UI styled with Bootstrap

# Technologies

C#, ASP.NET Core MVC, MongoDB (with replica sets), Razor Views, JavaScript, HTML, CSS, Bootstrap
