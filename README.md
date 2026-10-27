<h2>🎅Secret Santa</h2>
<p>This is a web application build around a Secret Santa theme event system created with <strong>ASP.NET Core MVC version 9.0.17!</strong> You can organize events, manage participants and randomly assign gift recipients. </p>

<h2>📖About the project</h2>
<p>Secret Santa is a web application designed to simplify the organization of gift exchanges. It let's you create events, invite participants, manage wishlist information and generate randomized assignments while respecting partner restrictions and previous rounds, so at the end of the day nobody get's mad when the draw sequence begins.</p>

<p>Participants can access the application through personal invitation links to view their assigned recipient and manage their own wishlist and also viewing their recipient wishlist. The reason behind this simple personal invitation links system is to let the recipeints joins the events as guests - there is no need to register to participate in the event!</p>

<h2>✨Features✨</h2>
<h4>📋Event Management</h4>
<ul>
  <li>Create, view, edit and delte Events.</li>
  <li>Configure event details, including the date, budget and minimum number of participants.</li>
  <li>Restrict event management to the Organizer who created the event.</li>
</ul>

<h4>🫂 Participant Management</h4>
<ul>
  <li>Manage participants within the event.</li>
  <li>Configure partner restrictions to prevent certain participants(couples) from drawing each other.</li>
  <li>Generate personal invitation links for the participants.</li>
  <li>Revoke existing invitation links and generate replacements.</li>
</ul>

<h4>🎁Secret Santa Draw</h4>
<ul>
  <li>Randomly assign a recipient to each participant.</li>
  <li>Prevent participants from drawing themselves.</li>
  <li>Respect partner restrictions.</li>
  <li>Prevent participants from receiving the same recipient in previous rounds.</li>
  <li>Store assignments by event and round.</li>
</ul>

<h4>✍️Wishlists</h4>
<ul>
  <li>Create, edit and delete personal wishlist items.</li>
  <li>Add product titles, descriptions and product URLs.</li>
  <li>View the wishlist of the participant assigned to you.</li>
  <li>Restrict wishlist access according to the participant identity and draw assignments</li>
</ul>

<h4>🔐Authentication and Security</h4>
<ul>
  <li>Organizer authentication using ASP.NET Core Identity.</li>
  <li>Separate cookie authentication for participants.</li>
  <li>Time-limited invitation links.</li>
  <li>Revocation and expiration checks for participant invitations.</li>
  <li>Server-side authorization checks for event and wishlist access.</li>
  <li>Antiforgery validation on protected form submissions.</li>
  <li>Database constraints to help maintain data integrity.</li>
</ul>

<h2>🧰Technologies Used</h2>
<ul>
  <li>VS Code v1.140.0.</li>
  <li>HTML and CSS.</li>
  <li>Bootstrap 5</li>
  <li>DBeaver v26.0.4.</li>
  <li>Microsoft Entity Framework Core v9.0.17</li>
  <li>Microsoft SQL Server</li>
  <li>ASP.NET Core MVC v9.0.17</li>
  <li>ASP.NET Core Identity v9.0.17</li>
</ul>

<h2>Getting Started</h2>
<h3><strong>Prequisites</strong></h3>
<p>Before running the application, make sure you have:</p>
<ul>
  <li>The .NET SDK version 9.0</li>
  <li>Microsoft SQL Server</li>
  <li>An IDE or code editor, such as Visual Studio, VS Code or JetBrains Rider</li>
  <li>Git(optional, for cloning the repository)</li>
</ul>
<h3><strong>1. Clone the repository</strong></h3>

```bash
git clone https://github.com/vnvladimirov/SecretSantaWIP
cd SecretSantaWIP
```

<h3><strong>2. Configure the database connection</strong></h3>
<p>Update the DefaultConnection connection string in appsettings.json to point to your SQL Server instance.</p>

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SecretSanta;User Id=sa;Password=123Noize123!;TrustServerCertificate=True;"
  }
```
<p>The database and the web application is developed in <strong>DBeaver</strong> and <strong>VS Code</strong> in <strong>Linux Mint 24.04 Cinnamon edition</strong> so the authentication for the SQL Server is <strong>SQL Server Authentication!</strong></p>

<h3>3. Restore dependancies</h3>

```bash
dotnet restore
```

<h3>4. Run the application!</h3>

```bash
dotnet run
```

<h3>5. User authorization</h3>
<p>Currently its not released yet! So in the Database there is one profile created completely fictional for testing the application!</p>
<h4><strong>Username: michael.scott@gmail.com</strong></h4>
<h4><strong>Password: 123Michael123!</strong></h4>

<h2>📁 Project Structure</h2>
<p>The project follows the <strong>ASP.NET Core MVC</strong> architecture</p>

<h3>Project Hierarchy</h3>
<img src="screenshots/project-hierarchy.png" alt="Home page" width="400">
<h3>Controllers</h3>
<img src="screenshots/project-controllers.png" alt="controllers" width="400">
<h3>Data</h3>
<img src="screenshots/project-data.png" alt="Home page" width="400">
<h3>Project Models</h3>
<img src="screenshots/project-models.png" alt="Home page" width="400">
<h3>Views</h3>
<img src="screenshots/project-views.png" alt="Home page" width="400">

<h2>🔄 How it Works</h2>
<p>Here is the <strong>step by step</strong> guide for the Organizer</p>

<ol type="1">
  <li>The Organizer registers or signs in.</li>
  <p align="center">
  <img src="screenshots/register.png" alt="register" width="45%">
  <img src="screenshots/login.png" alt="login" width="40%">
    <li>The Organizer creates new event from the button "Create New Event".</li>
    <p align="center">
  <img src="screenshots/create-event.png" alt="create-event" width="800">
      <li>The Organizer fill the form by giving the event Name, Description, Budget, Event Date, Minimum Participants and his Name and Email.</li>
    <p align="center">
  <img src="screenshots/create-newevent.png" alt="create-newevent" width="800">
      <li>The Organizer can add new participants into the event.</li>
    <p align="center">
  <img src="screenshots/event-page.png" alt="event-page" width="800">
  <img src="screenshots/event-add.png" alt="event-add" width="800">
      <li>The Organizer can edit participants in the event and also assign their partners(couples).</li>
      <li>The Organizer can delete participants from the event.</li>
      <li>The Organizer can draw the names by clicking "Draw names" button.</li>
      <li>After the drawing the Organizer can generate unique invitation link and send it to the participants!</li>
      <li>Participants use their links to enter the application and manage their wishlists.</li>
      <li>Each participant can view their assigned recipient and that recipient's wishlist.</li>
</ol>

<h2>🔐Security Notes</h2>
<ul>
  <li>Participant invitation tokens are generated using cryptographically secure random bytes.</li>
  <li>Invitation tokens are stored as hashes rather than in plaintext.</li>
  <li>Participant sessions use a separate authentication cookie.</li>
  <li>Invitation expiration and revocation are checked during participant authentication.</li>
  <li>Server-side checks restrict access to event data and wishlist items.</li>
</ul>

<h2>🪳Bugs and Future features</h2>
<p>Currently there is still to be added new features also probably some bugs along the proccess! If you encounter bugs feel free to give feedback!</p>
<p>This project is entirely open-sourced and there are no License, so feel free to use it, modify it and have fun with it!</p>



