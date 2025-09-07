using System;
using System.Linq;
using System.Threading.Tasks;
using IssueTrackingAPI.Context;
using IssueTrackingAPI.DTO.TicketDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IssueTrackingTest
{
    public class UnitTest1
    {
        private AppDBContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDBContext(options);
        }

        //
        // Verifies: when changing Status from Open -> Closed, the repository
        // sets ResolvedAt and ResolutionTime (KPI fields) accordingly.
        [Test]
        public async Task TicketRepo_UpdateTicket_WhenClosing_SetsResolvedFields()
        {
            using var db = CreateDb();
            var repo = new TicketRepo(db);

            var t = new TicketModel
            {
                Title = "T1",
                Description = "desc",
                Priority = "High",
                Type = "Software",
                Status = "Open",
                CreatedByUserId = 1,
                CreatedAt = DateTime.UtcNow
            };
            db.Tickets_Table.Add(t);
            await db.SaveChangesAsync();

            t.Status = "Closed";
            var updated = await repo.UpdateTicket(t);

            Assert.That(updated.Status, Is.EqualTo("Closed"));
            Assert.That(updated.ResolvedAt, Is.Not.Null);
            Assert.That(updated.ResolutionTime, Is.Not.Null);
        }

        //
        // Verifies: when changing Status from Closed -> Open (reopen), the repository
        // clears ResolvedAt and ResolutionTime (KPI fields) accordingly.
        [Test]
        public async Task TicketRepo_UpdateTicket_WhenReopening_ClearsResolvedFields()
        {
            using var db = CreateDb();
            var repo = new TicketRepo(db);

            var t = new TicketModel
            {
                Title = "T2",
                Description = "desc",
                Priority = "Medium",
                Type = "Hardware",
                Status = "Closed",
                CreatedByUserId = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                ResolvedAt = DateTime.UtcNow.AddHours(-1),
                ResolutionTime = TimeSpan.FromHours(4)
            };
            db.Tickets_Table.Add(t);
            await db.SaveChangesAsync();

            t.Status = "Open";
            var updated = await repo.UpdateTicket(t);

            Assert.That(updated.Status, Is.EqualTo("Open"));
            Assert.That(updated.ResolvedAt, Is.Null);
            Assert.That(updated.ResolutionTime, Is.Null);
        }

        //
        // Verifies: User deletion is blocked when the user has any related tickets
        // (either created or assigned), returning false and retaining the user row.
        [Test]
        public async Task UserRepo_DeleteUser_WithTickets_ReturnsFalse()
        {
            using var db = CreateDb();
            var userRepo = new UserRepo(db);

            var user = new UserModel { Name = "U1", Email = "u1@example.com", PasswordHash = "x", Role = "User" };
            db.Users_Table.Add(user);
            await db.SaveChangesAsync();

            db.Tickets_Table.Add(new TicketModel
            {
                Title = "T1",
                Description = "desc",
                Priority = "Low",
                Type = "Software",
                Status = "Open",
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await userRepo.DeleteUser(user.Id);

            Assert.That(result, Is.False);
            Assert.That(db.Users_Table.Any(u => u.Id == user.Id), Is.True);
        }

        //
        // Verifies: adding a user persists to the database and returns the created entity
        // with a generated Id that is greater than zero.
        [Test]
        public async Task UserRepo_AddUser_Persists_And_Returns_User()
        {
            using var db = CreateDb();
            var userRepo = new UserRepo(db);

            var user = new UserModel { Name = "Alice", Email = "alice@example.com", PasswordHash = "pwd", Role = "User" };

            var created = await userRepo.AddUser(user);

            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That(db.Users_Table.Any(u => u.Email == "alice@example.com"), Is.True);
        }

        //
        // Verifies: repository can fetch a user by email and returns the correct record
        // among multiple users in the store.
        [Test]
        public async Task UserRepo_GetUserByEmail_Returns_Correct_User()
        {
            using var db = CreateDb();
            var userRepo = new UserRepo(db);

            db.Users_Table.AddRange(
                new UserModel { Name = "A", Email = "a@ex.com", PasswordHash = "x", Role = "User" },
                new UserModel { Name = "B", Email = "b@ex.com", PasswordHash = "x", Role = "Admin" }
            );
            await db.SaveChangesAsync();

            var found = await userRepo.GetUserByEmail("b@ex.com");

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("B"));
            Assert.That(found.Role, Is.EqualTo("Admin"));
        }

        //
        // Verifies: updating fields on an existing user (name, password, role) persists
        // and the repository returns the updated values.
        [Test]
        public async Task UserRepo_UpdateUser_Updates_Fields()
        {
            using var db = CreateDb();
            var userRepo = new UserRepo(db);

            var user = new UserModel { Name = "Carol", Email = "c@ex.com", PasswordHash = "p1", Role = "User" };
            db.Users_Table.Add(user);
            await db.SaveChangesAsync();

            user.Name = "Carol Updated";
            user.PasswordHash = "p2";
            user.Role = "Admin";

            var updated = await userRepo.UpdateUser(user);

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Name, Is.EqualTo("Carol Updated"));
            Assert.That(updated.PasswordHash, Is.EqualTo("p2"));
            Assert.That(updated.Role, Is.EqualTo("Admin"));
        }

        //
        // Verifies: deleting a user without any related tickets succeeds and removes
        // the user from the database, returning true.
        [Test]
        public async Task UserRepo_DeleteUser_NoTickets_ReturnsTrue_And_RemovesUser()
        {
            using var db = CreateDb();
            var userRepo = new UserRepo(db);

            var user = new UserModel { Name = "D", Email = "d@ex.com", PasswordHash = "x", Role = "User" };
            db.Users_Table.Add(user);
            await db.SaveChangesAsync();

            var result = await userRepo.DeleteUser(user.Id);

            Assert.That(result, Is.True);
            Assert.That(db.Users_Table.Any(u => u.Id == user.Id), Is.False);
        }
    }
}