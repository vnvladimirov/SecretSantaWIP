namespace SecretSanta.Data;
using SecretSanta.Models;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Events.Any())
        {
            return;
        }


        var party = new Event
        {
            Name = "Office Christmas Party 2026",
            Description = "Annual gift exchange for the whole team :)",
            EventDate = new DateTime(2026, 12, 18),
            Budget = 50m,
            MinParticipants = 8,
            OrganizerName = "Alexandra Stoimenova",
            OrganizerEmail = "alexandra@example.com",
            Participants = new List<Participant>
            {
                new() {Name = "Petar Vitkov",       Email = "petarV@example.com"},
                new() {Name = "Milena Grigorova",   Email = "milena@example.com"},
                new() {Name = "Stoyan Stoyanov",    Email = "sapuniash@example.com"},
                new() {Name = "Dimitar Peev",       Email = "dpeev@example.com"},
                new() {Name = "Izabela Venelinova", Email = "iziVenelinova@example.com"},
                new() {Name = "Veselina Dragoeva",  Email = "rVeselina@example.com"},
                new() {Name = "Ivan Ivanov",        Email = "ivan.ivanov@example.com"},
                new() {Name = "Georgi Toshev",      Email = "gToshev@example.com"}
            }
        };


        context.Events.Add(party);
        context.SaveChanges();


        var coupledPeople = party.Participants.ToList();

        coupledPeople[0].PartnerId = coupledPeople[1].Id;
        coupledPeople[1].PartnerId = coupledPeople[0].Id;
        coupledPeople[2].PartnerId = coupledPeople[4].Id;
        coupledPeople[4].PartnerId = coupledPeople[2].Id;

        context.SaveChanges();
    }
}

