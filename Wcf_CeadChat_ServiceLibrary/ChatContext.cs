using System;
using System.Collections.Generic;
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
        public DbSet<Session> Sessions { get; set; }
        public DbSet<EmailCode> EmailCodes { get; set; }

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
            base.Seed(context);

            var system = new User
            {
                Login = $"system",
                PasswordHash = "system"
            };
            context.Users.Add(system);

            for (int i = 0; i < 25; i++)
            {
                context.Users.Add(new User
                {
                    Login = $"Qwerty{i}",
                    PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
                });
            }
            context.SaveChanges();

            var user00 = new User
            {
                Login = "Qwerty00",
                DisplayName = "Дизайнер",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user01 = new User
            {
                Login = "Qwerty01",
                DisplayName = "Дарт Вейдер",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user02 = new User
            {
                Login = "Qwerty02",
                DisplayName = "Гален",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user03 = new User
            {
                Login = "Qwerty03",
                DisplayName = "Ли",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user04 = new User
            {
                Login = "Qwerty04",
                DisplayName = "Макс Брайт",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user05 = new User
            {
                Login = "Qwerty05",
                DisplayName = "Лена Окстон",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            var user06 = new User
            {
                Login = "Qwerty06",
                DisplayName = "Кверти",
                Email = "okorokmahjonga@gmail.com",
                PasswordHash = "$argon2i$v=19$m=8192,t=3,p=1$OXhielVwUDJDNmZQWnQ2SEE5TWVuR0ZhUFI3WGNKU3lyNkVQeGh6ZU41cTdWaGVSVnNWQXRUYThhU1l3eWczVUp4$P9g/X76odAV67q3GLgb3wSHymjnV+fpMC+8wFQ0yuJc"
            };

            context.Users.AddRange(new[] { user00, user01, user02, user03, user04, user05, user06 });
            context.SaveChanges();

            var group00 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user00 }, };
            var group01 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user01 }, };
            var group02 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user02 }, };
            var group03 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user03 }, };
            var group04 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user04 }, };
            var group05 = new Group { Type = GroupType.MultyUser, Users = new[] { user06, user01, user02 }, Name = "Death Star 3.0" };
            var group06 = new Group { Type = GroupType.SingleUser, Users = new[] { user06, user05 }, };

            context.Groups.AddRange(new[] { group00, group01, group02, group03, group04, group05, group06, });
            context.SaveChanges();

            var file00 = new FileChat { Name = "N3W_Death$tar.p1ans.zip", CountPackages = 10, CountReadyPackages = 10, FullPath = "", Hash = "", Lenght = 11265000, Packages = new List<Package>(), };
            var message00 = new Message { DateTime = new DateTime(2019, 07, 01), Group = group00, IsRead = true, Sender = user06, Text = "Олег, где макет", };
            var message01 = new Message { DateTime = new DateTime(2019, 07, 09), Group = group01, IsRead = true, Sender = user01, Text = "ты недооцениваешь силу Тёмной стороны", };
            var message02 = new Message { DateTime = new DateTime(2019, 07, 10, 03, 52, 30), Group = group02, IsRead = false, Sender = user02, Text = "Hello, world!", };
            var message021 = new Message { DateTime = new DateTime(2019, 07, 10, 03, 53, 02), Group = group02, IsRead = false, Sender = user02, Text = "напоминает мне о китайском профессореr", };
            var message03 = new Message { DateTime = new DateTime(2019, 07, 10, 03, 59, 02), Group = group03, IsRead = true, Sender = user03, Text = "Мы можем назвать это Галактическая Звезда 7", };
            var message04 = new Message { DateTime = new DateTime(2019, 07, 10, 04, 02, 04), Group = group04, IsRead = true, Sender = user06, Text = "го на перекур :)", };
            var message05 = new Message { DateTime = new DateTime(2019, 07, 10, 04, 10, 06), Group = group05, IsRead = true, Sender = user06, Text = "Привет. Я переработал всю структуру", };
            var message06 = new Message { DateTime = new DateTime(2019, 07, 10, 04, 10, 08), Group = group05, IsRead = true, Sender = user06, Text = "Не обращайте внимания на задуманные планы", };
            var message07 = new MessageFile { DateTime = new DateTime(2019, 07, 10, 04, 10, 10), Group = group05, IsRead = true, Sender = user06, File = file00, Type = FileType.File };
            var message08 = new Message { DateTime = new DateTime(2019, 07, 10, 04, 11, 11), Group = group05, IsRead = true, Sender = system, Text = "Кверти добавил Дарт Вейдер", };
            var message09 = new Message { DateTime = new DateTime(2019, 07, 10, 04, 50, 12), Group = group05, IsRead = true, Sender = user01, Text = "на что повлияют изменения?", };
            var message010 = new Message { DateTime = new DateTime(2019, 07, 10, 05, 21, 00), Group = group05, IsRead = true, Sender = user02, Text = "Эта версия может взорвать две планеты на одном заряде. И мы можем использовать тот же дизайн, чтобы сделать «Звезду Cмерти 3 Плюс», которая делает то же самое, но больше. И вы не поверите качеству снимков, которые он может сделать :0", };
            var message011 = new Message { DateTime = new DateTime(2019, 07, 10), Group = group06, IsRead = false, Sender = user05, Text = "Стикер", };
            context.Messages.AddRange(new [] {message00, message01, message02, message021, message03, message04, message05, message06, message07, message08, message08, message09, message010, message011, });

            context.SaveChanges();
            group00.LastMessage = message00;
            group01.LastMessage = message01;
            group02.LastMessage = message021;
            group03.LastMessage = message03;
            group04.LastMessage = message04;
            group05.LastMessage = message010;
            group06.LastMessage = message011;
            context.SaveChanges();
        }
    }
}
