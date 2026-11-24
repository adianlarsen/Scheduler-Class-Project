using GroupSchedulerApp.Data;
using GroupSchedulerApp.Models;

namespace GroupSchedulerApp.Repositories;

// Saves users.
// Returned objects are snapshots; call a repository method to save changes.
public class UserRepository
{
    private readonly JsonDataStore _store;

    public UserRepository(JsonDataStore store) => _store = store;

    // Id == 0 creates a user. Otherwise, updates an existing user's name and email.
    public User Save(User user)
    {
        return _store.Update(data =>
        {
            var name = user.Name?.Trim() ?? "";
            var email = user.Email?.Trim() ?? "";

            if (name.Length == 0)
                throw new ArgumentException("Name is required.");
            if (email.Length == 0)
                throw new ArgumentException("Email is required.");

            var duplicate = data.Users.Any(u =>
                u.Id != user.Id &&
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                throw new ArgumentException("A user with that email already exists.");

            User saved;

            if (user.Id == 0)
            {
                saved = new User { Id = data.NextUserId++ };
                data.Users.Add(saved);
            }
            else
            {
                saved = data.Users.FirstOrDefault(u => u.Id == user.Id)
                    ?? throw new KeyNotFoundException("User not found.");
            }

            saved.Name = name;
            saved.Email = email;

            user.Id = saved.Id;
            return saved;
        });
    }

    public User? FindById(int id) =>
        _store.Read(data => data.Users.FirstOrDefault(u => u.Id == id));

    public User? FindByEmail(string email) =>
        _store.Read(data => data.Users.FirstOrDefault(u =>
            string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)));

    public List<User> FindAll() =>
        _store.Read(data => data.Users.ToList());
}