using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Transactions;

namespace Wcf_CeadChat_ServiceLibrary
{
    public class ChatContext : DbContext
    {
        public ChatContext()
            //: base("ZireaelDb")
        {
            Database.SetInitializer(new ChatInitializer());
        }

        static ChatContext()
        {
            var ensureDLLIsCopied = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Friends> Friends { get; set; }
        public DbSet<BlackList> BlackList { get; set; }
        public DbSet<AvatarUser> AvatarUsers { get; set; }
        public DbSet<AvatarGroup> AvatarGroups { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Recovery> Recoveries { get; set; }
        public DbSet<Session> Sessions{ get; set; }
        public DbSet<EmailCode> EmailCodes { get; set; }
        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    //modelBuilder.Entity<User>().HasMany(d => d.Users).WithMany(f => f.Users);
        //}

        public override int SaveChanges()
        {
            int result = 0;
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
             new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                result = base.SaveChanges();
                scope.Complete();
            }
            return result;
        }
    }

    public class ChatInitializer : DropCreateDatabaseIfModelChanges<ChatContext>
    {
        protected override void Seed(ChatContext context)
        {
            //заполняем данными поумолчанию
           
            base.Seed(context);

            var user = new User
            {
                Login = $"system",
                PasswordHash = "system"
            };
            context.Users.Add(user);

            for (int i = 0; i < 25; i++)
            {
                context.Users.Add(new User
                {
                    Login = $"Qwerty{i}",
                    PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
                });
            }
            

            context.SaveChanges();

            context.Groups.ToList().Add(new Group()
            {
                Creator = user, IsVisible = true, Name = "test", Type = GroupType.MultyUser,
                Users = context.Users.Where(u => u.Login != "system").ToList()
            });
            context.SaveChanges();

        }
    }
}
