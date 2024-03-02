using CQ.ApiElements.Dtos;
using CQ.ApiElements.Extensions;

namespace CQ.ApiElements.Test
{
    [TestClass]
    public class CollectionExtensionTest
    {
        [TestMethod]
        public void WhenMapEntityToResult_ThenCollectionOfResult()
        {
            var users = new List<User>
            {
                new User { Id = 1, Name= "Test" },
            };

            var usersMapped = users.MapTo<UserModel, User>();

            Assert.IsNotNull(usersMapped);
            Assert.AreEqual(1, usersMapped.First().Id);
        }
    }

    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public record class UserModel : Response<User>
    {
        public int Id { get; set; }

        public UserModel(User u) : base(u) { }

        protected override void Map(User entity)
        {
            Id = entity.Id;
        }
    }
}